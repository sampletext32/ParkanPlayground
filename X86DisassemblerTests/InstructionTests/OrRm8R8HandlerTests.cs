using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for OR r/m8, r8 instruction handler
/// </summary>
public class OrRm8R8HandlerTests
{
    /// <summary>
    /// Tests the OrRm8R8Handler for decoding OR [mem], reg8 instruction
    /// </summary>
    [Fact]
    public void OrRm8R8Handler_DecodesOrMemReg8_Correctly()
    {
        // Arrange
        // OR [ebx+ecx*4+0x41], al (08 44 8B 41)
        byte[] codeBuffer = new byte[] { 0x08, 0x44, 0x8B, 0x41 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("or", instruction.Mnemonic);
        Assert.Equal("byte ptr [ebx+ecx*4+0x41], al", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the OrRm8R8Handler for decoding OR reg8, reg8 instruction
    /// </summary>
    [Fact]
    public void OrRm8R8Handler_DecodesOrRegReg8_Correctly()
    {
        // Arrange
        // OR bl, ch (08 EB)
        byte[] codeBuffer = new byte[] { 0x08, 0xEB };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("or", instruction.Mnemonic);
        Assert.Equal("bl, ch", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the OrRm8R8Handler for handling insufficient bytes
    /// </summary>
    [Fact]
    public void OrRm8R8Handler_HandlesInsufficientBytes_Gracefully()
    {
        // Arrange
        // OR ?? (08) - missing ModR/M byte
        byte[] codeBuffer = new byte[] { 0x08 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("or", instruction.Mnemonic);
        Assert.Equal("??", instruction.Operands);
    }
}
