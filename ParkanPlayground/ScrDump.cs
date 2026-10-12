using System.Text;
using ScrLib;
using VarsetLib;

/// <summary>Выгрузка всех скриптов каталога <c>MISSIONS/SCRIPTS</c> в текстовые дампы (<c>docs/scripts</c>).</summary>
public static class ScrDump
{
    /// <summary>Пишет по <c>.txt</c> на каждый <c>.scr</c> и список переменных <c>varset.txt</c>.</summary>
    /// <param name="scriptsDir">Каталог <c>MISSIONS/SCRIPTS</c> игры.</param>
    /// <param name="outDir">Куда писать дампы.</param>
    public static void Run(string scriptsDir, string outDir)
    {
        Directory.CreateDirectory(outDir);

        var varsetPath = Path.Combine(scriptsDir, "varset.var");
        var items = VarsetParser.Parse(varsetPath);
        var names = items.Select(v => v.Name).ToList();

        var vs = new StringBuilder();
        vs.AppendLine("// varset.var: индекс - тип - имя - значение по умолчанию (индекс = порядок объявления, на него ссылаются инструкции .scr)");
        for (var i = 0; i < items.Count; i++)
        {
            vs.AppendLine($"{i,4}  {items[i].Type,-6} {items[i].Name,-32} {items[i].Value}");
        }

        File.WriteAllText(Path.Combine(outDir, "varset.txt"), vs.ToString(), new UTF8Encoding(false));

        var count = 0;
        foreach (var scrPath in Directory.GetFiles(scriptsDir, "*.scr").OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
        {
            var fmlPath = Path.ChangeExtension(scrPath, ".fml");
            var scr = ScrParser.ReadFile(scrPath);
            var fml = File.Exists(fmlPath) ? FmlParser.ReadFile(fmlPath) : null;
            var text = ScrTextDumper.Dump(Path.GetFileName(scrPath), scr, fml, names);
            File.WriteAllText(Path.Combine(outDir, Path.GetFileNameWithoutExtension(scrPath) + ".txt"), text, new UTF8Encoding(false));
            count++;
        }

        Console.WriteLine($"Записано дампов: {count}, переменных: {items.Count} -> {outDir}");
    }
}
