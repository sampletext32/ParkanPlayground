using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Misc;

/// <summary>
/// Handler for IN instruction (0xE4, 0xE5, 0xEC, 0xED)
/// </summary>
public class InHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the InHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public InHandler(InstructionDecoder decoder)
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
        // IN AL, imm8 is encoded as 0xE4
        // IN EAX, imm8 is encoded as 0xE5
        // IN AL, DX is encoded as 0xEC
        // IN EAX, DX is encoded as 0xED
        return opcode == 0xE4 || opcode == 0xE5 || opcode == 0xEC || opcode == 0xED;
    }

    /// <summary>
    /// Decodes an IN instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.In;

        // Determine the operands based on the opcode
        Operand destOperand;
        Operand srcOperand;

        switch (opcode)
        {
            case 0xE4: // IN AL, imm8
                destOperand = OperandFactory.CreateRegisterOperand8(RegisterIndex8.AL);
                
                // Check if we can read the immediate byte
                if (!Decoder.CanReadByte())
                    return false;
                
                // Read the immediate byte (port number)
                byte imm8 = Decoder.ReadByte();
                srcOperand = OperandFactory.CreateImmediateOperand(imm8);
                break;
                
            case 0xE5: // IN EAX, imm8
                destOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.A, 32);
                
                // Check if we can read the immediate byte
                if (!Decoder.CanReadByte())
                    return false;
                
                // Read the immediate byte (port number)
                imm8 = Decoder.ReadByte();
                srcOperand = OperandFactory.CreateImmediateOperand(imm8);
                break;
                
            case 0xEC: // IN AL, DX
                destOperand = OperandFactory.CreateRegisterOperand8(RegisterIndex8.AL);
                srcOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.D, 16);
                break;
                
            case 0xED: // IN EAX, DX
                destOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.A, 32);
                srcOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.D, 16);
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
