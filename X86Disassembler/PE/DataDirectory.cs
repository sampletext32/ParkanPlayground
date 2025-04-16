namespace X86Disassembler.PE;
/// <summary>
/// Represents a data directory in the optional header
/// </summary>
public class DataDirectory
{
    public uint VirtualAddress;   // RVA of the table
    public uint Size;             // Size of the table
}
