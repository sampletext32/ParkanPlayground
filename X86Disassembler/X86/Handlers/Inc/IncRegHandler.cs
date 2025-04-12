namespace X86Disassembler.X86.Handlers.Inc;

/// <summary>
/// Handler for INC r32 instructions (0x40-0x47)
/// </summary>
public class IncRegHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the IncRegHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public IncRegHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
        : base(codeBuffer, decoder, length)
    {
    }

    /// <summary>
    /// Checks if this handler can decode the given opcode
    /// </summary>
    /// <param name="opcode">The opcode to check</param>
    /// <returns>True if this handler can decode the opcode</returns>
    public override bool CanHandle(byte opcode)
    {
        // INC EAX = 0x40, INC ECX = 0x41, ..., INC EDI = 0x47
        return opcode >= 0x40 && opcode <= 0x47;
    }

    /// <summary>
    /// Decodes an INC r32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Calculate the register index (0 for EAX, 1 for ECX, etc.)
        byte reg = (byte)(opcode - 0x40);
        
        // Set the mnemonic
        instruction.Mnemonic = "inc";
        
        // Set the operand (register name)
        instruction.Operands = GetRegister32(reg);
        
        return true;
    }
}
