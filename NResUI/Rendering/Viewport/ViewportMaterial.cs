using TexmLib;

namespace NResUI.Rendering.Viewport;

public sealed class ViewportMaterial
{
    public string Name { get; }
    public uint TextureHandle { get; }
    public bool HasTexture => TextureHandle != 0;

    public TexmFile? Texm { get; }

    public ViewportMaterial(string name, uint textureHandle = 0, TexmFile? texm = null)
    {
        Name = name;
        TextureHandle = textureHandle;
        Texm = texm;
    }

    public static ViewportMaterial Untextured { get; } = new("Untextured");
}
