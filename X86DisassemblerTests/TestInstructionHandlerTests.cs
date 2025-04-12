using X86Disassembler.X86.Handlers.Test;

namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;
using X86Disassembler.X86.Handlers;

/// <summary>
/// Tests for TEST instruction handlers
/// </summary>
public class TestInstructionHandlerTests
{
    /// <summary>
    /// Tests the TestRegMemHandler for decoding TEST r/m32, r32 instructions
    /// </summary>
    [Fact]
    public void TestRegMemHandler_DecodesTestR32R32_Correctly()
    {
        // Arrange
        // TEST ECX, EAX (85 C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        // mod=3 means direct register addressing, reg=0 is EAX, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0x85, 0xC1 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("test", instruction.Mnemonic);
        Assert.Equal("ecx, eax", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the TestRegMem8Handler for decoding TEST r/m8, r8 instructions
    /// </summary>
    [Fact]
    public void TestRegMem8Handler_DecodesTestR8R8_Correctly()
    {
        // Arrange
        // TEST CL, AL (84 C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        // mod=3 means direct register addressing, reg=0 is AL, rm=1 is CL
        byte[] codeBuffer = new byte[] { 0x84, 0xC1 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("test", instruction.Mnemonic);
        Assert.Equal("cl, al", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the TestAlImmHandler for decoding TEST AL, imm8 instructions
    /// </summary>
    [Fact]
    public void TestAlImmHandler_DecodesTestAlImm8_Correctly()
    {
        // Arrange
        // TEST AL, 0x42 (A8 42)
        byte[] codeBuffer = new byte[] { 0xA8, 0x42 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("test", instruction.Mnemonic);
        // The handler should produce "al, 0xXX" as the operands
        Assert.Equal("al, 0x42", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the TestEaxImmHandler for decoding TEST EAX, imm32 instructions
    /// </summary>
    [Fact]
    public void TestEaxImmHandler_DecodesTestEaxImm32_Correctly()
    {
        // Arrange
        // TEST EAX, 0x12345678 (A9 78 56 34 12)
        byte[] codeBuffer = new byte[] { 0xA9, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("test", instruction.Mnemonic);
        // The handler should produce "eax, 0xXXXXXXXX" as the operands
        Assert.Equal("eax, 0x12345678", instruction.Operands);
    }

    /// <summary>
    /// Tests the TestImmWithRm8Handler for decoding TEST r/m8, imm8 instructions
    /// </summary>
    [Fact]
    public void TestImmWithRm8Handler_DecodesTestRm8Imm8_Correctly()
    {
        // Arrange
        // TEST AH, 0x01 (F6 C4 01) - ModR/M byte C4 = 11 000 100 (mod=3, reg=0, rm=4)
        // mod=3 means direct register addressing, reg=0 indicates TEST operation, rm=4 is AH
        byte[] codeBuffer = new byte[] { 0xF6, 0xC4, 0x01 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("test", instruction.Mnemonic);
        Assert.Equal("ah, 0x01", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the TestImmWithRm32Handler for decoding TEST r/m32, imm32 instructions
    /// </summary>
    [Fact]
    public void TestImmWithRm32Handler_DecodesTestRm32Imm32_Correctly()
    {
        // Arrange
        // TEST EDI, 0x12345678 (F7 C7 78 56 34 12) - ModR/M byte C7 = 11 000 111 (mod=3, reg=0, rm=7)
        // mod=3 means direct register addressing, reg=0 indicates TEST operation, rm=7 is EDI
        byte[] codeBuffer = new byte[] { 0xF7, 0xC7, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("test", instruction.Mnemonic);
        Assert.Equal("edi, 0x12345678", instruction.Operands);
    }
}
