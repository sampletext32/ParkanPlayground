using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for return instruction handlers
/// </summary>
public class ReturnInstructionTests
{
    /// <summary>
    /// Tests the RetHandler for decoding RET instruction
    /// </summary>
    [Fact]
    public void RetHandler_DecodesRet_Correctly()
    {
        // Arrange
        // RET (C3) - Return from procedure
        byte[] codeBuffer = new byte[] { 0xC3 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("ret", instruction.Mnemonic);
        Assert.Equal("", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the RetImmHandler for decoding RET imm16 instruction
    /// </summary>
    [Fact]
    public void RetImmHandler_DecodesRetImm16_Correctly()
    {
        // Arrange
        // RET 0x1234 (C2 34 12) - Return from procedure and pop 0x1234 bytes
        byte[] codeBuffer = new byte[] { 0xC2, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("ret", instruction.Mnemonic);
        Assert.Equal("0x1234", instruction.Operands);
    }
}
