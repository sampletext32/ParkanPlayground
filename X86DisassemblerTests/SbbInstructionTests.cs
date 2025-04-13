namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;

/// <summary>
/// Tests for SBB (Subtract with Borrow) instruction handlers
/// </summary>
public class SbbInstructionTests
{
    /// <summary>
    /// Tests the SbbImmFromRm32Handler for decoding SBB r/m32, imm32 instruction
    /// </summary>
    [Fact]
    public void SbbImmFromRm32Handler_DecodesSbbRm32Imm32_Correctly()
    {
        // Arrange
        // SBB EAX, 0x12345678 (81 D8 78 56 34 12) - ModR/M byte D8 = 11 011 000 (mod=3, reg=3, rm=0)
        // mod=3 means direct register addressing, reg=3 is the SBB opcode extension, rm=0 is EAX
        byte[] codeBuffer = new byte[] { 0x81, 0xD8, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("sbb", instruction.Mnemonic);
        Assert.Equal("eax, 0x12345678", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the SbbImmFromRm32SignExtendedHandler for decoding SBB r/m32, imm8 instruction
    /// </summary>
    [Fact]
    public void SbbImmFromRm32SignExtendedHandler_DecodesSbbRm32Imm8_Correctly()
    {
        // Arrange
        // SBB EAX, 0x42 (83 D8 42) - ModR/M byte D8 = 11 011 000 (mod=3, reg=3, rm=0)
        // mod=3 means direct register addressing, reg=3 is the SBB opcode extension, rm=0 is EAX
        byte[] codeBuffer = new byte[] { 0x83, 0xD8, 0x42 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("sbb", instruction.Mnemonic);
        Assert.Equal("eax, 0x00000042", instruction.Operands);
    }
}
