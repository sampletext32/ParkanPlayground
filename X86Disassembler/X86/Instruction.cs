namespace X86Disassembler.X86;

using System.Collections.Generic;

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
    /// Gets or sets the type of the instruction
    /// </summary>
    public InstructionType Type { get; set; } = InstructionType.Unknown;

    /// <summary>
    /// Gets or sets the structured operands of the instruction
    /// </summary>
    public List<Operand> StructuredOperands { get; set; } = [];
    
    /// <summary>
    /// Returns a string representation of the instruction
    /// </summary>
    /// <returns>A string representation of the instruction</returns>
    public override string ToString()
    {
        return $"{Address:X8} {Type:G} {string.Join(",", StructuredOperands)}";
    }
}
