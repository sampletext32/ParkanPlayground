using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for the InstructionDecoder class
/// </summary>
public class InstructionDecoderTests
{
    /// <summary>
    /// Tests that the decoder correctly decodes a TEST AH, imm8 instruction
    /// </summary>
    [Fact]
    public void DecodeInstruction_DecodesTestAhImm8_Correctly()
    {
        // Arrange
        // TEST AH, 0x01 (F6 C4 01) - ModR/M byte C4 = 11 000 100 (mod=3, reg=0, rm=4)
        byte[] codeBuffer = new byte[] { 0xF6, 0xC4, 0x01 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("test", instruction.Mnemonic);
        // The actual implementation produces "ah, 0x01" as the operands
        Assert.Equal("ah, 0x01", instruction.Operands);
        Assert.Equal(3, instruction.RawBytes.Length);
        Assert.Equal(0xF6, instruction.RawBytes[0]);
        Assert.Equal(0xC4, instruction.RawBytes[1]);
        Assert.Equal(0x01, instruction.RawBytes[2]);
    }
    
    /// <summary>
    /// Tests that the decoder correctly decodes a TEST r/m8, r8 instruction
    /// </summary>
    [Fact]
    public void DecodeInstruction_DecodesTestRm8R8_Correctly()
    {
        // Arrange
        // TEST CL, AL (84 C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        byte[] codeBuffer = new byte[] { 0x84, 0xC1 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("test", instruction.Mnemonic);
        Assert.Equal("cl, al", instruction.Operands);
        Assert.Equal(2, instruction.RawBytes.Length);
        Assert.Equal(0x84, instruction.RawBytes[0]);
        Assert.Equal(0xC1, instruction.RawBytes[1]);
    }
    
    /// <summary>
    /// Tests that the decoder correctly decodes a TEST r/m32, r32 instruction
    /// </summary>
    [Fact]
    public void DecodeInstruction_DecodesTestRm32R32_Correctly()
    {
        // Arrange
        // TEST ECX, EAX (85 C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        byte[] codeBuffer = new byte[] { 0x85, 0xC1 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("test", instruction.Mnemonic);
        Assert.Equal("ecx, eax", instruction.Operands);
        Assert.Equal(2, instruction.RawBytes.Length);
        Assert.Equal(0x85, instruction.RawBytes[0]);
        Assert.Equal(0xC1, instruction.RawBytes[1]);
    }
    
    /// <summary>
    /// Tests that the decoder correctly decodes a TEST AL, imm8 instruction
    /// </summary>
    [Fact]
    public void DecodeInstruction_DecodesTestAlImm8_Correctly()
    {
        // Arrange
        // TEST AL, 0x42 (A8 42)
        byte[] codeBuffer = new byte[] { 0xA8, 0x42 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("test", instruction.Mnemonic);
        Assert.Equal("al, 0x42", instruction.Operands);
        Assert.Equal(2, instruction.RawBytes.Length);
        Assert.Equal(0xA8, instruction.RawBytes[0]);
        Assert.Equal(0x42, instruction.RawBytes[1]);
    }
    
    /// <summary>
    /// Tests that the decoder correctly decodes a TEST EAX, imm32 instruction
    /// </summary>
    [Fact]
    public void DecodeInstruction_DecodesTestEaxImm32_Correctly()
    {
        // Arrange
        // TEST EAX, 0x12345678 (A9 78 56 34 12)
        byte[] codeBuffer = new byte[] { 0xA9, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("test", instruction.Mnemonic);
        Assert.Equal("eax, 0x12345678", instruction.Operands);
        Assert.Equal(5, instruction.RawBytes.Length);
        Assert.Equal(0xA9, instruction.RawBytes[0]);
        Assert.Equal(0x78, instruction.RawBytes[1]);
        Assert.Equal(0x56, instruction.RawBytes[2]);
        Assert.Equal(0x34, instruction.RawBytes[3]);
        Assert.Equal(0x12, instruction.RawBytes[4]);
    }
    
    /// <summary>
    /// Tests that the decoder correctly decodes a TEST r/m32, imm32 instruction
    /// </summary>
    [Fact]
    public void DecodeInstruction_DecodesTestRm32Imm32_Correctly()
    {
        // Arrange
        // TEST EDI, 0x12345678 (F7 C7 78 56 34 12) - ModR/M byte C7 = 11 000 111 (mod=3, reg=0, rm=7)
        byte[] codeBuffer = new byte[] { 0xF7, 0xC7, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("test", instruction.Mnemonic);
        Assert.Equal("edi, 0x12345678", instruction.Operands);
        Assert.Equal(6, instruction.RawBytes.Length);
        Assert.Equal(0xF7, instruction.RawBytes[0]);
        Assert.Equal(0xC7, instruction.RawBytes[1]);
        Assert.Equal(0x78, instruction.RawBytes[2]);
        Assert.Equal(0x56, instruction.RawBytes[3]);
        Assert.Equal(0x34, instruction.RawBytes[4]);
        Assert.Equal(0x12, instruction.RawBytes[5]);
    }
    
    /// <summary>
    /// Tests that the decoder correctly handles multiple instructions in sequence
    /// </summary>
    [Fact]
    public void DecodeInstruction_HandlesMultipleInstructions_Correctly()
    {
        // Arrange
        // TEST AH, 0x01 (F6 C4 01)
        // JZ +45 (74 2D)
        byte[] codeBuffer = new byte[] { 0xF6, 0xC4, 0x01, 0x74, 0x2D };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act - First instruction
        var instruction1 = decoder.DecodeInstruction();
        
        // Assert - First instruction
        Assert.NotNull(instruction1);
        Assert.Equal("test", instruction1.Mnemonic);
        Assert.Equal("ah, 0x01", instruction1.Operands);
        
        // Act - Second instruction
        var instruction2 = decoder.DecodeInstruction();
        
        // Assert - Second instruction
        Assert.NotNull(instruction2);
        Assert.Equal("jz", instruction2.Mnemonic);
        // The correct target address according to x86 architecture
        Assert.Equal("0x00000032", instruction2.Operands);
    }
}
