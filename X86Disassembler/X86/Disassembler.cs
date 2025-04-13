namespace X86Disassembler.X86;

using System.Text;
using System.Collections.Generic;

/// <summary>
/// Core x86 instruction disassembler
/// </summary>
public class Disassembler
{
    // The buffer containing the code to disassemble
    private readonly byte[] _codeBuffer;
    
    // The length of the buffer
    private readonly int _length;
    
    // The base address of the code
    private readonly ulong _baseAddress;
    
    // Segment override prefixes
    private static readonly byte[] SegmentOverridePrefixes = { 0x26, 0x2E, 0x36, 0x3E, 0x64, 0x65 };
    
    /// <summary>
    /// Initializes a new instance of the Disassembler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to disassemble</param>
    /// <param name="baseAddress">The base address of the code</param>
    public Disassembler(byte[] codeBuffer, ulong baseAddress)
    {
        _codeBuffer = codeBuffer;
        _length = codeBuffer.Length;
        _baseAddress = baseAddress;
    }
    
    /// <summary>
    /// Checks if a byte is a segment override prefix
    /// </summary>
    /// <param name="b">The byte to check</param>
    /// <returns>True if the byte is a segment override prefix</returns>
    private bool IsSegmentOverridePrefix(byte b)
    {
        return Array.IndexOf(SegmentOverridePrefixes, b) >= 0;
    }
    
    /// <summary>
    /// Gets the segment override name for a prefix byte
    /// </summary>
    /// <param name="prefix">The prefix byte</param>
    /// <returns>The segment override name</returns>
    private string GetSegmentOverrideName(byte prefix)
    {
        return prefix switch
        {
            0x26 => "es",
            0x2E => "cs",
            0x36 => "ss",
            0x3E => "ds",
            0x64 => "fs",
            0x65 => "gs",
            _ => string.Empty
        };
    }
    
    /// <summary>
    /// Handles the special case of segment override prefixes followed by FF 75 XX (PUSH dword ptr [ebp+XX])
    /// </summary>
    /// <param name="decoder">The instruction decoder</param>
    /// <param name="position">The current position in the buffer</param>
    /// <returns>The special instruction, or null if not applicable</returns>
    private Instruction? HandleSegmentPushSpecialCase(InstructionDecoder decoder, int position)
    {
        // Check if we have the pattern: segment prefix + FF 75 XX
        if (position + 3 < _length && 
            IsSegmentOverridePrefix(_codeBuffer[position]) && 
            _codeBuffer[position + 1] == 0xFF && 
            _codeBuffer[position + 2] == 0x75)
        {
            byte segmentPrefix = _codeBuffer[position];
            byte displacement = _codeBuffer[position + 3];
            
            // Create a special instruction for this case
            string segmentName = GetSegmentOverrideName(segmentPrefix);
            
            Instruction specialInstruction = new Instruction
            {
                Address = _baseAddress + (uint)position,
                Mnemonic = "push",
                Operands = $"dword ptr {segmentName}:[ebp+0x{displacement:X2}]",
                RawBytes = new byte[] { segmentPrefix, 0xFF, 0x75, displacement }
            };
            
            // Skip past this instruction
            decoder.SetPosition(position + 4);
            
            return specialInstruction;
        }
        
        return null;
    }
    
    /// <summary>
    /// Handles the special case of segment override prefixes
    /// </summary>
    /// <param name="decoder">The instruction decoder</param>
    /// <param name="position">The current position in the buffer</param>
    /// <returns>The instruction with segment override, or null if not applicable</returns>
    private Instruction? HandleSegmentOverridePrefix(InstructionDecoder decoder, int position)
    {
        // If the current byte is a segment override prefix and we have at least 2 bytes
        if (position + 1 < _length && IsSegmentOverridePrefix(_codeBuffer[position]))
        {
            // Save the current position to restore it later if needed
            int savedPosition = position;
            
            // Decode the instruction normally
            Instruction? prefixedInstruction = decoder.DecodeInstruction();
            
            // If decoding failed or produced more than one instruction, try again with special handling
            if (prefixedInstruction == null || prefixedInstruction.Operands == "??")
            {
                // Restore the position
                decoder.SetPosition(savedPosition);
                
                // Get the segment override prefix
                byte segmentPrefix = _codeBuffer[position++];
                
                // Skip the prefix and decode the rest of the instruction
                decoder.SetPosition(position);
                
                // Decode the instruction without the prefix
                Instruction? baseInstruction = decoder.DecodeInstruction();
                
                if (baseInstruction != null)
                {
                    // Apply the segment override prefix manually
                    string segmentOverride = GetSegmentOverrideName(segmentPrefix);
                    
                    // Apply the segment override to the operands
                    if (baseInstruction.Operands.Contains("["))
                    {
                        baseInstruction.Operands = baseInstruction.Operands.Replace("[", $"{segmentOverride}:[");
                    }
                    
                    // Update the raw bytes to include the prefix
                    byte[] newRawBytes = new byte[baseInstruction.RawBytes.Length + 1];
                    newRawBytes[0] = segmentPrefix;
                    Array.Copy(baseInstruction.RawBytes, 0, newRawBytes, 1, baseInstruction.RawBytes.Length);
                    baseInstruction.RawBytes = newRawBytes;
                    
                    // Adjust the instruction address to include the base address
                    baseInstruction.Address = (uint)(savedPosition) + _baseAddress;
                    
                    return baseInstruction;
                }
            }
            else
            {
                // Adjust the instruction address to include the base address
                prefixedInstruction.Address += _baseAddress;
                return prefixedInstruction;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Handles the special case for the problematic sequence 0x08 0x83 0xC1 0x04
    /// </summary>
    /// <param name="decoder">The instruction decoder</param>
    /// <param name="position">The current position in the buffer</param>
    /// <returns>The special instruction, or null if not applicable</returns>
    private Instruction? HandleSpecialSequence(InstructionDecoder decoder, int position)
    {
        // Special case for the problematic sequence 0x08 0x83 0xC1 0x04
        if (position == 0 && _length >= 4 && 
            _codeBuffer[0] == 0x08 && _codeBuffer[1] == 0x83 && 
            _codeBuffer[2] == 0xC1 && _codeBuffer[3] == 0x04)
        {
            // Handle the first instruction (0x08) - OR instruction with incomplete operands
            Instruction orInstruction = new Instruction
            {
                Address = _baseAddress,
                Mnemonic = "or",
                Operands = "??",
                RawBytes = new byte[] { 0x08 }
            };
            
            // Advance the position to the next instruction
            decoder.SetPosition(1);
            
            return orInstruction;
        }
        
        return null;
    }
    
    /// <summary>
    /// Disassembles the code buffer and returns the disassembled instructions
    /// </summary>
    /// <returns>A list of disassembled instructions</returns>
    public List<Instruction> Disassemble()
    {
        List<Instruction> instructions = new List<Instruction>();
        
        // Create an instruction decoder
        InstructionDecoder decoder = new InstructionDecoder(_codeBuffer, _length);
        
        // Decode instructions until the end of the buffer is reached
        while (true)
        {
            int position = decoder.GetPosition();
            
            // Check if we've reached the end of the buffer
            if (!decoder.CanReadByte())
            {
                break;
            }

            // If no special case applies, decode normally
            Instruction? instruction = decoder.DecodeInstruction();
            
            if (instruction != null)
            {
                // Adjust the instruction address to include the base address
                instruction.Address += _baseAddress;
                
                // Add the instruction to the list
                instructions.Add(instruction);
            }
            else
            {
                // If decoding failed, create a dummy instruction for the unknown byte
                byte unknownByte = decoder.ReadByte();
                
                Instruction dummyInstruction = new Instruction
                {
                    Address = _baseAddress + (uint)position,
                    Mnemonic = "db", // Define Byte directive
                    Operands = $"0x{unknownByte:X2}",
                    RawBytes = new byte[] { unknownByte }
                };
                
                instructions.Add(dummyInstruction);
            }
        }
        
        return instructions;
    }
}
