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

    /// <summary>Типизированное представление для <see cref="FxidComponentType.AnimatedObject"/>.</summary>
    /// <exception cref="InvalidOperationException">Компонент другого типа.</exception>
    public EffectType3Data AsType3()
    {
        if (Type != FxidComponentType.AnimatedObject)
            throw new InvalidOperationException($"Component {Index} is {Type}, not AnimatedObject");
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

    /// <summary>Типизированное представление для <see cref="FxidComponentType.ParticleSpawner"/> (тип 8): порождатель частиц-плоскостей вдоль пути точки привязки.</summary>
    /// <exception cref="InvalidOperationException">Компонент другого типа.</exception>
    public EffectSpawnerData AsSpawner()
    {
        if (Type != FxidComponentType.ParticleSpawner)
            throw new InvalidOperationException($"Component {Index} is {Type}, not ParticleSpawner");
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

    /// <summary>Типизированное представление для <see cref="FxidComponentType.PlaneParticles"/> (тип 7) и <see cref="FxidComponentType.PolyhedronParticles"/> (тип 10): эмиттеры частиц.</summary>
    /// <exception cref="InvalidOperationException">Компонент другого типа.</exception>
    public EffectParticleData AsParticle()
    {
        if (Type is not (FxidComponentType.PlaneParticles or FxidComponentType.PolyhedronParticles))
            throw new InvalidOperationException($"Component {Index} is {Type}, not PlaneParticles/PolyhedronParticles");
        return EffectParticleData.Read(Raw);
    }

    /// <summary>Типизированное представление для <see cref="FxidComponentType.Light"/> (в движке - динамический источник света).</summary>
    /// <exception cref="InvalidOperationException">Компонент другого типа.</exception>
    public EffectType1Data AsType1()
    {
        if (Type != FxidComponentType.Light)
            throw new InvalidOperationException($"Component {Index} is {Type}, not Light");
        return EffectType1Data.Read(Raw);
    }

    /// <summary>Типизированное представление для <see cref="FxidComponentType.AnimatedObjectByDistance"/>: раскладка типа 3 плюс один float в конце.</summary>
    /// <exception cref="InvalidOperationException">Компонент другого типа.</exception>
    public EffectType4Data AsType4()
    {
        if (Type != FxidComponentType.AnimatedObjectByDistance)
            throw new InvalidOperationException($"Component {Index} is {Type}, not AnimatedObjectByDistance");
        return new EffectType4Data(
            EffectType3Data.Read(Raw.AsSpan(0, 0xC8)),
            BinaryPrimitives.ReadSingleLittleEndian(Raw.AsSpan(0xC8)));
    }

    /// <summary>
    /// Типизированное представление для <see cref="FxidComponentType.AnimatedObjectDrawMode"/>: <c>SetParams</c> и <c>EvaluateAtTime</c> общие с типом 3,
    /// поэтому первые 0xC8 байт имеют раскладку типа 3.
    /// </summary>
    /// <exception cref="InvalidOperationException">Компонент другого типа.</exception>
    public EffectType9Data AsType9()
    {
        if (Type != FxidComponentType.AnimatedObjectDrawMode)
            throw new InvalidOperationException($"Component {Index} is {Type}, not AnimatedObjectDrawMode");
        return new EffectType9Data(
            EffectType3Data.Read(Raw.AsSpan(0, 0xC8)),
            BinaryPrimitives.ReadUInt32LittleEndian(Raw.AsSpan(0xC8)),
            BinaryPrimitives.ReadUInt32LittleEndian(Raw.AsSpan(0xCC)));
    }
}
