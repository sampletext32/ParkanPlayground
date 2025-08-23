using System.Buffers.Binary;
using System.Text;

namespace NResLib;

public static class NResParser
{
    public static NResParseResult ReadFile(string path)
    {
        using FileStream nResFs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        if (nResFs.Length < 16)
        {
            return new NResParseResult(null, "Файл не может быть NRes, менее 16 байт");
        }
        
        Span<byte> buffer = stackalloc byte[16];

        nResFs.ReadExactly(buffer);

        if (buffer[0] != 'N' || buffer[1] != 'R' || buffer[2] != 'e' || buffer[3] != 's')
        {
            return new NResParseResult(null, "Файл не начинается с NRes");
        }

        var header = new NResArchiveHeader(
            NRes: Encoding.ASCII.GetString(buffer[0..4]).TrimEnd('\0'),
            Version: BinaryPrimitives.ReadInt32LittleEndian(buffer[4..8]),
            FileCount: BinaryPrimitives.ReadInt32LittleEndian(buffer[8..12]),
            TotalFileLengthBytes: BinaryPrimitives.ReadInt32LittleEndian(buffer[12..16])
        );

        if (header.TotalFileLengthBytes != nResFs.Length)
        {
            return new NResParseResult(
                null,
                $"Длина файла не совпадает с заявленным в заголовке.\n" +
                $"Заявлено: {header.TotalFileLengthBytes}\n" +
                $"Фактически: {nResFs.Length}"
            );
        }

        nResFs.Seek(-header.FileCount * 64, SeekOrigin.End);

        var elements = new List<ListMetadataItem>(header.FileCount);

        Span<byte> metaDataBuffer = stackalloc byte[64];
        for (int i = 0; i < header.FileCount; i++)
        {
            nResFs.ReadExactly(metaDataBuffer);
            var type = "";

            for (int j = 0; j < 4; j++)
            {
                if (!char.IsLetterOrDigit((char)metaDataBuffer[j]))
                {
                    type += metaDataBuffer[j]
                        .ToString("X2") + " ";
                }
                else
                {
                    type += (char)metaDataBuffer[j];
                }
            }

            var type2 = BinaryPrimitives.ReadUInt32LittleEndian(metaDataBuffer.Slice(4));

            type = type.Trim();
            
            elements.Add(
                new ListMetadataItem(
                    FileType: type,
                    ElementCount: type2,
                    Magic1: BinaryPrimitives.ReadInt32LittleEndian(metaDataBuffer[8..12]),
                    FileLength: BinaryPrimitives.ReadInt32LittleEndian(metaDataBuffer[12..16]),
                    ElementSize: BinaryPrimitives.ReadInt32LittleEndian(metaDataBuffer[16..20]),
                    FileName: Encoding.ASCII.GetString(metaDataBuffer[20..40]).TrimEnd('\0'),
                    Magic3: BinaryPrimitives.ReadInt32LittleEndian(metaDataBuffer[40..44]),
                    Magic4: BinaryPrimitives.ReadInt32LittleEndian(metaDataBuffer[44..48]),
                    Magic5: BinaryPrimitives.ReadInt32LittleEndian(metaDataBuffer[48..52]),
                    Magic6: BinaryPrimitives.ReadInt32LittleEndian(metaDataBuffer[52..56]),
                    OffsetInFile: BinaryPrimitives.ReadInt32LittleEndian(metaDataBuffer[56..60]),
                    Index: BinaryPrimitives.ReadInt32LittleEndian(metaDataBuffer[60..64])
                )
            );

            metaDataBuffer.Clear();
        }

        return new NResParseResult(
            new NResArchive(
                Header: header,
                Files: elements
            )
        );
    }
}