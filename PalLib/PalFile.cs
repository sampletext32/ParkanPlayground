using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PalLib;

/// <summary>
/// PAL файл по сути это indexed текстура (1024 байт - 256 цветов lookup + 4 байта "Ipol" и затем 256x256 индексов в lookup)
/// </summary>
public class PalFile
{
    public required string FileName { get; set; }
    
    /// <summary>
    /// 256 цветов lookup (1024 байт)
    /// </summary>
    public required byte[] Palette { get; set; }
    
    /// <summary>
    /// 256x256 индексов в lookup
    /// </summary>
    public required byte[] Indices { get; set; }

    public void SaveAsPng(string outputPath)
    {
        const int width = 256;
        const int height = 256;

        var rgbaBytes = new byte[width * height * 4];

        for (int i = 0; i < Indices.Length; i++)
        {
            var index = Indices[i];
            
            // Palette is 256 colors * 4 bytes (ARGB usually, based on TexmLib)
            // TexmLib: r = lookup[i*4+0], g = lookup[i*4+1], b = lookup[i*4+2], a = lookup[i*4+3]
            // Assuming same format here.
            
            // since PAL is likely directx related, the format is is likely BGRA

            var b = Palette[index * 4 + 0];
            var g = Palette[index * 4 + 1];
            var r = Palette[index * 4 + 2];
            var a = Palette[index * 4 + 3]; // Alpha? Or is it unused/padding? TexmLib sets alpha to 255 manually for indexed.

            rgbaBytes[i * 4 + 0] = r;
            rgbaBytes[i * 4 + 1] = g;
            rgbaBytes[i * 4 + 2] = b;
            rgbaBytes[i * 4 + 3] = 255; 
        }

        using var image = Image.LoadPixelData<Rgba32>(rgbaBytes, width, height);
        image.SaveAsPng(outputPath);
    }
}
