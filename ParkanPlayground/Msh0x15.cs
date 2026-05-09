using System.Buffers.Binary;
using NResLib;

namespace ParkanPlayground;

/// <summary>
/// MSH-компонент 0x15: terrain-таблица треугольников
/// </summary>
public static class Msh0x15
{
    public static List<TerrainTriangle> ReadComponent(
        FileStream mshFs, NResArchive archive)
    {
        var entry = archive.Files.FirstOrDefault(x => x.FileType == "15 00 00 00");

        if (entry is null)
        {
            throw new Exception("Archive doesn't contain file (15)");
        }

        if (entry.ElementSize != 28)
        {
            throw new Exception("Terrain triangle component (0x15) element size is not 28");
        }

        if (entry.FileLength % entry.ElementSize != 0)
        {
            throw new Exception("Terrain triangle component (0x15) payload size is not divisible by element size");
        }

        var data = new byte[entry.FileLength];
        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var elementBytes = data.Chunk(28);

        var elements = elementBytes.Select(x => new TerrainTriangle(
            BinaryPrimitives.ReadUInt32LittleEndian(x.AsSpan(0)),
            BinaryPrimitives.ReadUInt32LittleEndian(x.AsSpan(4)),
            BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(8)),
            BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(10)),
            BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(12)),
            BinaryPrimitives.ReadUInt32LittleEndian(x.AsSpan(14)),
            BinaryPrimitives.ReadUInt32LittleEndian(x.AsSpan(18)),
            BinaryPrimitives.ReadUInt32LittleEndian(x.AsSpan(22)),
            BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(26)))).ToList();

        return elements;
    }

    /// <summary>Terrain-треугольник 0x15 (length = 0x1C)</summary>
    /// <param name="Flags">[0x00..0x04] Флаги треугольника для terrain path</param>
    /// <param name="MaterialData">[0x04..0x08] Данные материала terrain</param>
    /// <param name="Vertex1Index">[0x08..0x0A] Индекс первой вершины в position stream Msh0x03</param>
    /// <param name="Vertex2Index">[0x0A..0x0C] Индекс второй вершины в position stream Msh0x03</param>
    /// <param name="Vertex3Index">[0x0C..0x0E] Индекс третьей вершины в position stream Msh0x03</param>
    /// <param name="Opaque0E">[0x0E..0x12] Opaque поле</param>
    /// <param name="Opaque12">[0x12..0x16] Opaque поле</param>
    /// <param name="Opaque16">[0x16..0x1A] Opaque поле</param>
    /// <param name="Opaque1A">[0x1A..0x1C] Opaque поле</param>
    public readonly record struct TerrainTriangle(
        uint Flags,

        uint MaterialData,
        ushort Vertex1Index,
        ushort Vertex2Index,
        ushort Vertex3Index,
        
        uint Opaque0E,
        uint Opaque12,
        uint Opaque16,
        ushort Opaque1A);
}
