namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;

/// <summary>
/// Tests for MOV r/m32, imm32 instruction (0xC7)
/// </summary>
public class MovRm32Imm32Tests
{
    /// <summary>
    /// Tests the MOV r32, imm32 instruction (0xC7) with register operand
    /// </summary>
    [Fact]
    public void TestMovR32Imm32()
    {
        // Arrange
        byte[] code = { 0xC7, 0xC0, 0x78, 0x56, 0x34, 0x12 }; // MOV EAX, 0x12345678
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("mov", instructions[0].Mnemonic);
        Assert.Equal("eax, 0x12345678", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the MOV m32, imm32 instruction (0xC7) with memory operand
    /// </summary>
    [Fact]
    public void TestMovM32Imm32()
    {
        // Arrange
        byte[] code = { 0xC7, 0x00, 0x78, 0x56, 0x34, 0x12 }; // MOV DWORD PTR [EAX], 0x12345678
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("mov", instructions[0].Mnemonic);
        Assert.Equal("dword ptr [eax], 0x12345678", instructions[0].Operands);
    }
}
