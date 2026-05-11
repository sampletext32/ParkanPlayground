using System.Buffers.Binary;
using NResLib;

namespace ParkanPlayground;

/// <summary>
/// MSH-компонент 0x01: таблица узлов модели.
/// Для обычного AniMesh node имеет size 0x26.
/// У ландшафта metadata/magic1 может иметь другой смысл, например grid_x_count.
/// </summary>
public static class Msh0x01
{
    public const int NormalElementSize = 0x26;
    public const int LodCount = 3;
    public const int GroupCount = 5;
    public const int SlotCount = LodCount * GroupCount;

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

        if (entry.ElementSize < 8)
        {
            throw new Exception("Node table component (0x01) element size is too small");
        }

        var elementCount = entry.FileLength / entry.ElementSize;
        var data = new byte[entry.FileLength];

        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var dataSpan = data.AsSpan();

        var nodes = new List<Node>(elementCount);

        for (var i = 0; i < elementCount; i++)
        {
            var baseOffset = i * entry.ElementSize;
            var elementSpan = dataSpan.Slice(baseOffset, entry.ElementSize);

            var rawBytes = elementSpan.ToArray();

            var slotIndices = new ushort[SlotCount];
            Array.Fill(slotIndices, ushort.MaxValue);

            var slotWordCount = Math.Min(
                slotIndices.Length,
                Math.Max(0, (entry.ElementSize - 8) / 2));

            for (var slotIndex = 0; slotIndex < slotWordCount; slotIndex++)
            {
                slotIndices[slotIndex] =
                    BinaryPrimitives.ReadUInt16LittleEndian(elementSpan.Slice(0x08 + slotIndex * 2, 2));
            }

            nodes.Add(new Node(
                RawBytes: rawBytes,
                Flags: (NodeFlags)BinaryPrimitives.ReadUInt16LittleEndian(elementSpan.Slice(0x00, 2)),
                ParentIndexOrLink: BinaryPrimitives.ReadUInt16LittleEndian(elementSpan.Slice(0x02, 2)),
                AnimMapStart0x13: BinaryPrimitives.ReadUInt16LittleEndian(elementSpan.Slice(0x04, 2)),
                FallbackKey0x08: BinaryPrimitives.ReadUInt16LittleEndian(elementSpan.Slice(0x06, 2)),
                Msh02SlotIndicesByLodAndGroup: slotIndices));
        }

        return new Msh0x01Component(entry.ElementSize, nodes);
    }

    /// <summary>Результат чтения MSH-компонента 0x01.</summary>
    /// <param name="ElementSize">Размер node entry из NRes metadata.</param>
    /// <param name="Nodes">Узлы компонента 0x01.</param>
    public sealed record Msh0x01Component(
        int ElementSize,
        List<Node> Nodes)
    {
        public bool IsNormalAniMeshNodeTable => ElementSize == NormalElementSize;
    }

    /// <summary>Узел MSH 0x01.</summary>
    /// <param name="RawBytes">Сырые байты узла. Нужны для copy-through нестандартных вариантов.</param>
    /// <param name="Flags">[0x00..0x02] Флаги узла.</param>
    /// <param name="ParentIndexOrLink">[0x02..0x04] Parent node index. 0xFFFF обычно значит root/no parent.</param>
    /// <param name="AnimMapStart0x13">[0x04..0x06] Начало блока в MSH 0x13 animation map или 0xFFFF.</param>
    /// <param name="FallbackKey0x08">[0x06..0x08] Fallback key / index в MSH 0x08.</param>
    /// <param name="Msh02SlotIndicesByLodAndGroup">
    /// [0x08..0x26] Индексы geometry slot в MSH 0x02.
    /// Формула: slot = lod * 5 + group. 0xFFFF значит отсутствует.
    /// </param>
    public sealed record Node(
        byte[] RawBytes,
        NodeFlags Flags,
        ushort ParentIndexOrLink,
        ushort AnimMapStart0x13,
        ushort FallbackKey0x08,
        ushort[] Msh02SlotIndicesByLodAndGroup)
    {
        public bool IsRoot => ParentIndexOrLink == ushort.MaxValue;

        public int ParentIndexOrMinusOne =>
            ParentIndexOrLink == ushort.MaxValue ? -1 : ParentIndexOrLink;

        public ushort ResolveSlotIndex(int lod, int group = 0)
        {
            var index = lod * GroupCount + group;

            return index >= 0 && index < Msh02SlotIndicesByLodAndGroup.Length
                ? Msh02SlotIndicesByLodAndGroup[index]
                : ushort.MaxValue;
        }

        public bool HasGeometrySlot(int lod, int group = 0) =>
            ResolveSlotIndex(lod, group) != ushort.MaxValue;

        public int CountLodsForGroup(int group = 0)
        {
            var count = 0;

            for (var lod = 0; lod < LodCount; lod++)
            {
                if (!HasGeometrySlot(lod, group))
                {
                    break;
                }

                count++;
            }

            return count;
        }
    }
}

[Flags]
public enum NodeFlags : ushort
{
    None = 0,

    /// <summary>
    /// Still uncertain. In recursive bounds/intersection paths this can suppress/alter child recursion.
    /// Seen as child_node.flags &amp; 0x04.
    /// </summary>
    UnknownSkipChild0x04 = 0x0004,

    /// <summary>
    /// Stops recursive traversal into children for bounds/render/intersection helpers.
    /// </summary>
    StopChildTraversal = 0x0010,

    /// <summary>
    /// Special render-group-4 mode bit. In render group 4, selects alternate piece render mode.
    /// Earlier also seen in special render/clip behavior.
    /// </summary>
    Group4AltRenderMode = 0x0020,

    /// <summary>
    /// Exclude from shadow / no shadow. CAniMesh tracks has_any_shadow_casting_piece when this bit is absent.
    /// </summary>
    NoShadow = 0x0040,

    /// <summary>
    /// Used during attached MSH load: if parent/root description contains "central", piece gets hidden/excluded flag.
    /// Exact semantic name still provisional.
    /// </summary>
    CheckParentDescriptionCentral = 0x0800,
}