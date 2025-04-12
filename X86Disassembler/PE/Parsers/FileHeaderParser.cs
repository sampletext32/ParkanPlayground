using System.IO;

namespace X86Disassembler.PE.Parsers
{
    /// <summary>
    /// Parser for the File header of a PE file
    /// </summary>
    public class FileHeaderParser : IParser<FileHeader>
    {
        /// <summary>
        /// Parse the File header from the binary reader
        /// </summary>
        /// <param name="reader">The binary reader positioned at the start of the File header</param>
        /// <returns>The parsed File header</returns>
        public FileHeader Parse(BinaryReader reader)
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
    }
}
