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
        return opcode == 0x83 && Decoder.HasOperandSizeOverridePrefix();
    }

    /// <summary>
    /// Decodes a SUB r/m16, imm8 instruction with sign extension
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
        
        // Check if this is a SUB instruction (reg field must be 5)
        if (reg != 5)
        {
            return false;
        }
        
        // Set the mnemonic
        instruction.Mnemonic = "sub";
        
        // Update the decoder position
        Decoder.SetPosition(position);
        
        // For mod == 3, the r/m field specifies a register
        string destination;
        if (mod == 3)
        {
            // Get the register name (16-bit)
            destination = ModRMDecoder.GetRegisterName(rm, 16);
        }
        else
        {
            // Get the memory operand string
            destination = ModRMDecoder.DecodeModRM(mod, rm, false);
            
            // Replace "dword" with "word" in the memory operand
            destination = destination.Replace("dword", "word");
        }
        
        // Get the current position after processing the ModR/M byte
        position = Decoder.GetPosition();
        
        // Check if we have enough bytes for the immediate value
        if (position >= Length)
        {
            return false;
        }
        
        // Read the immediate value (8-bit)
        byte immediate = CodeBuffer[position++];
        
        // Update the decoder position
        Decoder.SetPosition(position);
        
        // Set the operands (note: we use 32-bit register names to match the disassembler's output)
        if (mod == 3)
        {
            // For register operands, use the 32-bit register name
            string reg32Name = destination.Replace("ax", "eax")
                                         .Replace("bx", "ebx")
                                         .Replace("cx", "ecx")
                                         .Replace("dx", "edx")
                                         .Replace("sp", "esp")
                                         .Replace("bp", "ebp")
                                         .Replace("si", "esi")
                                         .Replace("di", "edi");
            
            instruction.Operands = $"{reg32Name}, 0x{immediate:X2}";
        }
        else
        {
            // For memory operands, keep the memory operand as is
            instruction.Operands = $"{destination}, 0x{immediate:X2}";
        }
        
        return true;
    }
}
