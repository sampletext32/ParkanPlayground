using Common;

namespace FxidLib;

/// <summary>
/// Фиксированные размеры блоков компонентов FXID и смещения ссылок на ресурс.
/// Размеры взяты из <c>EffectHandle::InitFromFxidData</c> (Effect.dll) и проверены на всех 476 файлах архива effects.rlb.
/// Смещения ссылок проверены на 1567 компонентах тех же файлов: имя архива пустое или оканчивается на «.lib», имя элемента печатное.
/// </summary>
/// <remarks>
/// Имя элемента пусто у 261 компонента типа 1 (свет без текстуры) и у 11 компонентов типа 10 (пусто - движок выбирает случайный материал).
/// Архивы: <c>material.lib</c> (материалы) у типов 3, 4, 5, 7, 8, 9, 10 и у 1 компонента типа 1; <c>sounds.lib</c> у типа 2 (199 компонентов).
/// </remarks>
public static class FxidComponentLayout
{
    /// <summary>Размер блока компонента в файле, включая 4 байта <c>TypeAndFlags</c>.</summary>
    /// <param name="type">Тип компонента.</param>
    /// <exception cref="FxidFormatException">Неизвестный тип: движок при этом не продвигается вперёд, поэтому это ошибка формата.</exception>
    public static int GetSize(FxidComponentType type) => type switch
    {
        FxidComponentType.Light => 0xE0,
        FxidComponentType.Sound => 0x94,
        FxidComponentType.AnimatedObject => 0xC8,
        FxidComponentType.AnimatedObjectByDistance => 0xCC,
        FxidComponentType.Trail => 0x70,
        FxidComponentType.Empty => 0x04,
        FxidComponentType.PlaneParticles => 0xD0,
        FxidComponentType.ParticleSpawner => 0xF8,
        FxidComponentType.AnimatedObjectDrawMode => 0xD0,
        FxidComponentType.PolyhedronParticles => 0xD0,
        _ => throw new FxidFormatException($"Unknown FXID component type {(byte)type}")
    };

    /// <summary>
    /// Смещение 0x40-байтной ссылки <see cref="ResourceNamedRef"/> внутри компонента или -1, если у типа ссылки нет.
    /// У типов 4 и 9 после ссылки остаются дополнительные байты (4 и 8 соответственно).
    /// </summary>
    /// <param name="type">Тип компонента.</param>
    public static int GetResourceRefOffset(FxidComponentType type) => type switch
    {
        FxidComponentType.Light => 0xA0,
        FxidComponentType.Sound => 0x54,
        FxidComponentType.AnimatedObject => 0x88,
        FxidComponentType.AnimatedObjectByDistance => 0x88,
        FxidComponentType.Trail => 0x30,
        FxidComponentType.Empty => -1,
        FxidComponentType.PlaneParticles => 0x90,
        FxidComponentType.ParticleSpawner => 0xB8,
        FxidComponentType.AnimatedObjectDrawMode => 0x88,
        FxidComponentType.PolyhedronParticles => 0x90,
        _ => -1
    };

    /// <summary>
    /// Размер данных после ссылки на ресурс. Тип 4: один float (в файлах 150/300/500/700, расстояние до камеры);
    /// тип 9: два dword (в файлах значения 0/1, режим и флаг отрисовки).
    /// </summary>
    /// <param name="type">Тип компонента.</param>
    public static int GetTrailingSize(FxidComponentType type)
    {
        var refOffset = GetResourceRefOffset(type);
        return refOffset < 0 ? 0 : GetSize(type) - refOffset - ResourceNamedRef.Size;
    }
}
