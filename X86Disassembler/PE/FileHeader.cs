namespace X86Disassembler.PE;

/// <summary>
/// Represents the File header of a PE file
/// </summary>
public class FileHeader
{
    public ushort Machine;               // Target machine type
    public ushort NumberOfSections;      // Number of sections
    public uint TimeDateStamp;           // Time and date stamp
    public uint PointerToSymbolTable;    // File pointer to COFF symbol table
    public uint NumberOfSymbols;         // Number of symbols
    public ushort SizeOfOptionalHeader;  // Size of optional header
    public ushort Characteristics;       // Characteristics
}