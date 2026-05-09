using System.Buffers.Binary;
using Common;
using NResLib;

namespace ParkanPlayground;

/// <summary>
/// MSH-компонент 0x02: общий заголовок и таблица slot.
/// </summary>
public static class Msh0x02
{
    public static Msh0x02Component ReadComponent(FileStream mshFs, NResArchive archive)
    {
        var fileEntry = archive.Files.FirstOrDefault(x => x.FileType == "02 00 00 00");

        if (fileEntry is null)
        {
            throw new Exception("Archive doesn't contain slots component (0x02)");
        }

        if (fileEntry.FileLength < 0x8c)
        {
            throw new Exception("Slots component (0x02) is smaller than the 0x8C-byte header");
        }

        if ((fileEntry.FileLength - 0x8c) % 68 != 0)
        {
            throw new Exception("Slots component (0x02) payload after header is not divisible by 68");
        }

        var data = new byte[fileEntry.FileLength];
        mshFs.Seek(fileEntry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        var header = data.AsSpan(0, 0x8c); // заголовок (length = 0x8C)

        var center = new Vector3(
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(0x60)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(0x64)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(0x68))
        );
        var centerW = BinaryPrimitives.ReadSingleLittleEndian(header.Slice(0x6c));

        var bb = new BoundingBox(
            new Vector3(
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(0)),
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(4)),
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(8))
            ),
            new Vector3(
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(12)),
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(16)),
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(20))
            ),
            new Vector3(
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(24)),
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(28)),
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(32))
            ),
            new Vector3(
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(36)),
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(40)),
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(44))
            ),
            new Vector3(
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(48)),
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(52)),
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(56))
            ),
            new Vector3(
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(60)),
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(64)),
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(68))
            ),
            new Vector3(
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(72)),
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(76)),
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(80))
            ),
            new Vector3(
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(84)),
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(88)),
                BinaryPrimitives.ReadSingleLittleEndian(header.Slice(92))
            ));

        var bottom = new Vector3(
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(112)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(116)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(120))
        );

        var top = new Vector3(
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(124)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(128)),
            BinaryPrimitives.ReadSingleLittleEndian(header.Slice(132))
        );

        var xyRadius = BinaryPrimitives.ReadSingleLittleEndian(header.Slice(136));


        var elements = new List<Slot>();
        var skippedHeader = data.AsSpan(0x8c);
        var slotCount = skippedHeader.Length / 68;
        for (var i = 0; i < slotCount; i++)
        {
            var baseOffset = 68 * i;
            var opaque = new uint[5];
            for (var opaqueIndex = 0; opaqueIndex < opaque.Length; opaqueIndex++)
            {
                opaque[opaqueIndex] =
                    BinaryPrimitives.ReadUInt32LittleEndian(skippedHeader.Slice(baseOffset + 48 + opaqueIndex * 4));
            }

            elements.Add(new Slot(
                BinaryPrimitives.ReadUInt16LittleEndian(skippedHeader.Slice(baseOffset + 0)),
                BinaryPrimitives.ReadUInt16LittleEndian(skippedHeader.Slice(baseOffset + 2)),
                BinaryPrimitives.ReadUInt16LittleEndian(skippedHeader.Slice(baseOffset + 4)),
                BinaryPrimitives.ReadUInt16LittleEndian(skippedHeader.Slice(baseOffset + 6)),
                new Vector3(
                    BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(baseOffset + 8)),
                    BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(baseOffset + 12)),
                    BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(baseOffset + 16))
                ),
                new Vector3(
                    BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(baseOffset + 20)),
                    BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(baseOffset + 24)),
                    BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(baseOffset + 28))
                ),
                new Vector3(
                    BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(baseOffset + 32)),
                    BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(baseOffset + 36)),
                    BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(baseOffset + 40))
                ),
                BinaryPrimitives.ReadSingleLittleEndian(skippedHeader.Slice(baseOffset + 44)),
                opaque));

        }

        return new Msh0x02Component(
            new Msh02Header(bb, center, centerW, bottom, top, xyRadius),
            elements);
    }

    /// <summary>Результат чтения MSH-компонента 0x02.</summary>
    /// <param name="Header">Заголовок 0x02 (length = 0x8C).</param>
    /// <param name="Elements">Slot records после заголовка.</param>
    public record class Msh0x02Component(Msh02Header Header, List<Slot> Elements);

    /// <summary>Заголовок 0x02 (length = 0x8C).</summary>
    /// <param name="BoundingBox">[0x00..0x60] Bounding box из 8 точек.</param>
    /// <param name="Center">[0x60..0x6C] Центральная точка.</param>
    /// <param name="CenterW">[0x6C..0x70] W-компонента центра.</param>
    /// <param name="Bottom">[0x70..0x7C] Нижняя точка.</param>
    /// <param name="Top">[0x7C..0x88] Верхняя точка.</param>
    /// <param name="XYRadius">[0x88..0x8C] Радиус в плоскости XY.</param>
    public record class Msh02Header(
        BoundingBox BoundingBox,
        Vector3 Center,
        float CenterW,
        Vector3 Bottom,
        Vector3 Top,
        float XYRadius);

    /// <summary>Slot 0x02 (length = 0x44).</summary>
    /// <param name="TriStart">[0x00..0x02] Первый triangle descriptor в Msh0x07. Для terrain-гипотезы может быть диапазоном Msh0x15.</param>
    /// <param name="TriCount">[0x02..0x04] Количество triangle descriptor в Msh0x07. Для terrain-гипотезы может быть count для Msh0x15.</param>
    /// <param name="BatchStart">[0x04..0x06] Первый batch в таблице 0x0D.</param>
    /// <param name="BatchCount">[0x06..0x08] Количество batch в таблице 0x0D</param>
    /// <param name="LocalMinimum">[0x08..0x14] Минимум локального AABB</param>
    /// <param name="LocalMaximum">[0x14..0x20] Максимум локального AABB</param>
    /// <param name="Center">[0x20..0x2C] Центр bounding sphere</param>
    /// <param name="SphereRadius">[0x2C..0x30] Радиус bounding sphere</param>
    /// <param name="Opaque">[0x30..0x44] Пять opaque dword</param>
    public record class Slot(
        ushort TriStart,
        ushort TriCount,
        ushort BatchStart,
        ushort BatchCount,
        Vector3 LocalMinimum,
        Vector3 LocalMaximum,
        Vector3 Center,
        float SphereRadius,
        uint[] Opaque);

    /// <summary>Bounding box заголовка: 8 точек по 3 float (length = 0x60).</summary>
    /// <param name="BottomFrontLeft">[0x00..0x0C] Нижняя передняя левая точка.</param>
    /// <param name="BottomFrontRight">[0x0C..0x18] Нижняя передняя правая точка.</param>
    /// <param name="BottomBackRight">[0x18..0x24] Нижняя задняя правая точка.</param>
    /// <param name="BottomBackLeft">[0x24..0x30] Нижняя задняя левая точка.</param>
    /// <param name="TopFrontLeft">[0x30..0x3C] Верхняя передняя левая точка.</param>
    /// <param name="TopFrontRight">[0x3C..0x48] Верхняя передняя правая точка.</param>
    /// <param name="TopBackRight">[0x48..0x54] Верхняя задняя правая точка.</param>
    /// <param name="TopBackLeft">[0x54..0x60] Верхняя задняя левая точка.</param>
    public record class BoundingBox(
        Vector3 BottomFrontLeft,
        Vector3 BottomFrontRight,
        Vector3 BottomBackRight,
        Vector3 BottomBackLeft,
        Vector3 TopFrontLeft,
        Vector3 TopFrontRight,
        Vector3 TopBackRight,
        Vector3 TopBackLeft);
}
