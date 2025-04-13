namespace X86Disassembler.X86.Handlers.String;

/// <summary>
/// Handler for string instructions (MOVS, STOS, LODS, SCAS) with and without REP/REPNE prefixes
/// </summary>
public class StringInstructionHandler : InstructionHandler
{
    // Dictionary mapping opcodes to their mnemonics and operands
    private static readonly Dictionary<byte, (string Mnemonic, string Operands)> StringInstructions = new()
    {
        { 0xA4, ("movs", "byte ptr [edi], byte ptr [esi]") },  // MOVSB
        { 0xA5, ("movs", "dword ptr [edi], dword ptr [esi]") }, // MOVSD
        { 0xAA, ("stos", "byte ptr [edi], al") },              // STOSB
        { 0xAB, ("stos", "dword ptr [edi], eax") },            // STOSD
        { 0xAC, ("lods", "al, byte ptr [esi]") },              // LODSB
        { 0xAD, ("lods", "eax, dword ptr [esi]") },            // LODSD
        { 0xAE, ("scas", "al, byte ptr [edi]") },              // SCASB
        { 0xAF, ("scas", "eax, dword ptr [edi]") }             // SCASD
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
        if (StringInstructions.ContainsKey(opcode))
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
                return StringInstructions.ContainsKey(nextByte);
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
        string prefixString = "";
        
        // If this is a REP/REPNE prefix, get the actual string instruction opcode
        byte stringOpcode = opcode;
        
        if (hasRepPrefix)
        {
            // Set the prefix string based on the prefix opcode
            prefixString = opcode == REP_PREFIX ? "rep " : "repne ";
            
            // Read the next byte (the actual string instruction opcode)
            int position = Decoder.GetPosition();
            if (position >= Length)
            {
                return false;
            }
            
            stringOpcode = Decoder.ReadByte();
            if (!StringInstructions.ContainsKey(stringOpcode))
            {
                return false;
            }
        }
        
        // Get the mnemonic and operands for the string instruction
        if (StringInstructions.TryGetValue(stringOpcode, out var instructionInfo))
        {
            // Set the mnemonic with the prefix if present
            instruction.Mnemonic = prefixString + instructionInfo.Mnemonic;
            
            // Set the operands
            instruction.Operands = instructionInfo.Operands;
            
            return true;
        }
        
        // This shouldn't happen if CanHandle is called first
        return false;
    }
}
