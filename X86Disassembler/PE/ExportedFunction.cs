namespace X86Disassembler.PE;

/// <summary>
/// Represents an exported function in a PE file
/// </summary>
public class ExportedFunction
{
    public string Name;           // Function name
    public ushort Ordinal;        // Function ordinal
    public uint AddressRva;       // Function RVA
    public bool IsForwarder;      // True if this is a forwarder
    public string ForwarderName;  // Name of the forwarded function
    
    /// <summary>
    /// Initializes a new instance of the ExportedFunction class
    /// </summary>
    public ExportedFunction()
    {
        // Initialize string fields to avoid nullability warnings
        Name = string.Empty;
        ForwarderName = string.Empty;
    }
}
