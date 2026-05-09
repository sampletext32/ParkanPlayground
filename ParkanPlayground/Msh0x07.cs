using System.Buffers.Binary;
using NResLib;

namespace ParkanPlayground;

/// <summary>
/// MSH-компонент 0x07: описатели треугольников для коллизии/пикинга
/// </summary>
public static class Msh0x07
{
    public static List<TriangleDescriptor> ReadComponent(
        FileStream mshFs, NResArchive archive)
    {
        var entry = archive.Files.FirstOrDefault(x => x.FileType == "07 00 00 00");

        if (entry is null)
        {
            throw new Exception("Archive doesn't contain file (07)");
        }

        if (entry.ElementSize != 16)
        {
            throw new Exception("Triangle descriptor component (0x07) element size is not 16");
        }

        if (entry.FileLength % entry.ElementSize != 0)
        {
            throw new Exception("Triangle descriptor component (0x07) payload size is not divisible by element size");
        }

        var data = new byte[entry.FileLength];
        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var elementBytes = data.Chunk(16);

        var elements = elementBytes.Select(x => new TriangleDescriptor(
            BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0)),
            BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(2)),
            BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(4)),
            BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(6)),
            BinaryPrimitives.ReadInt16LittleEndian(x.AsSpan(8)),
            BinaryPrimitives.ReadInt16LittleEndian(x.AsSpan(10)),
            BinaryPrimitives.ReadInt16LittleEndian(x.AsSpan(12)),
            BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(14)))).ToList();

        return elements;
    }

    /// <summary>Описатель треугольника 0x07 (length = 0x10).</summary>
    /// <param name="TriFlags">[0x00..0x02] Флаги треугольника</param>
    /// <param name="Link0">[0x02..0x04] Связь/opaque поле 0</param>
    /// <param name="Link1">[0x04..0x06] Связь/opaque поле 1</param>
    /// <param name="Link2">[0x06..0x08] Связь/opaque поле 2</param>
    /// <param name="NormalX">[0x08..0x0A] Упакованная X-компонента нормали</param>
    /// <param name="NormalY">[0x0A..0x0C] Упакованная Y-компонента нормали</param>
    /// <param name="NormalZ">[0x0C..0x0E] Упакованная Z-компонента нормали</param>
    /// <param name="SelectorPacked">[0x0E..0x10] Три 2-битных селектора; значение 3 трактуется как 0xFFFF</param>
    public readonly record struct TriangleDescriptor(
        ushort TriFlags,
        ushort Link0,
        ushort Link1,
        ushort Link2,
        short NormalX,
        short NormalY,
        short NormalZ,
        ushort SelectorPacked)
    {
        public ushort GetSelector(int index)
        {
            if (index is < 0 or > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var selector = (SelectorPacked >> (index * 2)) & 0b11;
            return selector == 3 ? ushort.MaxValue : (ushort)selector;
        }
    }
}
