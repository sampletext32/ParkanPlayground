using Common;

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

        var palette = stream.ReadBytesExact(1024);
        var signature = stream.ReadFixedString(4);

        if (signature != "Ipol")
        {
            throw new InvalidDataException($"Invalid PAL file signature. Expected 'Ipol', got '{signature}'.");
        }

        var indices = stream.ReadBytesExact(65536);

        return new PalFile(filename, palette, indices);
    }
}
