namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;
using X86Disassembler.X86.Handlers.Group1;

/// <summary>
/// Tests for Group1 instruction handlers
/// </summary>
public class Group1InstructionTests
{
    /// <summary>
    /// Tests the AddImmToRm8Handler for decoding ADD r/m8, imm8 instruction
    /// </summary>
    [Fact]
    public void AddImmToRm8Handler_DecodesAddRm8Imm8_Correctly()
    {
        // Arrange
        // ADD AL, 0x42 (80 C0 42) - ModR/M byte C0 = 11 000 000 (mod=3, reg=0, rm=0)
        // mod=3 means direct register addressing, reg=0 indicates ADD operation, rm=0 is AL
        byte[] codeBuffer = new byte[] { 0x80, 0xC0, 0x42 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("add", instruction.Mnemonic);
        Assert.Equal("al, 0x42", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the AddImmToRm32Handler for decoding ADD r/m32, imm32 instruction
    /// </summary>
    [Fact]
    public void AddImmToRm32Handler_DecodesAddRm32Imm32_Correctly()
    {
        // Arrange
        // ADD ECX, 0x12345678 (81 C1 78 56 34 12) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        // mod=3 means direct register addressing, reg=0 indicates ADD operation, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0x81, 0xC1, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("add", instruction.Mnemonic);
        Assert.Equal("ecx, 0x12345678", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the OrImmToRm8Handler for decoding OR r/m8, imm8 instruction
    /// </summary>
    [Fact]
    public void OrImmToRm8Handler_DecodesOrRm8Imm8_Correctly()
    {
        // Arrange
        // OR BL, 0x42 (80 CB 42) - ModR/M byte CB = 11 001 011 (mod=3, reg=1, rm=3)
        // mod=3 means direct register addressing, reg=1 indicates OR operation, rm=3 is BL
        byte[] codeBuffer = new byte[] { 0x80, 0xCB, 0x42 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("or", instruction.Mnemonic);
        Assert.Equal("bl, 0x42", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the SubImmFromRm32Handler for decoding SUB r/m32, imm32 instruction
    /// </summary>
    [Fact]
    public void SubImmFromRm32Handler_DecodesSubRm32Imm32_Correctly()
    {
        // Arrange
        // SUB EDX, 0x12345678 (81 EA 78 56 34 12) - ModR/M byte EA = 11 101 010 (mod=3, reg=5, rm=2)
        // mod=3 means direct register addressing, reg=5 indicates SUB operation, rm=2 is EDX
        byte[] codeBuffer = new byte[] { 0x81, 0xEA, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("sub", instruction.Mnemonic);
        Assert.Equal("edx, 0x12345678", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the CmpImmWithRm32Handler for decoding CMP r/m32, imm32 instruction
    /// </summary>
    [Fact]
    public void CmpImmWithRm32Handler_DecodesCmpRm32Imm32_Correctly()
    {
        // Arrange
        // CMP EBX, 0x12345678 (81 FB 78 56 34 12) - ModR/M byte FB = 11 111 011 (mod=3, reg=7, rm=3)
        // mod=3 means direct register addressing, reg=7 indicates CMP operation, rm=3 is EBX
        byte[] codeBuffer = new byte[] { 0x81, 0xFB, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("cmp", instruction.Mnemonic);
        Assert.Equal("ebx, 0x12345678", instruction.Operands);
    }
}
