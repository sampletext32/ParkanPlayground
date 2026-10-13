namespace ControlLib;

/// <summary>
/// Файл управления CTL (тип NRes <c>CTLD</c>): корневые лимиты движения, граф состояний, привязки деталей, предметы и списки команд.
/// Игра читает его в <c>CControlBase</c> как пятую ссылку записи агента. Раскладка (по порядку в файле):
/// заголовок (5 dword), корневые параметры (0x6C байт), состояния, матрица весов рёбер (N × N float), привязки деталей (0x30 байт каждая),
/// записи предметов, таблица из 21 индекса списков команд, списки команд (по 0x64 байта на команду).
/// Описание относится к первой части игры (449 файлов); файлы второй части (36 шт., папка <c>is2_bases</c>) имеют иную раскладку и не разбираются.
/// </summary>
/// <param name="SourceName">Имя исходного файла или <c>null</c>, если читалось из потока.</param>
/// <param name="Header">Заголовок с числом записей каждого вида.</param>
/// <param name="RootParams">Корневые лимиты движения объекта.</param>
/// <param name="StateRecords">Узлы графа состояний управления (навигационного графа осей): в данных 692 узла, у 304 файлов графа нет.</param>
/// <param name="EdgeWeightMatrix">Матрица весов рёбер между состояниями, <c>N × N</c> float по строкам: <c>[to * N + from]</c> - вес ребра <c>from -&gt; to</c> (N = число состояний; до 17161 значений при N = 131). Значения: 0 - принудительное продолжение, 1 - разрешённый переход, 1e6 - перехода нет; игра умножает на неё расстояние скоростей (<c>BuildWeightedAdjacencyMatrix</c>).</param>
/// <param name="PieceBindings">Привязки деталей (628 в данных, до 37 на файл).</param>
/// <param name="ItemRecords">Записи предметов (679 в данных, до 22 на файл).</param>
/// <param name="CommandTable">Таблица хуков: индексы списков команд по слотам.</param>
/// <param name="CommandLists">Списки команд (491 в данных, 1271 команда).</param>
public sealed record CtlFile(
    string? SourceName,
    CtlHeader Header,
    CtlRootControlParams RootParams,
    IReadOnlyList<CtlStateRecordRaw> StateRecords,
    IReadOnlyList<float> EdgeWeightMatrix,
    IReadOnlyList<CtlPieceBindingRecordRaw> PieceBindings,
    IReadOnlyList<CtlItemRecordRaw> ItemRecords,
    ControlCommandListIndexTable CommandTable,
    IReadOnlyList<ControlCommandListRaw> CommandLists);
