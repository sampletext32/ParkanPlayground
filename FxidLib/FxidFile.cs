using System.Buffers.Binary;
using Common;

namespace FxidLib;

/// <summary>
/// Файл эффекта FXID целиком: содержимое одного элемента NRes типа «FXID» из архива <c>effects.rlb</c>
/// (в игре 476 файлов, 1567 компонентов). Состоит из 60-байтного заголовка и подряд идущих блоков компонентов.
/// </summary>
/// <param name="Header">Заголовок эффекта.</param>
/// <param name="Components">Компоненты в порядке следования в файле.</param>
public sealed record FxidFile(EffectHeader Header, IReadOnlyList<FxidComponent> Components)
{
    /// <summary>Размер заголовка в байтах.</summary>
    public const int HeaderSize = 0x3C;

    /// <summary>Разбирает содержимое FXID.</summary>
    /// <param name="data">Содержимое элемента.</param>
    /// <exception cref="FxidFormatException">Размеры не сходятся точно: усечённый блок, неизвестный тип компонента или лишние байты в конце.</exception>
    public static FxidFile Read(ReadOnlySpan<byte> data)
    {
        if (data.Length < HeaderSize)
            throw new FxidFormatException($"FXID payload is too short: {data.Length} bytes");

        var header = new EffectHeader(
            ComponentCount: U32(data, 0x00),
            StrengthSource: (EffectStrengthSource)U32(data, 0x04),
            Duration: F32(data, 0x08),
            StrengthJitter: F32(data, 0x0C),
            Flags: (FxidEffectFlags)U32(data, 0x10),
            SettingsId: U32(data, 0x14),
            RandShiftX: F32(data, 0x18),
            RandShiftY: F32(data, 0x1C),
            RandShiftZ: F32(data, 0x20),
            PivotX: F32(data, 0x24),
            PivotY: F32(data, 0x28),
            PivotZ: F32(data, 0x2C),
            ScaleX: F32(data, 0x30),
            ScaleY: F32(data, 0x34),
            ScaleZ: F32(data, 0x38));

        var components = new List<FxidComponent>((int)Math.Min(header.ComponentCount, 64u));
        var offset = HeaderSize;
        for (var i = 0; i < header.ComponentCount; i++)
        {
            if (offset + 4 > data.Length)
                throw new FxidFormatException($"Component {i}: truncated at offset 0x{offset:X}");

            var typeAndFlags = U32(data, offset);
            var type = (FxidComponentType)(typeAndFlags & 0xFF);

            // Движок на неизвестном типе не продвигается (перечитывал бы тот же блок); здесь это ошибка формата.
            var size = FxidComponentLayout.GetSize(type);
            if (offset + size > data.Length)
                throw new FxidFormatException($"Component {i} ({type}): truncated at offset 0x{offset:X}");

            var raw = data.Slice(offset, size).ToArray();
            ResourceNamedRef? resource = null;
            var refOffset = FxidComponentLayout.GetResourceRefOffset(type);
            if (refOffset >= 0)
                resource = ResourceNamedRef.Read(raw.AsSpan(refOffset, ResourceNamedRef.Size));

            components.Add(new FxidComponent(i, offset, type, typeAndFlags, raw, resource));
            offset += size;
        }

        if (offset != data.Length)
            throw new FxidFormatException(
                $"FXID size mismatch: components end at 0x{offset:X}, payload is 0x{data.Length:X} bytes");

        return new FxidFile(header, components);
    }

    private static uint U32(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadUInt32LittleEndian(d[o..]);
    private static float F32(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadSingleLittleEndian(d[o..]);
}
