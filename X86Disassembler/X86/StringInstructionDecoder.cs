namespace X86Disassembler.X86;

/// <summary>
/// Handles decoding of string instructions
/// </summary>
public class StringInstructionDecoder
{
    // The buffer containing the code to decode
    private readonly byte[] _codeBuffer;
    
    // The length of the buffer
    private readonly int _length;
    
    /// <summary>
    /// Initializes a new instance of the StringInstructionDecoder class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="length">The length of the buffer</param>
    public StringInstructionDecoder(byte[] codeBuffer, int length)
    {
        _codeBuffer = codeBuffer;
        _length = length;
    }
    
    /// <summary>
    /// Checks if the opcode is a string instruction
    /// </summary>
    /// <param name="opcode">The opcode to check</param>
    /// <returns>True if the opcode is a string instruction</returns>
    public bool IsStringInstruction(byte opcode)
    {
        return opcode == 0xA4 || opcode == 0xA5 || // MOVS
               opcode == 0xAA || opcode == 0xAB || // STOS
               opcode == 0xAC || opcode == 0xAD || // LODS
               opcode == 0xAE || opcode == 0xAF;   // SCAS
    }
    
    /// <summary>
    /// Creates an instruction for a string operation with REP/REPNE prefix
    /// </summary>
    /// <param name="prefix">The REP/REPNE prefix (0xF2 or 0xF3)</param>
    /// <param name="stringOp">The string operation opcode</param>
    /// <param name="startPosition">The start position of the instruction</param>
    /// <param name="currentPosition">The current position after reading the string opcode</param>
    /// <returns>The created instruction</returns>
    public Instruction CreateStringInstruction(byte prefix, byte stringOp, int startPosition, int currentPosition)
    {
        // Create a new instruction
        Instruction instruction = new Instruction
        {
            Address = (uint)startPosition,
        };
        
        // Get the mnemonic for the string operation
        string mnemonic = OpcodeMap.GetMnemonic(stringOp);
        instruction.Mnemonic = prefix == 0xF3 ? $"rep {mnemonic}" : $"repne {mnemonic}";
        
        // Set operands based on the string operation
        instruction.Operands = GetStringOperands(stringOp);
        
        // Set the raw bytes
        int length = currentPosition - startPosition;
        instruction.RawBytes = new byte[length];
        Array.Copy(_codeBuffer, startPosition, instruction.RawBytes, 0, length);
        
        return instruction;
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
