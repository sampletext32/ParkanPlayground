namespace X86Disassembler.X86;

using X86Disassembler.X86.Handlers;

/// <summary>
/// Decodes x86 instructions
/// </summary>
public class InstructionDecoder
{
    // Instruction prefix bytes
    private const byte PREFIX_LOCK = 0xF0;
    private const byte PREFIX_REPNE = 0xF2;
    private const byte PREFIX_REP = 0xF3;
    private const byte PREFIX_CS = 0x2E;
    private const byte PREFIX_SS = 0x36;
    private const byte PREFIX_DS = 0x3E;
    private const byte PREFIX_ES = 0x26;
    private const byte PREFIX_FS = 0x64;
    private const byte PREFIX_GS = 0x65;
    private const byte PREFIX_OPERAND_SIZE = 0x66;
    private const byte PREFIX_ADDRESS_SIZE = 0x67;
    
    // Buffer containing the code to decode
    private readonly byte[] _codeBuffer;
    
    // Current position in the code buffer
    private int _position;
    
    // Length of the buffer
    private readonly int _length;
    
    // List of instruction handlers
    private readonly List<InstructionHandler> _handlers;
    
    /// <summary>
    /// Initializes a new instance of the InstructionDecoder class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    public InstructionDecoder(byte[] codeBuffer)
    {
        _codeBuffer = codeBuffer;
        _position = 0;
        _length = codeBuffer.Length;
        
        // Initialize the instruction handlers
        _handlers = new List<InstructionHandler>
        {
            new Group1Handler(_codeBuffer, this, _length),
            new FloatingPointHandler(_codeBuffer, this, _length),
            new DataTransferHandler(_codeBuffer, this, _length),
            new ControlFlowHandler(_codeBuffer, this, _length),
            new Group3Handler(_codeBuffer, this, _length),
            new TestHandler(_codeBuffer, this, _length),
            new ArithmeticHandler(_codeBuffer, this, _length)
        };
    }
    
    /// <summary>
    /// Decodes an instruction at the current position in the code buffer
    /// </summary>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>The number of bytes read</returns>
    public int Decode(Instruction instruction)
    {
        // Store the starting position
        int startPosition = _position;
        
        // Check if we've reached the end of the buffer
        if (_position >= _length)
        {
            return 0;
        }
        
        // Handle instruction prefixes
        bool hasPrefix = true;
        bool operandSizePrefix = false;
        bool addressSizePrefix = false;
        string segmentOverride = string.Empty;
        
        while (hasPrefix && _position < _length)
        {
            byte prefix = _codeBuffer[_position];
            
            switch (prefix)
            {
                case PREFIX_LOCK:
                case PREFIX_REPNE:
                case PREFIX_REP:
                    _position++;
                    break;
                    
                case PREFIX_CS:
                    segmentOverride = "cs";
                    _position++;
                    break;
                    
                case PREFIX_SS:
                    segmentOverride = "ss";
                    _position++;
                    break;
                    
                case PREFIX_DS:
                    segmentOverride = "ds";
                    _position++;
                    break;
                    
                case PREFIX_ES:
                    segmentOverride = "es";
                    _position++;
                    break;
                    
                case PREFIX_FS:
                    segmentOverride = "fs";
                    _position++;
                    break;
                    
                case PREFIX_GS:
                    segmentOverride = "gs";
                    _position++;
                    break;
                    
                case PREFIX_OPERAND_SIZE:
                    operandSizePrefix = true;
                    _position++;
                    break;
                    
                case PREFIX_ADDRESS_SIZE:
                    addressSizePrefix = true;
                    _position++;
                    break;
                    
                default:
                    hasPrefix = false;
                    break;
            }
        }
        
        // We've reached the end of the buffer after processing prefixes
        if (_position >= _length)
        {
            return _position - startPosition;
        }
        
        // Read the opcode
        byte opcode = _codeBuffer[_position++];
        
        // Try to find a handler for this opcode
        bool handled = false;
        foreach (var handler in _handlers)
        {
            if (handler.CanHandle(opcode))
            {
                handled = handler.Decode(opcode, instruction);
                if (handled)
                {
                    break;
                }
            }
        }
        
        // If no handler was found or the instruction couldn't be decoded,
        // use a default mnemonic from the opcode map
        if (!handled)
        {
            instruction.Mnemonic = OpcodeMap.GetMnemonic(opcode);
            instruction.Operands = string.Empty;
        }
        
        // Copy the instruction bytes
        int bytesRead = _position - startPosition;
        instruction.Bytes = new byte[bytesRead];
        Array.Copy(_codeBuffer, startPosition, instruction.Bytes, 0, bytesRead);
        
        return bytesRead;
    }
    
    /// <summary>
    /// Sets the current position in the code buffer
    /// </summary>
    /// <param name="position">The new position</param>
    public void SetPosition(int position)
    {
        _position = position;
    }
    
    /// <summary>
    /// Gets the current position in the code buffer
    /// </summary>
    /// <returns>The current position</returns>
    public int GetPosition()
    {
        return _position;
    }
    
    /// <summary>
    /// Decodes an instruction at the specified position in the code buffer
    /// </summary>
    /// <param name="position">The position in the code buffer</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>The number of bytes read</returns>
    public int DecodeAt(int position, Instruction instruction)
    {
        _position = position;
        return Decode(instruction);
    }
}
