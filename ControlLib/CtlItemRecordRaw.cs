using Common;

namespace ControlLib;

/// <summary>
/// Запись предмета CTL (<c>CtlRawItem</c>): двигатель, радар, щит, батарея, оружие и так далее. Фабрика <c>LoadCtlItemRecordAndAdvance</c> (Control.dll 0x100102E0)
/// выбирает по <see cref="ItemType"/> класс предмета (<c>CItemType*</c>); значения <see cref="CommonParams"/> (CP0..CP15) читает <c>ResolveItemFloatParam</c>,
/// смысл CP зависит от типа (см. README, §5.7 и §10). В данных 679 предметов в 449 файлах (до 22 на файл).
/// </summary>
/// <param name="Offset">Смещение записи от начала файла (для отладки).</param>
/// <param name="EndOffset">Смещение конца записи (включая индексы привязок и имя).</param>
/// <param name="ItemType">+0x00: тип предмета. В данных: 0x02 орудие - 114, 0x15 дефлектор - 70, 0x08 радар - 66, 0x01 привод башни - 54, 0x04 прицеливание - 50,
/// 0x14 - 48, 0x1B броня - 42, 0x03 - 41, 0x13 батарея - 40, 0x09 щит - 38, 0x0F ремонт - 34, 0x05 двигатель - 33, 0x0A детектор-щит - 30, 0x11 - 8,
/// 0x1E строительное орудие - 6, 0x18 - 4, 0x16 - 1. Остальные значения в данных не встречаются.</param>
/// <param name="LocalPieceIndex">+0x04: локальный индекс детали, к которой привязан предмет.</param>
/// <param name="Flags">+0x08: флаги. Биты 0..7 - режим выхода (0 - интегрирование, иначе номер сигнала), 8..15 - сигнал A, 16..23 - сигнал B,
/// 0x01000000 / 0x02000000 - левая / правая группа колёс (у орудий 0x02000000 = залп всеми стволами), 0x04000000 - инверсия, 0x08000000 - признак привода башни,
/// 0x80000000 - пороговый режим (см. README, §5.5). В данных: 0 - 535 предметов, 1 - 30, 0x04000000 - 28, 0x02000000 - 18, 0x01070C00 и 0x02040C00 - по 14,
/// 0x08000000 - 7, 0x0C000000 - 7, 0x01000000 - 6, 4 - 6, 0x8000000C - 5, 0x03000000 - 3.</param>
/// <param name="Unknown0C">+0x0C: у орудий 0x02/0x1E - номер списка команд такта 3 (в данных −1 у 644 предметов, 1 - 30, 6 - 4, 2 - 1); у 0x19 - номера состояний.</param>
/// <param name="Unknown10">+0x10: не выяснено; во всех 679 предметах −1.</param>
/// <param name="Unknown14">+0x14: не выяснено; во всех 679 предметах −1.</param>
/// <param name="InitialRuntimeState">+0x18: начальное <c>runtime_state</c> предмета (<c>CItemBase::ParseCtlItemRecordAndInit</c>); −1 - оставить по умолчанию.
/// В данных −1 у 675 предметов, 33 - у 4 (тип 0x18).</param>
/// <param name="Param1C">+0x1C: ноль или кратное 12.5 (двигатели 450..1350, радары 175..1150, щиты 200..290, батареи 175..700); <c>CtlRawItem.mobility_contribution</c>:
/// вклад предмета в «массу» подвижности и в сигнатуру 0x45 (<c>InitializeNdpMobilityAccounting</c>). В данных 0..40000, ноль у 517 предметов.</param>
/// <param name="Param20">+0x20: базовая потребляемая мощность в секунду (<c>ITEM_PARAM_BASE_POWER</c>, селектор 0x30); у батареи (0x13) - максимум выдачи. В данных 0..1000.</param>
/// <param name="Param24">+0x24: у орудий копируется в +0x9C; назначение не выяснено. В данных 0..100000, ненулевое у 158 предметов.</param>
/// <param name="SignalAWeight">+0x28: вес управляющего сигнала A (биты флагов 8..15) в <c>CItemBase::method9</c>. Во всех предметах 1.0.</param>
/// <param name="SignalBWeight">+0x2C: вес сигнала B (биты 16..23); колёса используют 0.5 или 1.0. В данных 1.0 - 641 предмет, 0.5 - 38.</param>
/// <param name="CommonParams">+0x30: 16 общих параметров CP0..CP15 (float); смысл зависит от <see cref="ItemType"/>.</param>
/// <param name="Resource">+0x70: ссылка на ресурс: щит - FXID, оружие - BULL. Пуста у 521 предмета.</param>
/// <param name="PieceBindingIndices">Индексы привязок деталей, на которые ссылается предмет (0..36 на предмет).</param>
/// <param name="ItemNameLength">Длина имени предмета; движок читает <c>длина + 1</c> байт. У 449 предметов 0 (без имени).</param>
/// <param name="ItemName">Имя предмета (<c>i_eng_l</c>, <c>i_pws_l</c>, <c>i_dsh_l</c>, <c>i_arm_*</c>, <c>i_rdr_*</c>, <c>i_def_*</c> ...), совпадает с префиксом узла схемы объекта.</param>
public sealed record CtlItemRecordRaw(
    long Offset,
    long EndOffset,
    int ItemType,
    int LocalPieceIndex,
    uint Flags,
    int Unknown0C,
    int Unknown10,
    int Unknown14,
    int InitialRuntimeState,
    float Param1C,
    float Param20,
    float Param24,
    float SignalAWeight,
    float SignalBWeight,
    IReadOnlyList<float> CommonParams,
    ResourceNamedRef Resource,
    IReadOnlyList<int> PieceBindingIndices,
    int ItemNameLength,
    string ItemName);
