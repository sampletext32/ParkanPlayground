using System.Buffers.Binary;
using System.Text;
using NResLib;

namespace ParkanPlayground;

/// <summary>
/// MSH-компонент 0x0A: строки узлов.
/// У FParkan: Res10 / Node strings. Старое локальное имя: ExternalRefs.
/// </summary>
public class Msh0x0A
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
            if (pos + 4 > data.Length)
            {
                throw new Exception("Node strings component (0x0A) has truncated length prefix");
            }

            var len = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(pos));
            if (len < 0 || pos + 4 + len > data.Length)
            {
                throw new Exception("Node strings component (0x0A) has invalid string length");
            }

            if (len == 0)
            {
                pos += 4;
                strings.Add("");
            }
            else
            {
                var strBytes = data.AsSpan(pos + 4, len);
                var str = Encoding.ASCII.GetString(strBytes);
                strings.Add(str);
                pos += len + 4;
                if (pos < data.Length && data[pos] == 0)
                {
                    pos++;
                }
            }
        }

        if (strings.Count != aFileEntry.ElementCount)
        {
            throw new Exception("String count mismatch in 0A component");
        }

        return strings;
    }
}
