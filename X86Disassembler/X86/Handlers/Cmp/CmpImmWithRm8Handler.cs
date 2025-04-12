namespace X86Disassembler.X86.Handlers.Cmp;

/// <summary>
/// Handler for CMP r/m8, imm8 instruction (0x80 /7)
/// </summary>
public class CmpImmWithRm8Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CmpImmWithRm8Handler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public CmpImmWithRm8Handler(byte[] codeBuffer, InstructionDecoder decoder, int length)
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
        if (opcode != 0x80)
            return false;
            
        // Check if the reg field of the ModR/M byte is 7 (CMP)
        int position = Decoder.GetPosition();
        if (position >= Length)
            return false;
            
        byte modRM = CodeBuffer[position];
        byte reg = (byte)((modRM & 0x38) >> 3);
        
        return reg == 7; // 7 = CMP
    }

    /// <summary>
    /// Decodes a CMP r/m8, imm8 instruction
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
        
        // CMP r/m8, imm8 is encoded as 80 /7
        if (reg != 7)
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
        instruction.Mnemonic = "cmp";

        // Get the operand string
        string operand;
        if (mod != 3) // Memory operand
        {
            string memOperand = ModRMDecoder.DecodeModRM(mod, rm, true);
            
            // Replace the size prefix with "byte ptr"
            if (memOperand.StartsWith("qword ptr "))
            {
                operand = memOperand.Replace("qword ptr ", "byte ptr ");
            }
            else if (memOperand.StartsWith("dword ptr "))
            {
                operand = memOperand.Replace("dword ptr ", "byte ptr ");
            }
            else
            {
                operand = $"byte ptr {memOperand}";
            }
        }
        else // Register operand
        {
            operand = GetRegister8(rm);
        }

        // Set the operands
        instruction.Operands = $"{operand}, 0x{imm8:X2}";

        return true;
    }
}
