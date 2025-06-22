namespace X86Disassembler.X86.Handlers.FloatingPoint.LoadStore;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for FSTP extended-precision instruction (DB /7)
/// </summary>
public class FstpExtendedHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FstpExtendedHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FstpExtendedHandler(InstructionDecoder decoder)
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
        // FSTP extended-precision is DB /7
        if (opcode != 0xDB) return false;

        if (!Decoder.CanReadByte())
        {
            return false;
        }

        // Check if the ModR/M byte has reg field = 7
        byte modRm = Decoder.PeakByte();
        byte reg = (byte)((modRm >> 3) & 0x7);
        byte mod = (byte)((modRm >> 6) & 0x3);
        
        // Only handle memory operands (mod != 3)
        return reg == 7 && mod != 3;
    }
    
    /// <summary>
    /// Decodes a FSTP extended-precision instruction
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
        instruction.Type = InstructionType.Fstp;

        // Create an 80-bit memory operand for extended precision operations
        Operand memoryOperand;
        
        if (rawOperand is DirectMemoryOperand directMemory)
        {
            memoryOperand = OperandFactory.CreateDirectMemoryOperand(directMemory.Address, 80);
        }
        else if (rawOperand is BaseRegisterMemoryOperand baseRegMemory)
        {
            memoryOperand = OperandFactory.CreateBaseRegisterMemoryOperand(baseRegMemory.BaseRegister, 80);
        }
        else if (rawOperand is DisplacementMemoryOperand dispMemory)
        {
            memoryOperand = OperandFactory.CreateDisplacementMemoryOperand(dispMemory.BaseRegister, dispMemory.Displacement, 80);
        }
        else if (rawOperand is ScaledIndexMemoryOperand scaledMemory)
        {
            memoryOperand = OperandFactory.CreateScaledIndexMemoryOperand(scaledMemory.IndexRegister, scaledMemory.Scale, scaledMemory.BaseRegister, scaledMemory.Displacement, 80);
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
