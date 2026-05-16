using System.Buffers.Binary;
using Common;
using NResLib;

namespace ParkanPlayground;

/// <summary>
/// MSH-компонент 0x07: triangle descriptors.
/// Используется geometry walker-ами для raycast / point-inside / фильтрации triangle flags.
/// Геометрические vertex indices лежат не здесь, а в MSH 0x06.
/// </summary>
public static class Msh0x07
{
    public const int ElementSize = 0x10;
    public const float PackedNormalScale = 1.0f / 32767.0f;

    public static List<TriangleDescriptor> ReadComponent(FileStream mshFs, NResArchive archive)
    {
        var entry = archive.Files.FirstOrDefault(x => x.FileType == "07 00 00 00");

        if (entry is null)
        {
            throw new Exception("Archive doesn't contain triangle descriptor component (0x07)");
        }

        if (entry.ElementSize != ElementSize)
        {
            throw new Exception("Triangle descriptor component (0x07) element size is not 16");
        }

        if (entry.FileLength % ElementSize != 0)
        {
            throw new Exception("Triangle descriptor component (0x07) payload size is not divisible by 16");
        }

        var data = new byte[entry.FileLength];

        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var span = data.AsSpan();
        var descriptors = new List<TriangleDescriptor>(entry.FileLength / ElementSize);

        for (var offset = 0; offset < span.Length; offset += ElementSize)
        {
            var element = span.Slice(offset, ElementSize);

            descriptors.Add(new TriangleDescriptor(
                Flags: (TriangleFlags)BinaryPrimitives.ReadUInt16LittleEndian(element.Slice(0x00, 2)),
                LinkedTriangleIndex0_0x07: BinaryPrimitives.ReadUInt16LittleEndian(element.Slice(0x02, 2)),
                LinkedTriangleIndex1_0x07: BinaryPrimitives.ReadUInt16LittleEndian(element.Slice(0x04, 2)),
                LinkedTriangleIndex2_0x07: BinaryPrimitives.ReadUInt16LittleEndian(element.Slice(0x06, 2)),
                PackedNormalX: BinaryPrimitives.ReadInt16LittleEndian(element.Slice(0x08, 2)),
                PackedNormalY: BinaryPrimitives.ReadInt16LittleEndian(element.Slice(0x0A, 2)),
                PackedNormalZ: BinaryPrimitives.ReadInt16LittleEndian(element.Slice(0x0C, 2)),
                PackedSelector: BinaryPrimitives.ReadUInt16LittleEndian(element.Slice(0x0E, 2))));
        }

        return descriptors;
    }

    /// <summary>Описатель triangle MSH 0x07, length = 0x10.</summary>
    /// <param name="Flags">[0x00..0x02] Triangle flags. Используются GeometryWalkFilter require/exclude.</param>
    /// <param name="LinkedTriangleIndex0_0x07">[0x02..0x04] Указывает на треугольники в 0x07 (текущем) компоненте.</param>
    /// <param name="LinkedTriangleIndex1_0x07">[0x04..0x06] Указывает на треугольники в 0x07 (текущем) компоненте.</param>
    /// <param name="LinkedTriangleIndex2_0x07">[0x06..0x08] Указывает на треугольники в 0x07 (текущем) компоненте.</param>
    /// <param name="PackedNormalX">[0x08..0x0A] Packed normal X, int16, scale = 1 / 32767.</param>
    /// <param name="PackedNormalY">[0x0A..0x0C] Packed normal Y, int16, scale = 1 / 32767.</param>
    /// <param name="PackedNormalZ">[0x0C..0x0E] Packed normal Z, int16, scale = 1 / 32767.</param>
    /// <param name="PackedSelector">[0x0E..0x10] Packed selectors. 3 трактуется как 0xFFFF.</param>
    public readonly record struct TriangleDescriptor(
        TriangleFlags Flags,
        ushort LinkedTriangleIndex0_0x07,
        ushort LinkedTriangleIndex1_0x07,
        ushort LinkedTriangleIndex2_0x07,
        short PackedNormalX,
        short PackedNormalY,
        short PackedNormalZ,
        ushort PackedSelector)
    {
        public Vector3 Normal => new(
            PackedNormalX * PackedNormalScale,
            PackedNormalY * PackedNormalScale,
            PackedNormalZ * PackedNormalScale);

        public ushort GetSelector(int index)
        {
            if (index is < 0 or > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var selector = (PackedSelector >> (index * 2)) & 0b11;

            return selector == 3
                ? ushort.MaxValue
                : (ushort)selector;
        }

        public bool MatchesFilter(TriangleFlags required, TriangleFlags excluded)
        {
            return (Flags & required) == required
                && (Flags & excluded) == 0;
        }
    }
}

[Flags]
public enum TriangleFlags : ushort
{
    None = 0,

    // Пока не называем отдельные bits, потому что мы видели только require/exclude фильтрацию.
    // Добавляй конкретные имена по мере нахождения usage sites.
}