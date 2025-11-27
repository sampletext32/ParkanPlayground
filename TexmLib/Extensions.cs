namespace TexmLib;

public static class Extensions
{
    public static int AsStride(this int format)
    {
        return format switch
        {
            0x22B8 => 32,
            0x115C => 16,
            0x235 => 16,
            0x378 => 32,
            0 => 32
        };
    }
}