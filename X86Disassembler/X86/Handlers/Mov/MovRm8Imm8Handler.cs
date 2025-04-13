namespace X86Disassembler.X86.Handlers.Mov;

/// <summary>
/// Handler for MOV r/m8, imm8 instruction (0xC6)
/// </summary>
public class MovRm8Imm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the MovRm8Imm8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public MovRm8Imm8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        return opcode == 0xC6;
    }

    /// <summary>
    /// Decodes a MOV r/m8, imm8 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Save the original position for raw bytes calculation
        int startPosition = Decoder.GetPosition();
        int position = startPosition;

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
        
        // MOV r/m8, imm8 only uses reg=0
        if (reg != 0)
        {
            return false;
        }
        
        // Track the bytes needed for this instruction
        int bytesNeeded = 1; // ModR/M byte
        
        // Process SIB byte if needed
        byte sib = 0;
        if (mod != 3 && rm == 4) // SIB byte present
        {
            if (position >= Length)
            {
                return false;
            }
            sib = CodeBuffer[position++];
            bytesNeeded++;
        }
        
        // Handle displacement
        int dispSize = 0;
        if (mod == 0 && rm == 5) // 32-bit displacement
        {
            dispSize = 4;
        }
        else if (mod == 1) // 8-bit displacement
        {
            dispSize = 1;
        }
        else if (mod == 2) // 32-bit displacement
        {
            dispSize = 4;
        }
        
        // Check if we have enough bytes for the displacement
        if (position + dispSize > Length)
        {
            return false;
        }
        
        // Skip over the displacement bytes
        position += dispSize;
        bytesNeeded += dispSize;
        
        // Read the immediate byte
        if (position >= Length)
        {
            return false;
        }
        
        byte imm8 = CodeBuffer[position++];
        bytesNeeded++; // Immediate byte
        
        // Update the decoder position
        Decoder.SetPosition(position);

        // Set the mnemonic
        instruction.Mnemonic = "mov";

        // Use ModRMDecoder to get the operand string
        var modRMDecoder = new ModRMDecoder(CodeBuffer, Decoder, Length);
        
        // Reset the decoder position to after the ModR/M byte
        Decoder.SetPosition(startPosition + 1);
        
        // Get the operand string
        string operand;
        if (mod != 3) // Memory operand
        {
            string memOperand = modRMDecoder.DecodeModRM(mod, rm, false);
            
            // Replace the size prefix with "byte ptr"
            operand = memOperand.Replace("dword ptr", "byte ptr");
        }
        else // Register operand
        {
            operand = GetRegister8(rm);
        }

        // Set the operands
        instruction.Operands = $"{operand}, 0x{imm8:X2}";
        
        // Set the raw bytes
        byte[] rawBytes = new byte[bytesNeeded + 1]; // +1 for opcode
        rawBytes[0] = opcode;
        for (int i = 0; i < bytesNeeded; i++)
        {
            if (startPosition + i < Length)
            {
                rawBytes[i + 1] = CodeBuffer[startPosition + i];
            }
        }
        instruction.RawBytes = rawBytes;

        // Restore the decoder position
        Decoder.SetPosition(position);
        
        return true;
    }
}
