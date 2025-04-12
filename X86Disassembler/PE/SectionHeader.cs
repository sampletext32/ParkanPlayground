namespace X86Disassembler.PE
{
    /// <summary>
    /// Represents a section header in a PE file
    /// </summary>
    public class SectionHeader
    {
        public string Name;                 // Section name
        public uint VirtualSize;           // Virtual size
        public uint VirtualAddress;        // Virtual address
        public uint SizeOfRawData;         // Size of raw data
        public uint PointerToRawData;      // Pointer to raw data
        public uint PointerToRelocations;  // Pointer to relocations
        public uint PointerToLinenumbers;  // Pointer to line numbers
        public ushort NumberOfRelocations; // Number of relocations
        public ushort NumberOfLinenumbers; // Number of line numbers
        public uint Characteristics;       // Characteristics
    }
}
