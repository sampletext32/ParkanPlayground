namespace TexmLib;

public static class Extensions
{
    public static int AsStride(this int format)
    {
        return format switch
        {
            0x0 => 8,
            0x235 => 16,
            0x22C => 16,
            0x115C => 16,
            0x58 => 16,
            0x378 => 32,
            0x22B8 => 32,
            _ => throw new InvalidOperationException($"Unsupported Texm format {format}")
        };
    }
}
