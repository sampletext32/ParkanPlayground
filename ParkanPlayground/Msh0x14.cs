using System.Buffers.Binary;
using System.Numerics;
using NResLib;

namespace ParkanPlayground;

/// <summary>
/// MSH-компонент 0x14: таблица локальных directional/probe light entries.
/// Используется CAniMesh_IJointMesh::SampleMsh14Lights / AccumulateMsh14LightContributions.
/// </summary>
public static class Msh0x14
{
    public const int ElementSize = 48;

    public static List<LightProbe> ReadComponent(
        FileStream mshFs, NResArchive archive)
    {
        var entry = archive.Files.FirstOrDefault(x => x.FileType == "14 00 00 00");

        if (entry is null)
        {
            throw new Exception("Archive doesn't contain file (14)");
        }

        if (entry.ElementSize != ElementSize)
        {
            throw new Exception("Light probe component (0x14) element size is not 48");
        }

        if (entry.FileLength % entry.ElementSize != 0)
        {
            throw new Exception("Light probe component (0x14) payload size is not divisible by element size");
        }

        var data = new byte[entry.FileLength];
        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var elementBytes = data.Chunk(ElementSize);

        var elements = elementBytes.Select(x => new LightProbe(
            BinaryPrimitives.ReadInt32LittleEndian(x.AsSpan(0x00)),
            new Vector3(
                BinaryPrimitives.ReadInt32LittleEndian(x.AsSpan(0x04)),
                BinaryPrimitives.ReadInt32LittleEndian(x.AsSpan(0x08)),
                BinaryPrimitives.ReadInt32LittleEndian(x.AsSpan(0x0C))),
            new Vector3(
                BinaryPrimitives.ReadInt32LittleEndian(x.AsSpan(0x10)),
                BinaryPrimitives.ReadInt32LittleEndian(x.AsSpan(0x14)),
                BinaryPrimitives.ReadInt32LittleEndian(x.AsSpan(0x18))),
            new Vector4(
                BinaryPrimitives.ReadInt32LittleEndian(x.AsSpan(0x1C)),
                BinaryPrimitives.ReadInt32LittleEndian(x.AsSpan(0x20)),
                BinaryPrimitives.ReadInt32LittleEndian(x.AsSpan(0x24)),
                BinaryPrimitives.ReadInt32LittleEndian(x.AsSpan(0x28))),
            BinaryPrimitives.ReadInt32LittleEndian(x.AsSpan(0x2C)))).ToList();

        return elements;
    }

    /// <summary>Light/probe entry 0x14.</summary>
    /// <param name="PieceIndex">[0x00..0x04] Индекс MSH_piece, чью world matrix используют для transform position/direction.</param>
    /// <param name="LocalPosition">[0x04..0x10] Локальная позиция источника/probe относительно PieceIndex.</param>
    /// <param name="LocalDirection">[0x10..0x1C] Локальное направление, transform direction через matrix piece.</param>
    /// <param name="Color">[0x1C..0x2C] RGBA/intensity color multiplier. В коде умножается на вычисленный strength.</param>
    /// <param name="Intensity">[0x2C..0x30] Scalar intensity. В коде участвует как * Intensity * 3.0.</param>
    public readonly record struct LightProbe(
        int PieceIndex,
        Vector3 LocalPosition,
        Vector3 LocalDirection,
        Vector4 Color,
        int Intensity);
}