namespace X86Disassembler.PE
{
    /// <summary>
    /// Represents a section header in a PE file
    /// </summary>
    public class SectionHeader
    {
        // Section characteristics flags
        private const uint IMAGE_SCN_CNT_CODE = 0x00000020;     // Section contains code
        private const uint IMAGE_SCN_MEM_EXECUTE = 0x20000000;   // Section is executable
        private const uint IMAGE_SCN_MEM_READ = 0x40000000;      // Section is readable
        private const uint IMAGE_SCN_MEM_WRITE = 0x80000000;     // Section is writable
        
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
        
        /// <summary>
        /// Checks if the section contains code
        /// </summary>
        /// <returns>True if the section contains code, false otherwise</returns>
        public bool ContainsCode()
        {
            return (Characteristics & IMAGE_SCN_CNT_CODE) != 0 ||
                   (Characteristics & IMAGE_SCN_MEM_EXECUTE) != 0;
        }
        
        /// <summary>
        /// Checks if the section is readable
        /// </summary>
        /// <returns>True if the section is readable, false otherwise</returns>
        public bool IsReadable()
        {
            return (Characteristics & IMAGE_SCN_MEM_READ) != 0;
        }
        
        /// <summary>
        /// Checks if the section is writable
        /// </summary>
        /// <returns>True if the section is writable, false otherwise</returns>
        public bool IsWritable()
        {
            return (Characteristics & IMAGE_SCN_MEM_WRITE) != 0;
        }
        
        /// <summary>
        /// Checks if the section is executable
        /// </summary>
        /// <returns>True if the section is executable, false otherwise</returns>
        public bool IsExecutable()
        {
            return (Characteristics & IMAGE_SCN_MEM_EXECUTE) != 0;
        }
    }
}
