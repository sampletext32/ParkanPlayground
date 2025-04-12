namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;

/// <summary>
/// Tests for OR instruction handlers
/// </summary>
public class OrInstructionTests
{
    /// <summary>
    /// Tests the OR r8, r/m8 instruction (0x0A)
    /// </summary>
    [Fact]
    public void TestOrR8Rm8()
    {
        // Arrange
        byte[] code = { 0x0A, 0xC8 }; // OR CL, AL
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("or", instructions[0].Mnemonic);
        Assert.Equal("cl, al", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the OR r8, m8 instruction (0x0A) with memory operand
    /// </summary>
    [Fact]
    public void TestOrR8M8()
    {
        // Arrange
        byte[] code = { 0x0A, 0x00 }; // OR AL, BYTE PTR [EAX]
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("or", instructions[0].Mnemonic);
        Assert.Equal("al, byte ptr [eax]", instructions[0].Operands);
    }
}
