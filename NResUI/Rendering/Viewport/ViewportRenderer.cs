using System.Numerics;
using NResUI.Rendering.Viewport.OpenGL;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace NResUI.Rendering.Viewport;

public sealed class ViewportRenderer
{
    private readonly GL _gl;
    private readonly IWindow _window;
    private readonly MsaaFramebuffer _framebuffer;

    private ShaderProgram? _meshShader;
    private ShaderProgram? _outlineShader;

    private int _meshMvpLocation;
    private int _outlineMvpLocation;
    private int _outlineColorLocation;

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
        _gl.Enable(EnableCap.CullFace);
        _gl.CullFace(TriangleFace.Back);

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

        _framebuffer.Resolve();

        _gl.Disable(EnableCap.CullFace);
        _gl.Disable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.StencilTest);
        _gl.UseProgram(0);
        _gl.BindVertexArray(0);
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        var framebufferSize = _window.FramebufferSize;
        _gl.Viewport(0, 0, (uint)framebufferSize.X, (uint)framebufferSize.Y);

        return _framebuffer.ColorTexture;
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
        var mvp = model * view * projection;

        _gl.Disable(EnableCap.StencilTest);
        _gl.Disable(EnableCap.CullFace);

        _meshShader.Use();
        _meshShader.SetMatrix4(_meshMvpLocation, mvp);
        grid.Mesh.Draw();

        _gl.Enable(EnableCap.CullFace);
    }

    private void DrawScene(
        ViewportScene scene,
        Matrix4x4 sceneRotation,
        Matrix4x4 view,
        Matrix4x4 projection)
    {
        if (_meshShader == null || _outlineShader == null)
            throw new InvalidOperationException("Viewport renderer is not initialized.");

        _gl.Disable(EnableCap.StencilTest);
        _meshShader.Use();

        foreach (var piece in scene.Pieces)
        {
            if (piece.Id == scene.SelectedPieceId)
                continue;

            DrawPiece(piece, sceneRotation, view, projection);
        }

        var selectedPiece = scene.SelectedPiece;
        if (selectedPiece == null)
            return;

        _gl.Enable(EnableCap.StencilTest);
        _gl.StencilMask(0xFF);
        _gl.StencilFunc(StencilFunction.Always, 1, 0xFF);
        _gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);

        _meshShader.Use();
        DrawPiece(selectedPiece, sceneRotation, view, projection);

        _gl.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
        _gl.StencilMask(0x00);
        _gl.Disable(EnableCap.DepthTest);

        DrawPieceOutline(selectedPiece, sceneRotation, view, projection);

        _gl.Enable(EnableCap.DepthTest);
        _gl.StencilMask(0xFF);
        _gl.Disable(EnableCap.StencilTest);
    }

    private void DrawPiece(
        ViewportPiece piece,
        Matrix4x4 sceneRotation,
        Matrix4x4 view,
        Matrix4x4 projection)
    {
        if (_meshShader == null)
            throw new InvalidOperationException("Viewport mesh shader is not initialized.");

        var model = piece.LocalTransform * sceneRotation;
        var mvp = model * view * projection;

        _meshShader.Use();
        _meshShader.SetMatrix4(_meshMvpLocation, mvp);
        piece.Mesh.Draw();
    }

    private void DrawPieceOutline(
        ViewportPiece piece,
        Matrix4x4 sceneRotation,
        Matrix4x4 view,
        Matrix4x4 projection)
    {
        if (_outlineShader == null)
            throw new InvalidOperationException("Viewport outline shader is not initialized.");

        const float outlineScale = 1.06f;

        var model = Matrix4x4.CreateScale(outlineScale) * piece.LocalTransform * sceneRotation;
        var mvp = model * view * projection;

        _outlineShader.Use();
        _outlineShader.SetMatrix4(_outlineMvpLocation, mvp);
        _outlineShader.SetVector4(_outlineColorLocation, new Vector4(1.0f, 0.82f, 0.15f, 1.0f));

        piece.Mesh.Draw();
    }

    private void EnsureInitialized()
    {
        if (_initialized)
            return;

        CreateShaders();
        _initialized = true;
    }

    private void CreateShaders()
    {
        const string meshVertexShaderSource = """
        #version 330 core

        layout (location = 0) in vec3 aPosition;
        layout (location = 1) in vec3 aColor;

        uniform mat4 uMvp;

        out vec3 vColor;

        void main()
        {
            vColor = aColor;
            gl_Position = uMvp * vec4(aPosition, 1.0);
        }
        """;

        const string meshFragmentShaderSource = """
        #version 330 core

        in vec3 vColor;
        out vec4 FragColor;

        void main()
        {
            FragColor = vec4(vColor, 1.0);
        }
        """;

        const string outlineVertexShaderSource = """
        #version 330 core

        layout (location = 0) in vec3 aPosition;

        uniform mat4 uMvp;

        void main()
        {
            gl_Position = uMvp * vec4(aPosition, 1.0);
        }
        """;

        const string outlineFragmentShaderSource = """
        #version 330 core

        uniform vec4 uColor;
        out vec4 FragColor;

        void main()
        {
            FragColor = uColor;
        }
        """;

        _meshShader = new ShaderProgram(_gl, meshVertexShaderSource, meshFragmentShaderSource, "Viewport mesh");
        _outlineShader = new ShaderProgram(_gl, outlineVertexShaderSource, outlineFragmentShaderSource, "Viewport outline");

        _meshMvpLocation = _meshShader.GetUniformLocation("uMvp");
        _outlineMvpLocation = _outlineShader.GetUniformLocation("uMvp");
        _outlineColorLocation = _outlineShader.GetUniformLocation("uColor");
    }
}
