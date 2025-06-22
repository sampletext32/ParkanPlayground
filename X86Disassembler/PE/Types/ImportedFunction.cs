namespace X86Disassembler.PE.Types;

/// <summary>
/// Represents an imported function in a PE file
/// </summary>
public class ImportedFunction
{
    public string Name = ""; // Function name
    public ushort Hint; // Hint value
    public bool IsOrdinal; // True if imported by ordinal
    public ushort Ordinal; // Ordinal value (if imported by ordinal)
    public uint ThunkRva; // RVA of the thunk for this function
}