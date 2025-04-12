namespace X86Disassembler.X86.Handlers.Or;

/// <summary>
/// Handler for OR r/m32, imm8 sign-extended instruction (0x83 /1)
/// </summary>
public class OrImmWithRm32SignExtendedHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the OrImmWithRm32SignExtendedHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public OrImmWithRm32SignExtendedHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0x83;
    }

    /// <summary>
    /// Decodes an OR r/m32, imm8 sign-extended instruction
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
        
        // Extract the fields from the ModR/M byte
        byte mod = (byte)((modRM & 0xC0) >> 6);
        byte reg = (byte)((modRM & 0x38) >> 3);
        byte rm = (byte)(modRM & 0x07);
        
        // Check if this is an OR instruction (reg field = 1)
        if (reg != 1)
        {
            return false;
        }
        
        // Process SIB and displacement bytes if needed
        if (mod != 3 && rm == 4) // SIB byte present
        {
            if (position >= Length)
            {
                return false;
            }
            position++; // Skip SIB byte
        }
        
        // Handle displacement
        if ((mod == 1 && position >= Length) || (mod == 2 && position + 3 >= Length))
        {
            return false;
        }
        
        if (mod == 1) // 8-bit displacement
        {
            position++;
        }
        else if (mod == 2) // 32-bit displacement
        {
            position += 4;
        }
        
        // Read the immediate byte
        if (position >= Length)
        {
            return false;
        }
        
        byte imm8 = CodeBuffer[position++];
        Decoder.SetPosition(position);

        // Set the mnemonic
        instruction.Mnemonic = "or";

        // Get the operand string
        string operand;
        if (mod != 3) // Memory operand
        {
            operand = ModRMDecoder.DecodeModRM(mod, rm, false);
        }
        else // Register operand
        {
            operand = GetRegister32(rm);
        }

        // Set the operands
        instruction.Operands = $"{operand}, {imm8}";

        return true;
    }
}
