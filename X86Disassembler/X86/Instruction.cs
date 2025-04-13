namespace X86Disassembler.X86;

/// <summary>
/// Represents an x86 instruction
/// </summary>
public class Instruction
{
    /// <summary>
    /// Gets or sets the address of the instruction
    /// </summary>
    public ulong Address { get; set; }
    
    /// <summary>
    /// Gets or sets the mnemonic of the instruction
    /// </summary>
    public string Mnemonic { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the operands of the instruction
    /// </summary>
    public string Operands { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the raw bytes of the instruction
    /// </summary>
    public byte[] RawBytes { get; set; } = [];
    
    /// <summary>
    /// Returns a string representation of the instruction
    /// </summary>
    /// <returns>A string representation of the instruction</returns>
    public override string ToString()
    {
        // Format the address
        string addressStr = $"{Address:X8}";
        
        // Format the raw bytes
        string bytesStr = string.Empty;
        foreach (byte b in RawBytes)
        {
            bytesStr += $"{b:X2} ";
        }
        
        // Pad the bytes string to a fixed width
        bytesStr = bytesStr.PadRight(30);
        
        // Format the instruction
        string instructionStr = Mnemonic;
        if (!string.IsNullOrEmpty(Operands))
        {
            instructionStr += " " + Operands;
        }
        
        return $"  {addressStr}  {bytesStr}{instructionStr}";
    }
}
