namespace ControlLib;

/// <summary>
/// Роль ссылки по индексу внутри записи агента (для типов BTLU, BULL, WPNS, STAT, INTO, EXTO). Порядок задаёт <c>CAgent::try_load</c> (AniMesh.dll):
/// каждая ссылка - 64 байта (имя архива и имя ресурса).
/// </summary>
public enum AgentRefRole
{
    /// <summary>Ссылка 0 (+0x00): меш MSH (тип NRes <c>MESH</c>); <c>LoadAniMesh</c>.</summary>
    Mesh = 0,

    /// <summary>Ссылка 1 (+0x40): набор материалов WEA (<c>WEAR</c>); <c>LoadMatManager</c>.</summary>
    Wear = 1,

    /// <summary>Ссылка 2 (+0x80): контрольные точки CPT (<c>CTPT</c>); <c>LoadControlSystem</c>.</summary>
    ControlPoints = 2,

    /// <summary>Ссылка 3 (+0xC0): повреждения NDP (<c>NDPR</c>); <c>LoadControlSystem</c>.</summary>
    Damage = 3,

    /// <summary>Ссылка 4 (+0x100): управление CTL (<c>CTLD</c>); <c>LoadControlSystem</c>.</summary>
    Control = 4,

    /// <summary>Ссылка 5 (+0x140): поведение (<c>behpsp.res</c> и <c>chas_*.var</c>; не NRes, читает Behavior.dll через <c>CreateBehaviour</c>). Только у BTLU размером 448 байт.</summary>
    Behavior = 5,

    /// <summary>Ссылка 6 (+0x180): таблица <c>*.tbl</c> из корня игры (<c>CreateManManager</c>). Только у BTLU размером 448 байт;
    /// у 25 записей имя архива испорчено (первый байт 0), значимо только имя ресурса (<c>m1.tbl</c>, <c>m2.tbl</c>, <c>hero.tbl</c>).</summary>
    ManTable = 6,
}
