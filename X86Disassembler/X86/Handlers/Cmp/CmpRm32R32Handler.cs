namespace X86Disassembler.X86.Handlers.Cmp;

/// <summary>
/// Handler for CMP r/m32, r32 instruction (0x39)
/// </summary>
public class CmpRm32R32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CmpRm32R32Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public CmpRm32R32Handler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0x39;
    }
    
    /// <summary>
    /// Decodes a CMP r/m32, r32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = "cmp";
        
        int position = Decoder.GetPosition();
        
        if (position >= Length)
        {
            return false;
        }
        
        // Read the ModR/M byte
        byte modRM = CodeBuffer[position++];
        
        // Extract the fields from the ModR/M byte
        byte mod = (byte)((modRM & 0xC0) >> 6); // Top 2 bits
        byte reg = (byte)((modRM & 0x38) >> 3); // Middle 3 bits
        byte rm = (byte)(modRM & 0x07);         // Bottom 3 bits
        
        // Get the register name for the reg field
        string regName = GetRegister32(reg);
        
        // Handle the different addressing modes
        string rmOperand;
        
        if (mod == 3) // Direct register addressing
        {
            // Get the register name for the r/m field
            rmOperand = GetRegister32(rm);
        }
        else // Memory addressing
        {
            // Handle SIB byte if needed
            if (mod != 3 && rm == 4) // SIB byte present
            {
                if (position >= Length)
                {
                    return false;
                }
                
                byte sib = CodeBuffer[position++];
                
                // Extract the fields from the SIB byte
                byte scale = (byte)((sib & 0xC0) >> 6);
                byte index = (byte)((sib & 0x38) >> 3);
                byte base_ = (byte)(sib & 0x07);
                
                // TODO: Handle SIB byte properly
                rmOperand = $"[complex addressing]";
            }
            else if (mod == 0 && rm == 5) // Displacement only addressing
            {
                if (position + 3 >= Length)
                {
                    return false;
                }
                
                // Read the 32-bit displacement
                uint disp = (uint)(CodeBuffer[position] | 
                                  (CodeBuffer[position + 1] << 8) | 
                                  (CodeBuffer[position + 2] << 16) | 
                                  (CodeBuffer[position + 3] << 24));
                position += 4;
                
                rmOperand = $"[0x{disp:X8}]";
            }
            else // Simple addressing modes
            {
                string baseReg = GetRegister32(rm);
                
                if (mod == 0) // No displacement
                {
                    rmOperand = $"[{baseReg}]";
                }
                else // Displacement
                {
                    uint disp;
                    
                    if (mod == 1) // 8-bit displacement
                    {
                        if (position >= Length)
                        {
                            return false;
                        }
                        
                        // Sign-extend the 8-bit displacement
                        sbyte dispByte = (sbyte)CodeBuffer[position++];
                        disp = (uint)(int)dispByte;
                        
                        // Format the displacement
                        string dispStr = dispByte < 0 ? $"-0x{-dispByte:X2}" : $"0x{dispByte:X2}";
                        rmOperand = $"[{baseReg}+{dispStr}]";
                    }
                    else // 32-bit displacement
                    {
                        if (position + 3 >= Length)
                        {
                            return false;
                        }
                        
                        // Read the 32-bit displacement
                        disp = (uint)(CodeBuffer[position] | 
                                      (CodeBuffer[position + 1] << 8) | 
                                      (CodeBuffer[position + 2] << 16) | 
                                      (CodeBuffer[position + 3] << 24));
                        position += 4;
                        
                        rmOperand = $"[{baseReg}+0x{disp:X8}]";
                    }
                }
            }
        }
        
        // Update the decoder position
        Decoder.SetPosition(position);
        
        // Set the operands
        instruction.Operands = $"{rmOperand}, {regName}";
        
        return true;
    }
}
