using System.Buffers.Binary;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TexmLib;

var folder = "C:\\Projects\\CSharp\\ParkanPlayground\\ParkanPlayground\\bin\\Debug\\net8.0\\ui.lib";

var files = Directory.EnumerateFiles(folder);

List<TexmFile> textureFiles = [];

foreach (var file in files)
{
    try
    {
        var fs = new FileStream(file, FileMode.Open);

        var parseResult = TexmParser.ReadFromStream(fs, file);

        textureFiles.Add(parseResult.TexmFile);

        Console.WriteLine($"Successfully read: {file}");
    }
    catch
    {
        Console.WriteLine($"Failed read: {file}");
    }
}

foreach (var textureFile in textureFiles)
{
    await textureFile.WriteToFolder("unpacked");
    Console.WriteLine($"Unpacked {Path.GetFileName(textureFile.FileName)} into folder");
}