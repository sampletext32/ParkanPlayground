namespace TextureDecoder;

public static class Extensions
{
    public static int AsStride(this int format)
    {
        return format switch
        {
            8888 => 32,
            4444 => 16,
            565 => 16,
            888 => 32,
            0 => 32
        };
    }
}