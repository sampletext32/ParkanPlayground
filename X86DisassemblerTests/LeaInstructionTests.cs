namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;

/// <summary>
/// Tests for LEA instruction handlers
/// </summary>
public class LeaInstructionTests
{
    /// <summary>
    /// Tests the LEA r32, m instruction (0x8D) with simple memory operand
    /// </summary>
    [Fact]
    public void TestLeaR32M_Simple()
    {
        // Arrange
        byte[] code = { 0x8D, 0x00 }; // LEA EAX, [EAX]
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("lea", instructions[0].Mnemonic);
        Assert.Equal("eax, [eax]", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the LEA r32, m instruction (0x8D) with displacement
    /// </summary>
    [Fact]
    public void TestLeaR32M_WithDisplacement()
    {
        // Arrange
        byte[] code = { 0x8D, 0x7E, 0xFC }; // LEA EDI, [ESI - 0x4]
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("lea", instructions[0].Mnemonic);
        Assert.Equal("edi, [esi-0x04]", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the LEA r32, m instruction (0x8D) with SIB byte
    /// </summary>
    [Fact]
    public void TestLeaR32M_WithSIB()
    {
        // Arrange
        byte[] code = { 0x8D, 0x04, 0x11 }; // LEA EAX, [ECX+EDX]
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("lea", instructions[0].Mnemonic);
        Assert.Equal("eax, [ecx+edx]", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the LEA r32, m instruction (0x8D) with complex addressing
    /// </summary>
    [Fact]
    public void TestLeaR32M_Complex()
    {
        // Arrange
        byte[] code = { 0x8D, 0x44, 0x8A, 0x10 }; // LEA EAX, [EDX + ECX*4 + 0x10]
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("lea", instructions[0].Mnemonic);
        Assert.Equal("eax, [edx+ecx*4+0x10]", instructions[0].Operands);
    }
}
