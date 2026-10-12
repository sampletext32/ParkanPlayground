using Common;

namespace ControlLib;

/// <summary>
/// Запись агента (<c>objects.rlb</c>, <c>fortif.rlb</c>): массив 64-байтных ссылок на ресурсы объекта (32 байта имя архива и 32 байта имя ресурса).
/// Раскладка подтверждена по <c>CAgent::try_load</c> и по всем записям <c>objects.rlb</c>. Читает <c>LoadAgent</c> (AniMesh.dll).
/// </summary>
/// <param name="Type">Тип записи (4CC типа NRes), определяет число ссылок и их роли.</param>
/// <param name="Name">Имя записи в архиве (<c>r_b_02</c>, <c>bb_l_01</c>, <c>s_stone_04</c> ...).</param>
/// <param name="Refs">Ссылки в порядке следования; роль задаёт <see cref="AgentRefRole"/> (для FORT и SUNO порядок другой).</param>
public sealed record AgentRecord(AgentRecordType Type, string Name, IReadOnlyList<ResourceNamedRef> Refs)
{
    /// <summary>Размер одной ссылки в байтах.</summary>
    public const int RefSize = ResourceNamedRef.Size;

    /// <summary>Меш MSH (роль <see cref="AgentRefRole.Mesh"/>).</summary>
    public ResourceNamedRef Mesh => Get(AgentRefRole.Mesh);

    /// <summary>Набор материалов WEA (роль <see cref="AgentRefRole.Wear"/>).</summary>
    public ResourceNamedRef Wear => Get(AgentRefRole.Wear);

    /// <summary>Контрольные точки CPT (роль <see cref="AgentRefRole.ControlPoints"/>).</summary>
    public ResourceNamedRef ControlPoints => Get(AgentRefRole.ControlPoints);

    /// <summary>Повреждения NDP (роль <see cref="AgentRefRole.Damage"/>).</summary>
    public ResourceNamedRef Damage => Get(AgentRefRole.Damage);

    /// <summary>Управление CTL (роль <see cref="AgentRefRole.Control"/>).</summary>
    public ResourceNamedRef Control => Get(AgentRefRole.Control);

    /// <summary>Поведение (роль <see cref="AgentRefRole.Behavior"/>), только у BTLU размером 448 байт.</summary>
    public ResourceNamedRef Behavior => Get(AgentRefRole.Behavior);

    /// <summary>Таблица <c>*.tbl</c> (роль <see cref="AgentRefRole.ManTable"/>), только у BTLU размером 448 байт.</summary>
    public ResourceNamedRef ManTable => Get(AgentRefRole.ManTable);

    /// <summary>FORT: ссылка на BTLU-здание (<c>objects.rlb</c>).</summary>
    public ResourceNamedRef FortBuilding => Type == AgentRecordType.Fort ? Refs[0] : default;

    /// <summary>FORT: ссылка на BASE (<c>.bas</c>) в <c>fortif.rlb</c>.</summary>
    public ResourceNamedRef FortBase => Type == AgentRecordType.Fort ? Refs[1] : default;

    private ResourceNamedRef Get(AgentRefRole role) =>
        Type is AgentRecordType.Fort or AgentRecordType.Suno || (int)role >= Refs.Count ? default : Refs[(int)role];

    /// <summary>Разбирает данные записи (после разворота NRes, без заголовка).</summary>
    /// <param name="type">Тип записи.</param>
    /// <param name="name">Имя записи.</param>
    /// <param name="data">Содержимое записи: целое число 64-байтных ссылок.</param>
    /// <exception cref="ControlReadException">Размер не кратен 64 или равен нулю.</exception>
    public static AgentRecord Parse(AgentRecordType type, string name, ReadOnlySpan<byte> data)
    {
        if (data.Length == 0 || data.Length % RefSize != 0)
            throw new ControlReadException($"Agent record '{name}' has invalid size {data.Length} (must be a multiple of {RefSize}).");

        var count = data.Length / RefSize;
        var refs = new List<ResourceNamedRef>(count);
        for (var i = 0; i < count; i++)
            refs.Add(ResourceNamedRef.Read(data.Slice(i * RefSize, RefSize)));

        return new AgentRecord(type, name, refs);
    }
}
