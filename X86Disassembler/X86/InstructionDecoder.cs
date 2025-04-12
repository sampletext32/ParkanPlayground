namespace X86Disassembler.X86;

using Handlers;

/// <summary>
/// Decodes x86 instructions from a byte buffer
/// </summary>
public class InstructionDecoder
{
    // The buffer containing the code to decode
    private readonly byte[] _codeBuffer;
    
    // The length of the buffer
    private readonly int _length;
    
    // The current position in the buffer
    private int _position;
    
    // The instruction handler factory
    private readonly InstructionHandlerFactory _handlerFactory;
    
    // Instruction prefixes
    private bool _operandSizePrefix;
    private bool _addressSizePrefix;
    private bool _segmentOverridePrefix;
    private bool _lockPrefix;
    private bool _repPrefix;
    private string _segmentOverride;
    
    /// <summary>
    /// Initializes a new instance of the InstructionDecoder class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="length">The length of the buffer</param>
    public InstructionDecoder(byte[] codeBuffer, int length)
    {
        _codeBuffer = codeBuffer;
        _length = length;
        _position = 0;
        _segmentOverride = "";
        
        // Create the instruction handler factory
        _handlerFactory = new InstructionHandlerFactory(_codeBuffer, this, _length);
    }
    
    /// <summary>
    /// Decodes an instruction at the current position
    /// </summary>
    /// <returns>The decoded instruction, or null if the decoding failed</returns>
    public Instruction? DecodeInstruction()
    {
        if (_position >= _length)
        {
            return null;
        }
        
        // Reset prefix flags
        _operandSizePrefix = false;
        _addressSizePrefix = false;
        _segmentOverridePrefix = false;
        _lockPrefix = false;
        _repPrefix = false;
        _segmentOverride = string.Empty;
        
        // Save the start position of the instruction
        int startPosition = _position;
        
        // Create a new instruction
        Instruction instruction = new Instruction
        {
            Address = (uint)startPosition,
        };
        
        // Handle prefixes
        while (_position < _length)
        {
            byte prefix = _codeBuffer[_position];
            
            if (prefix == 0x66) // Operand size prefix
            {
                _operandSizePrefix = true;
                _position++;
            }
            else if (prefix == 0x67) // Address size prefix
            {
                _addressSizePrefix = true;
                _position++;
            }
            else if (prefix >= 0x26 && prefix <= 0x3E && (prefix & 0x7) == 0x6) // Segment override prefix
            {
                _segmentOverridePrefix = true;
                switch (prefix)
                {
                    case 0x26: _segmentOverride = "es"; break;
                    case 0x2E: _segmentOverride = "cs"; break;
                    case 0x36: _segmentOverride = "ss"; break;
                    case 0x3E: _segmentOverride = "ds"; break;
                    case 0x64: _segmentOverride = "fs"; break;
                    case 0x65: _segmentOverride = "gs"; break;
                }
                _position++;
            }
            else if (prefix == 0xF0) // LOCK prefix
            {
                _lockPrefix = true;
                _position++;
            }
            else if (prefix == 0xF2 || prefix == 0xF3) // REP/REPNE prefix
            {
                _repPrefix = true;
                _position++;
            }
            else
            {
                break;
            }
        }
        
        if (_position >= _length)
        {
            return null;
        }
        
        // Read the opcode
        byte opcode = _codeBuffer[_position++];
        
        // Get a handler for the opcode
        var handler = _handlerFactory.GetHandler(opcode);
        
        bool handlerSuccess = false;
        
        // Try to decode with a handler first
        if (handler != null)
        {
            handlerSuccess = handler.Decode(opcode, instruction);
        }
        
        // If no handler is found or decoding fails, create a default instruction
        if (!handlerSuccess)
        {
            instruction.Mnemonic = OpcodeMap.GetMnemonic(opcode);
            instruction.Operands = "??";
        }
        
        // Set the raw bytes
        int length = _position - startPosition;
        instruction.RawBytes = new byte[length];
        Array.Copy(_codeBuffer, startPosition, instruction.RawBytes, 0, length);
        
        return instruction;
    }
    
    /// <summary>
    /// Gets the current position in the buffer
    /// </summary>
    /// <returns>The current position</returns>
    public int GetPosition()
    {
        return _position;
    }
    
    /// <summary>
    /// Sets the current position in the buffer
    /// </summary>
    /// <param name="position">The new position</param>
    public void SetPosition(int position)
    {
        _position = position;
    }
    
    /// <summary>
    /// Checks if the operand size prefix is present
    /// </summary>
    /// <returns>True if the operand size prefix is present</returns>
    public bool HasOperandSizePrefix()
    {
        return _operandSizePrefix;
    }
    
    /// <summary>
    /// Checks if the address size prefix is present
    /// </summary>
    /// <returns>True if the address size prefix is present</returns>
    public bool HasAddressSizePrefix()
    {
        return _addressSizePrefix;
    }
    
    /// <summary>
    /// Checks if a segment override prefix is present
    /// </summary>
    /// <returns>True if a segment override prefix is present</returns>
    public bool HasSegmentOverridePrefix()
    {
        return _segmentOverridePrefix;
    }
    
    /// <summary>
    /// Gets the segment override prefix
    /// </summary>
    /// <returns>The segment override prefix, or an empty string if none is present</returns>
    public string GetSegmentOverride()
    {
        return _segmentOverride;
    }
    
    /// <summary>
    /// Checks if the LOCK prefix is present
    /// </summary>
    /// <returns>True if the LOCK prefix is present</returns>
    public bool HasLockPrefix()
    {
        return _lockPrefix;
    }
    
    /// <summary>
    /// Checks if the REP/REPNE prefix is present
    /// </summary>
    /// <returns>True if the REP/REPNE prefix is present</returns>
    public bool HasRepPrefix()
    {
        return _repPrefix;
    }
    
    /// <summary>
    /// Reads a byte from the buffer and advances the position
    /// </summary>
    /// <returns>The byte read</returns>
    public byte ReadByte()
    {
        if (_position >= _length)
        {
            return 0;
        }
        
        return _codeBuffer[_position++];
    }
    
    /// <summary>
    /// Reads a 16-bit value from the buffer and advances the position
    /// </summary>
    /// <returns>The 16-bit value read</returns>
    public ushort ReadUInt16()
    {
        if (_position + 1 >= _length)
        {
            return 0;
        }
        
        ushort value = BitConverter.ToUInt16(_codeBuffer, _position);
        _position += 2;
        return value;
    }
    
    /// <summary>
    /// Reads a 32-bit value from the buffer and advances the position
    /// </summary>
    /// <returns>The 32-bit value read</returns>
    public uint ReadUInt32()
    {
        if (_position + 3 >= _length)
        {
            return 0;
        }
        
        uint value = BitConverter.ToUInt32(_codeBuffer, _position);
        _position += 4;
        return value;
    }
}
