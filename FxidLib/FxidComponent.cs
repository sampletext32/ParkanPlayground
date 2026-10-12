using System.Buffers.Binary;
using Common;

namespace FxidLib;

/// <summary>
/// Один компонент файла FXID. <see cref="Raw"/> всегда содержит точные байты с диска (без потерь при обратной записи);
/// типизированные представления строятся методами <c>As*</c>.
/// </summary>
/// <param name="Index">Порядковый номер компонента в файле.</param>
/// <param name="FileOffset">Смещение блока от начала содержимого FXID.</param>
/// <param name="Type">Тип компонента (младший байт <paramref name="TypeAndFlags"/>).</param>
/// <param name="TypeAndFlags">Первое слово блока: тип в младшем байте, флаги выше. В файлах используется только бит 8 (см. <see cref="TypeFlagBit8_TBD"/>).</param>
/// <param name="Raw">Блок компонента целиком, размер определяет <see cref="FxidComponentLayout.GetSize"/>.</param>
/// <param name="Resource">Ссылка на материал/звук, или <c>null</c>, если у типа ссылки нет (тип 6).</param>
public sealed record FxidComponent(
    int Index,
    int FileOffset,
    FxidComponentType Type,
    uint TypeAndFlags,
    byte[] Raw,
    ResourceNamedRef? Resource)
{
    /// <summary>
    /// Бит 8 <c>TypeAndFlags</c>. Движок копирует его в байт +4 объекта эффекта и вместе с флагом заголовка 0x400
    /// ставит <c>EffectHandle.stateFlags |= 0x10</c>. Значение не выяснено. В данных установлен у 67 компонентов типа 3,
    /// 2 типа 4, 132 типа 7 и 16 типа 8.
    /// </summary>
    public bool TypeFlagBit8_TBD => ((TypeAndFlags >> 8) & 1) != 0;

    /// <summary>Типизированное представление для <see cref="FxidComponentType.AnimParticle"/>.</summary>
    /// <exception cref="InvalidOperationException">Компонент другого типа.</exception>
    public EffectType3Data AsType3()
    {
        if (Type != FxidComponentType.AnimParticle)
            throw new InvalidOperationException($"Component {Index} is {Type}, not AnimParticle");
        return EffectType3Data.Read(Raw);
    }

    /// <summary>Типизированное представление для <see cref="FxidComponentType.Trail"/> (тип 5): шлейф между предыдущей и текущей точкой привязки.</summary>
    /// <exception cref="InvalidOperationException">Компонент другого типа.</exception>
    public EffectTrailData AsTrail()
    {
        if (Type != FxidComponentType.Trail)
            throw new InvalidOperationException($"Component {Index} is {Type}, not Trail");
        return EffectTrailData.Read(Raw);
    }

    /// <summary>Типизированное представление для <see cref="FxidComponentType.Model"/> (тип 8): порождатель частиц-плоскостей вдоль пути точки привязки.</summary>
    /// <exception cref="InvalidOperationException">Компонент другого типа.</exception>
    public EffectSpawnerData AsSpawner()
    {
        if (Type != FxidComponentType.Model)
            throw new InvalidOperationException($"Component {Index} is {Type}, not Model");
        return EffectSpawnerData.Read(Raw);
    }

    /// <summary>Типизированное представление для <see cref="FxidComponentType.Sound"/> (тип 2).</summary>
    /// <exception cref="InvalidOperationException">Компонент другого типа.</exception>
    public EffectSoundData AsSound()
    {
        if (Type != FxidComponentType.Sound)
            throw new InvalidOperationException($"Component {Index} is {Type}, not Sound");
        return EffectSoundData.Read(Raw);
    }

    /// <summary>Типизированное представление для <see cref="FxidComponentType.Plane"/> (тип 7) и <see cref="FxidComponentType.Cube"/> (тип 10): эмиттеры частиц.</summary>
    /// <exception cref="InvalidOperationException">Компонент другого типа.</exception>
    public EffectParticleData AsParticle()
    {
        if (Type is not (FxidComponentType.Plane or FxidComponentType.Cube))
            throw new InvalidOperationException($"Component {Index} is {Type}, not Plane/Cube");
        return EffectParticleData.Read(Raw);
    }

    /// <summary>Типизированное представление для <see cref="FxidComponentType.Billboard"/> (в движке - динамический источник света).</summary>
    /// <exception cref="InvalidOperationException">Компонент другого типа.</exception>
    public EffectType1Data AsType1()
    {
        if (Type != FxidComponentType.Billboard)
            throw new InvalidOperationException($"Component {Index} is {Type}, not Billboard");
        return EffectType1Data.Read(Raw);
    }

    /// <summary>Типизированное представление для <see cref="FxidComponentType.AnimBillboard"/>: раскладка типа 3 плюс один float в конце.</summary>
    /// <exception cref="InvalidOperationException">Компонент другого типа.</exception>
    public EffectType4Data AsType4()
    {
        if (Type != FxidComponentType.AnimBillboard)
            throw new InvalidOperationException($"Component {Index} is {Type}, not AnimBillboard");
        return new EffectType4Data(
            EffectType3Data.Read(Raw.AsSpan(0, 0xC8)),
            BinaryPrimitives.ReadSingleLittleEndian(Raw.AsSpan(0xC8)));
    }

    /// <summary>
    /// Типизированное представление для <see cref="FxidComponentType.AnimModel"/>: <c>SetParams</c> и <c>EvaluateAtTime</c> общие с типом 3,
    /// поэтому первые 0xC8 байт имеют раскладку типа 3.
    /// </summary>
    /// <exception cref="InvalidOperationException">Компонент другого типа.</exception>
    public EffectType9Data AsType9()
    {
        if (Type != FxidComponentType.AnimModel)
            throw new InvalidOperationException($"Component {Index} is {Type}, not AnimModel");
        return new EffectType9Data(
            EffectType3Data.Read(Raw.AsSpan(0, 0xC8)),
            BinaryPrimitives.ReadUInt32LittleEndian(Raw.AsSpan(0xC8)),
            BinaryPrimitives.ReadUInt32LittleEndian(Raw.AsSpan(0xCC)));
    }
}
