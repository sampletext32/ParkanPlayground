using System.Buffers.Binary;
using MaterialLib;
using NResLib;
using ParkanPlayground;

// ========== ANALYZE MATERIALS 72 AND 88 FROM LANDSCAPE 0B ==========

// 1. Load Material.lib
var materialLibPath = @"E:\ParkanUnpacked\Material.lib.nres";
if (!File.Exists(materialLibPath))
{
    // Try alternative path
    materialLibPath = @"C:\Program Files (x86)\Nikita\Iron Strategy\DATA\Material.lib";
}

Console.WriteLine($"Loading Material.lib from: {materialLibPath}");
var matLibResult = NResParser.ReadFile(materialLibPath);

if (matLibResult.Archive is null)
{
    Console.WriteLine($"ERROR loading Material.lib: {matLibResult.Error}");
    Console.WriteLine("Trying to list available .lib files...");
    
    // List what's available
    var dataPath = @"C:\Program Files (x86)\Nikita\Iron Strategy\DATA";
    if (Directory.Exists(dataPath))
    {
        foreach (var f in Directory.GetFiles(dataPath, "*.lib"))
        {
            Console.WriteLine($"  Found: {f}");
        }
    }
    return;
}

Console.WriteLine($"Material.lib loaded: {matLibResult.Archive.Files.Count} files\n");

// 2. Find materials by index (72 and 88)
using var matFs = new FileStream(materialLibPath, FileMode.Open, FileAccess.Read, FileShare.Read);

var targetIds = new[] { 72, 88 };

foreach (var targetId in targetIds)
{
    // Material files are stored with their ID in the archive
    var matEntry = matLibResult.Archive.Files.FirstOrDefault(f => f.DirectoryIndex == targetId);
    
    if (matEntry == null)
    {
        Console.WriteLine($"=== Material {targetId}: NOT FOUND ===\n");
        continue;
    }
    
    Console.WriteLine($"=== Material {targetId} ===");
    Console.WriteLine($"  DirectoryIndex: {matEntry.DirectoryIndex}");
    Console.WriteLine($"  SortIndex: {matEntry.SortIndex}");
    Console.WriteLine($"  Name: {matEntry.FileName}");
    Console.WriteLine($"  ElementCount (Version): {matEntry.ElementCount}");
    Console.WriteLine($"  ElementSize (Magic1): {matEntry.ElementSize}");
    Console.WriteLine($"  Offset: {matEntry.OffsetInFile}");
    Console.WriteLine($"  Data size: {matEntry.ElementCount * matEntry.ElementSize} bytes");
    
    // Parse the material
    matFs.Seek(matEntry.OffsetInFile, SeekOrigin.Begin);
    try
    {
        var material = MaterialParser.ReadFromStream(
            matFs, 
            matEntry.FileName ?? $"MAT_{targetId}", 
            matEntry.ElementCount, 
            matEntry.ElementSize);
        
        Console.WriteLine($"\n  Parsed Material:");
        Console.WriteLine($"    Rendering Type: {material.MaterialRenderingType}");
        Console.WriteLine($"    Supports Bump: {material.SupportsBumpMapping}");
        Console.WriteLine($"    Source Blend: {material.SourceBlendMode}");
        Console.WriteLine($"    Dest Blend: {material.DestBlendMode}");
        Console.WriteLine($"    Stages: {material.Stages.Count}");
        Console.WriteLine($"    Animations: {material.Animations.Count}");
        
        for (int i = 0; i < material.Stages.Count; i++)
        {
            var stage = material.Stages[i];
            Console.WriteLine($"\n    Stage {i}:");
            Console.WriteLine($"      Texture: \"{stage.TextureName}\"");
            Console.WriteLine($"      TextureStageIndex: {stage.TextureStageIndex}");
            Console.WriteLine($"      Diffuse: ({stage.DiffuseR:F2}, {stage.DiffuseG:F2}, {stage.DiffuseB:F2}, {stage.DiffuseA:F2})");
            Console.WriteLine($"      Ambient: ({stage.AmbientR:F2}, {stage.AmbientG:F2}, {stage.AmbientB:F2}, {stage.AmbientA:F2})");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ERROR parsing: {ex.Message}");
    }
    
    Console.WriteLine();
}

// 3. Also show .wea file contents for context
Console.WriteLine("=== WEA FILES (landscape materials) ===");
var weaPath1 = @"C:\Program Files (x86)\Nikita\Iron Strategy\DATA\MAPS\SC_1\Land1.wea";
var weaPath2 = @"C:\Program Files (x86)\Nikita\Iron Strategy\DATA\MAPS\SC_1\Land2.wea";

if (File.Exists(weaPath1))
{
    Console.WriteLine($"\nLand1.wea:");
    foreach (var line in File.ReadAllLines(weaPath1))
        Console.WriteLine($"  {line}");
}

if (File.Exists(weaPath2))
{
    Console.WriteLine($"\nLand2.wea:");
    foreach (var line in File.ReadAllLines(weaPath2))
        Console.WriteLine($"  {line}");
}

// 4. Find materials referenced in .wea by name
Console.WriteLine("\n=== MATERIALS REFERENCED IN WEA ===");
var weaMatNames = new[] { "B_S0", "L04", "L02", "L00", "DEFAULT", "L05", "L03", "L01" };

foreach (var name in weaMatNames)
{
    var searchName = $"MAT0_{name}";
    var found = matLibResult.Archive.Files
        .FirstOrDefault(f => f.FileName?.Contains(searchName, StringComparison.OrdinalIgnoreCase) == true);
    
    if (found != null)
    {
        Console.WriteLine($"  {name,-10} -> DirectoryIndex {found.DirectoryIndex,3}, SortIndex {found.SortIndex,3}: {found.FileName}");
    }
    else
    {
        Console.WriteLine($"  {name,-10} -> NOT FOUND");
    }
}

Console.WriteLine("\nDone!");









