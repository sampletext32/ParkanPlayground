using System.Text;
using System.Collections.Generic;

namespace X86Disassembler.X86;

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
    private readonly uint _baseAddress;
    
    /// <summary>
    /// Initializes a new instance of the Disassembler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to disassemble</param>
    /// <param name="baseAddress">The base address of the code</param>
    public Disassembler(byte[] codeBuffer, uint baseAddress)
    {
        _codeBuffer = codeBuffer;
        _length = codeBuffer.Length;
        _baseAddress = baseAddress;
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
            if (position >= _length)
            {
                break;
            }
            
            // Decode the next instruction
            Instruction? instruction = decoder.DecodeInstruction();
            
            // Check if decoding failed
            if (instruction == null)
            {
                break;
            }
            
            // Adjust the instruction address to include the base address
            instruction.Address += _baseAddress;
            
            // Add the instruction to the list
            instructions.Add(instruction);
        }
        
        return instructions;
    }
}
