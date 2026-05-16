using System.Buffers.Binary;
using NResLib;

namespace ParkanPlayground;

/// <summary>
/// MSH-компонент 0x06: индексный буфер.
/// Используется batch-ами 0x0D через Batch.IndexStart / Batch.IndexCount.
/// Индексы являются ushort и обычно читаются тройками как triangle indices.
/// </summary>
public static class Msh0x06
{
    public const int ElementSize = 2;

    public static List<ushort> ReadComponent(FileStream mshFs, NResArchive archive)
    {
        var entry = archive.Files.FirstOrDefault(x => x.FileType == "06 00 00 00");

        if (entry is null)
        {
            throw new Exception("Archive doesn't contain index buffer component (0x06)");
        }

        if (entry.ElementSize != ElementSize)
        {
            throw new Exception("Index buffer component (0x06) element size is not 2");
        }

        if (entry.FileLength % ElementSize != 0)
        {
            throw new Exception("Index buffer component (0x06) payload size is not divisible by 2");
        }

        var data = new byte[entry.FileLength];

        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var span = data.AsSpan();
        var indices = new List<ushort>(entry.FileLength / ElementSize);

        for (var offset = 0; offset < span.Length; offset += ElementSize)
        {
            indices.Add(BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(offset, ElementSize)));
        }

        return indices;
    }

    /// <summary>
    /// Возвращает triangle triplets из диапазона индексов.
    /// Удобно для обхода batch.IndexStart / batch.IndexCount из 0x0D.
    /// </summary>
    public static IEnumerable<TriangleIndices> EnumerateTriangles(
        IReadOnlyList<ushort> indices,
        int indexStart,
        int indexCount
    )
    {
        if (indexStart < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(indexStart));
        }

        if (indexCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(indexCount));
        }

        if (indexStart + indexCount > indices.Count)
        {
            throw new ArgumentException("Index range is outside the 0x06 index buffer");
        }

        for (var i = 0; i + 2 < indexCount; i += 3)
        {
            yield return new TriangleIndices(
                indices[indexStart + i + 0],
                indices[indexStart + i + 1],
                indices[indexStart + i + 2]);
        }
    }

    /// <summary>Тройка индексов triangle из MSH 0x06.</summary>
    /// <param name="Vertex0">Первый vertex index внутри batch/base vertex range.</param>
    /// <param name="Vertex1">Второй vertex index внутри batch/base vertex range.</param>
    /// <param name="Vertex2">Третий vertex index внутри batch/base vertex range.</param>
    public readonly record struct TriangleIndices(
        ushort Vertex0,
        ushort Vertex1,
        ushort Vertex2
    );
}
