namespace X86Disassembler.X86.Handlers.Lea;

/// <summary>
/// Handler for LEA r32, m instruction (0x8D)
/// </summary>
public class LeaR32MHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the LeaR32MHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public LeaR32MHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0x8D;
    }

    /// <summary>
    /// Decodes a LEA r32, m instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        int position = Decoder.GetPosition();

        if (position >= Length)
        {
            return false;
        }

        // Read the ModR/M byte
        byte modRM = CodeBuffer[position++];
        Decoder.SetPosition(position);

        // Extract the fields from the ModR/M byte
        byte mod = (byte)((modRM & 0xC0) >> 6);
        byte reg = (byte)((modRM & 0x38) >> 3);
        byte rm = (byte)(modRM & 0x07);

        // LEA only works with memory operands, not registers
        if (mod == 3)
        {
            return false;
        }

        // Set the mnemonic
        instruction.Mnemonic = "lea";

        // Get the register name
        string regName = GetRegister32(reg);

        // Get the memory operand without the size prefix
        string operand = ModRMDecoder.DecodeModRM(mod, rm, false);
        
        // Remove the "dword ptr" prefix for LEA instructions
        operand = operand.Replace("dword ptr ", "");

        // Set the operands
        instruction.Operands = $"{regName}, {operand}";

        return true;
    }
}
