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

        if (entry.ElementSize != 0x1C)
        {
            throw new Exception("Terrain triangle component (0x15) element size is not 28");
        }

        if (entry.FileLength % entry.ElementSize != 0x0)
        {
            throw new Exception("Terrain triangle component (0x15) payload size is not divisible by element size");
        }

        var data = new byte[entry.FileLength];
        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0x0, data.Length);

        var elementBytes = data.Chunk(0x1C);

        var elements = elementBytes.Select(x => new TerrainTriangle(
            Flags: (TerrainTriangleFlags)BinaryPrimitives.ReadUInt32LittleEndian(x.AsSpan(0x0)),
            MaterialData: BinaryPrimitives.ReadUInt32LittleEndian(x.AsSpan(0x4)),
            Vertex1Index: BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0x8)),
            Vertex2Index: BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0xA)),
            Vertex3Index: BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0xC)),
            Neighbor0: BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0x0E)),
            Neighbor1: BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0x10)),
            Neighbor2: BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0x12)),
            NormalX: BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0x14)),
            NormalY: BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0x16)),
            NormalZ: BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0x18)),
            PackedEdgeOrSelector: BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0x1A))))
            .ToList();

        return elements;
    }

    /// <summary>Terrain-треугольник 0x15 (length = 0x1C)</summary>
    /// <param name="Flags">[0x00..0x04] Флаги треугольника для terrain path</param>
    /// <param name="MaterialData">[0x04..0x08] Данные материала terrain</param>
    /// <param name="Vertex1Index">[0x08..0x0A] Индекс первой вершины в position stream Msh0x03</param>
    /// <param name="Vertex2Index">[0x0A..0x0C] Индекс второй вершины в position stream Msh0x03</param>
    /// <param name="Vertex3Index">[0x0C..0x0E] Индекс третьей вершины в position stream Msh0x03</param>
    /// <param name="Neighbor0">[0x0E..0x10] Сосед 0</param>
    /// <param name="Neighbor1">[0x10..0x12] Сосед 1</param>
    /// <param name="Neighbor2">[0x12..0x14] Сосед 2</param>
    /// <param name="NormalX">[0x14..0x16] Направление нормали</param>
    /// <param name="NormalY">[0x16..0x18] Направление нормали</param>
    /// <param name="NormalZ">[0x18..0x1A] Направление нормали</param>
    /// <param name="PackedEdgeOrSelector">[0x1A..0x1C] TODO</param>
    public readonly record struct TerrainTriangle(
        TerrainTriangleFlags Flags,

        uint MaterialData,
        ushort Vertex1Index,
        ushort Vertex2Index,
        ushort Vertex3Index,
        
        ushort Neighbor0,
        ushort Neighbor1,
        ushort Neighbor2,
        ushort NormalX,
        ushort NormalY,
        ushort NormalZ,
        ushort PackedEdgeOrSelector);
}

public enum TerrainTriangleFlags : uint
{
    MSH15_FLAG_REFLECTIVE_SURFACE = 0x20000,
    MSH15_FLAG_HAS_MICROTEXTURE = 0x400,
    MSH15_FLAG_DISABLE_BACKFACE_TEST = 0x8
}