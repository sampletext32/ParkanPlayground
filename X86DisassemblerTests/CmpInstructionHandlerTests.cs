namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;
using X86Disassembler.X86.Handlers;
using X86Disassembler.X86.Handlers.Cmp;

/// <summary>
/// Tests for CMP instruction handlers
/// </summary>
public class CmpInstructionHandlerTests
{
    /// <summary>
    /// Tests the CmpAlImmHandler for decoding CMP AL, imm8 instructions
    /// </summary>
    [Fact]
    public void CmpAlImmHandler_DecodesCmpAlImm8_Correctly()
    {
        // Arrange
        // CMP AL, 0x03 (3C 03)
        byte[] codeBuffer = new byte[] { 0x3C, 0x03 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("cmp", instruction.Mnemonic);
        // The handler should produce "al, 0xXX" as the operands
        Assert.Equal("al, 0x03", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the CmpAlImmHandler with a different immediate value
    /// </summary>
    [Fact]
    public void CmpAlImmHandler_DecodesCmpAlImm8_WithDifferentValue()
    {
        // Arrange
        // CMP AL, 0xFF (3C FF)
        byte[] codeBuffer = new byte[] { 0x3C, 0xFF };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("cmp", instruction.Mnemonic);
        Assert.Equal("al, 0xFF", instruction.Operands);
    }
}
