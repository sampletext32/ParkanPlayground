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
        { 0xA4, (InstructionType.MovsB, () => new Operand[] {
            OperandFactory.CreateDirectMemoryOperand(0, 8, "edi"),
            OperandFactory.CreateDirectMemoryOperand(0, 8, "esi")
        }) },  // MOVSB
        { 0xA5, (InstructionType.MovsD, () => new Operand[] {
            OperandFactory.CreateDirectMemoryOperand(0, 32, "edi"),
            OperandFactory.CreateDirectMemoryOperand(0, 32, "esi")
        }) }, // MOVSD
        { 0xAA, (InstructionType.StosB, () => new Operand[] {
            OperandFactory.CreateDirectMemoryOperand(0, 8, "edi"),
            OperandFactory.CreateRegisterOperand(RegisterIndex.A, 8)
        }) },  // STOSB
        { 0xAB, (InstructionType.StosD, () => new Operand[] {
            OperandFactory.CreateDirectMemoryOperand(0, 32, "edi"),
            OperandFactory.CreateRegisterOperand(RegisterIndex.A, 32)
        }) },  // STOSD
        { 0xAC, (InstructionType.LodsB, () => new Operand[] {
            OperandFactory.CreateRegisterOperand(RegisterIndex.A, 8),
            OperandFactory.CreateDirectMemoryOperand(0, 8, "esi")
        }) },  // LODSB
        { 0xAD, (InstructionType.LodsD, () => new Operand[] {
            OperandFactory.CreateRegisterOperand(RegisterIndex.A, 32),
            OperandFactory.CreateDirectMemoryOperand(0, 32, "esi")
        }) },  // LODSD
        { 0xAE, (InstructionType.ScasB, () => new Operand[] {
            OperandFactory.CreateRegisterOperand(RegisterIndex.A, 8),
            OperandFactory.CreateDirectMemoryOperand(0, 8, "edi")
        }) },  // SCASB
        { 0xAF, (InstructionType.ScasD, () => new Operand[] {
            OperandFactory.CreateRegisterOperand(RegisterIndex.A, 32),
            OperandFactory.CreateDirectMemoryOperand(0, 32, "edi")
        }) }   // SCASD
    };
    
    // REP/REPNE prefix opcodes
    private const byte REP_PREFIX = 0xF3;
    private const byte REPNE_PREFIX = 0xF2;

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
        
        if (!Decoder.CanReadByte())
        {
            return false;
        }
        
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
            // Set the instruction type
            instruction.Type = instructionInfo.Type;

            // Create and set the structured operands
            instruction.StructuredOperands = instructionInfo.CreateOperands().ToList();

            return true;
        }

        // This shouldn't happen if CanHandle is called first
        return false;
    }
}