namespace X86Disassembler.PE.Parsers;

/// <summary>
/// Parser for the Optional header of a PE file
/// </summary>
public class OptionalHeaderParser : IParser<OptionalHeader>
{
    // Optional Header Magic values
    private const ushort PE32_MAGIC = 0x10B;           // 32-bit executable
    private const ushort PE32PLUS_MAGIC = 0x20B;       // 64-bit executable
        
    /// <summary>
    /// Parse the Optional header from the binary reader
    /// </summary>
    /// <param name="reader">The binary reader positioned at the start of the Optional header</param>
    /// <returns>The parsed Optional header</returns>
    public OptionalHeader Parse(BinaryReader reader)
    {
        OptionalHeader header = new OptionalHeader();
        bool is64Bit;
            
        // Standard fields
        header.Magic = reader.ReadUInt16();
            
        // Determine if this is a PE32 or PE32+ file
        is64Bit = header.Magic == PE32PLUS_MAGIC;
            
        header.MajorLinkerVersion = reader.ReadByte();
        header.MinorLinkerVersion = reader.ReadByte();
        header.SizeOfCode = reader.ReadUInt32();
        header.SizeOfInitializedData = reader.ReadUInt32();
        header.SizeOfUninitializedData = reader.ReadUInt32();
        header.AddressOfEntryPoint = reader.ReadUInt32();
        header.BaseOfCode = reader.ReadUInt32();
            
        // PE32 has BaseOfData, PE32+ doesn't
        if (!is64Bit)
        {
            header.BaseOfData = reader.ReadUInt32();
        }
            
        // Windows-specific fields
        if (is64Bit)
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
        if (is64Bit)
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
    /// Determines if the PE file is 64-bit based on the Optional header
    /// </summary>
    /// <param name="header">The Optional header</param>
    /// <returns>True if the PE file is 64-bit, false otherwise</returns>
    public bool Is64Bit(OptionalHeader header)
    {
        return header.Magic == PE32PLUS_MAGIC;
    }
}