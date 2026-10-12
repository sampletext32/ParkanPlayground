using System.Text;
using Common;

namespace ControlLib;

/// <summary>
/// Тип записи агента (4CC типа ресурса в NRes archive objects.rlb / fortif.rlb).
/// Значения совпадают с AgentType в CAgent::try_load (AniMesh.dll).
/// </summary>
public enum AgentRecordType : uint
{
    /// <summary>Юнит (робот / животное / здание-ангар). 7 ссылок (448 B) либо 5 ссылок (320 B) у зданий fortif.rlb.</summary>
    Btlu = 0x554C5442,   // "BTLU"
    /// <summary>Снаряд. 5 ссылок (320 B).</summary>
    Bull = 0x4C4C5542,   // "BULL"
    /// <summary>Оружие (корневое). 5 ссылок (320 B).</summary>
    Wpns = 0x534E5057,   // "WPNS"
    /// <summary>Статический объект. 6 ссылок (384 B), шестая всегда пустая.</summary>
    Stat = 0x54415453,   // "STAT"
    /// <summary>Внутренний элемент (INTO). 5 ссылок (320 B), грузится как приставка к родителю.</summary>
    Into = 0x4F544E49,   // "INTO"
    /// <summary>Внешний элемент (EXTO, турель / орудие / деталь). 5 ссылок (320 B).</summary>
    Exto = 0x4F545845,   // "EXTO"
    /// <summary>Укрепление: ссылка на BTLU-здание и на BASE (.bas). 2 ссылки (128 B).</summary>
    Fort = 0x54524F46,   // "FORT"
    /// <summary>Солнце: ссылка на .sun (system.rlb) и на образ (FXID effects.rlb). 2 ссылки (128 B).</summary>
    Suno = 0x4F4E5553,   // "SUNO"
}

/// <summary>Роль ссылки по индексу внутри записи агента (для типов BTLU/BULL/WPNS/STAT/INTO/EXTO).</summary>
public enum AgentRefRole
{
    Mesh = 0,        // MSH (тип NRes MESH)
    Wear = 1,        // WEA (WEAR, материалы)
    ControlPoints = 2, // CPT (CTPT)
    Damage = 3,      // NDP (NDPR)
    Control = 4,     // CTL (CTLD)
    Behavior = 5,    // behpsp.res / *.var  (только BTLU 448 B; не NRes-ресурс)
    ManTable = 6,    // *.tbl в корне игры (только BTLU 448 B); имя архива испорчено (первый байт = 0)
}

/// <summary>
/// Запись агента: массив 64-байтных ссылок (32 байта имя архива + 32 байта имя ресурса).
/// Раскладка подтверждена по CAgent::try_load и по всем записям objects.rlb.
/// </summary>
public sealed record AgentRecord(AgentRecordType Type, string Name, IReadOnlyList<ResourceNamedRef> Refs)
{
    public const int RefSize = 64;

    public ResourceNamedRef Mesh => Get(AgentRefRole.Mesh);
    public ResourceNamedRef Wear => Get(AgentRefRole.Wear);
    public ResourceNamedRef ControlPoints => Get(AgentRefRole.ControlPoints);
    public ResourceNamedRef Damage => Get(AgentRefRole.Damage);
    public ResourceNamedRef Control => Get(AgentRefRole.Control);
    public ResourceNamedRef Behavior => Get(AgentRefRole.Behavior);
    public ResourceNamedRef ManTable => Get(AgentRefRole.ManTable);

    /// <summary>FORT: ссылка на BTLU-здание (objects.rlb).</summary>
    public ResourceNamedRef FortBuilding => Type == AgentRecordType.Fort ? Refs[0] : default;
    /// <summary>FORT: ссылка на BASE (.bas) в fortif.rlb.</summary>
    public ResourceNamedRef FortBase => Type == AgentRecordType.Fort ? Refs[1] : default;

    private ResourceNamedRef Get(AgentRefRole role) =>
        Type is AgentRecordType.Fort or AgentRecordType.Suno || (int)role >= Refs.Count ? default : Refs[(int)role];

    /// <summary>Разбор данных записи (после NRes-разворота, без заголовка).</summary>
    public static AgentRecord Parse(AgentRecordType type, string name, ReadOnlySpan<byte> data)
    {
        if (data.Length == 0 || data.Length % RefSize != 0)
            throw new ControlReadException($"Agent record '{name}' has invalid size {data.Length} (must be a multiple of {RefSize}).");

        int count = data.Length / RefSize;
        var refs = new List<ResourceNamedRef>(count);
        for (int i = 0; i < count; i++)
        {
            var slice = data.Slice(i * RefSize, RefSize);
            refs.Add(new ResourceNamedRef(ReadCString(slice[..32]), ReadCString(slice[32..])));
        }
        return new AgentRecord(type, name, refs);
    }

    private static string ReadCString(ReadOnlySpan<byte> raw)
    {
        int nul = raw.IndexOf((byte)0);
        return Encoding.ASCII.GetString(nul >= 0 ? raw[..nul] : raw);
    }
}
