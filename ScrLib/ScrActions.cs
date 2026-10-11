namespace ScrLib;

/// <summary>
/// Встроенные функции скриптов (<c>ScrEntryInner.ScriptIndex</c> у инструкций типа Execute): таблица <c>action_0..action_58</c> в ai.dll
/// (<c>SuperAI::ctor</c>, 0x02461000). Названы по декомпилированному коду и по использованию в 75 .scr
/// (аргументы - переменные Varset, имена и константы в <c>MISSIONS/SCRIPTS/varset.var</c>). Поведение в игре не наблюдалось.
/// Функции без записи в таблице не разобраны.
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
        [37] = "FindNearestOwnBuilding",
        [38] = "CountSummaryClanBF",
        [49] = "CountFreeMinds",
        [50] = "GetUnitFromProblem",
        [51] = "SetEntityFreeOrReserved",
        [52] = "QueryBehaviourByLogicId",
        [54] = "GetUnitOrderState",
        [56] = "IsDistributorFlagSet",
        [57] = "HostCallKind2",
        [58] = "GetOwnBuildingByIndex",
    };

    public static string Describe(int scriptIndex) =>
        scriptIndex < 0 ? "-1" : Names.TryGetValue(scriptIndex, out var name) ? $"{scriptIndex}: {name}" : scriptIndex.ToString();
}
