namespace X86Disassembler.X86.Handlers;

/// <summary>
/// Handler for FNSTSW instruction (0xDFE0)
/// </summary>
public class FnstswHandler : InstructionHandler
{
    /// <summary>
    /// Initializes a new instance of the FnstswHandler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to decode</param>
    /// <param name="decoder">The instruction decoder that owns this handler</param>
    /// <param name="length">The length of the buffer</param>
    public FnstswHandler(byte[] codeBuffer, InstructionDecoder decoder, int length) 
        : base(codeBuffer, decoder, length)
    {
    }
    
    /// <summary>
    /// Checks if this handler can decode the given opcode
    /// </summary>
    /// <param name="opcode">The opcode to check</param>
    /// <returns>True if this handler can decode the opcode</returns>
    public override bool CanHandle(byte opcode)
    {
        // FNSTSW is a two-byte opcode (0xDF 0xE0)
        if (opcode == 0xDF)
        {
            int position = Decoder.GetPosition();
            if (position < Length && CodeBuffer[position] == 0xE0)
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Decodes an FNSTSW instruction
    /// </summary>
    /// <param name="opcode">The opcode of the instruction</param>
    /// <param name="instruction">The instruction object to populate</param>
    /// <returns>True if the instruction was successfully decoded</returns>
    public override bool Decode(byte opcode, Instruction instruction)
    {
        int position = Decoder.GetPosition();
        
        if (position >= Length || CodeBuffer[position] != 0xE0)
        {
            return false;
        }
        
        // Skip the second byte of the opcode
        Decoder.SetPosition(position + 1);
        
        // Set the mnemonic and operands
        instruction.Mnemonic = "fnstsw";
        instruction.Operands = "ax";
        
        return true;
    }
}
