using System.Buffers.Binary;
using NResLib;

namespace ParkanPlayground;

/// <summary>
/// MSH-компонент 0x0D: таблица batch.
/// </summary>
public static class Msh0x0D
{
    public const int ElementSize = 20;
    
    public static List<Batch> ReadComponent(
        FileStream mshFs, NResArchive archive)
    {
        var entry = archive.Files.FirstOrDefault(x => x.FileType == "0D 00 00 00");

        if (entry is null)
        {
            throw new Exception("Archive doesn't contain file (0D)");
        }

        if (entry.ElementSize != ElementSize)
        {
            throw new Exception("Batch table component (0x0D) element size is not 20");
        }

        if (entry.FileLength % entry.ElementSize != 0)
        {
            throw new Exception("Batch table component (0x0D) payload size is not divisible by element size");
        }

        var data = new byte[entry.FileLength];
        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var elementBytes = data.Chunk(ElementSize);

        var elements = elementBytes.Select(x => new Batch(
            BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0)),
            BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(2)),
            BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(4)),
            BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(6)),
            BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(8)),
            BinaryPrimitives.ReadUInt32LittleEndian(x.AsSpan(0xA)),
            BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0xE)),
            BinaryPrimitives.ReadUInt32LittleEndian(x.AsSpan(0x10)))).ToList();

        return elements;
    }

    /// <summary>Batch 0x0D</summary>
    /// <param name="BatchFlags">[0x00..0x02] Флаги batch. У FParkan: batchFlags.</param>
    /// <param name="MaterialIndex">[0x02..0x04] Индекс material slot, резолвится через WEAR/MAT0 pipeline.</param>
    /// <param name="Opaque4">[0x04..0x06] Opaque поле. Старое локальное имя: TriangleCount; не подтверждено.</param>
    /// <param name="Opaque6">[0x06..0x08] Opaque поле. Сохранять побайтно в writer.</param>
    /// <param name="IndexCount">[0x08..0x0A] Количество индексов в индексном буфере 0x06.</param>
    /// <param name="IndexStart">[0x0A..0x0E] Первый индекс в индексном буфере 0x06.</param>
    /// <param name="Opaque14">[0x0E..0x10] Opaque поле. Старое локальное имя: CountOf03; не подтверждено.</param>
    /// <param name="BaseVertex">[0x10..0x14] Базовая вершина в position stream 0x03. У FParkan: baseVertex.</param>
    public readonly record struct Batch(
        ushort BatchFlags,
        ushort MaterialIndex,
        ushort Opaque4,
        ushort Opaque6,
        ushort IndexCount,
        uint IndexStart,
        ushort Opaque14,
        uint BaseVertex);
}
