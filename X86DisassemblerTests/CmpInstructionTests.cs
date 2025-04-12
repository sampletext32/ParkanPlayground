namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;

/// <summary>
/// Tests for CMP instruction handlers
/// </summary>
public class CmpInstructionTests
{
    /// <summary>
    /// Tests the CMP r32, r/m32 instruction (0x3B) with register operand
    /// </summary>
    [Fact]
    public void TestCmpR32Rm32_Register()
    {
        // Arrange
        byte[] code = { 0x3B, 0xC7 }; // CMP EAX, EDI
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("cmp", instructions[0].Mnemonic);
        Assert.Equal("eax, edi", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the CMP r32, m32 instruction (0x3B) with memory operand
    /// </summary>
    [Fact]
    public void TestCmpR32M32()
    {
        // Arrange
        byte[] code = { 0x3B, 0x00 }; // CMP EAX, DWORD PTR [EAX]
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("cmp", instructions[0].Mnemonic);
        Assert.Equal("eax, dword ptr [eax]", instructions[0].Operands);
    }
}
