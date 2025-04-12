namespace X86Disassembler.X86.Handlers.String;

/// <summary>
/// Handler for string instructions (MOVS, STOS, LODS, SCAS) with and without REP/REPNE prefixes
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
    
    // REP/REPNE prefix opcodes
    private const byte REP_PREFIX = 0xF3;
    private const byte REPNE_PREFIX = 0xF2;
    
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
        if (_mnemonics.ContainsKey(opcode))
        {
            return true;
        }
        
        // Check if the opcode is a REP/REPNE prefix followed by a string instruction
        if (opcode == REP_PREFIX || opcode == REPNE_PREFIX)
        {
            int position = Decoder.GetPosition();
            if (position < Length)
            {
                byte nextByte = CodeBuffer[position];
                return _mnemonics.ContainsKey(nextByte);
            }
        }
        
        return false;
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
        string prefixString = opcode == REP_PREFIX ? "rep " : (opcode == REPNE_PREFIX ? "repne " : "");
        
        // If this is a REP/REPNE prefix, get the actual string instruction opcode
        byte stringOpcode = opcode;
        if (hasRepPrefix)
        {
            int position = Decoder.GetPosition();
            if (position >= Length)
            {
                return false;
            }
            
            stringOpcode = CodeBuffer[position];
            if (!_mnemonics.ContainsKey(stringOpcode))
            {
                return false;
            }
            
            // Skip the string instruction opcode
            Decoder.SetPosition(position + 1);
        }
        
        // Set the mnemonic
        if (_mnemonics.TryGetValue(stringOpcode, out string? mnemonic))
        {
            instruction.Mnemonic = prefixString + mnemonic;
        }
        else
        {
            // This shouldn't happen if CanHandle is called first
            return false;
        }
        
        // Set the operands based on the string operation
        switch (stringOpcode)
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
