namespace X86Disassembler.X86.Handlers.String;

/// <summary>
/// Handler for string instructions (MOVS, STOS, LODS, SCAS)
/// </summary>
public class StringInstructionHandler : InstructionHandler
{
    // Dictionary mapping opcodes to their mnemonics
    private static readonly Dictionary<byte, string> _mnemonics = new Dictionary<byte, string>
    {
        { 0xA4, "movs" }, // MOVSB
        { 0xA5, "movs" }, // MOVSD
        { 0xAA, "stos" }, // STOSB
        { 0xAB, "stos" }, // STOSD
        { 0xAC, "lods" }, // LODSB
        { 0xAD, "lods" }, // LODSD
        { 0xAE, "scas" }, // SCASB
        { 0xAF, "scas" }  // SCASD
    };
    
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
        // Check if the opcode is a string instruction
        return _mnemonics.ContainsKey(opcode);
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
        if (_mnemonics.TryGetValue(opcode, out string? mnemonic))
        {
            instruction.Mnemonic = mnemonic;
        }
        else
        {
            // This shouldn't happen if CanHandle is called first
            return false;
        }
        
        // Set the operands based on the string operation
        switch (opcode)
        {
            case 0xA4: // MOVSB
                instruction.Operands = "byte ptr [edi], byte ptr [esi]";
                break;
            case 0xA5: // MOVSD
                instruction.Operands = "dword ptr [edi], dword ptr [esi]";
                break;
            case 0xAA: // STOSB
                instruction.Operands = "byte ptr [edi], al";
                break;
            case 0xAB: // STOSD
                instruction.Operands = "dword ptr [edi], eax";
                break;
            case 0xAC: // LODSB
                instruction.Operands = "al, byte ptr [esi]";
                break;
            case 0xAD: // LODSD
                instruction.Operands = "eax, dword ptr [esi]";
                break;
            case 0xAE: // SCASB
                instruction.Operands = "al, byte ptr [edi]";
                break;
            case 0xAF: // SCASD
                instruction.Operands = "eax, dword ptr [edi]";
                break;
            default:
                instruction.Operands = "??";
                break;
        }
        
        return true;
    }
}
