using System.Buffers.Binary;
using Common;

namespace FxidLib;

/// <summary>
/// Компонент типа 2 (<see cref="FxidComponentType.Sound"/>), объект времени исполнения 0xA0 байт (указатель на файл по +0x24, в отличие от других типов),
/// блок в файле 0x94. Код: конструктор 10013d70, <c>SetParams</c> 10013df0, <c>EvaluateAtTime</c> 10013f90, <c>OnEnabledChanged</c> 10014250 (останавливает звук при выключении).
/// <see cref="Mode"/> 2 и 3 - зацикленные звуки, играющие, пока сила (dword1 контекста) внутри [<see cref="WindowStart"/>, <see cref="WindowEnd"/>];
/// прочие значения - одиночные звуки, стартующие, когда сила пересекает <see cref="WindowStart"/> вверх. Волна грузится менеджером звука
/// (<c>I3DSound::LoadWaveSoundBuffer</c>) по <see cref="Resource"/>. Смещение позиции и громкость интерполируются по силе; позиция преобразуется
/// матрицей родителя; вместе с минимальной/максимальной дистанцией и <see cref="Unknown50_TBD"/> они уходят в звуковую систему блоком параметров
/// (команда 0x38E), из того же вызова читается сфера границ звука. В данных 199 компонентов в 191 файле, архив <c>sounds.lib</c>.
/// </summary>
/// <param name="TypeAndFlags">+0x00: тип в младшем байте, флаги выше.</param>
/// <param name="Mode">+0x04: режим; 2 и 3 - зацикленные; передаётся и загрузчику волны. В данных: 0 - 119 компонентов, 2 - 80 (значение 3 не встречается).</param>
/// <param name="WindowStart">+0x08: начало окна по силе. Частые: (0; 0.1) - 51 компонент, (0.01; 1) - 26, (0.001; 1) - 25, (0; 1) - 24.</param>
/// <param name="WindowEnd">+0x0C: конец окна.</param>
/// <param name="PositionOffsetStart">+0x10: проверено: интерполяция по силе, затем преобразование матрицей родителя в мировую позицию. В данных (0, 0, 0) - 186 компонентов,
/// (0, −3, 0) - 12, (0, 0, −1) - 1; во всех файлах начало равно концу.</param>
/// <param name="PositionOffsetEnd">+0x1C: смещение позиции при силе 1.</param>
/// <param name="DirectionStart_unused">+0x28: копируется в объект в <c>SetParams</c>, но <c>EvaluateAtTime</c> его не читает; во всех файлах (1, 0, 0).</param>
/// <param name="DirectionEnd_unused">+0x34: то же.</param>
/// <param name="MinDistance">+0x40: проверено: константа (<c>SetParams</c> не интерполирует), уходит в параметры звука. В данных от 1 до 150 (23 значения: 10 - 51 компонент, 3 - 45, 15 - 22, 5 - 18).</param>
/// <param name="MaxDistance">+0x44: проверено: константа, от 15 до 1500 (23 значения: 100 - 41 компонент, 120 - 33, 50 - 27, 40 - 21), всегда больше <see cref="MinDistance"/>.</param>
/// <param name="VolumeStart">+0x48: проверено: громкость = <c>start + сила * (end − start)</c>. 1.0 у 198 из 199 компонентов, 0.8 у одного.</param>
/// <param name="VolumeEnd">+0x4C: во всех компонентах 1.0.</param>
/// <param name="Unknown50_TBD">+0x50: передаётся последним полем блока параметров звука (команда 0x38E); во всех файлах 0.</param>
/// <param name="Resource">+0x54: файл волны (<c>sounds.lib</c>).</param>
public sealed record EffectSoundData(
    uint TypeAndFlags,
    uint Mode,
    float WindowStart,
    float WindowEnd,
    Vector3 PositionOffsetStart,
    Vector3 PositionOffsetEnd,
    Vector3 DirectionStart_unused,
    Vector3 DirectionEnd_unused,
    float MinDistance,
    float MaxDistance,
    float VolumeStart,
    float VolumeEnd,
    float Unknown50_TBD,
    ResourceNamedRef Resource)
{
    /// <summary>Звук зациклен (режимы 2 и 3).</summary>
    public bool IsLooped => Mode is 2 or 3;

    /// <summary>Читает блок типа 2 (точно 0x94 байт).</summary>
    /// <param name="raw">Блок компонента.</param>
    /// <exception cref="FxidFormatException">Размер блока не 0x94.</exception>
    public static EffectSoundData Read(ReadOnlySpan<byte> raw)
    {
        if (raw.Length != 0x94)
            throw new FxidFormatException($"Type 2 block must be 0x94 bytes, got 0x{raw.Length:X}");
        static float F(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadSingleLittleEndian(d[o..]);
        static uint U(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadUInt32LittleEndian(d[o..]);
        static Vector3 V(ReadOnlySpan<byte> d, int o) => new(F(d, o), F(d, o + 4), F(d, o + 8));
        return new EffectSoundData(
            U(raw, 0), U(raw, 4), F(raw, 8), F(raw, 0xC),
            V(raw, 0x10), V(raw, 0x1C), V(raw, 0x28), V(raw, 0x34),
            F(raw, 0x40), F(raw, 0x44), F(raw, 0x48), F(raw, 0x4C), F(raw, 0x50),
            ResourceNamedRef.Read(raw.Slice(0x54, ResourceNamedRef.Size)));
    }
}
