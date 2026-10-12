using System.Buffers.Binary;
using Common;

namespace FxidLib;

/// <summary>
/// Эмиттеры частиц: тип 7 (<see cref="FxidComponentType.Plane"/>, частица 0xCC байт, не зацикленная) и тип 10 (<see cref="FxidComponentType.Cube"/>,
/// частица 0x1EC байт, зацикленная). Общие <c>EvaluateAtTime</c> (10001240), <c>Render</c> (10001310) и вспомогательная инициализация 10001010;
/// <c>SetParams</c> различается (100017e0 / 10001b70). Объекты времени исполнения одинаковы (0x48 байт: EffectBase, указатель на файл, массивы);
/// различается только класс элемента частицы, возвращаемый слотом vtable 7 (<c>GetParticle(index)</c>), и <c>SetParams</c>: тип 7 ставит признак
/// «не зациклен» = 1 и читает <see cref="SizeEnd"/>, тип 10 ставит 0, игнорирует <see cref="SizeEnd"/> и при ошибке загрузки ресурса берёт случайный материал.
/// Число частиц = <see cref="Groups"/> × <see cref="PerGroup"/>. Значения с пометкой «_maybe» в игре не подтверждены.
/// </summary>
/// <param name="TypeAndFlags">+0x00: тип в младшем байте, флаги выше.</param>
/// <param name="Mode_maybe">+0x04: проверено. Тип 7: 0 и 2 → значение отрисовки частицы 0, 1 → 3 (в данных 0 - 141, 1 - 37).
/// Тип 10: 0x10 и 0x20 выбирают таблицу многогранников формы (в данных 32 - 16, 16 - 6; см. README, FUN_1000caf0).</param>
/// <param name="ValueLerpStart_maybe">+0x08: начало интерполяции, используемой <c>Render</c> (<c>t^ValueExponent</c>); в файлах 1.0 → 0.0..0.5, похоже на затухание непрозрачности.</param>
/// <param name="ValueLerpEnd_maybe">+0x0C: конец интерполяции значения.</param>
/// <param name="ValueExponent_maybe">+0x10: показатель степени интерполяции значения.</param>
/// <param name="ActiveWindowStart">+0x14: окно по dword1 контекста (силе); <c>EvaluateAtTime</c> кладёт <c>(v − lo) / (hi − lo)</c> в <c>object+0x1C</c>, вне окна −1.</param>
/// <param name="ActiveWindowEnd">+0x18: конец окна активности.</param>
/// <param name="ParticleLifeFraction">+0x1C: время жизни одной частицы в нормированных единицах; смещение старта группы <c>i</c> = <c>(1 − this) * i / Groups</c>.
/// Тип 7: 0.85 - 100 компонентов, 1.0 - 22, 0.8 - 17, 0.7 - 13; тип 10: 0.25 - 17, 0.2 - 5.</param>
/// <param name="RepeatRate">+0x20: при &lt;= 1 делит время, при &gt; 1 умножает и берёт дробную часть (периодическое возрождение).
/// Тип 7: 0.7 - 82, 0.6 - 23, 0.8 - 21, 1.0 - 18, 0.4 - 12, −1 - 8, 1.5 - 5, 0.15 - 5; тип 10: всегда 1.0.</param>
/// <param name="Groups">+0x24: число групп. Тип 7: 3 - 43 компонента, 6 - 33, 10 - 28, 1 - 24, 4 - 17, 2 - 15, 8 - 12, 20 - 3; тип 10: всегда 1.</param>
/// <param name="PerGroup">+0x28: частиц в группе. Тип 7: 1 - 115, 5 - 23, 15 - 17, 10 - 16; тип 10: 7 - 8, 10 - 4, 12 - 3, 20 - 3.</param>
/// <param name="TrajectoryStart">+0x2C: начало траектории частицы (локальная система).</param>
/// <param name="TrajectoryEnd">+0x38: конец траектории; к нему добавляется случайный ящик.</param>
/// <param name="SpawnBox">+0x44: случайный разброс точки появления (ящик, центрированный).</param>
/// <param name="ColorExponent_maybe">+0x50: показатели степени по каналам, применяемые к <c>t</c> в <c>Render</c>.</param>
/// <param name="SizeStart">+0x5C: размер в начале.</param>
/// <param name="SizeEnd">+0x68: размер в конце; используется только типом 7.</param>
/// <param name="SizeJitterBox">+0x74: случайный разброс размера.</param>
/// <param name="SizeExponent_maybe">+0x80: показатели степени размера по осям.</param>
/// <param name="GravityFactor">+0x8C: проверено. Гравитация = <c>this * IWorld::QueryGravity() * 0.5</c> (<c>InitCommon</c>); падение = <c>gravity * t²</c> по третьей оси.
/// Тип 7: 0 - 142 компонента, 1 - 35, 2 - 1; тип 10: 1 - 13, 1.5 - 8, 3 - 1.</param>
/// <param name="Resource">+0x90: материал (при ошибке загрузки берётся случайный). У типа 7 задан у всех 178, у типа 10 пуст у 11 из 22.</param>
public sealed record EffectParticleData(
    uint TypeAndFlags,
    uint Mode_maybe,
    float ValueLerpStart_maybe,
    float ValueLerpEnd_maybe,
    float ValueExponent_maybe,
    float ActiveWindowStart,
    float ActiveWindowEnd,
    float ParticleLifeFraction,
    float RepeatRate,
    uint Groups,
    uint PerGroup,
    Vector3 TrajectoryStart,
    Vector3 TrajectoryEnd,
    Vector3 SpawnBox,
    Vector3 ColorExponent_maybe,
    Vector3 SizeStart,
    Vector3 SizeEnd,
    Vector3 SizeJitterBox,
    Vector3 SizeExponent_maybe,
    float GravityFactor,
    ResourceNamedRef Resource)
{
    /// <summary>Читает блок типа 7 или 10 (точно 0xD0 байт).</summary>
    /// <param name="raw">Блок компонента.</param>
    /// <exception cref="FxidFormatException">Размер блока не 0xD0.</exception>
    public static EffectParticleData Read(ReadOnlySpan<byte> raw)
    {
        if (raw.Length != 0xD0)
            throw new FxidFormatException($"Particle block must be 0xD0 bytes, got 0x{raw.Length:X}");
        static float F(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadSingleLittleEndian(d[o..]);
        static uint U(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadUInt32LittleEndian(d[o..]);
        static Vector3 V(ReadOnlySpan<byte> d, int o) => new(F(d, o), F(d, o + 4), F(d, o + 8));
        return new EffectParticleData(
            U(raw, 0), U(raw, 4), F(raw, 8), F(raw, 0xC), F(raw, 0x10), F(raw, 0x14), F(raw, 0x18), F(raw, 0x1C), F(raw, 0x20),
            U(raw, 0x24), U(raw, 0x28),
            V(raw, 0x2C), V(raw, 0x38), V(raw, 0x44), V(raw, 0x50), V(raw, 0x5C), V(raw, 0x68), V(raw, 0x74), V(raw, 0x80),
            F(raw, 0x8C),
            ResourceNamedRef.Read(raw.Slice(0x90, ResourceNamedRef.Size)));
    }
}
