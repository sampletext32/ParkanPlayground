namespace X86Disassembler.X86.Handlers.FloatingPoint.LoadStore;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FISTP int32 instruction (DB /3)
/// </summary>
public class FistpInt32Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FistpInt32Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FistpInt32Handler(InstructionDecoder decoder)
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
        // FISTP is DB /3
        if (opcode != 0xDB) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the ModR/M byte has reg field = 3
        byte modRm = Decoder.PeakByte();
        byte reg = (byte)((modRm >> 3) & 0x7);
        byte mod = (byte)((modRm >> 6) & 0x3);
        
        // Only handle memory operands (mod != 3)
        return reg == 3 && mod != 3;
    }
    
    /// <summary>
    /// Decodes a FISTP int32 instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Read the ModR/M byte
        var (mod, reg, rm, rawOperand) = ModRMDecoder.ReadModRM();
        
        // Set the instruction type
        instruction.Type = InstructionType.Fistp;

        // Create a 32-bit memory operand for integer operations
        Operand memoryOperand;
        
        if (rawOperand is DirectMemoryOperand directMemory)
        {
            memoryOperand = OperandFactory.CreateDirectMemoryOperand(directMemory.Address, 32);
        }
        else if (rawOperand is BaseRegisterMemoryOperand baseRegMemory)
        {
            memoryOperand = OperandFactory.CreateBaseRegisterMemoryOperand(baseRegMemory.BaseRegister, 32);
        }
        else if (rawOperand is DisplacementMemoryOperand dispMemory)
        {
            memoryOperand = OperandFactory.CreateDisplacementMemoryOperand(dispMemory.BaseRegister, dispMemory.Displacement, 32);
        }
        else if (rawOperand is ScaledIndexMemoryOperand scaledMemory)
        {
            memoryOperand = OperandFactory.CreateScaledIndexMemoryOperand(scaledMemory.IndexRegister, scaledMemory.Scale, scaledMemory.BaseRegister, scaledMemory.Displacement, 32);
        }
        else
        {
            memoryOperand = rawOperand;
        }

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            memoryOperand
        ];

        return true;
    }
}
