using System.Buffers.Binary;
using Common;

namespace FxidLib;

/// <summary>
/// Компонент типа 8 (<see cref="FxidComponentType.ParticleSpawner"/>), объект времени исполнения 0xAC байт, блок в файле 0xF8. Код: <c>SetParams</c> 10012000,
/// <c>EvaluateAtTime</c> 100123b0, <c>Render</c> 10012f90. <c>SetParams</c> копирует <c>SettingsId</c> заголовка (+0x14) в объект (+0x58; типы 7 и 10 делают то же по +0x34),
/// вероятно, для решений по качеству. Порождатель: пока точка привязки движется, он выпускает частицы (плоскости по 0xCC байт - тот же класс, что у типа 7)
/// в кольцевой буфер из <see cref="MaxParticles"/> слотов. Блок частицы по +0x58..+0xB7 в точности повторяет блок типов 7/10 по +0x2C..+0x8B (сдвиг 0x2C).
/// </summary>
/// <param name="TypeAndFlags">+0x00: тип в младшем байте, флаги выше.</param>
/// <param name="ValueLerpStart">+0x04: проверено (<c>Render</c>): значение частицы <c>start + pow(exponent, life) * (end − start)</c> - четвёртый аргумент вызова отрисовки;
/// похоже на непрозрачность. В данных 1.0 - 116 компонентов, 0.7 - 8, 0.8 - 3.</param>
/// <param name="ValueLerpEnd">+0x08: конец интерполяции значения. В данных 0 - 112 компонентов, 0.1 - 14, 0.3 - 1.</param>
/// <param name="ValueLerpExponent">+0x0C: показатель степени; во всех 127 компонентах 1.0.</param>
/// <param name="ActiveWindowStart">+0x10: окно по значению силы (dword1 контекста). В данных 13 значений начала: 0 - 86 компонентов, 0.01 - 10, 0.001 - 9, 0.05 - 6.</param>
/// <param name="ActiveWindowEnd">+0x14: конец окна. Чаще всего 1.0 - 94 компонента, 0.999 - 9, 0.5 - 6.</param>
/// <param name="SpawnIntervalStart">+0x18: секунд между порождениями при значении окна 0. В данных 0.08 - 62 компонента, 0.09 - 16, 0.1 - 15, 0.05 - 11, 0.15 - 9, 0.2 - 8.</param>
/// <param name="SpawnIntervalEnd">+0x1C: то же при значении окна 1. В данных 0.08 - 70, 0.07 - 14, 0.1 - 9, 0.15 - 8, 0.2 - 7.</param>
/// <param name="LifeExponent_unused">+0x20: <c>Render</c> считает <c>pow(this, life)</c>, но результат отбрасывает (мёртвый код). В данных 1.0 - 99 компонентов, −1.0 - 20, 0.3 - 8.</param>
/// <param name="MaxParticles">+0x24: размер кольцевого буфера. В данных 15 значений: 10 - 23 компонента, 20 - 22, 15 - 20, 8 - 13, 6 - 13, 4 - 9.</param>
/// <param name="SpawnOffsetStart">+0x28: смещение точки порождения от точки привязки при значении окна 0. Во всех файлах (0, 0, 0); конец и разброс тоже 0, показатели (1, 1, 1).</param>
/// <param name="SpawnOffsetEnd">+0x34: смещение при значении окна 1.</param>
/// <param name="SpawnOffsetJitter">+0x40: случайный ящик, добавляемый к <see cref="SpawnOffsetEnd"/> для каждой частицы.</param>
/// <param name="SpawnOffsetExponent_maybe">+0x4C: показатель степени интерполяции смещения по осям.</param>
/// <param name="TrajectoryStart">+0x58: блок частицы (см. <see cref="EffectParticleData.TrajectoryStart"/>).</param>
/// <param name="TrajectoryEnd">+0x64: конец траектории частицы.</param>
/// <param name="SpawnBox">+0x70: разброс точки появления.</param>
/// <param name="ColorExponent_maybe">+0x7C: показатели степени цвета по каналам.</param>
/// <param name="SizeStart">+0x88: размер в начале.</param>
/// <param name="SizeEnd">+0x94: размер в конце.</param>
/// <param name="SizeJitterBox">+0xA0: случайный разброс размера.</param>
/// <param name="SizeExponent_maybe">+0xAC: показатели степени размера по осям.</param>
/// <param name="Resource">+0xB8: материал (<c>material.lib</c>), у всех 127 компонентов задан.</param>
public sealed record EffectSpawnerData(
    uint TypeAndFlags,
    float ValueLerpStart,
    float ValueLerpEnd,
    float ValueLerpExponent,
    float ActiveWindowStart,
    float ActiveWindowEnd,
    float SpawnIntervalStart,
    float SpawnIntervalEnd,
    float LifeExponent_unused,
    uint MaxParticles,
    Vector3 SpawnOffsetStart,
    Vector3 SpawnOffsetEnd,
    Vector3 SpawnOffsetJitter,
    Vector3 SpawnOffsetExponent_maybe,
    Vector3 TrajectoryStart,
    Vector3 TrajectoryEnd,
    Vector3 SpawnBox,
    Vector3 ColorExponent_maybe,
    Vector3 SizeStart,
    Vector3 SizeEnd,
    Vector3 SizeJitterBox,
    Vector3 SizeExponent_maybe,
    ResourceNamedRef Resource)
{
    /// <summary>Читает блок типа 8 (точно 0xF8 байт).</summary>
    /// <param name="raw">Блок компонента.</param>
    /// <exception cref="FxidFormatException">Размер блока не 0xF8.</exception>
    public static EffectSpawnerData Read(ReadOnlySpan<byte> raw)
    {
        if (raw.Length != 0xF8)
            throw new FxidFormatException($"Type 8 block must be 0xF8 bytes, got 0x{raw.Length:X}");
        static float F(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadSingleLittleEndian(d[o..]);
        static uint U(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadUInt32LittleEndian(d[o..]);
        static Vector3 V(ReadOnlySpan<byte> d, int o) => new(F(d, o), F(d, o + 4), F(d, o + 8));
        return new EffectSpawnerData(
            U(raw, 0), F(raw, 4), F(raw, 8), F(raw, 0xC), F(raw, 0x10), F(raw, 0x14), F(raw, 0x18), F(raw, 0x1C), F(raw, 0x20), U(raw, 0x24),
            V(raw, 0x28), V(raw, 0x34), V(raw, 0x40), V(raw, 0x4C),
            V(raw, 0x58), V(raw, 0x64), V(raw, 0x70), V(raw, 0x7C), V(raw, 0x88), V(raw, 0x94), V(raw, 0xA0), V(raw, 0xAC),
            ResourceNamedRef.Read(raw.Slice(0xB8, ResourceNamedRef.Size)));
    }
}
