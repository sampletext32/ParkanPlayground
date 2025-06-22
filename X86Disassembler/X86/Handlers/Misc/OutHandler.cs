using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Misc;

/// <summary>
/// Handler for OUT instruction (0xE6, 0xE7, 0xEE, 0xEF)
/// </summary>
public class OutHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the OutHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public OutHandler(InstructionDecoder decoder)
        : base(decoder)
    {
    }

    /// <summary>
    /// Checks if this handler can decode the given opcode
    /// </summary>
    /// <param name="opcode">The opcode to check</param>
    /// <returns>True if this handler can decode the opcode</returns>
    public override bool CanHandle(byte opcode)
    {
        // OUT imm8, AL is encoded as 0xE6
        // OUT imm8, EAX is encoded as 0xE7
        // OUT DX, AL is encoded as 0xEE
        // OUT DX, EAX is encoded as 0xEF
        return opcode == 0xE6 || opcode == 0xE7 || opcode == 0xEE || opcode == 0xEF;
    }

    /// <summary>
    /// Decodes an OUT instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Out;

        // Determine the operands based on the opcode
        Operand destOperand;
        Operand srcOperand;

        switch (opcode)
        {
            case 0xE6: // OUT imm8, AL
                // Check if we can read the immediate byte
                if (!Decoder.CanReadByte())
                    return false;
                
                // Read the immediate byte (port number)
                byte imm8 = Decoder.ReadByte();
                destOperand = OperandFactory.CreateImmediateOperand(imm8);
                srcOperand = OperandFactory.CreateRegisterOperand8(RegisterIndex8.AL);
                break;
                
            case 0xE7: // OUT imm8, EAX
                // Check if we can read the immediate byte
                if (!Decoder.CanReadByte())
                    return false;
                
                // Read the immediate byte (port number)
                imm8 = Decoder.ReadByte();
                destOperand = OperandFactory.CreateImmediateOperand(imm8);
                srcOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.A, 32);
                break;
                
            case 0xEE: // OUT DX, AL
                destOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.D, 16);
                srcOperand = OperandFactory.CreateRegisterOperand8(RegisterIndex8.AL);
                break;
                
            case 0xEF: // OUT DX, EAX
                destOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.D, 16);
                srcOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.A, 32);
                break;
                
            default:
                return false;
        }

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            destOperand,
            srcOperand
        ];

        return true;
    }
}
