namespace X86Disassembler.X86.Handlers.String;

using X86Disassembler.X86.Operands;

/// <summary>
/// Handler for string instructions (MOVS, STOS, LODS, SCAS) with and without REP/REPNE prefixes
/// </summary>
public class StringInstructionHandler : InstructionHandler
{
    // Dictionary mapping opcodes to their instruction types and operand factories
    private static readonly Dictionary<byte, (InstructionType Type, Func<Operand[]> CreateOperands)> StringInstructions = new()
    {
        { 0xA4, (InstructionType.MovsB, () =>
        [
            OperandFactory.CreateBaseRegisterMemoryOperand(RegisterIndex.Di, 8, "es"),
            OperandFactory.CreateBaseRegisterMemoryOperand(RegisterIndex.Si, 8, "ds")
        ]) },  // MOVSB
        { 0xA5, (InstructionType.MovsD, () =>
        [
            OperandFactory.CreateBaseRegisterMemoryOperand(RegisterIndex.Di, 32, "es"),
            OperandFactory.CreateBaseRegisterMemoryOperand(RegisterIndex.Si, 32, "ds")
        ]) }, // MOVSD
        { 0xAA, (InstructionType.StosB, () =>
        [
            OperandFactory.CreateBaseRegisterMemoryOperand(RegisterIndex.Di, 8, "es"),
            OperandFactory.CreateRegisterOperand(RegisterIndex.A, 8)
        ]) },  // STOSB
        { 0xAB, (InstructionType.StosD, () =>
        [
            OperandFactory.CreateBaseRegisterMemoryOperand(RegisterIndex.Di, 32, "es"),
            OperandFactory.CreateRegisterOperand(RegisterIndex.A, 32)
        ]) },  // STOSD
        { 0xAC, (InstructionType.LodsB, () =>
        [
            OperandFactory.CreateRegisterOperand(RegisterIndex.A, 8),
            OperandFactory.CreateBaseRegisterMemoryOperand(RegisterIndex.Si, 8, "ds")
        ]) },  // LODSB
        { 0xAD, (InstructionType.LodsD, () =>
        [
            OperandFactory.CreateRegisterOperand(RegisterIndex.A, 32),
            OperandFactory.CreateBaseRegisterMemoryOperand(RegisterIndex.Si, 32, "ds")
        ]) },  // LODSD
        { 0xAE, (InstructionType.ScasB, () =>
        [
            OperandFactory.CreateRegisterOperand(RegisterIndex.A, 8),
            OperandFactory.CreateBaseRegisterMemoryOperand(RegisterIndex.Di, 8, "es")
        ]) },  // SCASB
        { 0xAF, (InstructionType.ScasD, () =>
        [
            OperandFactory.CreateRegisterOperand(RegisterIndex.A, 32),
            OperandFactory.CreateBaseRegisterMemoryOperand(RegisterIndex.Di, 32, "es")
        ]) }   // SCASD
    };
    
    // REP/REPNE prefix opcodes
    private const byte REP_PREFIX = 0xF3;
    private const byte REPNE_PREFIX = 0xF2;

    // Dictionary mapping base instruction types to their REP-prefixed versions
    private static readonly Dictionary<InstructionType, InstructionType> RepPrefixMap = new()
    {
        { InstructionType.MovsB, InstructionType.RepMovsB },
        { InstructionType.MovsD, InstructionType.RepMovsD },
        { InstructionType.StosB, InstructionType.RepStosB },
        { InstructionType.StosD, InstructionType.RepStosD },
        { InstructionType.LodsB, InstructionType.RepLodsB },
        { InstructionType.LodsD, InstructionType.RepLodsD },
        { InstructionType.ScasB, InstructionType.RepScasB },
        { InstructionType.ScasD, InstructionType.RepScasD }
    };

    // Dictionary mapping base instruction types to their REPNE-prefixed versions
    private static readonly Dictionary<InstructionType, InstructionType> RepnePrefixMap = new()
    {
        { InstructionType.ScasB, InstructionType.RepneScasB },
        { InstructionType.ScasD, InstructionType.RepneScasD }
    };

    /// <summary>
    /// Initializes a new instance of the StringInstructionHandler class
    /// </summary>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    public StringInstructionHandler(InstructionDecoder decoder) 
        : base(decoder)
    {
    }
    
    /// <summary>
    /// Checks if this handler can handle the given opcode
    /// </summary>
    /// <param name="opcode">The opcode to check</param>
    /// <returns>True if this handler can handle the opcode</returns>
    public override bool CanHandle(byte opcode)
    {
        // Check if the opcode is a string instruction
        if (StringInstructions.ContainsKey(opcode))
        {
            return true;
        }
        
        // Check if the opcode is a REP/REPNE prefix followed by a string instruction
        if (opcode != REP_PREFIX && opcode != REPNE_PREFIX)
        {
            return false;
        }
        
        // Check if we can read the next byte
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
        // Check if the next byte is a string instruction
        byte nextByte = Decoder.PeakByte();
        return StringInstructions.ContainsKey(nextByte);
    }
    
    /// <summary>
    /// Decodes a string instruction
    /// </summary>
    /// <param name="opcode">The opcode to decode</param>
    /// <param name="instruction">The instruction to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Check if this is a REP/REPNE prefix
        bool hasRepPrefix = opcode == REP_PREFIX || opcode == REPNE_PREFIX;
        
        // If this is a REP/REPNE prefix, get the actual string instruction opcode
        byte stringOpcode = opcode;
        
        if (hasRepPrefix)
        {
            // Read the next byte (the actual string instruction opcode)
            if (!Decoder.CanReadByte())
            {
                return false;
            }

            stringOpcode = Decoder.ReadByte();

            if (!StringInstructions.ContainsKey(stringOpcode))
            {
                return false;
            }
        }

        // Get the instruction type and operands for the string instruction
        if (StringInstructions.TryGetValue(stringOpcode, out var instructionInfo))
        {
            // Set the instruction type based on whether there's a REP/REPNE prefix
            if (hasRepPrefix)
            {
                // Determine the appropriate prefixed instruction type based on the prefix
                if (opcode == REP_PREFIX)
                {
                    // Use the REP prefix map to get the prefixed instruction type
                    instruction.Type = RepPrefixMap.TryGetValue(instructionInfo.Type, out var repType) 
                        ? repType 
                        : instructionInfo.Type;
                }
                else // REPNE prefix
                {
                    // Use the REPNE prefix map to get the prefixed instruction type
                    instruction.Type = RepnePrefixMap.TryGetValue(instructionInfo.Type, out var repneType) 
                        ? repneType 
                        : instructionInfo.Type;
                }
            }
            else
            {
                // No prefix, use the original instruction type
                instruction.Type = instructionInfo.Type;
            }

            // Create and set the structured operands
            instruction.StructuredOperands = instructionInfo.CreateOperands().ToList();

            return true;
        }

        // This shouldn't happen if CanHandle is called first
        return false;
    }
}