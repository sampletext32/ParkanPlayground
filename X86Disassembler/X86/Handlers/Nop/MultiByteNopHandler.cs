namespace X86Disassembler.X86.Handlers.Nop;

/// <summary>
/// Handler for multi-byte NOP instructions (0x0F 0x1F ...)
/// These are used for alignment and are encoded as NOP operations with specific memory operands
/// </summary>
public class MultiByteNopHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MultiByteNopHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public MultiByteNopHandler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        // Multi-byte NOPs start with 0x0F
        if (opcode != 0x0F)
        {
            return false;
        }
        
        int position = Decoder.GetPosition();
        
        // Check if we have enough bytes to read the second opcode
        if (position >= Length)
        {
            return false;
        }
        
        // Check if the second byte is 0x1F (part of the multi-byte NOP encoding)
        byte secondByte = CodeBuffer[position];
        return secondByte == 0x1F;
    }

    /// <summary>
    /// Decodes a multi-byte NOP instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "nop";
        
        int position = Decoder.GetPosition();
        
        // Skip the second byte (0x1F)
        position++;
        
        // Check if we have enough bytes to read the ModR/M byte
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
        
        // Update the decoder position
        Decoder.SetPosition(position);
        
        // Decode the memory operand
        string memOperand;
        
        if (mod == 3)
        {
            // This is a register operand, which is not a valid multi-byte NOP
            // But we'll handle it anyway
            memOperand = ModRMDecoder.GetRegisterName(rm, 32);
        }
        else
        {
            // Get the memory operand string
            memOperand = ModRMDecoder.DecodeModRM(mod, rm, false);
        }
        
        // Set the operands
        instruction.Operands = memOperand;
        
        return true;
    }
}
