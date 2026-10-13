using RsLiLib;

// Распаковка NL-архивов («RsLi», gamefont.rlb, sprites.lib): NLUnpacker <архив> [папка для файлов]

if (args.Length == 0)
{
    Console.WriteLine("Использование: NLUnpacker <файл NL> [папка вывода]");
    return 1;
}

var archive = RsLiArchive.Read(args[0]);
Console.WriteLine($"Записей: {archive.Entries.Count}, отсортирована по имени: {archive.IsNameSorted}");

string? outputDirectory = args.Length > 1 ? args[1] : null;
if (outputDirectory != null)
    Directory.CreateDirectory(outputDirectory);

foreach (var entry in archive.Entries)
{
    Console.WriteLine(
        $"{entry.Index,3} {entry.Name,-14} {entry.Method,-9} упаковано {entry.PackedSize,8} -> {entry.UnpackedSize,8}  смещение 0x{entry.DataOffset:X}");

    if (outputDirectory == null)
        continue;

    try
    {
        File.WriteAllBytes(Path.Combine(outputDirectory, entry.Name), archive.Extract(entry));
    }
    catch (Exception ex) when (ex is InvalidDataException or IOException)
    {
        Console.WriteLine($"    ошибка: {ex.Message}");
    }
}

return 0;
