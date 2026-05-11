using System.Buffers.Binary;
using Common;
using NResLib;

namespace ParkanPlayground;

/// <summary>
/// MSH-компонент 0x02: общий bounds header и таблица geometry slots.
/// Header size = 0x8C, slot size = 0x44.
/// </summary>
public static class Msh0x02
{
    public const int HeaderSize = 0x8C;
    public const int SlotSize = 0x44;

    public static Msh0x02Component ReadComponent(FileStream mshFs, NResArchive archive)
    {
        var entry = archive.Files.FirstOrDefault(x => x.FileType == "02 00 00 00");

        if (entry is null)
        {
            throw new Exception("Archive doesn't contain geometry slot component (0x02)");
        }

        if (entry.FileLength < HeaderSize)
        {
            throw new Exception("Geometry slot component (0x02) is smaller than the 0x8C-byte header");
        }

        if ((entry.FileLength - HeaderSize) % SlotSize != 0)
        {
            throw new Exception("Geometry slot component (0x02) payload after header is not divisible by 0x44");
        }

        var data = new byte[entry.FileLength];

        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var span = data.AsSpan();

        var header = ReadHeader(span.Slice(0, HeaderSize));

        var slotBytes = span.Slice(HeaderSize);
        var slotCount = slotBytes.Length / SlotSize;
        var slots = new List<GeometrySlot>(slotCount);

        for (var i = 0; i < slotCount; i++)
        {
            var slot = slotBytes.Slice(i * SlotSize, SlotSize);

            slots.Add(new GeometrySlot(
                TriStart0x07: BinaryPrimitives.ReadUInt16LittleEndian(slot.Slice(0x00, 2)),
                TriCount0x07: BinaryPrimitives.ReadUInt16LittleEndian(slot.Slice(0x02, 2)),
                BatchStart0x0D: BinaryPrimitives.ReadUInt16LittleEndian(slot.Slice(0x04, 2)),
                BatchCount0x0D: BinaryPrimitives.ReadUInt16LittleEndian(slot.Slice(0x06, 2)),
                LocalMinimum: ReadVector3(slot, 0x08),
                LocalMaximum: ReadVector3(slot, 0x14),
                BoundingSphere: ReadSphere(slot, 0x20),
                BaseXyArea: BinaryPrimitives.ReadSingleLittleEndian(slot.Slice(0x30, 4)),
                BaseVolume: BinaryPrimitives.ReadSingleLittleEndian(slot.Slice(0x34, 4)),
                Opaque38: BinaryPrimitives.ReadUInt32LittleEndian(slot.Slice(0x38, 4)),
                Opaque3C: BinaryPrimitives.ReadUInt32LittleEndian(slot.Slice(0x3C, 4)),
                Opaque40: BinaryPrimitives.ReadUInt32LittleEndian(slot.Slice(0x40, 4))));
        }

        return new Msh0x02Component(header, slots);
    }

    private static Msh02Header ReadHeader(ReadOnlySpan<byte> header)
    {
        var bbox = new BoundingBox(
            BottomFrontLeft: ReadVector3(header, 0x00),
            BottomFrontRight: ReadVector3(header, 0x0C),
            BottomBackRight: ReadVector3(header, 0x18),
            BottomBackLeft: ReadVector3(header, 0x24),
            TopFrontLeft: ReadVector3(header, 0x30),
            TopFrontRight: ReadVector3(header, 0x3C),
            TopBackRight: ReadVector3(header, 0x48),
            TopBackLeft: ReadVector3(header, 0x54));

        return new Msh02Header(
            BoundingBox: bbox,
            BoundingSphere: ReadSphere(header, 0x60),
            Bottom: ReadVector3(header, 0x70),
            Top: ReadVector3(header, 0x7C),
            XyRadius: BinaryPrimitives.ReadSingleLittleEndian(header.Slice(0x88, 4)));
    }

    private static Vector3 ReadVector3(ReadOnlySpan<byte> data, int offset)
    {
        return new Vector3(
            BinaryPrimitives.ReadSingleLittleEndian(data.Slice(offset + 0x00, 4)),
            BinaryPrimitives.ReadSingleLittleEndian(data.Slice(offset + 0x04, 4)),
            BinaryPrimitives.ReadSingleLittleEndian(data.Slice(offset + 0x08, 4)));
    }

    private static Sphere ReadSphere(ReadOnlySpan<byte> data, int offset)
    {
        return new Sphere(
            BinaryPrimitives.ReadSingleLittleEndian(data.Slice(offset + 0x00, 4)),
            BinaryPrimitives.ReadSingleLittleEndian(data.Slice(offset + 0x04, 4)),
            BinaryPrimitives.ReadSingleLittleEndian(data.Slice(offset + 0x08, 4)),
            BinaryPrimitives.ReadSingleLittleEndian(data.Slice(offset + 0x0C, 4)));
    }

    /// <summary>Результат чтения MSH-компонента 0x02.</summary>
    /// <param name="Header">Header 0x02, length = 0x8C.</param>
    /// <param name="Slots">Geometry slots после header.</param>
    public sealed record Msh0x02Component(
        Msh02Header Header,
        List<GeometrySlot> Slots)
    {
        /// <summary>
        /// Backward-compatible alias, если старый код ещё использует Elements.
        /// </summary>
        public List<GeometrySlot> Elements => Slots;
    }

    /// <summary>Заголовок MSH 0x02, length = 0x8C.</summary>
    /// <param name="BoundingBox">[0x00..0x60] Bounding box из 8 точек.</param>
    /// <param name="BoundingSphere">[0x60..0x70] Bounding sphere: xyz = center, w = radius.</param>
    /// <param name="Bottom">[0x70..0x7C] Нижняя/опорная точка меша.</param>
    /// <param name="Top">[0x7C..0x88] Верхняя точка меша.</param>
    /// <param name="XyRadius">[0x88..0x8C] Радиус/extent в плоскости XY.</param>
    public sealed record Msh02Header(
        BoundingBox BoundingBox,
        Sphere BoundingSphere,
        Vector3 Bottom,
        Vector3 Top,
        float XyRadius);

    /// <summary>Geometry slot MSH 0x02, length = 0x44.</summary>
    /// <param name="TriStart0x07">[0x00..0x02] Первый triangle descriptor в MSH 0x07.</param>
    /// <param name="TriCount0x07">[0x02..0x04] Количество triangle descriptor / triangle range count.</param>
    /// <param name="BatchStart0x0D">[0x04..0x06] Первый batch в MSH 0x0D.</param>
    /// <param name="BatchCount0x0D">[0x06..0x08] Количество batch в MSH 0x0D.</param>
    /// <param name="LocalMinimum">[0x08..0x14] Local AABB minimum.</param>
    /// <param name="LocalMaximum">[0x14..0x20] Local AABB maximum.</param>
    /// <param name="BoundingSphere">[0x20..0x30] Local bounding sphere.</param>
    /// <param name="BaseXyArea">[0x30..0x34] Базовая XY-площадь / footprint area до mesh scale.</param>
    /// <param name="BaseVolume">[0x34..0x38] Базовый объём до mesh scale.</param>
    /// <param name="Opaque38">[0x38..0x3C] Opaque dword.</param>
    /// <param name="Opaque3C">[0x3C..0x40] Opaque dword.</param>
    /// <param name="Opaque40">[0x40..0x44] Opaque dword.</param>
    public readonly record struct GeometrySlot(
        ushort TriStart0x07,
        ushort TriCount0x07,
        ushort BatchStart0x0D,
        ushort BatchCount0x0D,
        Vector3 LocalMinimum,
        Vector3 LocalMaximum,
        Sphere BoundingSphere,
        float BaseXyArea,
        float BaseVolume,
        uint Opaque38,
        uint Opaque3C,
        uint Opaque40)
    {
        public int BatchEndExclusive0x0D => BatchStart0x0D + BatchCount0x0D;

        public int TriEndExclusive0x07 => TriStart0x07 + TriCount0x07;

        public bool HasBatches => BatchCount0x0D != 0;

        public bool HasTriangles => TriCount0x07 != 0;
    }

    /// <summary>Bounding box заголовка: 8 точек по 3 float, length = 0x60.</summary>
    /// <param name="BottomFrontLeft">[0x00..0x0C] Нижняя передняя левая точка.</param>
    /// <param name="BottomFrontRight">[0x0C..0x18] Нижняя передняя правая точка.</param>
    /// <param name="BottomBackRight">[0x18..0x24] Нижняя задняя правая точка.</param>
    /// <param name="BottomBackLeft">[0x24..0x30] Нижняя задняя левая точка.</param>
    /// <param name="TopFrontLeft">[0x30..0x3C] Верхняя передняя левая точка.</param>
    /// <param name="TopFrontRight">[0x3C..0x48] Верхняя передняя правая точка.</param>
    /// <param name="TopBackRight">[0x48..0x54] Верхняя задняя правая точка.</param>
    /// <param name="TopBackLeft">[0x54..0x60] Верхняя задняя левая точка.</param>
    public sealed record BoundingBox(
        Vector3 BottomFrontLeft,
        Vector3 BottomFrontRight,
        Vector3 BottomBackRight,
        Vector3 BottomBackLeft,
        Vector3 TopFrontLeft,
        Vector3 TopFrontRight,
        Vector3 TopBackRight,
        Vector3 TopBackLeft);
}