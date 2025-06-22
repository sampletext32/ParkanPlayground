namespace X86Disassembler.X86.Handlers.FloatingPoint.LoadStore;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FISTTP int16 instruction (DF /1)
/// </summary>
public class FisttpInt16Handler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FisttpInt16Handler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FisttpInt16Handler(InstructionDecoder decoder)
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
        // FISTTP int16 is DF /1
        if (opcode != 0xDF) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the ModR/M byte has reg field = 1
        byte modRm = Decoder.PeakByte();
        byte reg = (byte)((modRm >> 3) & 0x7);
        byte mod = (byte)((modRm >> 6) & 0x3);
        
        // Only handle memory operands (mod != 3) with reg = 1
        return reg == 1 && mod != 3;
    }
    
    /// <summary>
    /// Decodes a FISTTP int16 instruction
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
        var (mod, reg, rm, rawMemoryOperand) = ModRMDecoder.ReadModRM();
        
        // Set the instruction type
        instruction.Type = InstructionType.Fisttp;

        // Create a 16-bit memory operand
        Operand memoryOperand;
        
        if (rawMemoryOperand is DirectMemoryOperand directMemory)
        {
            memoryOperand = OperandFactory.CreateDirectMemoryOperand(directMemory.Address, 16);
        }
        else if (rawMemoryOperand is BaseRegisterMemoryOperand baseMemory)
        {
            memoryOperand = OperandFactory.CreateBaseRegisterMemoryOperand(baseMemory.BaseRegister, 16);
        }
        else if (rawMemoryOperand is DisplacementMemoryOperand dispMemory)
        {
            memoryOperand = OperandFactory.CreateDisplacementMemoryOperand(dispMemory.BaseRegister, dispMemory.Displacement, 16);
        }
        else if (rawMemoryOperand is ScaledIndexMemoryOperand scaledMemory)
        {
            memoryOperand = OperandFactory.CreateScaledIndexMemoryOperand(scaledMemory.IndexRegister, scaledMemory.Scale, scaledMemory.BaseRegister, scaledMemory.Displacement, 16);
        }
        else
        {
            memoryOperand = rawMemoryOperand;
        }

        // Set the structured operands
        instruction.StructuredOperands = 
        [
            memoryOperand
        ];

        return true;
    }
}
