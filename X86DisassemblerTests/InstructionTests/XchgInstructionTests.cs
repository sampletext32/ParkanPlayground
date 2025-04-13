using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for exchange instruction handlers
/// </summary>
public class XchgInstructionTests
{
    /// <summary>
    /// Tests the XchgEaxRegHandler for decoding NOP instruction (XCHG EAX, EAX)
    /// </summary>
    [Fact]
    public void XchgEaxRegHandler_DecodesNop_Correctly()
    {
        // Arrange
        // NOP (90) - No operation (XCHG EAX, EAX)
        byte[] codeBuffer = new byte[] { 0x90 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("nop", instruction.Mnemonic);
        Assert.Equal("", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the XchgEaxRegHandler for decoding XCHG EAX, ECX instruction
    /// </summary>
    [Fact]
    public void XchgEaxRegHandler_DecodesXchgEaxEcx_Correctly()
    {
        // Arrange
        // XCHG EAX, ECX (91) - Exchange EAX and ECX
        byte[] codeBuffer = new byte[] { 0x91 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("xchg", instruction.Mnemonic);
        Assert.Equal("eax, ecx", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the XchgEaxRegHandler for decoding XCHG EAX, EDX instruction
    /// </summary>
    [Fact]
    public void XchgEaxRegHandler_DecodesXchgEaxEdx_Correctly()
    {
        // Arrange
        // XCHG EAX, EDX (92) - Exchange EAX and EDX
        byte[] codeBuffer = new byte[] { 0x92 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("xchg", instruction.Mnemonic);
        Assert.Equal("eax, edx", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the XchgEaxRegHandler for decoding XCHG EAX, EBX instruction
    /// </summary>
    [Fact]
    public void XchgEaxRegHandler_DecodesXchgEaxEbx_Correctly()
    {
        // Arrange
        // XCHG EAX, EBX (93) - Exchange EAX and EBX
        byte[] codeBuffer = new byte[] { 0x93 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("xchg", instruction.Mnemonic);
        Assert.Equal("eax, ebx", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the XchgEaxRegHandler for decoding XCHG EAX, ESP instruction
    /// </summary>
    [Fact]
    public void XchgEaxRegHandler_DecodesXchgEaxEsp_Correctly()
    {
        // Arrange
        // XCHG EAX, ESP (94) - Exchange EAX and ESP
        byte[] codeBuffer = new byte[] { 0x94 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("xchg", instruction.Mnemonic);
        Assert.Equal("eax, esp", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the XchgEaxRegHandler for decoding XCHG EAX, EBP instruction
    /// </summary>
    [Fact]
    public void XchgEaxRegHandler_DecodesXchgEaxEbp_Correctly()
    {
        // Arrange
        // XCHG EAX, EBP (95) - Exchange EAX and EBP
        byte[] codeBuffer = new byte[] { 0x95 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("xchg", instruction.Mnemonic);
        Assert.Equal("eax, ebp", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the XchgEaxRegHandler for decoding XCHG EAX, ESI instruction
    /// </summary>
    [Fact]
    public void XchgEaxRegHandler_DecodesXchgEaxEsi_Correctly()
    {
        // Arrange
        // XCHG EAX, ESI (96) - Exchange EAX and ESI
        byte[] codeBuffer = new byte[] { 0x96 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("xchg", instruction.Mnemonic);
        Assert.Equal("eax, esi", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the XchgEaxRegHandler for decoding XCHG EAX, EDI instruction
    /// </summary>
    [Fact]
    public void XchgEaxRegHandler_DecodesXchgEaxEdi_Correctly()
    {
        // Arrange
        // XCHG EAX, EDI (97) - Exchange EAX and EDI
        byte[] codeBuffer = new byte[] { 0x97 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("xchg", instruction.Mnemonic);
        Assert.Equal("eax, edi", instruction.Operands);
    }
    
    /// <summary>
    /// Tests a sequence with NOP instructions
    /// </summary>
    [Fact]
    public void XchgEaxRegHandler_DecodesNopSequence_Correctly()
    {
        // Arrange
        // Multiple NOPs followed by XCHG EAX, ECX
        byte[] codeBuffer = new byte[] { 0x90, 0x90, 0x90, 0x91 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Equal(4, instructions.Count);
        
        // First three instructions should be NOPs
        for (int i = 0; i < 3; i++)
        {
            Assert.Equal("nop", instructions[i].Mnemonic);
            Assert.Equal("", instructions[i].Operands);
        }
        
        // Last instruction should be XCHG EAX, ECX
        Assert.Equal("xchg", instructions[3].Mnemonic);
        Assert.Equal("eax, ecx", instructions[3].Operands);
    }
}
