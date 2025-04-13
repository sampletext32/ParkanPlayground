using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for Group3 instruction handlers
/// </summary>
public class Group3InstructionTests
{
    /// <summary>
    /// Tests the NotRm32Handler for decoding NOT r/m32 instruction
    /// </summary>
    [Fact]
    public void NotRm32Handler_DecodesNotRm32_Correctly()
    {
        // Arrange
        // NOT EAX (F7 D0) - ModR/M byte D0 = 11 010 000 (mod=3, reg=2, rm=0)
        // mod=3 means direct register addressing, reg=2 indicates NOT operation, rm=0 is EAX
        byte[] codeBuffer = new byte[] { 0xF7, 0xD0 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("not", instruction.Mnemonic);
        Assert.Equal("eax", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the NegRm32Handler for decoding NEG r/m32 instruction
    /// </summary>
    [Fact]
    public void NegRm32Handler_DecodesNegRm32_Correctly()
    {
        // Arrange
        // NEG ECX (F7 D9) - ModR/M byte D9 = 11 011 001 (mod=3, reg=3, rm=1)
        // mod=3 means direct register addressing, reg=3 indicates NEG operation, rm=1 is ECX
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
    /// Tests the MulRm32Handler for decoding MUL r/m32 instruction
    /// </summary>
    [Fact]
    public void MulRm32Handler_DecodesMulRm32_Correctly()
    {
        // Arrange
        // MUL EDX (F7 E2) - ModR/M byte E2 = 11 100 010 (mod=3, reg=4, rm=2)
        // mod=3 means direct register addressing, reg=4 indicates MUL operation, rm=2 is EDX
        byte[] codeBuffer = new byte[] { 0xF7, 0xE2 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("mul", instruction.Mnemonic);
        Assert.Equal("edx", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the ImulRm32Handler for decoding IMUL r/m32 instruction
    /// </summary>
    [Fact]
    public void ImulRm32Handler_DecodesImulRm32_Correctly()
    {
        // Arrange
        // IMUL EBX (F7 EB) - ModR/M byte EB = 11 101 011 (mod=3, reg=5, rm=3)
        // mod=3 means direct register addressing, reg=5 indicates IMUL operation, rm=3 is EBX
        byte[] codeBuffer = new byte[] { 0xF7, 0xEB };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("imul", instruction.Mnemonic);
        Assert.Equal("ebx", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the DivRm32Handler for decoding DIV r/m32 instruction
    /// </summary>
    [Fact]
    public void DivRm32Handler_DecodesDivRm32_Correctly()
    {
        // Arrange
        // DIV ESP (F7 F4) - ModR/M byte F4 = 11 110 100 (mod=3, reg=6, rm=4)
        // mod=3 means direct register addressing, reg=6 indicates DIV operation, rm=4 is ESP
        byte[] codeBuffer = new byte[] { 0xF7, 0xF4 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("div", instruction.Mnemonic);
        Assert.Equal("esp", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the IdivRm32Handler for decoding IDIV r/m32 instruction
    /// </summary>
    [Fact]
    public void IdivRm32Handler_DecodesIdivRm32_Correctly()
    {
        // Arrange
        // IDIV EBP (F7 FD) - ModR/M byte FD = 11 111 101 (mod=3, reg=7, rm=5)
        // mod=3 means direct register addressing, reg=7 indicates IDIV operation, rm=5 is EBP
        byte[] codeBuffer = new byte[] { 0xF7, 0xFD };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("idiv", instruction.Mnemonic);
        Assert.Equal("ebp", instruction.Operands);
    }
}
