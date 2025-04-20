namespace X86Disassembler.PE.Types;

/// <summary>
/// Represents an exported function in a PE file
/// </summary>
public class ExportedFunction
{
    public string Name = ""; // Function name
    public ushort Ordinal; // Function ordinal
    public uint AddressRva; // Function RVA
    public bool IsForwarder; // True if this is a forwarder
    public string ForwarderName = ""; // Name of the forwarded function
}