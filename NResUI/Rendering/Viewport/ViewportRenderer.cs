using System.Numerics;
using NResUI.Rendering.Viewport.Meshes;
using NResUI.Rendering.Viewport.OpenGL;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace NResUI.Rendering.Viewport;

public sealed class ViewportRenderer
{
    private readonly GL _gl;
    private readonly IWindow _window;
    private readonly MsaaFramebuffer _framebuffer;

    public GL Gl => _gl;

    private ShaderProgram? _meshShader;
    private ShaderProgram? _pickingShader;

    private GpuMesh? _unitWireBoxMesh;
    private GpuMesh? _axesMesh;

    private uint _pickingFramebuffer;
    private uint _pickingColorTexture;
    private uint _pickingDepthRenderbuffer;
    private int _pickingWidth;
    private int _pickingHeight;

    private int _modelLocation;
    private int _mvpLocation;
    private int _lightDirectionLocation;
    private int _useTextureLocation;
    private int _texture0Location;
    private int _colorAddLocation;
    private int _pickingMvpLocation;
    private int _pickingColorLocation;

    private bool _initialized;

    public ViewportRenderer(GL gl, IWindow window)
    {
        _gl = gl;
        _window = window;
        _framebuffer = new MsaaFramebuffer(gl, samples: 4);
    }

    public uint Render(int width, int height, ViewportScene scene, ViewportCamera camera)
    {
        EnsureInitialized();

        width = Math.Max(1, width);
        height = Math.Max(1, height);

        _framebuffer.EnsureSize(width, height);
        _framebuffer.BindForRender();

        _gl.Enable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.CullFace);

        _gl.ClearColor(0.12f, 0.13f, 0.15f, 1.0f);
        _gl.ClearStencil(0);
        _gl.Clear((uint)(
            ClearBufferMask.ColorBufferBit |
            ClearBufferMask.DepthBufferBit |
            ClearBufferMask.StencilBufferBit));

        var aspect = width / (float)Math.Max(1, height);
        var sceneRotation = camera.GetSceneRotationMatrix();
        var view = camera.GetViewMatrix();
        var projection = camera.GetProjectionMatrix(aspect);

        DrawGrid(scene, sceneRotation, view, projection);
        DrawScene(scene, sceneRotation, view, projection);
        DrawDebugOverlays(scene, sceneRotation, view, projection);

        _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        _framebuffer.Resolve();

        _gl.Disable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.StencilTest);
        _gl.UseProgram(0);
        _gl.BindVertexArray(0);
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        var framebufferSize = _window.FramebufferSize;
        _gl.Viewport(0, 0, (uint)framebufferSize.X, (uint)framebufferSize.Y);

        return _framebuffer.ColorTexture;
    }

    /// <summary>
    /// Делает GPU picking: рендерит pieces в служебный framebuffer уникальными цветами и читает один пиксель.
    /// Так выбор совпадает с видимой геометрией и не требует CPU-копии вершин.
    /// </summary>
    public unsafe int PickPiece(int width, int height, ViewportScene scene, ViewportCamera camera, Vector2 localMouse)
    {
        EnsureInitialized();

        if (_pickingShader == null || width <= 1 || height <= 1)
            return -1;

        var x = Math.Clamp((int)localMouse.X, 0, width - 1);
        var y = Math.Clamp(height - 1 - (int)localMouse.Y, 0, height - 1);

        EnsurePickingFramebuffer(width, height);

        try
        {
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _pickingFramebuffer);
            _gl.Viewport(0, 0, (uint)width, (uint)height);
            _gl.Enable(EnableCap.DepthTest);
            _gl.Disable(EnableCap.CullFace);
            _gl.Disable(EnableCap.Blend);
            _gl.Disable(EnableCap.StencilTest);
            _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
            _gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            var aspect = width / (float)Math.Max(1, height);
            var sceneRotation = camera.GetSceneRotationMatrix();
            var view = camera.GetViewMatrix();
            var projection = camera.GetProjectionMatrix(aspect);

            _pickingShader.Use();
            foreach (var piece in scene.Pieces)
                DrawPieceForPicking(piece, sceneRotation, view, projection);

            Span<byte> pixel = stackalloc byte[4];
            fixed (byte* pixelPtr = pixel)
            {
                _gl.ReadPixels(x, y, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, pixelPtr);
            }

            var encoded = pixel[0] | (pixel[1] << 8) | (pixel[2] << 16);
            return encoded == 0 ? -1 : encoded - 1;
        }
        finally
        {
            RestoreDefaultRenderTargetState();
        }
    }

    private void DrawGrid(
        ViewportScene scene,
        Matrix4x4 sceneRotation,
        Matrix4x4 view,
        Matrix4x4 projection)
    {
        if (_meshShader == null)
            throw new InvalidOperationException("Viewport mesh shader is not initialized.");

        var grid = scene.Grid;
        if (grid == null || !grid.IsVisible)
            return;

        var model = grid.LocalTransform * sceneRotation;

        _gl.Disable(EnableCap.StencilTest);
        _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);

        DrawMesh(grid.Mesh, model, view, projection);

    }

    private void DrawScene(
        ViewportScene scene,
        Matrix4x4 sceneRotation,
        Matrix4x4 view,
        Matrix4x4 projection)
    {
        if (_meshShader == null)
            throw new InvalidOperationException("Viewport renderer is not initialized.");

        _gl.Disable(EnableCap.StencilTest);
        _meshShader.Use();
        _gl.PolygonMode(
            TriangleFace.FrontAndBack,
            scene.Debug.Wireframe ? PolygonMode.Line : PolygonMode.Fill);

        foreach (var piece in scene.Pieces)
        {
            var colorAdd = piece.Id == scene.SelectedPieceId
                ? new Vector3(0.22f, 0.16f, 0.02f)
                : Vector3.Zero;
            DrawPiece(piece, sceneRotation, view, projection, colorAdd);
        }

        _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
    }

    private void DrawDebugOverlays(
        ViewportScene scene,
        Matrix4x4 sceneRotation,
        Matrix4x4 view,
        Matrix4x4 projection)
    {
        if (_meshShader == null || _unitWireBoxMesh == null || _axesMesh == null)
            throw new InvalidOperationException("Viewport debug resources are not initialized.");

        _gl.Disable(EnableCap.StencilTest);
        _gl.Disable(EnableCap.DepthTest);
        _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        _gl.LineWidth(2.0f);

        _meshShader.Use();

        if (scene.Debug.ShowOriginAxes)
            DrawMesh(_axesMesh, Matrix4x4.CreateScale(1.25f) * sceneRotation, view, projection);

        if (scene.Debug.ShowPieceOrigins)
        {
            foreach (var piece in scene.Pieces)
            {
                var axesModel = Matrix4x4.CreateScale(0.45f) * piece.LocalTransform * sceneRotation;
                DrawMesh(_axesMesh, axesModel, view, projection);
            }
        }

        if (scene.Debug.ShowSelectedBounds && scene.SelectedPiece != null)
        {
            var selectedPiece = scene.SelectedPiece;
            var boundsModel = BuildLocalBoundsModel(selectedPiece.BoundsMin, selectedPiece.BoundsMax) *
                              selectedPiece.LocalTransform *
                              sceneRotation;

            DrawMesh(_unitWireBoxMesh, boundsModel, view, projection);
        }

        if (scene.Debug.ShowSceneBounds && scene.TryGetSceneWorldBounds(out var sceneBounds))
        {
            var sceneBoundsModel = BuildLocalBoundsModel(sceneBounds.Min, sceneBounds.Max) * sceneRotation;
            DrawMesh(_unitWireBoxMesh, sceneBoundsModel, view, projection);
        }

        _gl.LineWidth(1.0f);
        _gl.Enable(EnableCap.DepthTest);
    }

    private void DrawPiece(
        ViewportPiece piece,
        Matrix4x4 sceneRotation,
        Matrix4x4 view,
        Matrix4x4 projection,
        Vector3 colorAdd)
    {
        var model = piece.LocalTransform * sceneRotation;
        foreach (var mesh in piece.Meshes)
            DrawMesh(mesh, model, view, projection, colorAdd);
    }

    private void DrawPieceForPicking(
        ViewportPiece piece,
        Matrix4x4 sceneRotation,
        Matrix4x4 view,
        Matrix4x4 projection)
    {
        if (_pickingShader == null)
            throw new InvalidOperationException("Viewport picking shader is not initialized.");

        var encodedColor = EncodePickingColor(piece.Id);
        var model = piece.LocalTransform * sceneRotation;
        var mvp = model * view * projection;

        _pickingShader.SetMatrix4(_pickingMvpLocation, mvp);
        _pickingShader.SetVector4(_pickingColorLocation, encodedColor);

        // Рисуем теми же VAO, что и обычный viewport. Depth buffer сам выберет ближайшую piece под курсором.
        foreach (var mesh in piece.Meshes)
            mesh.Draw();
    }

    private void DrawMesh(
        GpuMesh mesh,
        Matrix4x4 model,
        Matrix4x4 view,
        Matrix4x4 projection,
        Vector3? colorAdd = null)
    {
        if (_meshShader == null)
            throw new InvalidOperationException("Viewport mesh shader is not initialized.");

        var mvp = model * view * projection;

        var lightDirection = Vector3.Normalize(new Vector3(-0.35f, -0.75f, -0.55f));

        _meshShader.Use();
        _meshShader.SetMatrix4(_mvpLocation, mvp);
        _meshShader.SetMatrix4(_modelLocation, model);
        _meshShader.SetVector3(_lightDirectionLocation, lightDirection);
        _meshShader.SetVector3(_colorAddLocation, colorAdd ?? Vector3.Zero);

        if (mesh.Material.HasTexture)
        {
            _gl.ActiveTexture(TextureUnit.Texture0);
            _gl.BindTexture(TextureTarget.Texture2D, mesh.Material.TextureHandle);
            _meshShader.SetInt(_texture0Location, 0);
            _meshShader.SetInt(_useTextureLocation, 1);
        }
        else
        {
            _gl.BindTexture(TextureTarget.Texture2D, 0);
            _meshShader.SetInt(_useTextureLocation, 0);
        }

        mesh.Draw();
    }

    private static Matrix4x4 BuildLocalBoundsModel(Vector3 boundsMin, Vector3 boundsMax)
    {
        var center = (boundsMin + boundsMax) * 0.5f;
        var size = boundsMax - boundsMin;

        return Matrix4x4.CreateScale(size * 0.5f) * Matrix4x4.CreateTranslation(center);
    }

    private unsafe void EnsurePickingFramebuffer(int width, int height)
    {
        width = Math.Max(1, width);
        height = Math.Max(1, height);

        if (_pickingFramebuffer != 0 && _pickingWidth == width && _pickingHeight == height)
            return;

        _pickingWidth = width;
        _pickingHeight = height;

        if (_pickingFramebuffer == 0)
        {
            _pickingFramebuffer = _gl.GenFramebuffer();
            _pickingColorTexture = _gl.GenTexture();
            _pickingDepthRenderbuffer = _gl.GenRenderbuffer();
        }

        _gl.BindTexture(TextureTarget.Texture2D, _pickingColorTexture);
        _gl.TexImage2D(
            TextureTarget.Texture2D,
            0,
            InternalFormat.Rgba8,
            (uint)width,
            (uint)height,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            null);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _pickingDepthRenderbuffer);
        _gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent24, (uint)width, (uint)height);

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _pickingFramebuffer);
        _gl.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            _pickingColorTexture,
            0);
        _gl.FramebufferRenderbuffer(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthAttachment,
            RenderbufferTarget.Renderbuffer,
            _pickingDepthRenderbuffer);

        var status = _gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != GLEnum.FramebufferComplete)
            throw new InvalidOperationException($"Viewport picking framebuffer is incomplete: {status}");

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _gl.BindTexture(TextureTarget.Texture2D, 0);
        _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
    }

    private void RestoreDefaultRenderTargetState()
    {
        _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        _gl.Disable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.StencilTest);
        _gl.UseProgram(0);
        _gl.BindVertexArray(0);
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        var framebufferSize = _window.FramebufferSize;
        _gl.Viewport(0, 0, (uint)framebufferSize.X, (uint)framebufferSize.Y);
    }

    private static Vector4 EncodePickingColor(int pieceId)
    {
        var encoded = pieceId + 1;
        if (encoded <= 0 || encoded > 0x00FFFFFF)
            return Vector4.Zero;

        var r = (encoded & 0x0000FF) / 255.0f;
        var g = ((encoded >> 8) & 0x0000FF) / 255.0f;
        var b = ((encoded >> 16) & 0x0000FF) / 255.0f;
        return new Vector4(r, g, b, 1.0f);
    }

    private void EnsureInitialized()
    {
        if (_initialized)
            return;

        CreateShaders();
        _unitWireBoxMesh = PrimitiveMeshes.CreateUnitWireBox(_gl);
        _axesMesh = PrimitiveMeshes.CreateAxes(_gl);
        _initialized = true;
    }

    private void CreateShaders()
    {
        const string meshVertexShaderSource = """
        #version 330 core
        
        layout (location = 0) in vec3 aPosition;
        layout (location = 1) in vec3 aColor;
        layout (location = 2) in vec3 aNormal;
        layout (location = 3) in vec2 aTexCoord;
        
        uniform mat4 uModel;
        uniform mat4 uMvp;
        
        out vec3 vColor;
        out vec3 vNormalWorld;
        out vec2 vTexCoord;
        
        void main()
        {
            vColor = aColor;
            vNormalWorld = mat3(transpose(inverse(uModel))) * aNormal;
            vTexCoord = aTexCoord;
            gl_Position = uMvp * vec4(aPosition, 1.0);
        }
        """;

        const string meshFragmentShaderSource = """
        #version 330 core
        
        in vec3 vColor;
        in vec3 vNormalWorld;
        in vec2 vTexCoord;
        
        uniform vec3 uLightDirectionWorld;
        uniform vec3 uColorAdd;
        uniform bool uUseTexture;
        uniform sampler2D uTexture0;
        
        out vec4 FragColor;
        
        void main()
        {
            vec3 normal = normalize(vNormalWorld);
        
            // Useful when backface display is enabled.
            if (!gl_FrontFacing)
                normal = -normal;
        
            vec3 lightDir = normalize(-uLightDirectionWorld);
        
            float diffuse = max(dot(normal, lightDir), 0.0);
            float lighting = 0.35 + diffuse * 0.65;
        
            vec4 texel = uUseTexture ? texture(uTexture0, vTexCoord) : vec4(1.0);
            if (texel.a < 0.05)
                discard;

            vec3 litColor = vColor * texel.rgb * lighting;
            FragColor = vec4(clamp(litColor + uColorAdd, 0.0, 1.0), texel.a);
        }
        """;

        const string pickingVertexShaderSource = """
        #version 330 core

        layout (location = 0) in vec3 aPosition;

        uniform mat4 uMvp;

        void main()
        {
            gl_Position = uMvp * vec4(aPosition, 1.0);
        }
        """;

        const string pickingFragmentShaderSource = """
        #version 330 core

        uniform vec4 uPickColor;
        out vec4 FragColor;

        void main()
        {
            FragColor = uPickColor;
        }
        """;

        _meshShader = new ShaderProgram(_gl, meshVertexShaderSource, meshFragmentShaderSource, "Viewport mesh");
        _pickingShader = new ShaderProgram(_gl, pickingVertexShaderSource, pickingFragmentShaderSource,
            "Viewport picking");

        _modelLocation = _meshShader.GetUniformLocation("uModel");
        _mvpLocation = _meshShader.GetUniformLocation("uMvp");
        _lightDirectionLocation = _meshShader.GetUniformLocation("uLightDirectionWorld");
        _useTextureLocation = _meshShader.GetUniformLocation("uUseTexture");
        _texture0Location = _meshShader.GetUniformLocation("uTexture0");
        _colorAddLocation = _meshShader.GetUniformLocation("uColorAdd");

        _pickingMvpLocation = _pickingShader.GetUniformLocation("uMvp");
        _pickingColorLocation = _pickingShader.GetUniformLocation("uPickColor");
    }
}
