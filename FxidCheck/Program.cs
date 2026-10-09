using Common;
using System.Reflection;
using FxidLib;

// Проверка FxidLib на распакованных FXID: dotnet run --project FxidCheck -- "E:\ParkanUnpacked\effects.rlb"
var dir = args.Length > 0 ? args[0] : @"E:\ParkanUnpacked\effects.rlb";
var files = Directory.GetFiles(dir, "*FXID*.bin", SearchOption.AllDirectories);
Console.WriteLine($"Файлов: {files.Length} ({dir})");

var problems = new List<string>();
var typeCounts = new SortedDictionary<FxidComponentType, int>();
var categories = new SortedDictionary<int, int>();
var lodClasses = new SortedDictionary<int, int>();
var strengthSources = new SortedDictionary<uint, int>();
var unknownFlagBits = new SortedDictionary<uint, int>();
var spaces = new SortedDictionary<string, int>();
const uint knownFlags = 0x1 | 0x2 | 0x4 | 0x8 | 0x10 | 0x20 | 0x40 | 0x80 | 0x100 | 0x200 | 0x400 | 0x800 | 0x1000 | 0x2000 | 0x8000;
var components = 0;
var emptyResources = new SortedDictionary<FxidComponentType, int>();

foreach (var file in files.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
{
    var name = Path.GetFileName(file);
    FxidFile fxid;
    var bytes = File.ReadAllBytes(file);
    try
    {
        fxid = FxidFile.Read(bytes);
    }
    catch (Exception e)
    {
        problems.Add($"{name}: чтение: {e.Message}");
        continue;
    }

    // round-trip: сумма Raw = данные после заголовка
    var raw = fxid.Components.SelectMany(c => c.Raw).ToArray();
    if (!raw.AsSpan().SequenceEqual(bytes.AsSpan(FxidFile.HeaderSize)))
        problems.Add($"{name}: Raw компонентов не совпадает с файлом");

    var h = fxid.Header;
    if (!float.IsFinite(h.Duration) || h.Duration < 0)
        problems.Add($"{name}: Duration = {h.Duration}");
    if (h.SettingsCategory >= 20)
        problems.Add($"{name}: SettingsCategory = {h.SettingsCategory} (ожидалось 0..19)");
    if (h.SettingsLodClass >= 4)
        problems.Add($"{name}: SettingsLodClass = {h.SettingsLodClass} (ожидалось 0..3)");
    Bump(categories, h.SettingsCategory);
    Bump(lodClasses, h.SettingsLodClass);
    Bump(strengthSources, (uint)h.StrengthSource);
    if (!Enum.IsDefined(h.StrengthSource))
        problems.Add($"{name}: StrengthSource = {(uint)h.StrengthSource} не описан в enum");
    var extra = (uint)h.Flags & ~knownFlags;
    if (extra != 0)
        Bump(unknownFlagBits, extra);

    foreach (var c in fxid.Components)
    {
        components++;
        typeCounts[c.Type] = typeCounts.GetValueOrDefault(c.Type) + 1;
        try
        {
            object? typed = c.Type switch
            {
                FxidComponentType.Billboard => c.AsType1(),
                FxidComponentType.Sound => c.AsSound(),
                FxidComponentType.AnimParticle => c.AsType3(),
                FxidComponentType.AnimBillboard => c.AsType4(),
                FxidComponentType.Trail => c.AsTrail(),
                FxidComponentType.Plane or FxidComponentType.Cube => c.AsParticle(),
                FxidComponentType.Model => c.AsSpawner(),
                FxidComponentType.AnimModel => c.AsType9(),
                _ => null
            };
            if (typed != null)
                CheckFinite(name, c, typed, "");

            if (c.Resource is { } r)
            {
                if (!(r.ArchiveName.Length == 0 || r.ArchiveName.EndsWith(".lib", StringComparison.OrdinalIgnoreCase)))
                    problems.Add($"{name}#{c.Index} {c.Type}: странное имя архива '{r.ArchiveName}'");
                // Пустое имя допустимо у света (Billboard) и у Cube (движок берёт случайный материал).
                var emptyAllowed = c.Type is FxidComponentType.Billboard or FxidComponentType.Cube;
                if ((r.ResourceName.Length == 0 && !emptyAllowed) || r.ResourceName.Any(ch => ch < 32 || ch > 126))
                    problems.Add($"{name}#{c.Index} {c.Type}: странное имя ресурса '{r.ResourceName}'");
                if (r.ResourceName.Length == 0)
                    emptyResources[c.Type] = emptyResources.GetValueOrDefault(c.Type) + 1;
            }
        }
        catch (Exception e)
        {
            problems.Add($"{name}#{c.Index} {c.Type}: типизированный разбор: {e.Message}");
        }
    }
}

Console.WriteLine($"Компонентов: {components}");
Console.WriteLine("Типы: " + string.Join(", ", typeCounts.Select(p => $"{p.Key}={p.Value}")));
Console.WriteLine("Категории SettingsId: " + string.Join(", ", categories.Select(p => $"{p.Key}:{p.Value}")));
Console.WriteLine("Классы LOD: " + string.Join(", ", lodClasses.Select(p => $"{p.Key}:{p.Value}")));
Console.WriteLine("StrengthSource: " + string.Join(", ", strengthSources.Select(p => $"{p.Key}:{p.Value}")));
Console.WriteLine("Неизвестные биты флагов: " + (unknownFlagBits.Count == 0 ? "нет" : string.Join(", ", unknownFlagBits.Select(p => $"0x{p.Key:X}:{p.Value}"))));
Console.WriteLine("Пустое имя ресурса: " + string.Join(", ", emptyResources.Select(p => $"{p.Key}={p.Value}")));
Console.WriteLine($"Проблем: {problems.Count}");
// Группируем по тексту без имени файла, чтобы не тонуть в повторах.
foreach (var g in problems.GroupBy(p => string.Join(' ', p[(p.IndexOf(' ') + 1)..].Split(' ').Take(3))).OrderByDescending(g => g.Count()))
{
    Console.WriteLine($"  [{g.Count()}] {g.Key}");
    foreach (var p in g.Take(3))
        Console.WriteLine("      " + p);
}
return problems.Count == 0 ? 0 : 1;

static void Bump<T>(SortedDictionary<T, int> d, T key) where T : notnull => d[key] = d.GetValueOrDefault(key) + 1;

// Рефлексия: все float / Vector3 в типизированной записи должны быть конечными.
void CheckFinite(string fileName, FxidComponent c, object obj, string prefix)
{
    foreach (var p in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
    {
        if (p.GetIndexParameters().Length != 0)
            continue;
        object? v;
        try { v = p.GetValue(obj); }
        catch { continue; }

        switch (v)
        {
            case float f when !float.IsFinite(f):
                problems.Add($"{fileName}#{c.Index} {c.Type}: {prefix}{p.Name} = {f}");
                break;
            case Vector3 vec when !(float.IsFinite(vec.X) && float.IsFinite(vec.Y) && float.IsFinite(vec.Z)):
                problems.Add($"{fileName}#{c.Index} {c.Type}: {prefix}{p.Name} = {vec}");
                break;
            case EffectType3Data or EffectType4Data or EffectType9Data:
                CheckFinite(fileName, c, v, prefix + p.Name + ".");
                break;
        }
    }
}
