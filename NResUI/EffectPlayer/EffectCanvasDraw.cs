using System.Numerics;

namespace NResUI.EffectPlayer;

/// <summary>Рисование команд <see cref="DrawItem"/> на холсте <see cref="EffectCanvasRenderer"/> (общее для панелей).</summary>
public static class EffectCanvasDraw
{
    /// <summary>Рисует спрайт на холсте. Возвращает false, если материал/текстура недоступны.</summary>
    public static bool Sprite(
        EffectCanvasRenderer canvas, EffectAssetCache assets, DrawItem item, Vector2 pos, float ppu,
        float spriteScale, int blendModeIndex, float timeMs)
    {
        var scale = ppu * spriteScale;

        // Параллелограмм: стороны 2*HalfU и 2*HalfV в плоскости вида (экранная Y направлена вниз).
        Vector2 u;
        Vector2 v;
        if (item.HalfU != Vector2.Zero || item.HalfV != Vector2.Zero)
        {
            u = new Vector2(item.HalfU.X, -item.HalfU.Y) * scale;
            v = new Vector2(item.HalfV.X, -item.HalfV.Y) * scale;
        }
        else
        {
            var width = item.Width > 0f ? item.Width : item.Size;
            var height = item.Height > 0f ? item.Height : item.Size;
            u = new Vector2(width * 0.5f * scale, 0f);
            v = new Vector2(0f, height * 0.5f * scale);
        }

        // Минимальный видимый размер: плоскость, видимая ребром, остаётся тонкой линией.
        const float minHalf = 1f;
        if (u.Length() < minHalf && u != Vector2.Zero)
            u = Vector2.Normalize(u) * minHalf;
        if (v.Length() < minHalf && v != Vector2.Zero)
            v = Vector2.Normalize(v) * minHalf;
        if (u == Vector2.Zero)
            u = new Vector2(minHalf, 0f);
        if (v == Vector2.Zero)
            v = new Vector2(0f, minHalf);

        if (string.IsNullOrEmpty(item.Material) ||
            !assets.TryGetSprite(item.Material, timeMs, out var frame, item.Phase))
            return false;

        var additive = blendModeIndex switch
        {
            1 => false,
            2 => true,
            _ => frame.Additive,
        };

        var alpha = Math.Clamp(item.Alpha * frame.Tint.W, 0f, 1f);
        var color = new Vector4(
            frame.Tint.X * item.Color.X, frame.Tint.Y * item.Color.Y, frame.Tint.Z * item.Color.Z, alpha);
        canvas.AddTexturedQuad(
            frame.TextureId,
            pos - u - v, pos + u - v, pos + u + v, pos - u + v,
            new Vector2(frame.Uv0.X, frame.Uv0.Y), new Vector2(frame.Uv1.X, frame.Uv0.Y),
            new Vector2(frame.Uv1.X, frame.Uv1.Y), new Vector2(frame.Uv0.X, frame.Uv1.Y),
            color,
            additive ? CanvasBlend.Additive : CanvasBlend.Alpha,
            alphaFromLuma: !additive && !frame.HasAlpha);
        return true;
    }

    public static void Light(EffectCanvasRenderer canvas, EffectAssetCache assets, DrawItem item, Vector2 pos, float ppu)
    {
        // Дальность света в данных (3..12 ед.) намного больше самого эффекта, поэтому рисуется компактное мягкое
        // свечение с размером, слабо зависящим от дальности, а не область освещения.
        var radius = (10f + 14f * MathF.Sqrt(MathF.Max(item.Size, 0f))) * ppu / 120f;
        var color = new Vector3(
            Math.Clamp(item.Color.X, 0f, 1f), Math.Clamp(item.Color.Y, 0f, 1f), Math.Clamp(item.Color.Z, 0f, 1f));
        var glow = assets.GetGlowTextureId();

        // Широкий ореол цвета источника и небольшое светлое ядро (аддитивно, как свечение).
        AddGlow(canvas, glow, pos, radius, new Vector4(color, 0.85f));
        var white = Vector3.Lerp(color, Vector3.One, 0.6f);
        AddGlow(canvas, glow, pos, radius * 0.4f, new Vector4(white, 0.9f));
    }

    private static void AddGlow(EffectCanvasRenderer canvas, IntPtr texture, Vector2 pos, float radius, Vector4 color)
    {
        var r = new Vector2(radius, radius);
        canvas.AddTexturedQuad(
            texture,
            pos - r, new Vector2(pos.X + radius, pos.Y - radius), pos + r, new Vector2(pos.X - radius, pos.Y + radius),
            Vector2.Zero, new Vector2(1, 0), Vector2.One, new Vector2(0, 1),
            color, CanvasBlend.Additive, alphaFromLuma: false);
    }
}
