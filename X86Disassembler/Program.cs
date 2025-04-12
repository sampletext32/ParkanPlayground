using System;
using System.IO;
using X86Disassembler.PE;

namespace X86Disassembler
{
    internal class Program
    {
        // Path to the DLL file to disassemble
        private const string DllPath = @"C:\Program Files (x86)\Nikita\Iron Strategy\Terrain.dll"; // Example path, replace with your target DLL
        
        static void Main(string[] args)
        {
            Console.WriteLine("X86 Disassembler and Decompiler");
            Console.WriteLine("--------------------------------");
            
            Console.WriteLine($"Loading file: {DllPath}");
            
            // Load the DLL file
            byte[] binaryData = File.ReadAllBytes(DllPath);
            
            Console.WriteLine($"Successfully loaded {DllPath}");
            Console.WriteLine($"File size: {binaryData.Length} bytes");
            
            // Parse the PE format
            Console.WriteLine("\nParsing PE format...");
            PEFormat peFile = new PEFormat(binaryData);
            
            // Display basic PE information
            DisplayPEInfo(peFile);
            
            // Display exported functions
            DisplayExportedFunctions(peFile);
            
            // Display imported functions
            DisplayImportedFunctions(peFile);
            
            // Find code sections for disassembly
            var codeSections = peFile.GetCodeSections();
            Console.WriteLine($"\nFound {codeSections.Count} code section(s):");
            
            foreach (int sectionIndex in codeSections)
            {
                var section = peFile.SectionHeaders[sectionIndex];
                Console.WriteLine($"  - {section.Name}: Size={section.SizeOfRawData} bytes, RVA=0x{section.VirtualAddress:X8}");
                
                // Get the section data for disassembly
                byte[] sectionData = peFile.GetSectionData(sectionIndex);
                
                // TODO: Implement disassembling logic here
                // This is where we would pass the section data to our disassembler
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        
        private static void DisplayPEInfo(PEFormat peFile)
        {
            Console.WriteLine("\nPE File Information:");
            Console.WriteLine($"Architecture: {(peFile.Is64Bit ? "64-bit" : "32-bit")}");
            Console.WriteLine($"Entry Point: 0x{peFile.OptionalHeader.AddressOfEntryPoint:X8}");
            Console.WriteLine($"Image Base: 0x{peFile.OptionalHeader.ImageBase:X}");
            Console.WriteLine($"Number of Sections: {peFile.FileHeader.NumberOfSections}");
            
            // Display section information
            Console.WriteLine("\nSections:");
            for (int i = 0; i < peFile.SectionHeaders.Count; i++)
            {
                var section = peFile.SectionHeaders[i];
                string flags = "";
                
                if ((section.Characteristics & 0x00000020) != 0) flags += "Code "; // IMAGE_SCN_CNT_CODE
                if ((section.Characteristics & 0x20000000) != 0) flags += "Exec "; // IMAGE_SCN_MEM_EXECUTE
                if ((section.Characteristics & 0x40000000) != 0) flags += "Read "; // IMAGE_SCN_MEM_READ
                if ((section.Characteristics & 0x80000000) != 0) flags += "Write"; // IMAGE_SCN_MEM_WRITE
                
                Console.WriteLine($"  {i}: {section.Name,-8} VA=0x{section.VirtualAddress:X8} Size={section.SizeOfRawData,-8} [{flags}]");
            }
        }
        
        private static void DisplayExportedFunctions(PEFormat peFile)
        {
            if (peFile.ExportDirectory == null)
            {
                Console.WriteLine("\nNo exported functions found.");
                return;
            }
            
            Console.WriteLine("\nExported Functions:");
            Console.WriteLine($"DLL Name: {peFile.ExportDirectory.DllName}");
            Console.WriteLine($"Number of Functions: {peFile.ExportDirectory.NumberOfFunctions}");
            Console.WriteLine($"Number of Names: {peFile.ExportDirectory.NumberOfNames}");
            
            // Display the first 10 exported functions (if any)
            int count = Math.Min(10, peFile.ExportedFunctions.Count);
            for (int i = 0; i < count; i++)
            {
                var function = peFile.ExportedFunctions[i];
                Console.WriteLine($"  {i}: {function.Name} (Ordinal={function.Ordinal}, RVA=0x{function.Address:X8})");
            }
            
            if (peFile.ExportedFunctions.Count > 10)
            {
                Console.WriteLine($"  ... and {peFile.ExportedFunctions.Count - 10} more");
            }
        }
        
        private static void DisplayImportedFunctions(PEFormat peFile)
        {
            if (peFile.ImportDescriptors.Count == 0)
            {
                Console.WriteLine("\nNo imported functions found.");
                return;
            }
            
            Console.WriteLine("\nImported Functions:");
            Console.WriteLine($"Number of Imported DLLs: {peFile.ImportDescriptors.Count}");
            
            // Display the first 5 imported DLLs and their functions
            int dllCount = Math.Min(5, peFile.ImportDescriptors.Count);
            for (int i = 0; i < dllCount; i++)
            {
                var descriptor = peFile.ImportDescriptors[i];
                Console.WriteLine($"  DLL: {descriptor.DllName}");
                
                // Display the first 5 functions from this DLL
                int funcCount = Math.Min(5, descriptor.Functions.Count);
                for (int j = 0; j < funcCount; j++)
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
                
                if (descriptor.Functions.Count > 5)
                {
                    Console.WriteLine($"    ... and {descriptor.Functions.Count - 5} more");
                }
            }
            
            if (peFile.ImportDescriptors.Count > 5)
            {
                Console.WriteLine($"  ... and {peFile.ImportDescriptors.Count - 5} more DLLs");
            }
        }
    }
}
