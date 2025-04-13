namespace X86Disassembler.X86.Handlers.Sub;

/// <summary>
/// Handler for SUB r/m16, imm8 instruction (0x83 /5 with 0x66 prefix and sign extension)
/// </summary>
public class SubImmFromRm16SignExtendedHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the SubImmFromRm16SignExtendedHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public SubImmFromRm16SignExtendedHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        // Check if the opcode is 0x83 and we have a 0x66 prefix
        if (opcode != 0x83 || !Decoder.HasOperandSizeOverridePrefix())
        {
            return false;
        }
        
        // Check if we have enough bytes to read the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Check if the reg field is 5 (SUB)
        byte modRM = CodeBuffer[Decoder.GetPosition()];
        byte reg = (byte)((modRM & 0x38) >> 3);
        
        return reg == 5; // 5 = SUB
    }

    /// <summary>
    /// Decodes a SUB r/m16, imm8 instruction with sign extension
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "sub";
        
        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Extract the fields from the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();

        // For memory operands, replace "dword" with "word"
        string destination = destOperand;
        if (mod != 3) // Memory operand
        {
            destination = destOperand.Replace("dword", "word");
        }

        // Check if we have enough bytes for the immediate value
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the immediate value (8-bit)
        byte immediate = Decoder.ReadByte();

        // Set the operands
        instruction.Operands = $"{destination}, 0x{immediate:X2}";

        return true;
    }
}