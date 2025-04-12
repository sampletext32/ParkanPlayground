namespace X86Disassembler.X86.Handlers.String;

/// <summary>
/// Handler for string instructions (MOVS, STOS, LODS, SCAS)
/// </summary>
public class StringInstructionHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the StringInstructionHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public StringInstructionHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
        : base(codeBuffer, decoder, length)
    {
    }
    
    /// <summary>
    /// Checks if this handler can handle the given opcode
    /// </summary>
    /// <param name="opcode">The opcode to check</param>
    /// <returns>True if this handler can handle the opcode</returns>
    public override bool CanHandle(byte opcode)
    {
        return IsStringInstruction(opcode);
    }
    
    /// <summary>
    /// Decodes a string instruction
    /// </summary>
    /// <param name="opcode">The opcode to decode</param>
    /// <param name="instruction">The instruction to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        // Set the mnemonic
        instruction.Mnemonic = OpcodeMap.GetMnemonic(opcode);
        
        // Set the operands
        instruction.Operands = GetStringOperands(opcode);
        
        return true;
    }
    
    /// <summary>
    /// Checks if the opcode is a string instruction
    /// </summary>
    /// <param name="opcode">The opcode to check</param>
    /// <returns>True if the opcode is a string instruction</returns>
    private bool IsStringInstruction(byte opcode)
    {
        return opcode == 0xA4 || opcode == 0xA5 || // MOVS
               opcode == 0xAA || opcode == 0xAB || // STOS
               opcode == 0xAC || opcode == 0xAD || // LODS
               opcode == 0xAE || opcode == 0xAF;   // SCAS
    }
    
    /// <summary>
    /// Gets the operands for a string instruction
    /// </summary>
    /// <param name="stringOp">The string operation opcode</param>
    /// <returns>The operands string</returns>
    private string GetStringOperands(byte stringOp)
    {
        switch (stringOp)
        {
            case 0xA4: // MOVSB
                return "byte ptr [edi], byte ptr [esi]";
            case 0xA5: // MOVSD
                return "dword ptr [edi], dword ptr [esi]";
            case 0xAA: // STOSB
                return "byte ptr [edi], al";
            case 0xAB: // STOSD
                return "dword ptr [edi], eax";
            case 0xAC: // LODSB
                return "al, byte ptr [esi]";
            case 0xAD: // LODSD
                return "eax, dword ptr [esi]";
            case 0xAE: // SCASB
                return "al, byte ptr [edi]";
            case 0xAF: // SCASD
                return "eax, dword ptr [edi]";
            default:
                return "??";
        }
    }
}
