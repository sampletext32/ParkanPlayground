using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using X86Disassembler.PE;
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
        
        // Find code sections
        var codeSections = peFile.SectionHeaders.FindAll(s => s.ContainsCode());
        Console.WriteLine($"Found {codeSections.Count} code section(s):");
        foreach (var section in codeSections)
        {
            Console.WriteLine($"  - {section.Name}: Size={section.VirtualSize} bytes, RVA=0x{section.VirtualAddress:X8}");
        }
        Console.WriteLine();
        
        // Disassemble the first code section
        if (codeSections.Count > 0)
        {
            var section = codeSections[0];
            byte[] codeBytes = peFile.GetSectionData(peFile.SectionHeaders.IndexOf(section));
            
            Console.WriteLine($"Disassembling section {section.Name} at RVA 0x{section.VirtualAddress:X8}:");
            
            // Create a disassembler for the code section
            Disassembler disassembler = new Disassembler(codeBytes, section.VirtualAddress);
            
            // Disassemble all instructions
            var instructions = disassembler.Disassemble();

            var unknownIndex = instructions.FindIndex(
                x => x.ToString()
                    .Contains("??") || x.ToString()
                    .Contains("TODO")
            );
            if (unknownIndex != -1)
            {
                _ = 5;
            }
            
            // Print the first 100 instructions
            int count = Math.Min(100, instructions.Count);
            for (int i = 0; i < count; i++)
            {
                Console.WriteLine(instructions[i]);
            }
            
            // Print a summary of how many more instructions there are
            if (instructions.Count > count)
            {
                Console.WriteLine($"... ({instructions.Count - count} more instructions not shown)");
            }
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