namespace X86Disassembler.X86.Handlers.Add;

/// <summary>
/// Handler for ADD r/m32, imm8 (sign-extended) instruction (0x83 /0)
/// </summary>
public class AddImmToRm32SignExtendedHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the AddImmToRm32SignExtendedHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public AddImmToRm32SignExtendedHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        if (opcode != 0x83)
            return false;
            
        // Check if the reg field of the ModR/M byte is 0 (ADD)
        int position = Decoder.GetPosition();
        if (position >= Length)
            return false;
            
        byte modRM = CodeBuffer[position];
        byte reg = (byte)((modRM & 0x38) >> 3);
        
        return reg == 0; // 0 = ADD
    }
    
    /// <summary>
    /// Decodes an ADD r/m32, imm8 (sign-extended) instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Save the original position for raw bytes calculation
        int startPosition = Decoder.GetPosition();
        
        // Set the mnemonic
        instruction.Mnemonic = "add";
        
        if (startPosition >= Length)
        {
            instruction.Operands = "??";
            instruction.RawBytes = new byte[] { opcode };
            return true;
        }
        
        // Read the ModR/M byte
        var (mod, reg, rm, destOperand) = ModRMDecoder.ReadModRM();
        
        // Track the bytes needed for this instruction
        int bytesNeeded = 1; // ModR/M byte
        
        // Process SIB byte if needed
        byte sib = 0;
        if (mod != 3 && rm == RegisterIndex.Si) // SIB byte present
        {
            if (startPosition + bytesNeeded >= Length)
            {
                instruction.Operands = "??";
                instruction.RawBytes = new byte[] { opcode, CodeBuffer[startPosition] };
                return true;
            }
            sib = CodeBuffer[startPosition + bytesNeeded];
            bytesNeeded++; // SIB byte
        }
        
        // Handle displacement
        int dispSize = 0;
        if (mod == 0 && rm == RegisterIndex.Di) // 32-bit displacement
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
        if (startPosition + bytesNeeded + dispSize >= Length)
        {
            instruction.Operands = "??";
            instruction.RawBytes = new byte[] { opcode, CodeBuffer[startPosition] };
            return true;
        }
        
        bytesNeeded += dispSize; // Add displacement bytes
        
        // Set the decoder position to after the ModR/M byte
        Decoder.SetPosition(startPosition + 1);

        // Get the position after decoding the ModR/M byte
        int newPosition = Decoder.GetPosition();
        
        // Read the immediate value
        if (newPosition >= Length)
        {
            instruction.Operands = $"{destOperand}, ??";
            
            // Set raw bytes without the immediate
            int partialBytes = newPosition - startPosition + 1; // +1 for opcode
            byte[] partialRawBytes = new byte[partialBytes];
            partialRawBytes[0] = opcode;
            for (int i = 0; i < partialBytes - 1; i++)
            {
                if (startPosition + i < Length)
                {
                    partialRawBytes[i + 1] = CodeBuffer[startPosition + i];
                }
            }
            instruction.RawBytes = partialRawBytes;
            
            return true;
        }
        
        // Read the immediate value as a signed byte and automatically sign-extend it to int
        int signExtendedImm = (sbyte)Decoder.ReadByte();
        
        // Format the immediate value as a 32-bit hex value
        string immStr;
        if (signExtendedImm < 0)
        {
            // For negative values, use the full 32-bit representation (0xFFFFFFxx)
            immStr = $"0x{(uint)signExtendedImm:X8}";
        }
        else
        {
            // For positive values, use the regular format
            immStr = $"0x{signExtendedImm:X8}";
        }
        
        // Set the operands
        instruction.Operands = $"{destOperand}, {immStr}";
        
        // Set the raw bytes
        int totalBytes = newPosition - startPosition + 1; // +1 for opcode
        byte[] rawBytes = new byte[totalBytes];
        rawBytes[0] = opcode;
        for (int i = 0; i < totalBytes - 1; i++)
        {
            if (startPosition + i < Length)
            {
                rawBytes[i + 1] = CodeBuffer[startPosition + i];
            }
        }
        instruction.RawBytes = rawBytes;
        
        return true;
    }
}
