namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;

/// <summary>
/// Tests for MOV r/m8, imm8 instruction (0xC6)
/// </summary>
public class MovRm8Imm8Tests
{
    /// <summary>
    /// Tests the MOV r8, imm8 instruction (0xC6) with register operand
    /// </summary>
    [Fact]
    public void TestMovR8Imm8()
    {
        // Arrange
        byte[] code = { 0xC6, 0xC0, 0x42 }; // MOV AL, 0x42
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("mov", instructions[0].Mnemonic);
        Assert.Equal("al, 0x42", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the MOV m8, imm8 instruction (0xC6) with memory operand
    /// </summary>
    [Fact]
    public void TestMovM8Imm8()
    {
        // Arrange
        byte[] code = { 0xC6, 0x01, 0x01 }; // MOV BYTE PTR [ECX], 0x01
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("mov", instructions[0].Mnemonic);
        Assert.Equal("byte ptr [ecx], 0x01", instructions[0].Operands);
    }
}
