using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86.Handlers.FloatingPoint.Control;

/// <summary>
/// Handler for FSTSW instruction (with WAIT prefix 0x9B)
/// Handles both:
/// - FSTSW AX (0x9B 0xDF 0xE0)
/// - FSTSW m2byte (0x9B 0xDD /7)
/// </summary>
public class FstswHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FstswHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public FstswHandler(InstructionDecoder decoder)
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
        // FSTSW starts with the WAIT prefix (0x9B)
        if (opcode != 0x9B) return false;

        // Check if we can read the next byte
        if (!Decoder.CanReadByte())
            return false;

        // Check if the next byte is 0xDF (for FSTSW AX) or 0xDD (for FSTSW m2byte)

        var (nextByte, modRM) = Decoder.PeakTwoBytes();

        if (nextByte != 0xDF && nextByte != 0xDD)
            return false;

        if (nextByte == 0xDF)
        {
            // For FSTSW AX, check if we can peek at the third byte and it's 0xE0

            return modRM == 0xE0;
        }
        else // nextByte == 0xDD
        {
            // For FSTSW m2byte, check if we can peek at ModR/M byte and reg field = 7
            byte regField = ModRMDecoder.GetRegFromModRM(modRM);
            
            // The reg field must be 7 for FSTSW m2byte
            return regField == 7;
        }
    }
    
    /// <summary>
    /// Decodes an FSTSW instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Skip the WAIT prefix (0x9B) - we already read it in CanHandle
        if (!Decoder.CanReadByte())
            return false;

        // Read the second byte (0xDF for AX variant, 0xDD for memory variant)
        byte secondByte = Decoder.ReadByte();
        
        // Set the instruction type
        instruction.Type = InstructionType.Fstsw;

        if (secondByte == 0xDF)
        {
            // FSTSW AX variant
            // Read the 0xE0 byte
            if (!Decoder.CanReadByte())
                return false;

            byte e0Byte = Decoder.ReadByte();
            if (e0Byte != 0xE0)
                return false;
            
            // Create the AX register operand
            var axOperand = OperandFactory.CreateRegisterOperand(RegisterIndex.A, 16);
            
            // Set the structured operands
            instruction.StructuredOperands = 
            [
                axOperand
            ];
        }
        else if (secondByte == 0xDD)
        {
            // FSTSW m2byte variant
            // Use ModRMDecoder to read and decode the ModR/M byte for 16-bit memory operand
            var (mod, reg, rm, memoryOperand) = ModRMDecoder.ReadModRM16();
                
            // Set the structured operands
            instruction.StructuredOperands = 
            [
                memoryOperand
            ];
        }
        else
        {
            return false;
        }
        
        return true;
    }
}
