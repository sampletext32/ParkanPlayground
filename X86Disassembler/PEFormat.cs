using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace X86Disassembler
{
    /// <summary>
    /// Represents a Portable Executable (PE) file format parser
    /// </summary>
    public class PEFormat
    {
        // DOS Header constants
        private const ushort DOS_SIGNATURE = 0x5A4D;       // 'MZ'
        private const uint PE_SIGNATURE = 0x00004550;      // 'PE\0\0'
        
        // Optional Header Magic values
        private const ushort PE32_MAGIC = 0x10B;           // 32-bit executable
        private const ushort PE32PLUS_MAGIC = 0x20B;       // 64-bit executable
        
        // Section characteristics flags
        private const uint IMAGE_SCN_CNT_CODE = 0x00000020;            // Section contains code
        private const uint IMAGE_SCN_MEM_EXECUTE = 0x20000000;         // Section is executable
        private const uint IMAGE_SCN_MEM_READ = 0x40000000;            // Section is readable
        private const uint IMAGE_SCN_MEM_WRITE = 0x80000000;           // Section is writable
        
        // Data directories
        private const int IMAGE_DIRECTORY_ENTRY_EXPORT = 0;             // Export Directory
        private const int IMAGE_DIRECTORY_ENTRY_IMPORT = 1;             // Import Directory
        private const int IMAGE_DIRECTORY_ENTRY_RESOURCE = 2;           // Resource Directory
        private const int IMAGE_DIRECTORY_ENTRY_EXCEPTION = 3;          // Exception Directory
        private const int IMAGE_DIRECTORY_ENTRY_SECURITY = 4;           // Security Directory
        private const int IMAGE_DIRECTORY_ENTRY_BASERELOC = 5;          // Base Relocation Table
        private const int IMAGE_DIRECTORY_ENTRY_DEBUG = 6;              // Debug Directory
        private const int IMAGE_DIRECTORY_ENTRY_ARCHITECTURE = 7;       // Architecture Specific Data
        private const int IMAGE_DIRECTORY_ENTRY_GLOBALPTR = 8;          // RVA of GP
        private const int IMAGE_DIRECTORY_ENTRY_TLS = 9;                // TLS Directory
        private const int IMAGE_DIRECTORY_ENTRY_LOAD_CONFIG = 10;       // Load Configuration Directory
        private const int IMAGE_DIRECTORY_ENTRY_BOUND_IMPORT = 11;      // Bound Import Directory
        private const int IMAGE_DIRECTORY_ENTRY_IAT = 12;               // Import Address Table
        private const int IMAGE_DIRECTORY_ENTRY_DELAY_IMPORT = 13;      // Delay Load Import Descriptors
        private const int IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR = 14;    // COM Runtime descriptor
        
        // PE file data
        private byte[] _fileData;
        
        // Parsed headers
        public DOSHeader DosHeader { get; private set; }
        public FileHeader FileHeader { get; private set; }
        public OptionalHeader OptionalHeader { get; private set; }
        public List<SectionHeader> SectionHeaders { get; private set; }
        public bool Is64Bit { get; private set; }
        
        // Export and Import information
        public ExportDirectory ExportDirectory { get; private set; }
        public List<ExportedFunction> ExportedFunctions { get; private set; }
        public List<ImportDescriptor> ImportDescriptors { get; private set; }
        
        /// <summary>
        /// Parses a PE file from the given byte array
        /// </summary>
        /// <param name="fileData">The raw file data</param>
        public PEFormat(byte[] fileData)
        {
            _fileData = fileData;
            SectionHeaders = new List<SectionHeader>();
            ExportedFunctions = new List<ExportedFunction>();
            ImportDescriptors = new List<ImportDescriptor>();
            Parse();
        }
        
        /// <summary>
        /// Parses the PE file structure
        /// </summary>
        private void Parse()
        {
            using (MemoryStream stream = new MemoryStream(_fileData))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                // Parse DOS header
                DosHeader = ParseDOSHeader(reader);
                
                // Move to PE header
                reader.BaseStream.Seek(DosHeader.e_lfanew, SeekOrigin.Begin);
                
                // Verify PE signature
                uint peSignature = reader.ReadUInt32();
                if (peSignature != PE_SIGNATURE)
                {
                    throw new InvalidDataException("Invalid PE signature");
                }
                
                // Parse File Header
                FileHeader = ParseFileHeader(reader);
                
                // Parse Optional Header
                OptionalHeader = ParseOptionalHeader(reader);
                
                // Parse Section Headers
                for (int i = 0; i < FileHeader.NumberOfSections; i++)
                {
                    SectionHeaders.Add(ParseSectionHeader(reader));
                }
                
                // Parse Export Directory
                if (OptionalHeader.DataDirectories.Length > IMAGE_DIRECTORY_ENTRY_EXPORT && 
                    OptionalHeader.DataDirectories[IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress != 0)
                {
                    ExportDirectory = ParseExportDirectory(reader, OptionalHeader.DataDirectories[IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress);
                    ParseExportedFunctions(reader);
                }
                
                // Parse Import Descriptors
                if (OptionalHeader.DataDirectories.Length > IMAGE_DIRECTORY_ENTRY_IMPORT && 
                    OptionalHeader.DataDirectories[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress != 0)
                {
                    ImportDescriptors = ParseImportDescriptors(reader, OptionalHeader.DataDirectories[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress);
                }
            }
        }
        
        /// <summary>
        /// Parses the DOS header
        /// </summary>
        private DOSHeader ParseDOSHeader(BinaryReader reader)
        {
            DOSHeader header = new DOSHeader();
            
            header.e_magic = reader.ReadUInt16();
            if (header.e_magic != DOS_SIGNATURE)
            {
                throw new InvalidDataException("Invalid DOS signature (MZ)");
            }
            
            header.e_cblp = reader.ReadUInt16();
            header.e_cp = reader.ReadUInt16();
            header.e_crlc = reader.ReadUInt16();
            header.e_cparhdr = reader.ReadUInt16();
            header.e_minalloc = reader.ReadUInt16();
            header.e_maxalloc = reader.ReadUInt16();
            header.e_ss = reader.ReadUInt16();
            header.e_sp = reader.ReadUInt16();
            header.e_csum = reader.ReadUInt16();
            header.e_ip = reader.ReadUInt16();
            header.e_cs = reader.ReadUInt16();
            header.e_lfarlc = reader.ReadUInt16();
            header.e_ovno = reader.ReadUInt16();
            
            header.e_res = new ushort[4];
            for (int i = 0; i < 4; i++)
            {
                header.e_res[i] = reader.ReadUInt16();
            }
            
            header.e_oemid = reader.ReadUInt16();
            header.e_oeminfo = reader.ReadUInt16();
            
            header.e_res2 = new ushort[10];
            for (int i = 0; i < 10; i++)
            {
                header.e_res2[i] = reader.ReadUInt16();
            }
            
            header.e_lfanew = reader.ReadUInt32();
            
            return header;
        }
        
        /// <summary>
        /// Parses the File header
        /// </summary>
        private FileHeader ParseFileHeader(BinaryReader reader)
        {
            FileHeader header = new FileHeader();
            
            header.Machine = reader.ReadUInt16();
            header.NumberOfSections = reader.ReadUInt16();
            header.TimeDateStamp = reader.ReadUInt32();
            header.PointerToSymbolTable = reader.ReadUInt32();
            header.NumberOfSymbols = reader.ReadUInt32();
            header.SizeOfOptionalHeader = reader.ReadUInt16();
            header.Characteristics = reader.ReadUInt16();
            
            return header;
        }
        
        /// <summary>
        /// Parses the Optional header
        /// </summary>
        private OptionalHeader ParseOptionalHeader(BinaryReader reader)
        {
            OptionalHeader header = new OptionalHeader();
            
            // Standard fields
            header.Magic = reader.ReadUInt16();
            
            // Determine if this is a PE32 or PE32+ file
            Is64Bit = header.Magic == PE32PLUS_MAGIC;
            
            header.MajorLinkerVersion = reader.ReadByte();
            header.MinorLinkerVersion = reader.ReadByte();
            header.SizeOfCode = reader.ReadUInt32();
            header.SizeOfInitializedData = reader.ReadUInt32();
            header.SizeOfUninitializedData = reader.ReadUInt32();
            header.AddressOfEntryPoint = reader.ReadUInt32();
            header.BaseOfCode = reader.ReadUInt32();
            
            // PE32 has BaseOfData, PE32+ doesn't
            if (!Is64Bit)
            {
                header.BaseOfData = reader.ReadUInt32();
            }
            
            // Windows-specific fields
            if (Is64Bit)
            {
                header.ImageBase = reader.ReadUInt64();
            }
            else
            {
                header.ImageBase = reader.ReadUInt32();
            }
            
            header.SectionAlignment = reader.ReadUInt32();
            header.FileAlignment = reader.ReadUInt32();
            header.MajorOperatingSystemVersion = reader.ReadUInt16();
            header.MinorOperatingSystemVersion = reader.ReadUInt16();
            header.MajorImageVersion = reader.ReadUInt16();
            header.MinorImageVersion = reader.ReadUInt16();
            header.MajorSubsystemVersion = reader.ReadUInt16();
            header.MinorSubsystemVersion = reader.ReadUInt16();
            header.Win32VersionValue = reader.ReadUInt32();
            header.SizeOfImage = reader.ReadUInt32();
            header.SizeOfHeaders = reader.ReadUInt32();
            header.CheckSum = reader.ReadUInt32();
            header.Subsystem = reader.ReadUInt16();
            header.DllCharacteristics = reader.ReadUInt16();
            
            // Size fields differ between PE32 and PE32+
            if (Is64Bit)
            {
                header.SizeOfStackReserve = reader.ReadUInt64();
                header.SizeOfStackCommit = reader.ReadUInt64();
                header.SizeOfHeapReserve = reader.ReadUInt64();
                header.SizeOfHeapCommit = reader.ReadUInt64();
            }
            else
            {
                header.SizeOfStackReserve = reader.ReadUInt32();
                header.SizeOfStackCommit = reader.ReadUInt32();
                header.SizeOfHeapReserve = reader.ReadUInt32();
                header.SizeOfHeapCommit = reader.ReadUInt32();
            }
            
            header.LoaderFlags = reader.ReadUInt32();
            header.NumberOfRvaAndSizes = reader.ReadUInt32();
            
            // Data directories
            int numDirectories = (int)Math.Min(header.NumberOfRvaAndSizes, 16); // Maximum of 16 directories
            header.DataDirectories = new DataDirectory[numDirectories];
            
            for (int i = 0; i < numDirectories; i++)
            {
                DataDirectory dir = new DataDirectory();
                dir.VirtualAddress = reader.ReadUInt32();
                dir.Size = reader.ReadUInt32();
                header.DataDirectories[i] = dir;
            }
            
            return header;
        }
        
        /// <summary>
        /// Parses a section header
        /// </summary>
        private SectionHeader ParseSectionHeader(BinaryReader reader)
        {
            SectionHeader header = new SectionHeader();
            
            // Read section name (8 bytes)
            byte[] nameBytes = reader.ReadBytes(8);
            // Convert to string, removing any null characters
            header.Name = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0');
            
            header.VirtualSize = reader.ReadUInt32();
            header.VirtualAddress = reader.ReadUInt32();
            header.SizeOfRawData = reader.ReadUInt32();
            header.PointerToRawData = reader.ReadUInt32();
            header.PointerToRelocations = reader.ReadUInt32();
            header.PointerToLinenumbers = reader.ReadUInt32();
            header.NumberOfRelocations = reader.ReadUInt16();
            header.NumberOfLinenumbers = reader.ReadUInt16();
            header.Characteristics = reader.ReadUInt32();
            
            return header;
        }
        
        /// <summary>
        /// Parses the Export Directory
        /// </summary>
        private ExportDirectory ParseExportDirectory(BinaryReader reader, uint rva)
        {
            ExportDirectory directory = new ExportDirectory();
            
            reader.BaseStream.Seek(RvaToOffset(rva), SeekOrigin.Begin);
            
            directory.Characteristics = reader.ReadUInt32();
            directory.TimeDateStamp = reader.ReadUInt32();
            directory.MajorVersion = reader.ReadUInt16();
            directory.MinorVersion = reader.ReadUInt16();
            directory.Name = reader.ReadUInt32();
            directory.Base = reader.ReadUInt32();
            directory.NumberOfFunctions = reader.ReadUInt32();
            directory.NumberOfNames = reader.ReadUInt32();
            directory.AddressOfFunctions = reader.ReadUInt32();
            directory.AddressOfNames = reader.ReadUInt32();
            directory.AddressOfNameOrdinals = reader.ReadUInt32();
            
            // Read the DLL name
            uint dllNameRVA = directory.Name;
            reader.BaseStream.Seek(RvaToOffset(dllNameRVA), SeekOrigin.Begin);
            byte[] dllNameBytes = reader.ReadBytes(256);
            directory.DllName = Encoding.ASCII.GetString(dllNameBytes).TrimEnd('\0');
            
            return directory;
        }
        
        /// <summary>
        /// Parses the Import Descriptors
        /// </summary>
        private List<ImportDescriptor> ParseImportDescriptors(BinaryReader reader, uint rva)
        {
            List<ImportDescriptor> descriptors = new List<ImportDescriptor>();
            
            try
            {
                uint importTableOffset = RvaToOffset(rva);
                reader.BaseStream.Seek(importTableOffset, SeekOrigin.Begin);
                
                int descriptorCount = 0;
                
                while (true)
                {
                    descriptorCount++;
                    
                    // Read the import descriptor
                    uint originalFirstThunk = reader.ReadUInt32();
                    uint timeDateStamp = reader.ReadUInt32();
                    uint forwarderChain = reader.ReadUInt32();
                    uint nameRva = reader.ReadUInt32();
                    uint firstThunk = reader.ReadUInt32();
                    
                    // Check if we've reached the end of the import descriptors
                    if (originalFirstThunk == 0 && nameRva == 0 && firstThunk == 0)
                    {
                        break;
                    }
                    
                    ImportDescriptor descriptor = new ImportDescriptor
                    {
                        OriginalFirstThunk = originalFirstThunk,
                        TimeDateStamp = timeDateStamp,
                        ForwarderChain = forwarderChain,
                        Name = nameRva,
                        FirstThunk = firstThunk,
                        DllName = "Unknown" // Default name in case we can't read it
                    };
                    
                    // Try to read the DLL name
                    try
                    {
                        if (nameRva != 0)
                        {
                            uint nameOffset = RvaToOffset(nameRva);
                            reader.BaseStream.Seek(nameOffset, SeekOrigin.Begin);
                            
                            // Read the null-terminated ASCII string
                            StringBuilder nameBuilder = new StringBuilder();
                            byte b;
                            
                            while ((b = reader.ReadByte()) != 0)
                            {
                                nameBuilder.Append((char)b);
                            }
                            
                            descriptor.DllName = nameBuilder.ToString();
                        }
                    }
                    catch (Exception)
                    {
                        // If we can't read the name, keep the default "Unknown"
                    }
                    
                    // Parse the imported functions
                    ParseImportedFunctions(reader, descriptor);
                    
                    descriptors.Add(descriptor);
                    
                    // Return to the import table to read the next descriptor
                    reader.BaseStream.Seek(importTableOffset + (descriptorCount * 20), SeekOrigin.Begin);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing import descriptors: {ex.Message}");
                // Return whatever descriptors we've managed to parse
            }
            
            return descriptors;
        }
        
        /// <summary>
        /// Parses the imported functions for a given import descriptor
        /// </summary>
        private void ParseImportedFunctions(BinaryReader reader, ImportDescriptor descriptor)
        {
            try
            {
                // Use OriginalFirstThunk if available, otherwise use FirstThunk
                uint thunkRva = descriptor.OriginalFirstThunk != 0 ? descriptor.OriginalFirstThunk : descriptor.FirstThunk;
                
                if (thunkRva == 0)
                {
                    return; // No functions to parse
                }
                
                uint thunkOffset = RvaToOffset(thunkRva);
                int functionCount = 0;
                
                while (true)
                {
                    reader.BaseStream.Seek(thunkOffset + (functionCount * 4), SeekOrigin.Begin);
                    uint thunkData = reader.ReadUInt32();
                    
                    if (thunkData == 0)
                    {
                        break; // End of the function list
                    }
                    
                    ImportedFunction function = new ImportedFunction
                    {
                        ThunkRVA = thunkRva + (uint)(functionCount * 4)
                    };
                    
                    // Check if imported by ordinal (high bit set)
                    if ((thunkData & 0x80000000) != 0)
                    {
                        function.IsOrdinal = true;
                        function.Ordinal = (ushort)(thunkData & 0xFFFF);
                        function.Name = $"Ordinal_{function.Ordinal}";
                    }
                    else
                    {
                        // Imported by name - the thunkData is an RVA to a hint/name structure
                        try
                        {
                            uint hintNameOffset = RvaToOffset(thunkData);
                            reader.BaseStream.Seek(hintNameOffset, SeekOrigin.Begin);
                            
                            // Read the hint (2 bytes)
                            function.Hint = reader.ReadUInt16();
                            
                            // Read the function name (null-terminated ASCII string)
                            StringBuilder nameBuilder = new StringBuilder();
                            byte b;
                            
                            while ((b = reader.ReadByte()) != 0)
                            {
                                nameBuilder.Append((char)b);
                            }
                            
                            function.Name = nameBuilder.ToString();
                            
                            if (string.IsNullOrEmpty(function.Name))
                            {
                                function.Name = $"Function_at_{thunkData:X8}";
                            }
                        }
                        catch (Exception)
                        {
                            function.Name = $"Function_at_{thunkData:X8}";
                        }
                    }
                    
                    descriptor.Functions.Add(function);
                    functionCount++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing imported functions for {descriptor.DllName}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Parses the exported functions using the export directory information
        /// </summary>
        private void ParseExportedFunctions(BinaryReader reader)
        {
            if (ExportDirectory == null)
            {
                return;
            }
            
            // Read the array of function addresses (RVAs)
            uint[] functionRVAs = new uint[ExportDirectory.NumberOfFunctions];
            reader.BaseStream.Seek(RvaToOffset(ExportDirectory.AddressOfFunctions), SeekOrigin.Begin);
            for (int i = 0; i < ExportDirectory.NumberOfFunctions; i++)
            {
                functionRVAs[i] = reader.ReadUInt32();
            }
            
            // Read the array of name RVAs
            uint[] nameRVAs = new uint[ExportDirectory.NumberOfNames];
            reader.BaseStream.Seek(RvaToOffset(ExportDirectory.AddressOfNames), SeekOrigin.Begin);
            for (int i = 0; i < ExportDirectory.NumberOfNames; i++)
            {
                nameRVAs[i] = reader.ReadUInt32();
            }
            
            // Read the array of name ordinals
            ushort[] nameOrdinals = new ushort[ExportDirectory.NumberOfNames];
            reader.BaseStream.Seek(RvaToOffset(ExportDirectory.AddressOfNameOrdinals), SeekOrigin.Begin);
            for (int i = 0; i < ExportDirectory.NumberOfNames; i++)
            {
                nameOrdinals[i] = reader.ReadUInt16();
            }
            
            // Create a dictionary to map ordinals to names
            Dictionary<ushort, string> ordinalToName = new Dictionary<ushort, string>();
            for (int i = 0; i < ExportDirectory.NumberOfNames; i++)
            {
                // Read the function name
                reader.BaseStream.Seek(RvaToOffset(nameRVAs[i]), SeekOrigin.Begin);
                List<byte> nameBytes = new List<byte>();
                byte b;
                while ((b = reader.ReadByte()) != 0)
                {
                    nameBytes.Add(b);
                }
                string name = Encoding.ASCII.GetString(nameBytes.ToArray());
                
                // Map the ordinal to the name
                ordinalToName[nameOrdinals[i]] = name;
            }
            
            // Create the exported functions
            for (ushort i = 0; i < ExportDirectory.NumberOfFunctions; i++)
            {
                uint functionRVA = functionRVAs[i];
                if (functionRVA == 0)
                {
                    continue; // Skip empty entries
                }
                
                ExportedFunction function = new ExportedFunction();
                function.Ordinal = (ushort)(i + ExportDirectory.Base);
                function.Address = functionRVA;
                
                // Check if this function has a name
                if (ordinalToName.TryGetValue(i, out string name))
                {
                    function.Name = name;
                }
                else
                {
                    function.Name = $"Ordinal_{function.Ordinal}";
                }
                
                // Check if this is a forwarder
                uint exportDirStart = OptionalHeader.DataDirectories[IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress;
                uint exportDirEnd = exportDirStart + OptionalHeader.DataDirectories[IMAGE_DIRECTORY_ENTRY_EXPORT].Size;
                
                if (functionRVA >= exportDirStart && functionRVA < exportDirEnd)
                {
                    function.IsForwarder = true;
                    
                    // Read the forwarder string
                    reader.BaseStream.Seek(RvaToOffset(functionRVA), SeekOrigin.Begin);
                    List<byte> forwarderBytes = new List<byte>();
                    byte b;
                    while ((b = reader.ReadByte()) != 0)
                    {
                        forwarderBytes.Add(b);
                    }
                    function.ForwarderName = Encoding.ASCII.GetString(forwarderBytes.ToArray());
                }
                
                ExportedFunctions.Add(function);
            }
        }
        
        /// <summary>
        /// Gets the raw data for a specific section
        /// </summary>
        /// <param name="sectionIndex">Index of the section</param>
        /// <returns>Byte array containing the section data</returns>
        public byte[] GetSectionData(int sectionIndex)
        {
            if (sectionIndex < 0 || sectionIndex >= SectionHeaders.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(sectionIndex));
            }
            
            SectionHeader section = SectionHeaders[sectionIndex];
            byte[] sectionData = new byte[section.SizeOfRawData];
            
            Array.Copy(_fileData, section.PointerToRawData, sectionData, 0, section.SizeOfRawData);
            
            return sectionData;
        }
        
        /// <summary>
        /// Gets the raw data for a section by name
        /// </summary>
        /// <param name="sectionName">Name of the section</param>
        /// <returns>Byte array containing the section data</returns>
        public byte[] GetSectionData(string sectionName)
        {
            for (int i = 0; i < SectionHeaders.Count; i++)
            {
                if (SectionHeaders[i].Name == sectionName)
                {
                    return GetSectionData(i);
                }
            }
            
            throw new ArgumentException($"Section '{sectionName}' not found");
        }
        
        /// <summary>
        /// Checks if a section contains code
        /// </summary>
        /// <param name="section">The section to check</param>
        /// <returns>True if the section contains code, false otherwise</returns>
        public bool IsSectionContainsCode(SectionHeader section)
        {
            return (section.Characteristics & IMAGE_SCN_CNT_CODE) != 0 ||
                   (section.Characteristics & IMAGE_SCN_MEM_EXECUTE) != 0;
        }
        
        /// <summary>
        /// Gets all code sections
        /// </summary>
        /// <returns>List of section indices that contain code</returns>
        public List<int> GetCodeSections()
        {
            List<int> codeSections = new List<int>();
            
            for (int i = 0; i < SectionHeaders.Count; i++)
            {
                if (IsSectionContainsCode(SectionHeaders[i]))
                {
                    codeSections.Add(i);
                }
            }
            
            return codeSections;
        }
        
        /// <summary>
        /// Converts a Relative Virtual Address (RVA) to a file offset
        /// </summary>
        /// <param name="rva">The RVA to convert</param>
        /// <returns>The corresponding file offset</returns>
        public uint RvaToOffset(uint rva)
        {
            if (rva == 0)
            {
                return 0;
            }
            
            foreach (var section in SectionHeaders)
            {
                // Check if the RVA is within this section
                if (rva >= section.VirtualAddress && rva < section.VirtualAddress + section.VirtualSize)
                {
                    // Calculate the offset within the section
                    uint offsetInSection = rva - section.VirtualAddress;
                    
                    // Make sure we don't exceed the raw data size
                    if (offsetInSection < section.SizeOfRawData)
                    {
                        return section.PointerToRawData + offsetInSection;
                    }
                }
            }
            
            // If the RVA is not within any section, it might be in the headers
            if (rva < OptionalHeader.SizeOfHeaders)
            {
                return rva;
            }
            
            throw new ArgumentException($"RVA {rva:X8} is not within any section");
        }
    }
    
    #region PE Format Structures
    
    /// <summary>
    /// DOS Header structure
    /// </summary>
    public class DOSHeader
    {
        public ushort e_magic;       // Magic number ("MZ")
        public ushort e_cblp;        // Bytes on last page of file
        public ushort e_cp;          // Pages in file
        public ushort e_crlc;        // Relocations
        public ushort e_cparhdr;     // Size of header in paragraphs
        public ushort e_minalloc;    // Minimum extra paragraphs needed
        public ushort e_maxalloc;    // Maximum extra paragraphs needed
        public ushort e_ss;          // Initial (relative) SS value
        public ushort e_sp;          // Initial SP value
        public ushort e_csum;        // Checksum
        public ushort e_ip;          // Initial IP value
        public ushort e_cs;          // Initial (relative) CS value
        public ushort e_lfarlc;      // File address of relocation table
        public ushort e_ovno;        // Overlay number
        public ushort[] e_res;       // Reserved words
        public ushort e_oemid;       // OEM identifier
        public ushort e_oeminfo;     // OEM information
        public ushort[] e_res2;      // Reserved words
        public uint e_lfanew;        // File address of new exe header
    }
    
    /// <summary>
    /// File Header structure
    /// </summary>
    public class FileHeader
    {
        public ushort Machine;               // Target machine type
        public ushort NumberOfSections;      // Number of sections
        public uint TimeDateStamp;           // Time stamp
        public uint PointerToSymbolTable;    // File offset of symbol table
        public uint NumberOfSymbols;         // Number of symbols
        public ushort SizeOfOptionalHeader;  // Size of optional header
        public ushort Characteristics;       // Characteristics
    }
    
    /// <summary>
    /// Optional Header structure
    /// </summary>
    public class OptionalHeader
    {
        // Standard fields
        public ushort Magic;                 // Magic number (PE32 or PE32+)
        public byte MajorLinkerVersion;      // Major linker version
        public byte MinorLinkerVersion;      // Minor linker version
        public uint SizeOfCode;              // Size of code section
        public uint SizeOfInitializedData;   // Size of initialized data
        public uint SizeOfUninitializedData; // Size of uninitialized data
        public uint AddressOfEntryPoint;     // Entry point RVA
        public uint BaseOfCode;              // Base of code section
        public uint BaseOfData;              // Base of data section (PE32 only)
        
        // Windows-specific fields
        public dynamic ImageBase;            // Preferred image base (uint for PE32, ulong for PE32+)
        public uint SectionAlignment;        // Section alignment
        public uint FileAlignment;           // File alignment
        public ushort MajorOperatingSystemVersion; // Major OS version
        public ushort MinorOperatingSystemVersion; // Minor OS version
        public ushort MajorImageVersion;     // Major image version
        public ushort MinorImageVersion;     // Minor image version
        public ushort MajorSubsystemVersion; // Major subsystem version
        public ushort MinorSubsystemVersion; // Minor subsystem version
        public uint Win32VersionValue;       // Win32 version value
        public uint SizeOfImage;             // Size of image
        public uint SizeOfHeaders;           // Size of headers
        public uint CheckSum;                // Checksum
        public ushort Subsystem;             // Subsystem
        public ushort DllCharacteristics;    // DLL characteristics
        public dynamic SizeOfStackReserve;   // Size of stack reserve (uint for PE32, ulong for PE32+)
        public dynamic SizeOfStackCommit;    // Size of stack commit (uint for PE32, ulong for PE32+)
        public dynamic SizeOfHeapReserve;    // Size of heap reserve (uint for PE32, ulong for PE32+)
        public dynamic SizeOfHeapCommit;     // Size of heap commit (uint for PE32, ulong for PE32+)
        public uint LoaderFlags;             // Loader flags
        public uint NumberOfRvaAndSizes;     // Number of data directories
        
        // Data directories
        public DataDirectory[] DataDirectories; // Data directories
    }
    
    /// <summary>
    /// Data Directory structure
    /// </summary>
    public class DataDirectory
    {
        public uint VirtualAddress;  // RVA of the directory
        public uint Size;            // Size of the directory
    }
    
    /// <summary>
    /// Section Header structure
    /// </summary>
    public class SectionHeader
    {
        public string Name;                  // Section name
        public uint VirtualSize;             // Virtual size
        public uint VirtualAddress;          // Virtual address (RVA)
        public uint SizeOfRawData;           // Size of raw data
        public uint PointerToRawData;        // File pointer to raw data
        public uint PointerToRelocations;    // File pointer to relocations
        public uint PointerToLinenumbers;    // File pointer to line numbers
        public ushort NumberOfRelocations;   // Number of relocations
        public ushort NumberOfLinenumbers;   // Number of line numbers
        public uint Characteristics;         // Characteristics
    }
    
    #endregion
    
    #region Export and Import Structures
    
    /// <summary>
    /// Export Directory structure
    /// </summary>
    public class ExportDirectory
    {
        public uint Characteristics;
        public uint TimeDateStamp;
        public ushort MajorVersion;
        public ushort MinorVersion;
        public uint Name;                  // RVA to the DLL name
        public string DllName;             // Actual DLL name
        public uint Base;                  // Ordinal base
        public uint NumberOfFunctions;     // Number of exported functions
        public uint NumberOfNames;         // Number of exported names
        public uint AddressOfFunctions;    // RVA to function addresses
        public uint AddressOfNames;        // RVA to function names
        public uint AddressOfNameOrdinals; // RVA to ordinals
    }
    
    /// <summary>
    /// Represents an exported function
    /// </summary>
    public class ExportedFunction
    {
        public string Name;        // Function name
        public uint Address;       // Function RVA
        public ushort Ordinal;     // Function ordinal
        public bool IsForwarder;   // True if this is a forwarder
        public string ForwarderName; // Name of the forwarded function (if IsForwarder is true)
    }
    
    /// <summary>
    /// Import Descriptor structure
    /// </summary>
    public class ImportDescriptor
    {
        public uint OriginalFirstThunk;   // RVA to Import Lookup Table
        public uint TimeDateStamp;
        public uint ForwarderChain;
        public uint Name;                 // RVA to the DLL name
        public string DllName;           // Actual DLL name
        public uint FirstThunk;           // RVA to Import Address Table
        public List<ImportedFunction> Functions; // List of imported functions
        
        public ImportDescriptor()
        {
            Functions = new List<ImportedFunction>();
        }
    }
    
    /// <summary>
    /// Represents an imported function
    /// </summary>
    public class ImportedFunction
    {
        public bool IsOrdinal;    // True if imported by ordinal
        public ushort Ordinal;    // Ordinal value (if IsOrdinal is true)
        public string Name;       // Function name (if IsOrdinal is false)
        public ushort Hint;       // Hint value (if IsOrdinal is false)
        public uint ThunkRVA;     // RVA in the Import Address Table
    }
    
    #endregion
}
