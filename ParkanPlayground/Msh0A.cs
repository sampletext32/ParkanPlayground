using System.Buffers.Binary;
using System.Text;
using NResLib;

namespace ParkanPlayground;

public class Msh0A
{
    public static List<string> ReadComponent(FileStream mshFs, NResArchive archive)
    {
        var aFileEntry = archive.Files.FirstOrDefault(x => x.FileType == "0A 00 00 00");

        if (aFileEntry is null)
        {
            throw new Exception("Archive doesn't contain 0A component");
        }

        var data = new byte[aFileEntry.FileLength];
        mshFs.Seek(aFileEntry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        int pos = 0;
        var strings = new List<string>();
        while (pos < data.Length)
        {
            var len = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(pos));
            if (len == 0)
            {
                pos += 4; // empty entry, no string attached
                strings.Add(""); // add empty string
            }
            else
            {
                // len is not 0, we need to read it
                var strBytes = data.AsSpan(pos + 4, len);
                var str = Encoding.UTF8.GetString(strBytes);
                strings.Add(str);
                pos += len + 4 + 1; // skip length prefix and string itself, +1, because it's null-terminated
            }
        }

        if (strings.Count != aFileEntry.ElementCount)
        {
            throw new Exception("String count mismatch in 0A component");
        }

        return strings;
    }
}