using System.IO;
using System.Text;

namespace X86Disassembler.PE.Parsers
{
    /// <summary>
    /// Parser for section headers in a PE file
    /// </summary>
    public class SectionHeaderParser : IParser<SectionHeader>
    {
        /// <summary>
        /// Parse a section header from the binary reader
        /// </summary>
        /// <param name="reader">The binary reader positioned at the start of the section header</param>
        /// <returns>The parsed section header</returns>
        public SectionHeader Parse(BinaryReader reader)
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
        /// Checks if a section contains code
        /// </summary>
        /// <param name="section">The section to check</param>
        /// <returns>True if the section contains code, false otherwise</returns>
        public bool IsSectionContainsCode(SectionHeader section)
        {
            const uint IMAGE_SCN_CNT_CODE = 0x00000020;     // Section contains code
            const uint IMAGE_SCN_MEM_EXECUTE = 0x20000000;   // Section is executable
            
            return (section.Characteristics & IMAGE_SCN_CNT_CODE) != 0 ||
                   (section.Characteristics & IMAGE_SCN_MEM_EXECUTE) != 0;
        }
    }
}
