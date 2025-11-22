using System.Text;

namespace PalLib;

public class PalParser
{
    public static PalFile ReadFromStream(Stream stream, string filename)
    {
        // Expected size: 1024 (palette) + 4 ("Ipol") + 65536 (indices) = 66564
        if (stream.Length != 66564)
        {
            throw new InvalidDataException($"Invalid PAL file size. Expected 66564 bytes, got {stream.Length}.");
        }

        var palette = new byte[1024];
        stream.ReadExactly(palette, 0, 1024);

        var signatureBytes = new byte[4];
        stream.ReadExactly(signatureBytes, 0, 4);
        var signature = Encoding.ASCII.GetString(signatureBytes);

        if (signature != "Ipol")
        {
            throw new InvalidDataException($"Invalid PAL file signature. Expected 'Ipol', got '{signature}'.");
        }

        var indices = new byte[65536];
        stream.ReadExactly(indices, 0, 65536);

        return new PalFile
        {
            FileName = filename,
            Palette = palette,
            Indices = indices
        };
    }
}
