using System.IO;

namespace X86Disassembler.PE.Parsers
{
    /// <summary>
    /// Interface for PE format component parsers
    /// </summary>
    /// <typeparam name="T">The type of component to parse</typeparam>
    public interface IParser<T>
    {
        /// <summary>
        /// Parse a component from the binary reader
        /// </summary>
        /// <param name="reader">The binary reader positioned at the start of the component</param>
        /// <returns>The parsed component</returns>
        T Parse(BinaryReader reader);
    }
}
