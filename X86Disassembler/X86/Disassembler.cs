using System.Text;

namespace X86Disassembler.X86;

/// <summary>
/// Core x86 instruction disassembler
/// </summary>
public class Disassembler
{
    // Buffer containing the code to disassemble
    private readonly byte[] _codeBuffer;
    
    // Base address for the code (RVA)
    private readonly ulong _baseAddress;
    
    // Current position in the code buffer
    private int _position;
    
    // Instruction decoder
    private readonly InstructionDecoder _decoder;
    
    /// <summary>
    /// Initializes a new instance of the Disassembler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to disassemble</param>
    /// <param name="baseAddress">The base address (RVA) of the code</param>
    public Disassembler(byte[] codeBuffer, ulong baseAddress)
    {
        _codeBuffer = codeBuffer;
        _baseAddress = baseAddress;
        _position = 0;
        _decoder = new InstructionDecoder(codeBuffer);
    }
    
    /// <summary>
    /// Disassembles the next instruction in the code buffer
    /// </summary>
    /// <returns>The disassembled instruction, or null if the end of the buffer is reached</returns>
    public Instruction? DisassembleNext()
    {
        if (_position >= _codeBuffer.Length)
        {
            return null; // End of buffer reached
        }
        
        // Create a new instruction
        Instruction instruction = new Instruction
        {
            Address = _baseAddress + (uint)_position
        };
        
        // Decode the instruction
        int bytesRead = _decoder.DecodeAt(_position, instruction);
        
        if (bytesRead == 0)
        {
            return null; // Failed to decode instruction
        }
        
        // Update position
        _position += bytesRead;
        
        return instruction;
    }
    
    /// <summary>
    /// Disassembles all instructions in the code buffer
    /// </summary>
    /// <returns>A list of disassembled instructions</returns>
    public List<Instruction> DisassembleAll()
    {
        List<Instruction> instructions = new List<Instruction>();
        
        // Reset position
        _position = 0;
        
        // Disassemble all instructions
        Instruction? instruction;
        while ((instruction = DisassembleNext()) != null)
        {
            instructions.Add(instruction);
        }
        
        return instructions;
    }
}
