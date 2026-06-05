using Silk.NET.OpenGL;

namespace NResUI.Rendering.Viewport.OpenGL;

public sealed class MsaaFramebuffer
{
    private readonly GL _gl;
    private readonly uint _samples;

    private uint _msaaFramebuffer;
    private uint _msaaColorRenderbuffer;
    private uint _msaaDepthStencilRenderbuffer;

    private uint _resolveFramebuffer;
    private uint _resolveColorTexture;

    private int _width;
    private int _height;

    public uint ColorTexture => _resolveColorTexture;

    public MsaaFramebuffer(GL gl, uint samples = 4)
    {
        _gl = gl;
        _samples = samples;
    }

    public void EnsureSize(int width, int height)
    {
        width = Math.Max(1, width);
        height = Math.Max(1, height);

        if (_msaaFramebuffer != 0 && _width == width && _height == height)
            return;

        _width = width;
        _height = height;

        if (_msaaFramebuffer == 0)
        {
            _msaaFramebuffer = _gl.GenFramebuffer();
            _msaaColorRenderbuffer = _gl.GenRenderbuffer();
            _msaaDepthStencilRenderbuffer = _gl.GenRenderbuffer();

            _resolveFramebuffer = _gl.GenFramebuffer();
            _resolveColorTexture = _gl.GenTexture();
        }

        _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _msaaColorRenderbuffer);
        _gl.RenderbufferStorageMultisample(
            RenderbufferTarget.Renderbuffer,
            _samples,
            InternalFormat.Rgba8,
            (uint)width,
            (uint)height);

        _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _msaaDepthStencilRenderbuffer);
        _gl.RenderbufferStorageMultisample(
            RenderbufferTarget.Renderbuffer,
            _samples,
            InternalFormat.Depth24Stencil8,
            (uint)width,
            (uint)height);

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _msaaFramebuffer);

        _gl.FramebufferRenderbuffer(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            RenderbufferTarget.Renderbuffer,
            _msaaColorRenderbuffer);

        _gl.FramebufferRenderbuffer(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.DepthStencilAttachment,
            RenderbufferTarget.Renderbuffer,
            _msaaDepthStencilRenderbuffer);

        var msaaStatus = _gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (msaaStatus != GLEnum.FramebufferComplete)
            throw new InvalidOperationException($"MSAA viewport framebuffer is incomplete: {msaaStatus}");

        _gl.BindTexture(TextureTarget.Texture2D, _resolveColorTexture);
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

        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Linear);

        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear);

        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToEdge);

        _gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToEdge);

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _resolveFramebuffer);

        _gl.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            _resolveColorTexture,
            0);

        var resolveStatus = _gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (resolveStatus != GLEnum.FramebufferComplete)
            throw new InvalidOperationException($"Resolve viewport framebuffer is incomplete: {resolveStatus}");

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _gl.BindTexture(TextureTarget.Texture2D, 0);
        _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
    }

    public void BindForRender()
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, _msaaFramebuffer);
        _gl.Viewport(0, 0, (uint)_width, (uint)_height);
    }

    public void Resolve()
    {
        _gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _msaaFramebuffer);
        _gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, _resolveFramebuffer);

        _gl.BlitFramebuffer(
            0, 0, _width, _height,
            0, 0, _width, _height,
            ClearBufferMask.ColorBufferBit,
            BlitFramebufferFilter.Nearest);
    }
}
