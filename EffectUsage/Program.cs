using System.Text.RegularExpressions;
using ControlLib;
using FxidLib;

// Какие эффекты запускают CTL и с какими параметрами.
// dotnet run --project EffectUsage -- "E:\ParkanUnpacked"
// Эффекты ищутся в <dir>\effects.rlb, CTL — во всех подпапках <dir> (*CTLD*.ctl), кроме is2_bases (вторая часть игры).
var root = args.Length > 0 ? args[0] : @"E:\ParkanUnpacked";

// имя элемента (без регистра) -> файл FXID
var fxidFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
foreach (var f in Directory.GetFiles(Path.Combine(root, "effects.rlb"), "*FXID*.bin"))
{
    var m = Regex.Match(Path.GetFileName(f), @"^\d+_FXID_(.*)\.bin$", RegexOptions.IgnoreCase);
    if (m.Success) fxidFiles[m.Groups[1].Value] = f;
}

var spawnCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
var spawnOps = new Dictionary<string, SortedSet<uint>>(StringComparer.OrdinalIgnoreCase);
var ctlFilesPerEffect = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
var startSources = new SortedDictionary<uint, int>();
var failed = new List<string>();
var parsed = 0;

foreach (var path in Directory.GetFiles(root, "*CTLD*.ctl", SearchOption.AllDirectories))
{
    // is2_bases — файлы второй части игры, движок первой части их не поддерживает
    if (path.Contains("is2_bases", StringComparison.OrdinalIgnoreCase)) continue;
    CtlFile ctl;
    try { ctl = ControlResourceReader.ReadCtlFile(path); }
    catch (Exception e) { failed.Add($"{Path.GetRelativePath(root, path)}: {e.Message}"); continue; }
    parsed++;

    foreach (var list in ctl.CommandLists)
    foreach (var cmd in list.Records)
    {
        switch (cmd.Opcode)
        {
            case EControlCommandOpcode.CONTROL_CMD_SPAWN_EFFECT_ON_JOINT_BOUNDS:
            case EControlCommandOpcode.CONTROL_CMD_SPAWN_ORIENTED_EFFECT_FROM_CPTS:
            case EControlCommandOpcode.CONTROL_CMD_SPAWN_EFFECT_ON_OBJECT_BOUNDS:
                var name = cmd.Resource.ResourceName;
                spawnCounts[name] = spawnCounts.GetValueOrDefault(name) + 1;
                if (!spawnOps.TryGetValue(name, out var ops)) spawnOps[name] = ops = new SortedSet<uint>();
                ops.Add((uint)cmd.Opcode);
                if (!ctlFilesPerEffect.TryGetValue(name, out var set)) ctlFilesPerEffect[name] = set = new HashSet<string>();
                set.Add(Path.GetFileName(path));
                break;
            case EControlCommandOpcode.CONTROL_CMD_START_EFFECT:
                startSources[cmd.Arg1] = startSources.GetValueOrDefault(cmd.Arg1) + 1;
                break;
        }
    }
}

Console.WriteLine($"CTL разобрано: {parsed}, не разобрано: {failed.Count}");
foreach (var f in failed.Take(5)) Console.WriteLine($"  {f}");
if (failed.Count > 5) Console.WriteLine($"  ... и ещё {failed.Count - 5}");

Console.WriteLine();
Console.WriteLine("START_EFFECT, источник силы (Arg1): " + string.Join(", ", startSources.Select(p => $"{(EffectStrengthSource)p.Key}({p.Key}) x{p.Value}")));

var missing = spawnCounts.Keys.Where(n => !fxidFiles.ContainsKey(n)).OrderBy(n => n).ToList();
Console.WriteLine($"Эффектов, запускаемых из CTL: {spawnCounts.Count}, не найдено в effects.rlb: {missing.Count}");
foreach (var n in missing) Console.WriteLine($"  нет FXID: {n}");
var unreferenced = fxidFiles.Keys.Count(n => !spawnCounts.ContainsKey(n));
Console.WriteLine($"FXID в effects.rlb: {fxidFiles.Count}, не запускаются командами CTL (используются иначе): {unreferenced}");

Console.WriteLine();
Console.WriteLine("Эффект                          использований  CTL  опкоды  Strength        Duration  компонентов");
foreach (var (name, count) in spawnCounts.OrderByDescending(p => p.Value).ThenBy(p => p.Key))
{
    if (!fxidFiles.TryGetValue(name, out var file)) continue;
    var fxid = FxidFile.Read(File.ReadAllBytes(file));
    var h = fxid.Header;
    Console.WriteLine($"{name,-32}{count,8}{ctlFilesPerEffect[name].Count,8}  {string.Join("/", spawnOps[name]),-6}  {h.StrengthSource,-14}{h.Duration,9:F2}{fxid.Components.Count,8}");
}
