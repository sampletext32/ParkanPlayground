using System.Buffers.Binary;
using NResLib;
using System.Numerics;
using System.Text;

namespace LandscapeExplorer;

public static class Program
{
    private const string MapsDirectory = @"C:\Program Files (x86)\Nikita\Iron Strategy\DATA\MAPS\SC_3";

    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("Parkan 1 Landscape Explorer\n");

        // Get all .map and .msh files in the directory
        var mapFiles = Directory.GetFiles(MapsDirectory, "*.map");
        var mshFiles = Directory.GetFiles(MapsDirectory, "*.msh");

        Console.WriteLine($"Found {mapFiles.Length} .map files and {mshFiles.Length} .msh files in {MapsDirectory}\n");

        // Process .map files
        Console.WriteLine("=== MAP Files Analysis ===\n");
        foreach (var mapFile in mapFiles)
        {
            AnalyzeNResFile(mapFile);
        }

        // Process .msh files
        Console.WriteLine("\n=== MSH Files Analysis ===\n");
        foreach (var mshFile in mshFiles)
        {
            AnalyzeNResFile(mshFile);

            // Perform detailed landscape analysis on MSH files
            AnalyzeLandscapeMeshFile(mshFile);
        }

        Console.WriteLine("\nAnalysis complete.");
    }

    /// <summary>
    /// Analyzes an NRes file and displays its structure
    /// </summary>
    /// <param name="filePath">Path to the NRes file</param>
    private static void AnalyzeNResFile(string filePath)
    {
        Console.WriteLine($"Analyzing file: {Path.GetFileName(filePath)}");

        var parseResult = NResParser.ReadFile(filePath);

        if (parseResult.Error != null)
        {
            Console.WriteLine($"  Error: {parseResult.Error}");
            return;
        }

        var archive = parseResult.Archive!;

        Console.WriteLine($"  Header: {archive.Header.NRes}, Version: {archive.Header.Version:X}, Files: {archive.Header.FileCount}, Size: {archive.Header.TotalFileLengthBytes} bytes");

        // Group files by type for better analysis
        var filesByType = archive.Files.GroupBy(f => f.FileType);

        foreach (var group in filesByType)
        {
            Console.WriteLine($"  File Type: {group.Key}, Count: {group.Count()}");

            // Display details of the first file of each type as an example
            var example = group.First();
            Console.WriteLine($"    Example: {example.FileName}");
            Console.WriteLine($"      Elements: {example.ElementCount}, Element Size: {example.ElementSize} bytes");
            Console.WriteLine($"      File Length: {example.FileLength} bytes, Offset: {example.OffsetInFile}");

            // If this is a landscape-related file, provide more detailed analysis
            if (IsLandscapeRelatedType(group.Key))
            {
                AnalyzeLandscapeData(example, filePath);
            }
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Determines if a file type is related to landscape data
    /// </summary>
    private static bool IsLandscapeRelatedType(string fileType)
    {
        // Based on the Landscape constructor analysis, these types might be related to landscape
        return fileType == "LAND" || fileType == "TERR" || fileType == "MSH0" ||
               fileType == "MESH" || fileType == "MATR" || fileType == "TEXT";
    }

    /// <summary>
    /// Analyzes landscape-specific data in a file
    /// </summary>
    private static void AnalyzeLandscapeData(ListMetadataItem item, string filePath)
    {
        Console.WriteLine($"      [Landscape Data Analysis]:");

        // Read the file data for this specific item
        using var fs = new FileStream(filePath, FileMode.Open);
        fs.Seek(item.OffsetInFile, SeekOrigin.Begin);

        var buffer = new byte[Math.Min(item.FileLength, 256)]; // Read at most 256 bytes for analysis
        fs.Read(buffer, 0, buffer.Length);

        // Display some basic statistics based on the file type
        if (item.FileType == "LAND" || item.FileType == "TERR")
        {
            Console.WriteLine($"      Terrain data with {item.ElementCount} elements");
            // If element size is known, we can calculate grid dimensions
            if (item.ElementCount > 0 && item.ElementSize > 0)
            {
                // Assuming square terrain, which is common in games from this era
                var gridSize = Math.Sqrt(item.ElementCount);
                if (Math.Abs(gridSize - Math.Round(gridSize)) < 0.001) // If it's close to a whole number
                {
                    Console.WriteLine($"      Terrain grid size: {Math.Round(gridSize)} x {Math.Round(gridSize)}");
                }
            }
        }
        else if (item.FileType == "MSH0" || item.FileType == "MESH")
        {
            // For mesh data, try to estimate vertex/face counts
            Console.WriteLine($"      Mesh data, possibly with vertices and faces");

            // Common sizes: vertices are often 12 bytes (3 floats), faces are often 12 bytes (3 indices)
            if (item.ElementSize == 12)
            {
                Console.WriteLine($"      Possibly {item.ElementCount} vertices or faces");
            }
        }

        // Display first few bytes as hex for debugging
        var hexPreview = BitConverter.ToString(
                buffer.Take(32)
                    .ToArray()
            )
            .Replace("-", " ");
        Console.WriteLine($"      Data preview (hex): {hexPreview}...");
    }

    /// <summary>
    /// Performs a detailed analysis of a landscape mesh file
    /// </summary>
    /// <param name="filePath">Path to the MSH file</param>
    private static void AnalyzeLandscapeMeshFile(string filePath)
    {
        Console.WriteLine($"\nDetailed Landscape Analysis for: {Path.GetFileName(filePath)}\n");

        var parseResult = NResParser.ReadFile(filePath);
        if (parseResult.Error != null || parseResult.Archive == null)
        {
            Console.WriteLine($"  Error analyzing file: {parseResult.Error}");
            return;
        }

        var archive = parseResult.Archive;

        // Based on the Landscape constructor and the file analysis, we can identify specific sections
        // File types in MSH files appear to be numeric values (01, 02, 03, etc.)

        // First, let's extract all the different data sections
        var sections = new Dictionary<string, (ListMetadataItem Meta, byte[] Data)>();

        foreach (var item in archive.Files)
        {
            using var fs = new FileStream(filePath, FileMode.Open);
            fs.Seek(item.OffsetInFile, SeekOrigin.Begin);

            var buffer = new byte[item.FileLength];
            fs.Read(buffer, 0, buffer.Length);

            sections[item.FileType] = (item, buffer);
        }

        // Now analyze each section based on what we know from the Landscape constructor
        Console.WriteLine("  Landscape Structure Analysis:");

        // Type 01 appears to be basic landscape information (possibly header/metadata)
        if (sections.TryGetValue("01 00 00 00", out var section01))
        {
            Console.WriteLine($"  Section 01: Basic Landscape Info");
            Console.WriteLine($"    Elements: {section01.Meta.ElementCount}, Element Size: {section01.Meta.ElementSize} bytes");
            Console.WriteLine($"    Total Size: {section01.Meta.FileLength} bytes");

            // Try to extract some basic info if the format is as expected
            if (section01.Meta.ElementSize == 38 && section01.Data.Length >= 38)
            {
                // This is speculative based on common terrain formats
                var width = BitConverter.ToInt32(section01.Data, 0);
                var height = BitConverter.ToInt32(section01.Data, 4);
                Console.WriteLine($"    Possible Dimensions: {width} x {height}");
            }
        }

        // Type 03 appears to be vertex data (based on element size of 12 bytes which is typical for 3D vertices)
        if (sections.TryGetValue("03 00 00 00", out var section03))
        {
            Console.WriteLine($"\n  Section 03: Vertex Data");
            Console.WriteLine($"    Vertex Count: {section03.Meta.ElementCount}");
            Console.WriteLine($"    Vertex Size: {section03.Meta.ElementSize} bytes");

            // If we have vertex data in expected format (3 floats per vertex)
            if (section03.Meta.ElementSize == 12 && section03.Data.Length >= 36)
            {
                // Display first 3 vertices as example
                Console.WriteLine("    Sample Vertices:");
                for (int i = 0; i < Math.Min(3, section03.Meta.ElementCount); i++)
                {
                    var offset = i * 12;
                    var x = BitConverter.ToSingle(section03.Data, offset);
                    var y = BitConverter.ToSingle(section03.Data, offset + 4);
                    var z = BitConverter.ToSingle(section03.Data, offset + 8);
                    Console.WriteLine($"      Vertex {i}: ({x}, {y}, {z})");
                }

                // Calculate terrain bounds
                var minX = float.MaxValue;
                var minY = float.MaxValue;
                var minZ = float.MaxValue;
                var maxX = float.MinValue;
                var maxY = float.MinValue;
                var maxZ = float.MinValue;

                for (int i = 0; i < section03.Meta.ElementCount; i++)
                {
                    var offset = i * 12;
                    if (offset + 12 <= section03.Data.Length)
                    {
                        var x = BitConverter.ToSingle(section03.Data, offset);
                        var y = BitConverter.ToSingle(section03.Data, offset + 4);
                        var z = BitConverter.ToSingle(section03.Data, offset + 8);

                        minX = Math.Min(minX, x);
                        minY = Math.Min(minY, y);
                        minZ = Math.Min(minZ, z);
                        maxX = Math.Max(maxX, x);
                        maxY = Math.Max(maxY, y);
                        maxZ = Math.Max(maxZ, z);
                    }
                }

                Console.WriteLine("    Terrain Bounds:");
                Console.WriteLine($"      Min: ({minX}, {minY}, {minZ})");
                Console.WriteLine($"      Max: ({maxX}, {maxY}, {maxZ})");
                Console.WriteLine($"      Dimensions: {maxX - minX} x {maxY - minY} x {maxZ - minZ}");
            }
        }

        // Type 02 might be face/index data for the mesh
        if (sections.TryGetValue("02 00 00 00", out var section02))
        {
            Console.WriteLine($"\n  Section 02: Possible Face/Index Data");
            Console.WriteLine($"    Elements: {section02.Meta.ElementCount}");
            Console.WriteLine($"    Element Size: {section02.Meta.ElementSize} bytes");

            // If element size is divisible by 4 (common for index data)
            if (section02.Meta.ElementSize % 4 == 0 && section02.Data.Length >= 12)
            {
                // Display first triangle as example (assuming 3 indices per triangle)
                Console.WriteLine("    Sample Indices (if this is index data):");
                var indicesPerElement = section02.Meta.ElementSize / 4;
                for (int i = 0; i < Math.Min(1, section02.Meta.ElementCount); i++)
                {
                    Console.Write($"      Element {i}: ");
                    for (int j = 0; j < indicesPerElement; j++)
                    {
                        var offset = i * section02.Meta.ElementSize + j * 4;
                        if (offset + 4 <= section02.Data.Length)
                        {
                            var index = BitConverter.ToInt32(section02.Data, offset);
                            Console.Write($"{index} ");
                        }
                    }

                    Console.WriteLine();
                }
            }
        }

        // Types 04, 05, 12, 0E, 0B might be texture coordinates, normals, colors, etc.
        var otherSections = new[] {"04 00 00 00", "05 00 00 00", "12 00 00 00", "0E 00 00 00", "0B 00 00 00"};
        foreach (var sectionType in otherSections)
        {
            if (sections.TryGetValue(sectionType, out var section))
            {
                Console.WriteLine($"\n  Section {sectionType.Substring(0, 2)}: Additional Mesh Data");
                Console.WriteLine($"    Elements: {section.Meta.ElementCount}");
                Console.WriteLine($"    Element Size: {section.Meta.ElementSize} bytes");

                // If element size is 4 bytes, it could be color data, texture indices, etc.
                if (section.Meta.ElementSize == 4 && section.Data.Length >= 12)
                {
                    Console.WriteLine("    Sample Data (as integers):");
                    for (int i = 0; i < Math.Min(3, section.Meta.ElementCount); i++)
                    {
                        var offset = i * 4;
                        var value = BitConverter.ToInt32(section.Data, offset);
                        Console.WriteLine($"      Element {i}: {value}");
                    }
                }
            }
        }

        // Type 15 might be material or special data (Msh_15 in the decompiled code)
        if (sections.TryGetValue("15 00 00 00", out var section15) && sections.TryGetValue("03 00 00 00", out var vertexSection))
        {
            Console.WriteLine($"\n  Section 15: Special Data (Msh_15 type in decompiled code)");
            Console.WriteLine($"    Elements: {section15.Meta.ElementCount}");
            Console.WriteLine($"    Element Size: {section15.Meta.ElementSize} bytes");

            int count = 0;
            for (var i = 0; i < section15.Data.Length; i += 28)
            {
                var first = BinaryPrimitives.ReadUInt32LittleEndian(section15.Data.AsSpan(i));

                if ((first & 0x20000) != 0)
                {
                    Console.WriteLine($"Found {first}/0x{first:X8} 0x20000 at index {i / 28}. &0x20000={first&0x20000}/0x{first&0x20000:X8} offset: {i:X8}");
                    count++;
                }
            }

            Console.WriteLine($"Total found: {count}");

        }
    }
}