using System.Buffers.Binary;
using Common;

namespace FxidLib;

/// <summary>
/// Компонент типа 1 (<see cref="FxidComponentType.Light"/>, раньше назывался Billboard): <c>EffectType1::SetParams</c> работает с <c>ILightManager</c>, то есть это динамический источник света
/// с анимированными позицией, направлением, цветом и дальностью. Раскладка выведена из <c>SetParams</c> и файлов (например, <c>aim_light_L</c>:
/// направление (0, −1, 0), цвет (1, 0.1, 0.1) → (2.5, 0.1, 0.1), дальность 3 → 12). Байты после +0x78 до ресурса по +0xA0 не расшифрованы.
/// </summary>
/// <param name="TypeAndFlags">+0x00: тип в младшем байте, флаги выше.</param>
/// <param name="LightType_maybe">+0x04: вид источника: 1, 2, 5, 6, 7 (значения 3 и 4 движок обрабатывает, но в данных их нет); отображается в тип <c>ILightManager</c> и флаги.
/// В данных: 6 - 148 компонентов, 5 - 95, 7 - 15, 1 - 3, 2 - 1.</param>
/// <param name="ActiveTimeStart">+0x08: начало окна по значению силы эффекта (dword1 контекста); при источниках по жизни равно нормализованной жизни.
/// Частые окна: (0.75; 1) - 88 компонентов, (0; 1) - 43, (0; 0.5) - 27, (0.01; 1) - 20.</param>
/// <param name="ActiveTimeEnd">+0x0C: конец окна активности.</param>
/// <param name="PositionStart">+0x10: позиция источника в начале окна.</param>
/// <param name="PositionEnd">+0x1C: позиция в конце окна.</param>
/// <param name="DirectionStart_maybe">+0x28: направление в начале окна.</param>
/// <param name="DirectionEnd_maybe">+0x34: направление в конце окна.</param>
/// <param name="ColorStart_maybe">+0x40: цвет в начале (4 float: RGB и четвёртый, в файлах 0); встречаются значения больше 1.</param>
/// <param name="ColorEnd_maybe">+0x50: цвет в конце.</param>
/// <param name="ColorJitter_TBD">+0x60: амплитуда случайного разброса по каналам.</param>
/// <param name="RangeStart_maybe">+0x70: дальность света в начале.</param>
/// <param name="RangeEnd_maybe">+0x74: дальность в конце.</param>
/// <param name="Resource">+0xA0: ссылка на материал/текстуру света. Пуста у 261 из 262 компонентов (свет без текстуры).</param>
public sealed record EffectType1Data(
    uint TypeAndFlags,
    uint LightType_maybe,
    float ActiveTimeStart,
    float ActiveTimeEnd,
    Vector3 PositionStart,
    Vector3 PositionEnd,
    Vector3 DirectionStart_maybe,
    Vector3 DirectionEnd_maybe,
    float[] ColorStart_maybe,
    float[] ColorEnd_maybe,
    float[] ColorJitter_TBD,
    float RangeStart_maybe,
    float RangeEnd_maybe,
    ResourceNamedRef Resource)
{
    /// <summary>Читает блок типа 1 (точно 0xE0 байт).</summary>
    /// <param name="raw">Блок компонента.</param>
    /// <exception cref="FxidFormatException">Размер блока не 0xE0.</exception>
    public static EffectType1Data Read(ReadOnlySpan<byte> raw)
    {
        if (raw.Length != 0xE0)
            throw new FxidFormatException($"Type 1 block must be 0xE0 bytes, got 0x{raw.Length:X}");

        static float F(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadSingleLittleEndian(d[o..]);
        static Vector3 V(ReadOnlySpan<byte> d, int o) => new(F(d, o), F(d, o + 4), F(d, o + 8));
        static float[] Q(ReadOnlySpan<byte> d, int o) => [F(d, o), F(d, o + 4), F(d, o + 8), F(d, o + 12)];

        return new EffectType1Data(
            BinaryPrimitives.ReadUInt32LittleEndian(raw),
            BinaryPrimitives.ReadUInt32LittleEndian(raw[4..]),
            F(raw, 0x08), F(raw, 0x0C),
            V(raw, 0x10), V(raw, 0x1C), V(raw, 0x28), V(raw, 0x34),
            Q(raw, 0x40), Q(raw, 0x50), Q(raw, 0x60),
            F(raw, 0x70), F(raw, 0x74),
            ResourceNamedRef.Read(raw.Slice(0xA0, ResourceNamedRef.Size)));
    }
}
