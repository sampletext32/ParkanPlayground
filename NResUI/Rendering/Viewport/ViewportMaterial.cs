namespace NResUI.Rendering.Viewport;

public sealed class ViewportMaterial
{
    public string Name { get; }
    public uint TextureHandle { get; }
    public bool HasTexture => TextureHandle != 0;

    public ViewportMaterial(string name, uint textureHandle = 0)
    {
        Name = name;
        TextureHandle = textureHandle;
    }

    public static ViewportMaterial Untextured { get; } = new("Untextured");
}
