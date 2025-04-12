namespace X86Disassembler;

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using X86Disassembler.PE;
using X86Disassembler.X86;

internal class Program
{
    // Path to the DLL file to disassemble
    private const string DllPath = @"C:\Program Files (x86)\Nikita\Iron Strategy\Terrain.dll"; // Example path, replace with your target DLL
    
    // Maximum number of instructions to display per section
    private const int MaxInstructionsToDisplay = 50;
    
    static void Main(string[] args)
    {
        Console.WriteLine("X86 Disassembler and Decompiler");
        Console.WriteLine("--------------------------------");
        
        string filePath = DllPath;
        
        Console.WriteLine($"Loading file: {filePath}");
        
        try
        {
            // Load the file into memory
            byte[] fileBytes = File.ReadAllBytes(filePath);
            Console.WriteLine($"Successfully loaded {filePath}");
            Console.WriteLine($"File size: {fileBytes.Length} bytes");
            Console.WriteLine();
            
            Console.WriteLine("Parsing PE format...");
            Console.WriteLine();
            
            // Parse the PE format
            PEFormat peFormat = new PEFormat(fileBytes);
            if (!peFormat.Parse())
            {
                Console.WriteLine("Failed to parse PE file.");
                return;
            }
            
            // Display PE information
            DisplayPEInfo(peFormat);
            
            // Disassemble code sections
            DisassembleCodeSections(peFormat);
            
            Console.WriteLine();
            Console.WriteLine("Press Enter to exit...");
            Console.Read(); // Use Read instead of ReadKey to avoid errors in redirected console
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
    
    /// <summary>
    /// Displays information about the PE file
    /// </summary>
    /// <param name="peFormat">The PE format object</param>
    private static void DisplayPEInfo(PEFormat peFormat)
    {
        Console.WriteLine("PE File Information:");
        Console.WriteLine($"Architecture: {(peFormat.OptionalHeader.Is64Bit() ? "64-bit" : "32-bit")}");
        Console.WriteLine($"Entry Point: 0x{peFormat.OptionalHeader.AddressOfEntryPoint:X8}");
        Console.WriteLine($"Image Base: 0x{peFormat.OptionalHeader.ImageBase:X8}");
        Console.WriteLine($"Number of Sections: {peFormat.FileHeader.NumberOfSections}");
        
        Console.WriteLine("\nSections:");
        for (int i = 0; i < peFormat.SectionHeaders.Count; i++)
        {
            var section = peFormat.SectionHeaders[i];
            string flags = "";
            
            // Use the section's methods to determine characteristics
            if (section.ContainsCode()) flags += "Code ";
            if (section.IsExecutable()) flags += "Exec ";
            if (section.IsReadable()) flags += "Read ";
            if (section.IsWritable()) flags += "Write";
            
            Console.WriteLine($"  {i}: {section.Name,-8} VA=0x{section.VirtualAddress:X8} Size={section.VirtualSize,-8} [{flags}]");
        }
        
        // Display exported functions
        if (peFormat.ExportDirectory != null)
        {
            Console.WriteLine("\nExported Functions:");
            Console.WriteLine($"DLL Name: {peFormat.ExportDirectory.DllName}");
            Console.WriteLine($"Number of Functions: {peFormat.ExportDirectory.NumberOfFunctions}");
            Console.WriteLine($"Number of Names: {peFormat.ExportDirectory.NumberOfNames}");
            
            for (int i = 0; i < peFormat.ExportedFunctions.Count; i++)
            {
                var function = peFormat.ExportedFunctions[i];
                Console.WriteLine($"  {i}: {function.Name} (Ordinal={function.Ordinal}, RVA=0x{function.Address:X8})");
            }
        }
        
        // Display imported functions
        if (peFormat.ImportDescriptors.Count > 0)
        {
            Console.WriteLine("\nImported Functions:");
            Console.WriteLine($"Number of Imported DLLs: {peFormat.ImportDescriptors.Count}");
            
            for (int i = 0; i < peFormat.ImportDescriptors.Count; i++)
            {
                var descriptor = peFormat.ImportDescriptors[i];
                Console.WriteLine($"  DLL: {descriptor.DllName}");
                
                for (int j = 0; j < descriptor.Functions.Count; j++)
                {
                    var function = descriptor.Functions[j];
                    if (function.IsOrdinal)
                    {
                        Console.WriteLine($"    {j}: Ordinal {function.Ordinal}");
                    }
                    else
                    {
                        Console.WriteLine($"    {j}: {function.Name} (Hint={function.Hint})");
                    }
                }
                
                if (i < peFormat.ImportDescriptors.Count - 1)
                {
                    Console.WriteLine(); // Add a blank line between DLLs for better readability
                }
            }
        }
    }
    
    /// <summary>
    /// Disassembles the code sections of the PE file
    /// </summary>
    /// <param name="peFormat">The PE format object</param>
    private static void DisassembleCodeSections(PEFormat peFormat)
    {
        // Find code sections
        var codeSections = peFormat.SectionHeaders.FindAll(s => s.ContainsCode());
        
        Console.WriteLine($"\nFound {codeSections.Count} code section(s):");
        foreach (var section in codeSections)
        {
            Console.WriteLine($"  - {section.Name}: Size={section.VirtualSize} bytes, RVA=0x{section.VirtualAddress:X8}");
        }
        Console.WriteLine();
        
        // Disassemble each code section
        for (int i = 0; i < peFormat.SectionHeaders.Count; i++)
        {
            var section = peFormat.SectionHeaders[i];
            
            // Skip non-code sections
            if (!section.ContainsCode())
                continue;
                
            Console.WriteLine($"Disassembling section {section.Name} at RVA 0x{section.VirtualAddress:X8}:");
            
            // Get section data using the section index
            byte[] sectionData = peFormat.GetSectionData(i);
            
            // Create a disassembler for this section
            ulong baseAddress = peFormat.OptionalHeader.ImageBase + section.VirtualAddress;
            Disassembler disassembler = new Disassembler(sectionData, baseAddress);
            
            // Disassemble and display instructions
            int count = 0;
            int maxInstructions = MaxInstructionsToDisplay; // Use the constant
            
            while (count < maxInstructions)
            {
                Instruction? instruction = disassembler.DisassembleNext();
                if (instruction == null)
                {
                    break;
                }
                
                // Format the instruction bytes
                StringBuilder bytesStr = new StringBuilder();
                foreach (byte b in instruction.Bytes)
                {
                    bytesStr.Append($"{b:X2} ");
                }
                
                // Format the instruction
                // Calculate the RVA by subtracting the image base
                ulong rva = instruction.Address - peFormat.OptionalHeader.ImageBase;
                string addressStr = $"{rva:X8}";
                string bytesDisplay = bytesStr.ToString().PadRight(20); // Pad to 20 characters
                string operandsStr = string.IsNullOrEmpty(instruction.Operands) ? "" : $" {instruction.Operands}";
                
                Console.WriteLine($"  {addressStr}  {bytesDisplay}  {instruction.Mnemonic}{operandsStr}");
                
                count++;
            }
            
            if (sectionData.Length > count * 10) // If we've only shown a small portion
            {
                Console.WriteLine($"  ... ({sectionData.Length - (count * 10)} more bytes not shown)");
            }
        }
    }
}