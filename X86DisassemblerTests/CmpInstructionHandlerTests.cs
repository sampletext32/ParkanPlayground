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
    
    /// <summary>
    /// Tests the CmpRm32R32Handler for decoding CMP r/m32, r32 instructions with register operands
    /// </summary>
    [Fact]
    public void CmpRm32R32Handler_DecodesCmpRm32R32_WithRegisterOperands()
    {
        // Arrange
        // CMP ECX, EAX (39 C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        // mod=3 means direct register addressing, reg=0 is EAX, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0x39, 0xC1 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("cmp", instruction.Mnemonic);
        Assert.Equal("ecx, eax", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the CmpRm32R32Handler for decoding CMP r/m32, r32 instructions with memory operands
    /// </summary>
    [Fact]
    public void CmpRm32R32Handler_DecodesCmpRm32R32_WithMemoryOperands()
    {
        // Arrange
        // CMP [EBX+0x10], EDX (39 53 10) - ModR/M byte 53 = 01 010 011 (mod=1, reg=2, rm=3)
        // mod=1 means memory addressing with 8-bit displacement, reg=2 is EDX, rm=3 is EBX
        byte[] codeBuffer = new byte[] { 0x39, 0x53, 0x10 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("cmp", instruction.Mnemonic);
        Assert.Equal("[ebx+0x10], edx", instruction.Operands);
    }
}
