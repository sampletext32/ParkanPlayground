namespace X86Disassembler.X86.Handlers.FloatingPoint;

/// <summary>
/// Handler for floating-point load, store, and control operations (D9 opcode)
/// </summary>
public class LoadStoreControlHandler : FloatingPointBaseHandler
{
    // D9 opcode - load, store, and control operations
    private static readonly string[] Mnemonics =
    [
        "fld",
        "??",
        "fst",
        "fstp",
        "fldenv",
        "fldcw",
        "fnstenv",
        "fnstcw"
    ];
    
    /// <summary>
    /// Initializes a new instance of the LoadStoreControlHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public LoadStoreControlHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
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
        return opcode == 0xD9;
    }
    
    /// <summary>
    /// Decodes a floating-point instruction for load, store, and control operations
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
        
        // Set the mnemonic based on the opcode and reg field
        instruction.Mnemonic = Mnemonics[reg];
        
        // For memory operands, set the operand
        if (mod != 3) // Memory operand
        {
            string operand = ModRMDecoder.DecodeModRM(mod, rm, false);
            
            // Different operand types based on the instruction
            if (reg == 0 || reg == 2 || reg == 3) // fld, fst, fstp
            {
                // Keep the dword ptr prefix from ModRMDecoder
                instruction.Operands = operand;
            }
            else // fldenv, fldcw, fnstenv, fnstcw
            {
                if (reg == 5) // fldcw - should use word ptr
                {
                    instruction.Operands = operand.Replace("dword ptr", "word ptr");
                }
                else // fldenv, fnstenv, fnstcw
                {
                    // Remove the dword ptr prefix for other control operations
                    instruction.Operands = operand.Replace("dword ptr ", "");
                }
            }
        }
        else // Register operand (ST(i))
        {
            // Special handling for D9C0-D9FF (register-register operations)
            if (reg == 0) // FLD ST(i)
            {
                instruction.Operands = $"st({rm})";
            }
            else if (reg == 1) // FXCH ST(i)
            {
                instruction.Mnemonic = "fxch";
                instruction.Operands = $"st({rm})";
            }
            else if (reg == 4)
            {
                // D9E0-D9EF special instructions
                switch (rm)
                {
                    case 0:
                        instruction.Mnemonic = "fchs";
                        instruction.Operands = "";
                        break;
                    case 1:
                        instruction.Mnemonic = "fabs";
                        instruction.Operands = "";
                        break;
                    case 4:
                        instruction.Mnemonic = "ftst";
                        instruction.Operands = "";
                        break;
                    case 5:
                        instruction.Mnemonic = "fxam";
                        instruction.Operands = "";
                        break;
                    default:
                        instruction.Mnemonic = "??";
                        instruction.Operands = "";
                        break;
                }
            }
            else if (reg == 5)
            {
                // D9F0-D9FF special instructions
                switch (rm)
                {
                    case 0:
                        instruction.Mnemonic = "f2xm1";
                        instruction.Operands = "";
                        break;
                    case 1:
                        instruction.Mnemonic = "fyl2x";
                        instruction.Operands = "";
                        break;
                    case 2:
                        instruction.Mnemonic = "fptan";
                        instruction.Operands = "";
                        break;
                    case 3:
                        instruction.Mnemonic = "fpatan";
                        instruction.Operands = "";
                        break;
                    case 4:
                        instruction.Mnemonic = "fxtract";
                        instruction.Operands = "";
                        break;
                    case 5:
                        instruction.Mnemonic = "fprem1";
                        instruction.Operands = "";
                        break;
                    case 6:
                        instruction.Mnemonic = "fdecstp";
                        instruction.Operands = "";
                        break;
                    case 7:
                        instruction.Mnemonic = "fincstp";
                        instruction.Operands = "";
                        break;
                    default:
                        instruction.Mnemonic = "??";
                        instruction.Operands = "";
                        break;
                }
            }
            else if (reg == 6)
            {
                // D9F0-D9FF more special instructions
                switch (rm)
                {
                    case 0:
                        instruction.Mnemonic = "fprem";
                        instruction.Operands = "";
                        break;
                    case 1:
                        instruction.Mnemonic = "fyl2xp1";
                        instruction.Operands = "";
                        break;
                    case 2:
                        instruction.Mnemonic = "fsqrt";
                        instruction.Operands = "";
                        break;
                    case 3:
                        instruction.Mnemonic = "fsincos";
                        instruction.Operands = "";
                        break;
                    case 4:
                        instruction.Mnemonic = "frndint";
                        instruction.Operands = "";
                        break;
                    case 5:
                        instruction.Mnemonic = "fscale";
                        instruction.Operands = "";
                        break;
                    case 6:
                        instruction.Mnemonic = "fsin";
                        instruction.Operands = "";
                        break;
                    case 7:
                        instruction.Mnemonic = "fcos";
                        instruction.Operands = "";
                        break;
                    default:
                        instruction.Mnemonic = "??";
                        instruction.Operands = "";
                        break;
                }
            }
        }
        
        return true;
    }
}
