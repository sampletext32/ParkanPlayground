namespace X86Disassembler.X86.Handlers.Group1;

/// <summary>
/// Base class for Group 1 instruction handlers (ADD, OR, ADC, SBB, AND, SUB, XOR, CMP)
/// </summary>
public abstract class Group1BaseHandler : InstructionHandler
{
    // ModR/M decoder
    protected readonly ModRMDecoder _modRMDecoder;
    
    /// <summary>
    /// Initializes a new instance of the Group1BaseHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    protected Group1BaseHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
        : base(codeBuffer, decoder, length)
    {
        _modRMDecoder = new ModRMDecoder(codeBuffer, decoder, length);
    }
    
    /// <summary>
    /// Gets the 32-bit register name for the given register index
    /// </summary>
    /// <param name="reg">The register index</param>
    /// <returns>The register name</returns>
    protected static string GetRegister32(byte reg)
    {
        string[] registerNames = { "eax", "ecx", "edx", "ebx", "esp", "ebp", "esi", "edi" };
        return registerNames[reg & 0x07];
    }
    
    /// <summary>
    /// Gets the 8-bit register name for the given register index
    /// </summary>
    /// <param name="reg">The register index</param>
    /// <returns>The register name</returns>
    protected static string GetRegister8(byte reg)
    {
        string[] registerNames = { "al", "cl", "dl", "bl", "ah", "ch", "dh", "bh" };
        return registerNames[reg & 0x07];
    }
}
