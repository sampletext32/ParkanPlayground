namespace X86Disassembler.PE;

/// <summary>
/// Represents an imported function in a PE file
/// </summary>
public class ImportedFunction
{
    public string Name;         // Function name
    public ushort Hint;         // Hint value
    public bool IsOrdinal;      // True if imported by ordinal
    public ushort Ordinal;      // Ordinal value (if imported by ordinal)
    public uint ThunkRva;       // RVA of the thunk for this function
    
    /// <summary>
    /// Initializes a new instance of the ImportedFunction class
    /// </summary>
    public ImportedFunction()
    {
        // Initialize string field to avoid nullability warning
        Name = string.Empty;
    }
}
