namespace X86Disassembler.X86;

/// <summary>
/// Represents a decoded x86 instruction
/// </summary>
public class Instruction
{
    /// <summary>
    /// The address of the instruction in memory
    /// </summary>
    public ulong Address { get; set; }
    
    /// <summary>
    /// The raw bytes of the instruction
    /// </summary>
    public byte[] Bytes { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// The mnemonic of the instruction (e.g., "mov", "add", "jmp")
    /// </summary>
    public string Mnemonic { get; set; } = string.Empty;
    
    /// <summary>
    /// The operands of the instruction as a formatted string
    /// </summary>
    public string Operands { get; set; } = string.Empty;
    
    /// <summary>
    /// The length of the instruction in bytes
    /// </summary>
    public int Length => Bytes.Length;
    
    /// <summary>
    /// Returns a string representation of the instruction
    /// </summary>
    /// <returns>A formatted string representing the instruction</returns>
    public override string ToString()
    {
        return $"{Address:X8}  {BytesToString()}  {Mnemonic} {Operands}".Trim();
    }
    
    /// <summary>
    /// Converts the instruction bytes to a formatted hex string
    /// </summary>
    /// <returns>A formatted hex string of the instruction bytes</returns>
    private string BytesToString()
    {
        return string.Join(" ", Bytes.Select(b => b.ToString("X2")));
    }
}
