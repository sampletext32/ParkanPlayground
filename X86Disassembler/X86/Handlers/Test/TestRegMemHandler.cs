namespace X86Disassembler.X86.Handlers.Test;

/// <summary>
/// Handler for TEST r/m32, r32 instruction (0x85)
/// </summary>
public class TestRegMemHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the TestRegMemHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public TestRegMemHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0x85;
    }

    /// <summary>
    /// Decodes a TEST r/m32, r32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "test";

        int position = Decoder.GetPosition();

        if (position >= Length)
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        // For direct register addressing (mod == 3), the r/m field specifies a register
        if (mod == 3)
        {
            // Get the register names
            string rmReg = ModRMDecoder.GetRegisterName(rm, 32);
            string regReg = ModRMDecoder.GetRegisterName(reg, 32);

            // Set the operands (TEST r/m32, r32)
            // In x86 assembly, the TEST instruction has the operand order r/m32, r32
            // According to Ghidra and standard x86 assembly convention, it should be TEST ECX,EAX
            // where ECX is the r/m operand and EAX is the reg operand
            instruction.Operands = $"{rmReg}, {regReg}";
        }
        else
        {
            // Get the register name
            string regReg = ModRMDecoder.GetRegisterName(reg, 32);

            // Set the operands (TEST r/m32, r32)
            instruction.Operands = $"{destOperand}, {regReg}";
        }

        return true;
    }
}