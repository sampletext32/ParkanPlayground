namespace X86Disassembler.X86.Handlers.Test;

/// <summary>
/// Handler for TEST r/m8, r8 instruction (0x84)
/// </summary>
public class TestRegMem8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the TestRegMem8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public TestRegMem8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0x84;
    }

    /// <summary>
    /// Decodes a TEST r/m8, r8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "test";

        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        // Get the register name for the reg field
        string regOperand = ModRMDecoder.GetRegisterName(reg, 8);
        
        // For direct register addressing (mod == 3), get the r/m register name
        if (mod == 3)
        {
            string rmOperand = ModRMDecoder.GetRegisterName(rm, 8);
            instruction.Operands = $"{rmOperand}, {regOperand}";
        }
        else
        {
            // For memory operands, use the decoded operand string
            instruction.Operands = $"{destOperand}, {regOperand}";
        }

        return true;
    }
}