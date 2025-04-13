namespace X86Disassembler.X86.Handlers.Dec;

/// <summary>
/// Handler for DEC r32 instructions (0x48-0x4F)
/// </summary>
public class DecRegHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the DecRegHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public DecRegHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        // DEC EAX = 0x48, DEC ECX = 0x49, ..., DEC EDI = 0x4F
        return opcode >= 0x48 && opcode <= 0x4F;
    }

    /// <summary>
    /// Decodes a DEC r32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Calculate the register index (0 for EAX, 1 for ECX, etc.)
        RegisterIndex reg = (RegisterIndex)(opcode - 0x48);
        
        // Set the mnemonic
        instruction.Mnemonic = "dec";
        
        // Set the operand (register name)
        instruction.Operands = ModRMDecoder.GetRegisterName(reg, 32);
        
        return true;
    }
}
