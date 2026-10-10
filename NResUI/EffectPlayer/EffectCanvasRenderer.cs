using System.Numerics;
using NResUI.Abstractions;
using Silk.NET.OpenGL;

namespace NResUI.EffectPlayer;

/// <summary>Режим смешивания спрайта на холсте.</summary>
public enum CanvasBlend
{
    /// <summary>Обычное alpha-смешивание: src*a + dst*(1-a).</summary>
    Alpha,

    /// <summary>Аддитивное: src*a + dst.</summary>
    Additive,
}

/// <summary>
/// Маленький GL-рендер холста проигрывателя: рисует четырёхугольники с текстурой в offscreen-текстуру (FBO).
/// ImGui умеет только alpha-смешивание, а эффекты игры (огонь, лазеры, свечение) смешиваются аддитивно, поэтому
/// холст собирается здесь, а в окно выводится готовой текстурой.
/// </summary>
public sealed class EffectCanvasRenderer : IService, IDisposable
{
    private const string VertexSource = """
        #version 330 core
        layout(location = 0) in vec2 aPos;
        layout(location = 1) in vec2 aUv;
        layout(location = 2) in vec4 aColor;
        uniform vec2 uSize;
        out vec2 vUv;
        out vec4 vColor;
        void main()
        {
            vUv = aUv;
            vColor = aColor;
            gl_Position = vec4(aPos.x / uSize.x * 2.0 - 1.0, aPos.y / uSize.y * 2.0 - 1.0, 0.0, 1.0);
        }
        """;

    private const string FragmentSource = """
        #version 330 core
        in vec2 vUv;
        in vec4 vColor;
        uniform sampler2D uTex;
        uniform int uAlphaFromLuma;
        out vec4 oColor;
        void main()
        {
            vec4 t = texture(uTex, vUv);
            float a = uAlphaFromLuma != 0 ? max(max(t.r, t.g), t.b) : t.a;
            oColor = vec4(t.rgb * vColor.rgb, a * vColor.a);
        }
        """;

    private readonly record struct Batch(uint Texture, CanvasBlend Blend, bool AlphaFromLuma, int FirstVertex, int VertexCount);

    private readonly GL _gl;
    private readonly List<float> _vertices = [];
    private readonly List<Batch> _batches = [];

    private uint _program;
    private uint _vao;
    private uint _vbo;
    private uint _fbo;
    private uint _colorTexture;
    private uint _whiteTexture;
    private int _width;
    private int _height;
    private int _sizeLocation;
    private int _lumaLocation;
    private int _texLocation;
    private bool _initialized;

    public EffectCanvasRenderer(GL gl)
    {
        _gl = gl;
    }

    /// <summary>Текстура результата последнего <see cref="Render"/>.</summary>
    public IntPtr TextureId => (IntPtr)_colorTexture;

    /// <summary>Начинает новый кадр: очищает список четырёхугольников.</summary>
    public void Begin()
    {
        _vertices.Clear();
        _batches.Clear();
    }

    /// <summary>Однотонный четырёхугольник (координаты в пикселях холста, начало сверху слева).</summary>
    public void AddSolidQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector4 color, CanvasBlend blend = CanvasBlend.Alpha)
    {
        EnsureInitialized();
        AddQuad(_whiteTexture, a, b, c, d, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero, color, blend, false);
    }

    /// <summary>Линия заданной толщины как тонкий четырёхугольник.</summary>
    public void AddLine(Vector2 from, Vector2 to, Vector4 color, float thickness = 1f)
    {
        var dir = to - from;
        if (dir.LengthSquared() < 1e-6f)
            return;

        var n = new Vector2(-dir.Y, dir.X);
        n = Vector2.Normalize(n) * (thickness * 0.5f);
        AddSolidQuad(from - n, to - n, to + n, from + n, color);
    }

    /// <summary>
    /// Четырёхугольник с текстурой. Вершины a,b,c,d обходятся по кругу; uv заданы в том же порядке.
    /// <paramref name="alphaFromLuma"/>: альфа берётся из яркости (для непрозрачных текстур, нарисованных без аддитивного режима).
    /// </summary>
    public void AddTexturedQuad(
        IntPtr texture,
        Vector2 a, Vector2 b, Vector2 c, Vector2 d,
        Vector2 uvA, Vector2 uvB, Vector2 uvC, Vector2 uvD,
        Vector4 color,
        CanvasBlend blend,
        bool alphaFromLuma)
    {
        EnsureInitialized();
        AddQuad((uint)texture, a, b, c, d, uvA, uvB, uvC, uvD, color, blend, alphaFromLuma);
    }

    private void AddQuad(
        uint texture,
        Vector2 a, Vector2 b, Vector2 c, Vector2 d,
        Vector2 uvA, Vector2 uvB, Vector2 uvC, Vector2 uvD,
        Vector4 color,
        CanvasBlend blend,
        bool alphaFromLuma)
    {
        var first = _vertices.Count / 8;
        Push(a, uvA, color);
        Push(b, uvB, color);
        Push(c, uvC, color);
        Push(a, uvA, color);
        Push(c, uvC, color);
        Push(d, uvD, color);

        if (_batches.Count > 0)
        {
            var last = _batches[^1];
            if (last.Texture == texture && last.Blend == blend && last.AlphaFromLuma == alphaFromLuma)
            {
                _batches[^1] = last with { VertexCount = last.VertexCount + 6 };
                return;
            }
        }

        _batches.Add(new Batch(texture, blend, alphaFromLuma, first, 6));
    }

    private void Push(Vector2 p, Vector2 uv, Vector4 color)
    {
        _vertices.Add(p.X);
        _vertices.Add(p.Y);
        _vertices.Add(uv.X);
        _vertices.Add(uv.Y);
        _vertices.Add(color.X);
        _vertices.Add(color.Y);
        _vertices.Add(color.Z);
        _vertices.Add(color.W);
    }

    /// <summary>Рисует накопленные четырёхугольники в текстуру размера width x height поверх цвета фона.</summary>
    public unsafe void Render(int width, int height, Vector4 background)
    {
        EnsureInitialized();
        width = Math.Max(width, 1);
        height = Math.Max(height, 1);
        ResizeTarget(width, height);

        // Сохраняем состояние, которое ImGui-бэкенд не восстанавливает сам.
        var prevFbo = _gl.GetInteger(GLEnum.FramebufferBinding);
        Span<int> viewport = stackalloc int[4];
        _gl.GetInteger(GLEnum.Viewport, viewport);
        var prevProgram = _gl.GetInteger(GLEnum.CurrentProgram);
        var prevVao = _gl.GetInteger(GLEnum.VertexArrayBinding);
        var prevArrayBuffer = _gl.GetInteger(GLEnum.ArrayBufferBinding);
        var prevTexture = _gl.GetInteger(GLEnum.TextureBinding2D);
        var prevActive = _gl.GetInteger(GLEnum.ActiveTexture);
        var blendWasOn = _gl.IsEnabled(EnableCap.Blend);
        var scissorWasOn = _gl.IsEnabled(EnableCap.ScissorTest);
        var cullWasOn = _gl.IsEnabled(EnableCap.CullFace);
        var depthWasOn = _gl.IsEnabled(EnableCap.DepthTest);

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        _gl.Viewport(0, 0, (uint)width, (uint)height);
        _gl.Disable(EnableCap.ScissorTest);
        _gl.Disable(EnableCap.CullFace);
        _gl.Disable(EnableCap.DepthTest);
        _gl.ClearColor(background.X, background.Y, background.Z, 1f);
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        if (_vertices.Count > 0)
        {
            _gl.UseProgram(_program);
            _gl.Uniform2(_sizeLocation, (float)width, (float)height);
            _gl.Uniform1(_texLocation, 0);
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.Enable(EnableCap.Blend);
            _gl.BlendEquation(BlendEquationModeEXT.FuncAdd);

            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            var data = _vertices.ToArray();
            fixed (float* ptr = data)
            {
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(float)), ptr, BufferUsageARB.StreamDraw);
            }

            foreach (var batch in _batches)
            {
                if (batch.Blend == CanvasBlend.Additive)
                    _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
                else
                    _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                _gl.BindTexture(TextureTarget.Texture2D, batch.Texture);
                _gl.Uniform1(_lumaLocation, batch.AlphaFromLuma ? 1 : 0);
                _gl.DrawArrays(PrimitiveType.Triangles, batch.FirstVertex, (uint)batch.VertexCount);
            }
        }

        // Возвращаем состояние.
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, (uint)prevFbo);
        _gl.Viewport(viewport[0], viewport[1], (uint)viewport[2], (uint)viewport[3]);
        _gl.UseProgram((uint)prevProgram);
        _gl.BindVertexArray((uint)prevVao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, (uint)prevArrayBuffer);
        _gl.ActiveTexture((TextureUnit)prevActive);
        _gl.BindTexture(TextureTarget.Texture2D, (uint)prevTexture);
        SetCap(EnableCap.Blend, blendWasOn);
        SetCap(EnableCap.ScissorTest, scissorWasOn);
        SetCap(EnableCap.CullFace, cullWasOn);
        SetCap(EnableCap.DepthTest, depthWasOn);
    }

    private void SetCap(EnableCap cap, bool on)
    {
        if (on)
            _gl.Enable(cap);
        else
            _gl.Disable(cap);
    }

    private unsafe void EnsureInitialized()
    {
        if (_initialized)
            return;

        _initialized = true;
        var vs = Compile(ShaderType.VertexShader, VertexSource);
        var fs = Compile(ShaderType.FragmentShader, FragmentSource);
        _program = _gl.CreateProgram();
        _gl.AttachShader(_program, vs);
        _gl.AttachShader(_program, fs);
        _gl.LinkProgram(_program);
        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out var linked);
        if (linked == 0)
            throw new InvalidOperationException("Шейдер холста эффектов: " + _gl.GetProgramInfoLog(_program));

        _gl.DeleteShader(vs);
        _gl.DeleteShader(fs);
        _sizeLocation = _gl.GetUniformLocation(_program, "uSize");
        _lumaLocation = _gl.GetUniformLocation(_program, "uAlphaFromLuma");
        _texLocation = _gl.GetUniformLocation(_program, "uTex");

        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();
        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        const uint stride = 8 * sizeof(float);
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, (void*)0);
        _gl.EnableVertexAttribArray(1);
        _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)(2 * sizeof(float)));
        _gl.EnableVertexAttribArray(2);
        _gl.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, stride, (void*)(4 * sizeof(float)));
        _gl.BindVertexArray(0);

        _whiteTexture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, _whiteTexture);
        var white = new byte[] { 255, 255, 255, 255 };
        fixed (byte* p = white)
        {
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, 1, 1, 0, PixelFormat.Rgba, PixelType.UnsignedByte, p);
        }

        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
    }

    private uint Compile(ShaderType type, string source)
    {
        var shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);
        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out var ok);
        if (ok == 0)
            throw new InvalidOperationException($"Шейдер холста эффектов ({type}): {_gl.GetShaderInfoLog(shader)}");

        return shader;
    }

    private unsafe void ResizeTarget(int width, int height)
    {
        if (_fbo != 0 && width == _width && height == _height)
            return;

        _width = width;
        _height = height;
        if (_fbo == 0)
        {
            _fbo = _gl.GenFramebuffer();
            _colorTexture = _gl.GenTexture();
        }

        _gl.BindTexture(TextureTarget.Texture2D, _colorTexture);
        _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        var prev = _gl.GetInteger(GLEnum.FramebufferBinding);
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _colorTexture, 0);
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, (uint)prev);
    }

    public void Dispose()
    {
        if (!_initialized)
            return;

        _gl.DeleteProgram(_program);
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteTexture(_whiteTexture);
        if (_fbo != 0)
        {
            _gl.DeleteFramebuffer(_fbo);
            _gl.DeleteTexture(_colorTexture);
        }
    }
}
