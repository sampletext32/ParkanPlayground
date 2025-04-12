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
    
    /// <summary>
    /// Tests the OR r32, r/m32 instruction (0x0B)
    /// </summary>
    [Fact]
    public void TestOrR32Rm32()
    {
        // Arrange
        byte[] code = { 0x0B, 0xC8 }; // OR ECX, EAX
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("or", instructions[0].Mnemonic);
        Assert.Equal("ecx, eax", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the OR r32, m32 instruction (0x0B) with memory operand
    /// </summary>
    [Fact]
    public void TestOrR32M32()
    {
        // Arrange
        byte[] code = { 0x0B, 0x00 }; // OR EAX, DWORD PTR [EAX]
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("or", instructions[0].Mnemonic);
        Assert.Equal("eax, dword ptr [eax]", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the OR AL, imm8 instruction (0x0C)
    /// </summary>
    [Fact]
    public void TestOrAlImm8()
    {
        // Arrange
        byte[] code = { 0x0C, 0x42 }; // OR AL, 0x42
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("or", instructions[0].Mnemonic);
        Assert.Equal("al, 0x42", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the OR EAX, imm32 instruction (0x0D)
    /// </summary>
    [Fact]
    public void TestOrEaxImm32()
    {
        // Arrange
        byte[] code = { 0x0D, 0x78, 0x56, 0x34, 0x12 }; // OR EAX, 0x12345678
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("or", instructions[0].Mnemonic);
        Assert.Equal("eax, 0x12345678", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the OR r/m32, imm32 instruction (0x81 /1)
    /// </summary>
    [Fact]
    public void TestOrRm32Imm32()
    {
        // Arrange
        byte[] code = { 0x81, 0xC8, 0x78, 0x56, 0x34, 0x12 }; // OR EAX, 0x12345678
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("or", instructions[0].Mnemonic);
        Assert.Equal("eax, 0x12345678", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the OR r/m32, imm8 sign-extended instruction (0x83 /1)
    /// </summary>
    [Fact]
    public void TestOrRm32Imm8SignExtended()
    {
        // Arrange
        byte[] code = { 0x83, 0xC8, 0x42 }; // OR EAX, 0x42
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("or", instructions[0].Mnemonic);
        Assert.Equal("eax, 0x00000042", instructions[0].Operands);
    }
}
