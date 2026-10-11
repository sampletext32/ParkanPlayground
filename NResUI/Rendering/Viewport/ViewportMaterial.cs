using TexmLib;

namespace NResUI.Rendering.Viewport;

public sealed class ViewportMaterial
{
    public string Name { get; }
    public uint TextureHandle { get; }
    public bool HasTexture => TextureHandle != 0;

    public TexmFile? Texm { get; }

    /// <summary>
    /// UV страницы атласа: UV модели заданы относительно страницы стадии материала (0..1 внутри прямоугольника страницы),
    /// итоговый UV = uv * Scale + Offset (в долях всей текстуры). Для материалов без страницы - единичное преобразование.
    /// </summary>
    public System.Numerics.Vector2 UvScale { get; }

    public System.Numerics.Vector2 UvOffset { get; }

    public ViewportMaterial(
        string name,
        uint textureHandle = 0,
        TexmFile? texm = null,
        System.Numerics.Vector2? uvScale = null,
        System.Numerics.Vector2? uvOffset = null)
    {
        Name = name;
        TextureHandle = textureHandle;
        Texm = texm;
        UvScale = uvScale ?? System.Numerics.Vector2.One;
        UvOffset = uvOffset ?? System.Numerics.Vector2.Zero;
    }

    public System.Numerics.Vector2 TransformUv(System.Numerics.Vector2 uv) => uv * UvScale + UvOffset;

    public static ViewportMaterial Untextured { get; } = new("Untextured");
}
