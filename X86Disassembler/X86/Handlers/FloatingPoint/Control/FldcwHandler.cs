namespace X86Disassembler.X86.Handlers.FloatingPoint.Control;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FLDCW instruction (D9 /5)
/// </summary>
public class FldcwHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FldcwHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FldcwHandler(InstructionDecoder decoder)
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
        // FLDCW is D9 /5
        if (opcode != 0xD9) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the ModR/M byte has reg field = 5
        byte modRm = Decoder.PeakByte();
        byte reg = (byte)((modRm >> 3) & 0x7);
        byte mod = (byte)((modRm >> 6) & 0x3);
        
        // Only handle memory operands (mod != 3)
        return reg == 5 && mod != 3;
    }
    
    /// <summary>
    /// Decodes a FLDCW instruction
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
        instruction.Type = InstructionType.Fldcw;

        // Create a 16-bit memory operand for control word operations
        Operand memoryOperand;
        
        if (rawOperand is DirectMemoryOperand directMemory)
        {
            memoryOperand = OperandFactory.CreateDirectMemoryOperand16(directMemory.Address);
        }
        else if (rawOperand is BaseRegisterMemoryOperand baseRegMemory)
        {
            memoryOperand = OperandFactory.CreateBaseRegisterMemoryOperand16(baseRegMemory.BaseRegister);
        }
        else if (rawOperand is DisplacementMemoryOperand dispMemory)
        {
            memoryOperand = OperandFactory.CreateDisplacementMemoryOperand16(dispMemory.BaseRegister, dispMemory.Displacement);
        }
        else if (rawOperand is ScaledIndexMemoryOperand scaledMemory)
        {
            memoryOperand = OperandFactory.CreateScaledIndexMemoryOperand16(scaledMemory.IndexRegister, scaledMemory.Scale, scaledMemory.BaseRegister, scaledMemory.Displacement);
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
