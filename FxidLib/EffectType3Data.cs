using System.Buffers.Binary;
using Common;

namespace FxidLib;

/// <summary>
/// Типизированное представление компонента типа 3 (<c>EffectType3FileData</c> в Ghidra, класс времени исполнения <c>EffectType3</c>):
/// анимированный по жизни спрайт/модель с материалом. Смысл полей взят из <c>EffectType3::SetParams</c>, <c>EvaluateAtTime</c>, <c>method3</c> (Effect.dll);
/// пока компонент активен, значения интерполируются по формуле <c>Начало + (Конец − Начало) * pow(жизнь, Показатель)</c> для каждой оси.
/// Заменяет прежний <c>AnimParticleComponentData</c>, раскладка которого сдвигалась на 4 байта с +0x28 и перекрывала ссылку на ресурс.
/// </summary>
/// <param name="TypeAndFlags">+0x00: тип в младшем байте, флаги выше (см. <see cref="FxidComponent.TypeFlagBit8_TBD"/>).</param>
/// <param name="Space">+0x04: система координат <c>ESpace</c>, копируется в <c>EffectContents.space</c>: 0 - меш, 1 - локальная, 2 - мировая.
/// В данных у типа 3: 2 - 239 компонентов, 1 - 79, 0 - 76.</param>
/// <param name="PhaseStart">+0x08: начало диапазона фазы анимации. Значение <c>v = pow(t, PhaseExponent) * (PhaseEnd − max(PhaseStart, 0)) + max(PhaseStart, 0)</c>,
/// где <c>t</c> - сила эффекта при <c>PhaseStart &lt; 0</c>, иначе нормализованная жизнь. Дробная часть <c>v</c> передаётся в вызов отрисовки как фаза кадра.
/// В данных: −1.0 - 317 компонентов из 394, 0.0 - 77.</param>
/// <param name="PhaseEnd">+0x0C: конец диапазона фазы (число кадров/стадий материала). В данных 12 значений: 9 - 179 компонентов, 1 - 85, 5 - 41, 12 - 19, 11 - 18, 3 - 17, 6 - 13, 7 - 8.</param>
/// <param name="PhaseExponent">+0x10: показатель степени фазы; во всех 394 компонентах 1.0.</param>
/// <param name="IntensityStart">+0x14: начало интенсивности (прозрачность/размер): <c>lerp(IntensityStart, IntensityEnd, pow(жизнь, IntensityExponent))</c> передаётся в вызов отрисовки.</param>
/// <param name="IntensityEnd">+0x18: конец интенсивности.</param>
/// <param name="IntensityExponent">+0x1C: показатель степени интенсивности; в данных почти везде 1.0.</param>
/// <param name="ActiveTimeStart">+0x20: начало окна активности. Компонент активен, пока <c>ActiveTimeStart &lt;= t &lt;= ActiveTimeEnd</c>, жизнь = <c>(t − start) / (end − start)</c>.
/// Единицы: вход <c>t</c> (dword1 контекста <c>EvaluateAtTime</c>) - значение силы эффекта 0..1 (см. <see cref="FxidStrength"/>), а не обязательно время:
/// типы 7 и 10 (проверено в 10001240) сравнивают его со своим окном и нормируют; при источнике силы по жизни он равен нормализованной жизни.
/// Частые окна: (0; 1) - 140 компонентов, (0.5; 1) - 46, (0.01; 1) - 37, (0; 0.3) - 26, (0.75; 1) - 26.</param>
/// <param name="ActiveTimeEnd">+0x24: конец окна активности.</param>
/// <param name="PositionStart">+0x28: позиция в начале жизни (в системе <paramref name="Space"/>).</param>
/// <param name="PositionEnd">+0x34: позиция в конце жизни.</param>
/// <param name="PositionExponent">+0x40: показатели степени интерполяции позиции по осям; в данных почти всегда (1, 1, 1).</param>
/// <param name="DirectionStart">+0x4C: направление в начале жизни (показателя нет: интерполяция линейная).</param>
/// <param name="DirectionEnd">+0x58: направление в конце жизни.</param>
/// <param name="ScaleStart">+0x64: масштаб в начале жизни.</param>
/// <param name="ScaleEnd">+0x70: масштаб в конце жизни.</param>
/// <param name="ScaleExponent">+0x7C: показатели степени интерполяции масштаба по осям.</param>
/// <param name="Resource">+0x88: материал (<c>material.lib</c>), у всех 394 компонентов задан.</param>
public sealed record EffectType3Data(
    uint TypeAndFlags,
    uint Space,
    float PhaseStart,
    float PhaseEnd,
    float PhaseExponent,
    float IntensityStart,
    float IntensityEnd,
    float IntensityExponent,
    float ActiveTimeStart,
    float ActiveTimeEnd,
    Vector3 PositionStart,
    Vector3 PositionEnd,
    Vector3 PositionExponent,
    Vector3 DirectionStart,
    Vector3 DirectionEnd,
    Vector3 ScaleStart,
    Vector3 ScaleEnd,
    Vector3 ScaleExponent,
    ResourceNamedRef Resource)
{
    /// <summary>Читает блок типа 3 (первые 0xC8 байт; типы 4 и 9 используют ту же раскладку).</summary>
    /// <param name="raw">Не менее 0xC8 байт.</param>
    /// <exception cref="FxidFormatException">Блок короче 0xC8 байт.</exception>
    public static EffectType3Data Read(ReadOnlySpan<byte> raw)
    {
        if (raw.Length < 0xC8)
            throw new FxidFormatException($"Type 3 block must be at least 0xC8 bytes, got 0x{raw.Length:X}");

        static float F(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadSingleLittleEndian(d[o..]);
        static Vector3 V(ReadOnlySpan<byte> d, int o) => new(F(d, o), F(d, o + 4), F(d, o + 8));

        return new EffectType3Data(
            BinaryPrimitives.ReadUInt32LittleEndian(raw),
            BinaryPrimitives.ReadUInt32LittleEndian(raw[4..]),
            F(raw, 0x08), F(raw, 0x0C), F(raw, 0x10),
            F(raw, 0x14), F(raw, 0x18), F(raw, 0x1C),
            F(raw, 0x20), F(raw, 0x24),
            V(raw, 0x28), V(raw, 0x34), V(raw, 0x40),
            V(raw, 0x4C), V(raw, 0x58),
            V(raw, 0x64), V(raw, 0x70), V(raw, 0x7C),
            ResourceNamedRef.Read(raw.Slice(0x88, ResourceNamedRef.Size)));
    }

    /// <summary>Истина, когда фаза определяется силой эффекта, а не локальной жизнью (<c>PhaseStart &lt; 0</c>).</summary>
    public bool PhaseUsesEffectStrength => PhaseStart < 0f;

    /// <summary>Нормализованная жизнь 0..1 в момент <paramref name="t"/> или <c>null</c> вне окна активности (движок хранит там −1).</summary>
    /// <param name="t">Значение силы эффекта.</param>
    public float? LifeAt(float t) =>
        t >= ActiveTimeStart && t <= ActiveTimeEnd
            ? (t - ActiveTimeStart) / (ActiveTimeEnd - ActiveTimeStart)
            : null;

    /// <summary>
    /// Радиус границ в точности как считает <c>EffectType3::SetParams</c> (хранится в четвёртом dword первых 0x10 байт эффекта):
    /// <c>max(maxComponent(PositionStart), maxComponent(PositionEnd)) + max(|ScaleStart|, |ScaleEnd|)</c>.
    /// Максимум берётся по знаковым компонентам, а не по модулям.
    /// </summary>
    public float BoundRadius()
    {
        static float MaxComp(Vector3 v) => MathF.Max(v.X, MathF.Max(v.Y, v.Z));
        static float Len(Vector3 v) => MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        return MathF.Max(MaxComp(PositionStart), MaxComp(PositionEnd)) + MathF.Max(Len(ScaleStart), Len(ScaleEnd));
    }
}
