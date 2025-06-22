using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.Call;

/// <summary>
/// Handler for CALL m16:32 instruction (FF /3) - Far call with memory operand
/// </summary>
public class CallFarPtrHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the CallFarPtrHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public CallFarPtrHandler(InstructionDecoder decoder)
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
        // CALL m16:32 is encoded as FF /3
        if (opcode != 0xFF)
        {
            return false;
        }
        
        // Check if we have enough bytes to read the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Extract the reg field (bits 3-5)
        var reg = ModRMDecoder.PeakModRMReg();
        
        // CALL m16:32 is encoded as FF /3 (reg field = 3)
        return reg == 3;
    }

    /// <summary>
    /// Decodes a CALL m16:32 instruction (far call)
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the instruction type
        instruction.Type = InstructionType.Call;

        // Check if we have enough bytes for the ModR/M byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        // For CALL m16:32 (FF /3):
        // - The r/m field with mod specifies the memory operand
        // - This instruction can only reference memory, not registers
        var (mod, reg, rm, operand) = ModRMDecoder.ReadModRM();

        // For far calls, we need to ensure this is a memory operand, not a register
        // If mod == 3, then it's a register operand, which is invalid for far calls
        if (mod == 3)
        {
            return false;
        }

        // Create a special far pointer operand by modifying the memory operand
        // to indicate it's a far pointer (fword ptr)
        // We need to ensure the operand is a memory operand before converting it
        if (!(operand is MemoryOperand memOperand))
        {
            return false;
        }
        
        var farPtrOperand = OperandFactory.CreateFarPointerOperand(memOperand);

        // Set the structured operands
        // CALL has only one operand
        instruction.StructuredOperands = 
        [
            farPtrOperand
        ];

        return true;
    }
}
