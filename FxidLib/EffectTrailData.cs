using System.Buffers.Binary;
using Common;

namespace FxidLib;

/// <summary>
/// Компонент типа 5 (<see cref="FxidComponentType.Trail"/>), объект времени исполнения 0x54 байт, блок в файле 0x70. Код: <c>SetParams</c> 10002a50,
/// <c>EvaluateAtTime</c> 10002bd0, <c>Render</c> 10002da0, <c>OnAttachPointMoved</c> 10003290 (запоминает новую точку как начало шлейфа).
/// Компонент помнит точку привязки предыдущего обновления и рисует ленту от неё до текущей точки, разбитую на <c>ceil(длина / MaxPieceLength)</c> кусков
/// (не больше <see cref="MaxPieces"/>), каждый кусок - квад-объект 0x14C байт. Обновление идёт, только пока сила (dword1 контекста) внутри [start, end);
/// вне окна число кусков сбрасывается в 0 (<c>SetParams</c> начинает с 1). В данных 7 компонентов в 7 файлах: builder_tail, tas_tail и лазерные las_tail_b/g/h/r/v.
/// </summary>
/// <param name="TypeAndFlags">+0x00: тип в младшем байте, флаги выше.</param>
/// <param name="ValueStart">+0x04: проверено (<c>Render</c>): <c>value = ValueStart + окно * (ValueEnd − ValueStart)</c>, передаётся в вызов отрисовки; во всех файлах 1.0 (похоже на непрозрачность).</param>
/// <param name="ValueEnd">+0x08: во всех файлах 0.0.</param>
/// <param name="ActiveWindowStart">+0x0C: окно по значению силы (dword1), полуоткрытое [start, end): при значении, равном end, шлейф исчезает. Во всех файлах (0; 1).</param>
/// <param name="ActiveWindowEnd">+0x10: конец окна.</param>
/// <param name="MaxPieces">+0x14: размер кольца, он же предел числа кусков; во всех файлах 20.</param>
/// <param name="WidthStart">+0x18: масштаб куска (y, z) в начале; масштаб объекта = (1, w, w). В данных 0.8 - 4 файла, 0.4, 0.25 и 1.0 - по одному.</param>
/// <param name="WidthEnd">+0x1C: масштаб куска в конце (интерполируется вдоль ленты); во всех файлах равен <paramref name="WidthStart"/>.</param>
/// <param name="TextureTileLength">+0x20: мировая длина, покрываемая одним повтором текстуры (u-масштаб = длина куска / это значение). В данных 5.0 - 5 файлов, 1.0 - 2.</param>
/// <param name="MaxPieceLength">+0x24: целевая длина одного куска; во всех файлах 50.</param>
/// <param name="TextureScrollStart">+0x28: смещение u при значении окна 0; если отрицательно, прокрутка идёт по прошедшим секундам (и начало ограничивается 0). Во всех файлах −1 (прокрутка по времени).</param>
/// <param name="TextureScrollEnd">+0x2C: смещение u при значении окна 1; если отрицательно, у каждого куска случайное смещение u. В данных 2.5 - 6 файлов, 3.0 - 1.</param>
/// <param name="Resource">+0x30: материал (<c>material.lib</c>).</param>
public sealed record EffectTrailData(
    uint TypeAndFlags,
    float ValueStart,
    float ValueEnd,
    float ActiveWindowStart,
    float ActiveWindowEnd,
    uint MaxPieces,
    float WidthStart,
    float WidthEnd,
    float TextureTileLength,
    float MaxPieceLength,
    float TextureScrollStart,
    float TextureScrollEnd,
    ResourceNamedRef Resource)
{
    /// <summary>Читает блок типа 5 (точно 0x70 байт).</summary>
    /// <param name="raw">Блок компонента.</param>
    /// <exception cref="FxidFormatException">Размер блока не 0x70.</exception>
    public static EffectTrailData Read(ReadOnlySpan<byte> raw)
    {
        if (raw.Length != 0x70)
            throw new FxidFormatException($"Type 5 block must be 0x70 bytes, got 0x{raw.Length:X}");
        static float F(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadSingleLittleEndian(d[o..]);
        static uint U(ReadOnlySpan<byte> d, int o) => BinaryPrimitives.ReadUInt32LittleEndian(d[o..]);
        return new EffectTrailData(
            U(raw, 0), F(raw, 4), F(raw, 8), F(raw, 0xC), F(raw, 0x10), U(raw, 0x14),
            F(raw, 0x18), F(raw, 0x1C), F(raw, 0x20), F(raw, 0x24), F(raw, 0x28), F(raw, 0x2C),
            ResourceNamedRef.Read(raw.Slice(0x30, ResourceNamedRef.Size)));
    }
}
