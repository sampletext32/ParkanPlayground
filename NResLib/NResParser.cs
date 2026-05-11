using System.Buffers.Binary;
using System.Text;

namespace NResLib;

public static class NResParser
{
    public static NResParseResult ReadFile(string path)
    {
        using FileStream nResFs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        return ReadFile(nResFs);
    }
    
    public static NResParseResult ReadFile(Stream nResFs)
    {
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

        if (header.Version != 0x100)
        {
            return new NResParseResult(null, $"Неожиданная версия NRes: 0x{header.Version:X}");
        }

        if (header.FileCount < 0)
        {
            return new NResParseResult(null, $"Некорректное количество записей NRes: {header.FileCount}");
        }

        if (header.TotalFileLengthBytes != nResFs.Length)
        {
            return new NResParseResult(
                null,
                $"Длина файла не совпадает с заявленным в заголовке.\n" +
                $"Заявлено: {header.TotalFileLengthBytes}\n" +
                $"Фактически: {nResFs.Length}"
            );
        }

        var directorySize = header.FileCount * 64L;
        var directoryOffset = header.TotalFileLengthBytes - directorySize;
        if (directoryOffset < 16 || directoryOffset + directorySize != header.TotalFileLengthBytes)
        {
            return new NResParseResult(null, "Некорректное расположение каталога NRes");
        }

        nResFs.Seek(directoryOffset, SeekOrigin.Begin);

        var elements = new List<ListMetadataItem>(header.FileCount);

        Span<byte> metaDataBuffer = stackalloc byte[64];
        for (int i = 0; i < header.FileCount; i++)
        {
            nResFs.ReadExactly(metaDataBuffer);
            var typeId = BinaryPrimitives.ReadUInt32LittleEndian(metaDataBuffer[0..4]);
            var type = FormatType(typeId, metaDataBuffer[0..4]);
            var attr1 = BinaryPrimitives.ReadUInt32LittleEndian(metaDataBuffer[4..8]);
            var attr2 = BinaryPrimitives.ReadUInt32LittleEndian(metaDataBuffer[8..12]);
            var fileLength = BinaryPrimitives.ReadInt32LittleEndian(metaDataBuffer[12..16]);
            var attr3 = BinaryPrimitives.ReadUInt32LittleEndian(metaDataBuffer[16..20]);
            var name = ReadNResName(metaDataBuffer[20..56]);
            var offset = BinaryPrimitives.ReadInt32LittleEndian(metaDataBuffer[56..60]);
            var sortIndex = BinaryPrimitives.ReadInt32LittleEndian(metaDataBuffer[60..64]);

            if (offset < 16 || fileLength < 0 || (long)offset + fileLength > directoryOffset)
            {
                return new NResParseResult(null, $"Запись '{name}' выходит за границы data region");
            }
            
            elements.Add(new ListMetadataItem(
                typeId,
                type,
                attr1,
                unchecked((int)attr2),
                fileLength,
                unchecked((int)attr3),
                name,
                offset,
                sortIndex,
                i));

            metaDataBuffer.Clear();
        }

        return new NResParseResult(
            new NResArchive(
                Header: header,
                Files: elements
            )
        );
    }

    private static string FormatType(uint typeId, ReadOnlySpan<byte> bytes)
    {
        bool formattable = true;
        foreach (var b in bytes)
        {
            if (!char.IsLetterOrDigit((char)b))
            {
                formattable = false;
                break;
            }
        }
        if (formattable)
        {
            return Encoding.ASCII.GetString(bytes);
        }

        return string.Join(" ", bytes.ToArray().Select(x => x.ToString("X2")));
    }

    private static string ReadNResName(ReadOnlySpan<byte> raw)
    {
        var nul = raw.IndexOf((byte)0);
        if (nul < 0)
        {
            return Encoding.ASCII.GetString(raw);
        }

        return Encoding.ASCII.GetString(raw[..nul]);
    }
}
