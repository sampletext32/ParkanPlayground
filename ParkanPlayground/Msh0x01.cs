using System.Buffers.Binary;
using NResLib;

namespace ParkanPlayground;

/// <summary>
/// MSH-компонент 0x01: таблица узлов модели.
/// </summary>
/// У ландшафта magic1 - grid_x_count
public static class Msh0x01
{
    public static Msh0x01Component ReadComponent(FileStream mshFs, NResArchive archive)
    {
        var entry = archive.Files.FirstOrDefault(x => x.FileType == "01 00 00 00");

        if (entry is null)
        {
            throw new Exception("Archive doesn't contain node table component (0x01)");
        }

        if (entry.ElementSize <= 0)
        {
            throw new Exception("Node table component (0x01) has invalid element size");
        }

        if (entry.FileLength % entry.ElementSize != 0)
        {
            throw new Exception("Node table component (0x01) payload size is not divisible by element size");
        }

        var elementCount = entry.FileLength / entry.ElementSize;
        var data = new byte[entry.FileLength];
        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var dataSpan = data.AsSpan();

        var elements = new List<Node>(elementCount);
        for (var i = 0; i < elementCount; i++)
        {
            var baseOffset = i * entry.ElementSize;
            var rawBytes = dataSpan.Slice(baseOffset, entry.ElementSize).ToArray();
            var slots = new ushort[15];
            Array.Fill(slots, ushort.MaxValue);
            var slotWords = Math.Min(slots.Length, Math.Max(0, (entry.ElementSize - 8) / 2));
            for (var slotIndex = 0; slotIndex < slotWords; slotIndex++)
            {
                slots[slotIndex] =
                    BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(baseOffset + 8 + slotIndex * 2));
            }

            elements.Add(new Node(
                rawBytes,
                (NodeFlags)BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(baseOffset)),
                BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(baseOffset + 2)),
                BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(baseOffset + 4)),
                BinaryPrimitives.ReadUInt16LittleEndian(dataSpan.Slice(baseOffset + 6)),
                slots));
        }

        return new Msh0x01Component(elements);
    }


    /// <summary>Результат чтения MSH-компонента 0x01.</summary>
    /// <param name="Elements">Узлы компонента 0x01.</param>
    public record Msh0x01Component(List<Node> Elements);

    /// <summary>Узел 0x01.</summary>
    /// <param name="RawBytes">Сырые байты узла (length = attr3). Нужны для copy-through редких вариантов, например attr3 = 24.</param>
    /// <param name="Flags">[0x00..0x02] Флаги узла</param>
    /// <param name="ParentOrLink_possibly_0x02_index">[0x02..0x04] Индекс родителя или связанного узла</param>
    /// <param name="AnimMapStart">[0x04..0x06] Начало блока в карте анимации Msh0x13 или 0xFFFF</param>
    /// <param name="FallbackKey">[0x06..0x08] Индекс fallback-ключа в пуле ключей Msh0x08</param>
    /// <param name="Msh02SlotIndicesByLodAndGroupSlotIndex">[0x08..0x26] Индексы slot в Msh0x02 по формуле lod * 5 + group</param>
    public record Node(
        byte[] RawBytes,
        NodeFlags Flags,
        ushort ParentOrLink_possibly_0x02_index,
        ushort AnimMapStart,
        ushort FallbackKey,
        ushort[] Msh02SlotIndicesByLodAndGroupSlotIndex)
    {
        public ushort ResolveSlotIndex(int lod, int group = 0)
        {
            var index = lod * 5 + group;
            return index >= 0 && index < Msh02SlotIndicesByLodAndGroupSlotIndex.Length ? Msh02SlotIndicesByLodAndGroupSlotIndex[index] : ushort.MaxValue;
        }
    }
}

public enum NodeFlags : ushort
{
    // very uncertain
    MSH01_NODE_FLAG_UNKNOWN_SKIP_RECURSE_0x04 = 0x4,
    MSH01_NODE_FLAG_STOP_CHILD_TRAVERSAL = 0x10,
    MSH01_NODE_FLAG_NO_SHADOW  = 0x40
}