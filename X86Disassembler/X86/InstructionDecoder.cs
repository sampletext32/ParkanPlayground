using System.Diagnostics;

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
    
    // Specialized decoders
    private readonly PrefixDecoder _prefixDecoder;
    private readonly ModRMDecoder _modRMDecoder;
    
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
        
        // Create specialized decoders
        _prefixDecoder = new PrefixDecoder();
        _modRMDecoder = new ModRMDecoder(codeBuffer, this, length);
        
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
        _prefixDecoder.Reset();
        
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
            
            if (_prefixDecoder.DecodePrefix(prefix))
            {
                _position++;
            }
            else
            {
                break;
            }
        }
        
        if (_position >= _length)
        {
            // If we reached the end of the buffer while processing prefixes,
            // create an instruction with just the prefix information
            if (_prefixDecoder.HasSegmentOverridePrefix())
            {
                instruction.Mnemonic = _prefixDecoder.GetSegmentOverride();
                instruction.Operands = "";
                
                // Set the raw bytes
                int length = _position - startPosition;
                instruction.RawBytes = new byte[length];
                Array.Copy(_codeBuffer, startPosition, instruction.RawBytes, 0, length);
                
                return instruction;
            }
            
            return null;
        }
        
        // Read the opcode
        byte opcode = _codeBuffer[_position++];
        
        // Get a handler for the opcode
        var handler = _handlerFactory.GetHandler(opcode);
        
        Debug.WriteLine($"Resolved handler {handler?.GetType().Name}");
        
        bool handlerSuccess = false;
        
        // Try to decode with a handler first
        if (handler != null)
        {
            // Store the current segment override state
            bool hasSegmentOverride = _prefixDecoder.HasSegmentOverridePrefix();
            string segmentOverride = _prefixDecoder.GetSegmentOverride();
            
            // Decode the instruction
            handlerSuccess = handler.Decode(opcode, instruction);
            
            // Apply segment override prefix to the operands if needed
            if (handlerSuccess && hasSegmentOverride)
            {
                instruction.Operands = _prefixDecoder.ApplySegmentOverride(instruction.Operands);
            }
        }
        
        // If no handler is found or decoding fails, create a default instruction
        if (!handlerSuccess)
        {
            instruction.Mnemonic = OpcodeMap.GetMnemonic(opcode);
            instruction.Operands = "??";
        }
        
        // Apply REP/REPNE prefix to the mnemonic if needed
        instruction.Mnemonic = _prefixDecoder.ApplyRepPrefix(instruction.Mnemonic);
        
        // Set the raw bytes
        int bytesLength = _position - startPosition;
        instruction.RawBytes = new byte[bytesLength];
        Array.Copy(_codeBuffer, startPosition, instruction.RawBytes, 0, bytesLength);
        
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
        return _prefixDecoder.HasOperandSizePrefix();
    }
    
    /// <summary>
    /// Checks if the address size prefix is present
    /// </summary>
    /// <returns>True if the address size prefix is present</returns>
    public bool HasAddressSizePrefix()
    {
        return _prefixDecoder.HasAddressSizePrefix();
    }
    
    /// <summary>
    /// Checks if a segment override prefix is present
    /// </summary>
    /// <returns>True if a segment override prefix is present</returns>
    public bool HasSegmentOverridePrefix()
    {
        return _prefixDecoder.HasSegmentOverridePrefix();
    }
    
    /// <summary>
    /// Gets the segment override prefix
    /// </summary>
    /// <returns>The segment override prefix, or an empty string if none is present</returns>
    public string GetSegmentOverride()
    {
        return _prefixDecoder.GetSegmentOverride();
    }
    
    /// <summary>
    /// Checks if the LOCK prefix is present
    /// </summary>
    /// <returns>True if the LOCK prefix is present</returns>
    public bool HasLockPrefix()
    {
        return _prefixDecoder.HasLockPrefix();
    }
    
    /// <summary>
    /// Checks if the REP/REPNE prefix is present
    /// </summary>
    /// <returns>True if the REP/REPNE prefix is present</returns>
    public bool HasRepPrefix()
    {
        return _prefixDecoder.HasRepPrefix();
    }
    
    /// <summary>
    /// Checks if the instruction has an operand size override prefix (0x66)
    /// </summary>
    /// <returns>True if the instruction has an operand size override prefix</returns>
    public bool HasOperandSizeOverridePrefix()
    {
        return _prefixDecoder.HasOperandSizePrefix();
    }

    /// <summary>
    /// Checks if a single byte can be read from the current position
    /// </summary>
    /// <returns>True if there is at least one byte available to read</returns>
    public bool CanReadByte()
    {
        return _position < _length;
    }

    /// <summary>
    /// Checks if a 16-bit unsigned short (2 bytes) can be read from the current position
    /// </summary>
    /// <returns>True if there are at least two bytes available to read</returns>
    public bool CanReadUShort()
    {
        return _position + 1 < _length;
    }

    /// <summary>
    /// Checks if a 32-bit unsigned integer (4 bytes) can be read from the current position
    /// </summary>
    /// <returns>True if there are at least four bytes available to read</returns>
    public bool CanReadUInt()
    {
        return _position + 3 < _length;
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
        
        ushort value = (ushort)(_codeBuffer[_position] | (_codeBuffer[_position + 1] << 8));
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
        
        uint value = (uint)(_codeBuffer[_position] | 
                          (_codeBuffer[_position + 1] << 8) | 
                          (_codeBuffer[_position + 2] << 16) | 
                          (_codeBuffer[_position + 3] << 24));
        _position += 4;
        return value;
    }
}
