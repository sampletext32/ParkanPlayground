using X86Disassembler.Analysers;
using X86Disassembler.PE;
using X86Disassembler.ProjectSystem;
using X86Disassembler.X86;

namespace X86Disassembler;

/// <summary>
/// Main program class
/// </summary>
public class Program
{
    // Hardcoded file path for testing
    private const string FilePath = @"C:\Program Files (x86)\Nikita\Iron Strategy\Terrain.dll";

    /// <summary>
    /// Main entry point
    /// </summary>
    /// <param name="args">Command line arguments</param>
    public static void Main(string[] args)
    {
        Console.WriteLine("X86 Disassembler and Decompiler");
        Console.WriteLine("--------------------------------");

        // Load the file
        Console.WriteLine($"Loading file: {FilePath}");
        byte[] fileBytes = File.ReadAllBytes(FilePath);
        Console.WriteLine($"Successfully loaded {FilePath}");
        Console.WriteLine($"File size: {fileBytes.Length} bytes\n");

        // Parse the PE format
        Console.WriteLine("Parsing PE format...\n");
        PeFile peFile = new PeFile(fileBytes);
        peFile.Parse();

        // Print PE file information
        Console.WriteLine("PE File Information:");
        Console.WriteLine($"Architecture: {(peFile.OptionalHeader.Is64Bit() ? "64-bit" : "32-bit")}");
        Console.WriteLine($"Entry Point: 0x{peFile.OptionalHeader.AddressOfEntryPoint:X8}");
        Console.WriteLine($"Image Base: 0x{peFile.OptionalHeader.ImageBase:X8}");
        Console.WriteLine($"Number of Sections: {peFile.FileHeader.NumberOfSections}");
        Console.WriteLine();
        
        // Print section information
        PrintPeSections(peFile);

        // Print export information
        PrintPeExports(peFile);

        // Print import information
        PrintPeImports(peFile);

        var projectPeFile = new ProjectPeFile()
        {
            ImageBase = new VirtualAddress(0, peFile.OptionalHeader.ImageBase),
            Architecture = peFile.OptionalHeader.Is64Bit()
                ? "64-bit"
                : "32-bit",
            Name = Path.GetFileName(FilePath),
            EntryPointAddress = new FileAbsoluteAddress(peFile.OptionalHeader.AddressOfEntryPoint, peFile.OptionalHeader.ImageBase)
        };

        // Find code sections
        var codeSections = peFile.SectionHeaders.FindAll(s => s.ContainsCode());
        Console.WriteLine($"Found {codeSections.Count} code section(s):");
        foreach (var section in codeSections)
        {
            Console.WriteLine($"  - {section.Name}: Size={section.VirtualSize} bytes, RVA=0x{section.VirtualAddress:X8}");
        }

        Console.WriteLine();

        var projectPeFileSections = peFile.SectionHeaders.Select(
            x => new ProjectPeFileSection()
            {
                Name = x.Name,
                Flags = (x.ContainsCode() ? SectionFlags.Code : SectionFlags.None) |
                        (x.IsReadable() ? SectionFlags.Read : SectionFlags.None) | 
                        (x.IsWritable() ? SectionFlags.Write : SectionFlags.None) | 
                        (x.IsExecutable() ? SectionFlags.Exec : SectionFlags.None) ,
                VirtualAddress = new VirtualAddress(x.VirtualAddress, peFile.OptionalHeader.ImageBase),
                Size = x.VirtualSize
            }
        ).ToList();

        // Disassemble the first code section
        if (codeSections.Count > 0)
        {
            var section = codeSections[0];
            byte[] codeBytes = peFile.GetSectionData(peFile.SectionHeaders.IndexOf(section));

            var disassembler = new BlockDisassembler(codeBytes, section.VirtualAddress);

            var asmFunction = disassembler.DisassembleFromAddress(peFile.OptionalHeader.AddressOfEntryPoint);
            Console.WriteLine(asmFunction);
        }

        // Console.WriteLine("\nPress Enter to exit...");
        // Console.ReadLine();
    }

    private static void PrintPeImports(PeFile peFile)
    {
        Console.WriteLine("Imported Functions:");
        Console.WriteLine($"Number of Imported DLLs: {peFile.ImportDescriptors.Count}");

        foreach (var import in peFile.ImportDescriptors)
        {
            Console.WriteLine($"  DLL: {import.DllName}");

            for (int i = 0; i < import.Functions.Count; i++)
            {
                var function = import.Functions[i];
                if (function.IsOrdinal)
                {
                    Console.WriteLine($"    {i}: Ordinal {function.Ordinal}");
                }
                else
                {
                    Console.WriteLine($"    {i}: {function.Name} (Hint={function.Hint})");
                }
            }
        }

        Console.WriteLine();
    }

    private static void PrintPeExports(PeFile peFile)
    {
        Console.WriteLine("Exported Functions:");
        Console.WriteLine($"DLL Name: {peFile.ExportDirectory.DllName}");
        Console.WriteLine($"Number of Functions: {peFile.ExportDirectory.NumberOfFunctions}");
        Console.WriteLine($"Number of Names: {peFile.ExportDirectory.NumberOfNames}");

        for (int i = 0; i < peFile.ExportedFunctions.Count; i++)
        {
            var export = peFile.ExportedFunctions[i];
            Console.WriteLine($"  {i}: {export.Name} (Ordinal={export.Ordinal}, RVA=0x{export.AddressRva:X8})");
        }

        Console.WriteLine();
    }

    private static void PrintPeSections(PeFile peFile)
    {
        Console.WriteLine("Sections:");
        foreach (var section in peFile.SectionHeaders)
        {
            string flags = "";
            if (section.ContainsCode()) flags += "Code ";
            if (section.IsExecutable()) flags += "Exec ";
            if (section.IsReadable()) flags += "Read ";
            if (section.IsWritable()) flags += "Write";

            Console.WriteLine($"  {peFile.SectionHeaders.IndexOf(section)}: {section.Name,-8} VA=0x{section.VirtualAddress:X8} Size={section.VirtualSize,-8} [{flags}]");
        }

        Console.WriteLine();
    }
}