using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PalLib;

/// <summary>PAL файл: indexed texture с lookup таблицей, подписью Ipol и индексами.</summary>
/// <param name="FileName">Имя PAL файла.</param>
/// <param name="Palette">[0x0000..0x0400] 256 цветов lookup (1024 байта).</param>
/// <param name="Indices">[0x0404..0x10404] 256x256 индексов в lookup.</param>
public record class PalFile(string FileName, byte[] Palette, byte[] Indices)
{
    public void SaveAsPng(string outputPath)
    {
        const int width = 256;
        const int height = 256;

        var rgbaBytes = new byte[width * height * 4];

        for (int i = 0; i < Indices.Length; i++)
        {
            var index = Indices[i];
            
            // PAL, вероятно, связан с DirectX, поэтому порядок каналов похож на BGRA.

            var b = Palette[index * 4 + 0];
            var g = Palette[index * 4 + 1];
            var r = Palette[index * 4 + 2];
            var a = Palette[index * 4 + 3]; // Альфа или padding; ниже пока используется непрозрачность.

            rgbaBytes[i * 4 + 0] = r;
            rgbaBytes[i * 4 + 1] = g;
            rgbaBytes[i * 4 + 2] = b;
            rgbaBytes[i * 4 + 3] = 255; 
        }

        using var image = Image.LoadPixelData<Rgba32>(rgbaBytes, width, height);
        image.SaveAsPng(outputPath);
    }
}
