using System.Buffers.Binary;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TextureDecoder;

var folder = "C:\\Projects\\CSharp\\ParkanPlayground\\ParkanPlayground\\bin\\Debug\\net8.0\\ui.lib";

var files = Directory.EnumerateFiles(folder);

List<TextureFile> textureFiles = [];

foreach (var file in files)
{
    try
    {
        var fs = new FileStream(file, FileMode.Open);

        var textureFile = TextureFile.ReadFromStream(fs, file);

        textureFiles.Add(textureFile);

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