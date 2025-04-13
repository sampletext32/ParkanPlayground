using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for arithmetic unary operations (DIV, IDIV, MUL, IMUL, NEG, NOT)
/// </summary>
public class ArithmeticUnaryTests
{
    /// <summary>
    /// Tests the DivRm32Handler for decoding DIV r/m32 instruction
    /// </summary>
    [Fact]
    public void DivRm32Handler_DecodesDivRm32_Correctly()
    {
        // Arrange
        // DIV ECX (F7 F1) - ModR/M byte F1 = 11 110 001 (mod=3, reg=6, rm=1)
        // mod=3 means direct register addressing, reg=6 is the DIV opcode extension, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0xF7, 0xF1 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("div", instruction.Mnemonic);
        Assert.Equal("ecx", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the IdivRm32Handler for decoding IDIV r/m32 instruction
    /// </summary>
    [Fact]
    public void IdivRm32Handler_DecodesIdivRm32_Correctly()
    {
        // Arrange
        // IDIV ECX (F7 F9) - ModR/M byte F9 = 11 111 001 (mod=3, reg=7, rm=1)
        // mod=3 means direct register addressing, reg=7 is the IDIV opcode extension, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0xF7, 0xF9 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("idiv", instruction.Mnemonic);
        Assert.Equal("ecx", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the MulRm32Handler for decoding MUL r/m32 instruction
    /// </summary>
    [Fact]
    public void MulRm32Handler_DecodesMulRm32_Correctly()
    {
        // Arrange
        // MUL ECX (F7 E1) - ModR/M byte E1 = 11 100 001 (mod=3, reg=4, rm=1)
        // mod=3 means direct register addressing, reg=4 is the MUL opcode extension, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0xF7, 0xE1 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("mul", instruction.Mnemonic);
        Assert.Equal("ecx", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the ImulRm32Handler for decoding IMUL r/m32 instruction
    /// </summary>
    [Fact]
    public void ImulRm32Handler_DecodesImulRm32_Correctly()
    {
        // Arrange
        // IMUL ECX (F7 E9) - ModR/M byte E9 = 11 101 001 (mod=3, reg=5, rm=1)
        // mod=3 means direct register addressing, reg=5 is the IMUL opcode extension, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0xF7, 0xE9 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("imul", instruction.Mnemonic);
        Assert.Equal("ecx", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the NegRm32Handler for decoding NEG r/m32 instruction
    /// </summary>
    [Fact]
    public void NegRm32Handler_DecodesNegRm32_Correctly()
    {
        // Arrange
        // NEG ECX (F7 D9) - ModR/M byte D9 = 11 011 001 (mod=3, reg=3, rm=1)
        // mod=3 means direct register addressing, reg=3 is the NEG opcode extension, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0xF7, 0xD9 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("neg", instruction.Mnemonic);
        Assert.Equal("ecx", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the NotRm32Handler for decoding NOT r/m32 instruction
    /// </summary>
    [Fact]
    public void NotRm32Handler_DecodesNotRm32_Correctly()
    {
        // Arrange
        // NOT ECX (F7 D1) - ModR/M byte D1 = 11 010 001 (mod=3, reg=2, rm=1)
        // mod=3 means direct register addressing, reg=2 is the NOT opcode extension, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0xF7, 0xD1 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("not", instruction.Mnemonic);
        Assert.Equal("ecx", instruction.Operands);
    }
}
