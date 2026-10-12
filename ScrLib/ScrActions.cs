namespace ScrLib;

/// <summary>
/// Встроенные функции скриптов (<c>ScrEntryInner.NativeActionIndex</c> у инструкций типа Execute): таблица <c>action_0..action_58</c> в ai.dll
/// (<c>SuperAI::ctor</c>, 0x02461000). Названы по декомпилированному коду и по использованию в 75 .scr
/// (аргументы - переменные Varset, имена и константы в <c>MISSIONS/SCRIPTS/varset.var</c>). Поведение в игре не наблюдалось.
/// Функции без записи в таблице (1, 4, 5, 10, 21, 22, 23, 26, 33, 39, 42, 46, 48, 53, 55) не встречаются ни в одном .scr игры, реверс по ним остановлен.
/// </summary>
public static class ScrActions
{
    public static readonly IReadOnlyDictionary<int, string> Names = new Dictionary<int, string>
    {
        [0] = "BindGlobalSuperAI",
        [2] = "RequestUnitsForProblem",
        [3] = "GetDistributorSurplus",
        [6] = "SetUnitCriticalStatus",
        [7] = "DeleteCurrentProblem",
        [8] = "SetCurrentProblemStatus",
        [9] = "Nop",
        [11] = "CheckBuildLimitForType",
        [12] = "GetCurrentProblemPriority",
        [13] = "FindFreeUnitOfType",
        [14] = "FindNearestUnitOfType",
        [15] = "IssueOrderToUnit",
        [16] = "GetStorageResourceAmount",
        [17] = "GetMineResourceAmount",
        [18] = "FindBuildPlaceNearBase",
        [19] = "GetClanBasePlaceAndId",
        [20] = "RegisterProblemTargetPoint",
        [24] = "DisbandGroup",
        [25] = "CreateGroup",
        [27] = "AddExpression",
        [28] = "IssueOrderToGroup",
        [29] = "GetCurrentProblemField",
        [30] = "SendGameMessage",
        [31] = "CountClanUnitsByClass",
        [32] = "ArealContainsLogicId",
        [34] = "CountOwnUnitsByType",
        [35] = "FindNearestEnemyUnit",
        [36] = "FindWeakestEnemyTarget",
        [37] = "FindNearestOwnBuilding",
        [38] = "CountSummaryClanBF",
        [40] = "FindFreeDepositOrEnemyMine",
        [41] = "StartSelectedResearch",
        [43] = "SelectCheapestScheme",
        [44] = "CalcWeightAroundPoint",
        [45] = "GetProblemAssignedWeight",
        [47] = "FindEnemyNearBase",
        [49] = "CountFreeMinds",
        [50] = "GetUnitFromProblem",
        [51] = "SetEntityFreeOrReserved",
        [52] = "QueryBehaviourByLogicId",
        [54] = "GetUnitOrderState",
        [56] = "IsDistributorFlagSet",
        [57] = "HostCallKind2",
        [58] = "GetOwnBuildingByIndex",
    };

    /// <summary>
    /// Ожидаемые аргументы и результат встроенных функций (по использованию в 75 .scr и коду ai.dll; поведение в игре не наблюдалось).
    /// Аргументы - переменные Varset, перечислены по порядку; у функций без записи роль аргументов не выяснена.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, ScrActionSignature> Signatures = new Dictionary<int, ScrActionSignature>
    {
        [0] = new(["игнорируется"], "true"),
        [2] = new(["тип проблемы PBM_* (по имени переменной ищутся секции <имя>_Start/_Continue)", "приоритет (float)", "число шагов", "убывание шага за тик", "target_x (у ROBOT_NEEDED - класс юнита, у ATTACK_UNIT - LogicID цели)", "target_y (обычно NONE)", "target_extra (критерий SELECT_* или требуемая сила)"], null),
        [3] = new([], "float: остаток распределителя ресурсов"),
        [6] = new(["LogicID юнита", "UNIT_NORMAL / UNIT_CRITICAL"], null),
        [7] = new([], null),
        [8] = new(["ST_SOLVING / ST_SOLVED"], null),
        [9] = new([], null),
        [11] = new(["тип здания / робота"], "лимит достигнут или нет"),
        [12] = new([], "float: приоритет"),
        [13] = new(["UNIT_FREE_UNIT / UNIT_ANY_UNIT", "тип"], "LogicID юнита"),
        [14] = new(["UNIT_ANY_UNIT / UNIT_ANY_CAPTURER / ...", "тип"], "LogicID юнита"),
        [15] = new(["юнит", "приказ ORDER_*", "режим вставки INSERT_ORDER_*", "проблема", "fSuccess", "fSurvive", "fTime", "fIndependence", "способ цели TARGET_BY_*", "цель"], null),
        [16] = new(["BUILDING_STORAGE", "RESOURCE_ORE"], "float: количество ресурса"),
        [17] = new(["BUILDING_MINE", "RESOURCE_LEFT_ORE"], "float: количество ресурса"),
        [18] = new(["X (выход)", "Y (выход; 0, 0 - места нет)"], null),
        [19] = new(["переменная для X базы", "переменная для Y базы", "переменная для индекса клана"], null),
        [20] = new(["проблема", "X", "режим", "значение"], null),
        [24] = new(["номер группы"], null),
        [25] = new(["TAKE_* / CREATE_EMPTY", "число"], "номер группы"),
        [27] = new(["действие ACTION_*", "способ цели EXP_TARGET_BY_*", "цель"], null),
        [28] = new(["группа", "приказ ORDER_*", "... как у IssueOrderToUnit (№15)"], null),
        [29] = new(["номер поля: 0, 1 или 2"], "float / int: поле проблемы"),
        [30] = new(["вид сообщения", "значение"], null),
        [31] = new(["клан", "маска CLASS_*"], "число юнитов"),
        [32] = new(["ареал", "значение"], "true / false"),
        [34] = new(["тип BUILDING_*"], "число"),
        [35] = new(["дальность", "переменная под расстояние"], "LogicID"),
        [36] = new(["радиус (float)", "переменная под минимальный вес"], "LogicID вражеского объекта с минимальным весом вокруг"),
        [37] = new(["переменная под расстояние"], "LogicID"),
        [38] = new(["клан", "FREE_UNITS / ALL_UNITS"], "число"),
        [40] = new(["X (выход)", "Y (выход)", "вес вокруг (выход)"], "ERROR; LogicID вражеского рудника, если свободных месторождений нет"),
        [41] = new(["LogicID здания (института)"], "bool: исследование запущено"),
        [43] = new([], null),
        [44] = new(["радиус (float)", "клан или ERROR (все чужие)", "TARGET_BY_LOGIC_ID / TARGET_BY_PLACE", "LogicID или X", "Y (только для TARGET_BY_PLACE)"], "суммарный вес объектов в радиусе (целое)"),
        [45] = new([], "суммарный вес юнитов, назначенных на текущую проблему"),
        [47] = new(["запас дальности (в данных 0)", "переменная под вес (выход)"], "LogicID вражеского объекта около базы или ERROR"),
        [49] = new([], "число"),
        [50] = new(["ORDER_ROBOT_*"], "LogicID"),
        [51] = new(["LogicID", "TRUE / FALSE"], null),
        [52] = new(["LogicID"], "значение или ошибка"),
        [54] = new(["LogicID"], null),
        [56] = new([], "bool"),
        [57] = new(["значение 1", "значение 2"], null),
        [58] = new(["порядковый номер"], "LogicID"),
    };

    public static string Describe(int scriptIndex) =>
        scriptIndex < 0 ? "-1" : Names.TryGetValue(scriptIndex, out var name) ? $"{scriptIndex}: {name}" : scriptIndex.ToString();
}

/// <summary>Ожидаемые аргументы и результат встроенной функции скрипта.</summary>
/// <param name="Arguments">Роли аргументов по порядку.</param>
/// <param name="Result">Что возвращает функция в переменную результата; null - ничего.</param>
public record ScrActionSignature(string[] Arguments, string? Result);
