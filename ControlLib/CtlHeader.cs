namespace ControlLib;

/// <summary>Заголовок CTL (20 байт): числа записей каждого вида, по которым читаются последующие блоки.</summary>
/// <param name="ControlStateCount">Число состояний управления (узлов графа). В данных: 0 - 304 файла, 1 - 131, 5 - 5, 3 - 2, до 131 (11 различных значений).</param>
/// <param name="ControlStateLinkSlotCount">Число звеньев (<see cref="NavNodeLinkSlotRaw"/>) у каждого состояния; одинаково для всех состояний файла.
/// В данных: 0 - 367 файлов, 1 - 68, 4 - 6, 2 - 4, 6 - 3, 3 - 1.</param>
/// <param name="PieceBindingCount">Число привязок деталей. В данных от 0 до 37 на файл.</param>
/// <param name="ItemRecordCount">Число записей предметов. В данных от 0 до 22 на файл.</param>
/// <param name="CommandListCount">Число списков команд. В данных: 0 - 248 файлов, 1 - 87, 4 - 46, 2 - 30, 3 - 23, 7 - 8, всего 491.</param>
public readonly record struct CtlHeader(
    uint ControlStateCount,
    uint ControlStateLinkSlotCount,
    uint PieceBindingCount,
    uint ItemRecordCount,
    uint CommandListCount);
