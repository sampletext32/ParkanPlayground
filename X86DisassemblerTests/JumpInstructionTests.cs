namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;
using X86Disassembler.X86.Handlers.Jump;

/// <summary>
/// Tests for jump instruction handlers
/// </summary>
public class JumpInstructionTests
{
    /// <summary>
    /// Tests the JmpRel8Handler for decoding JMP rel8 instruction
    /// </summary>
    [Fact]
    public void JmpRel8Handler_DecodesJmpRel8_Correctly()
    {
        // Arrange
        // JMP +5 (EB 05) - Jump 5 bytes forward
        byte[] codeBuffer = new byte[] { 0xEB, 0x05 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("jmp", instruction.Mnemonic);
        Assert.Equal("0x00000007", instruction.Operands); // Current position (2) + offset (5) = 7
    }
    
    /// <summary>
    /// Tests the JmpRel32Handler for decoding JMP rel32 instruction
    /// </summary>
    [Fact]
    public void JmpRel32Handler_DecodesJmpRel32_Correctly()
    {
        // Arrange
        // JMP +0x12345678 (E9 78 56 34 12) - Jump 0x12345678 bytes forward
        byte[] codeBuffer = new byte[] { 0xE9, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("jmp", instruction.Mnemonic);
        Assert.Equal("0x1234567D", instruction.Operands); // Current position (5) + offset (0x12345678) = 0x1234567D
    }
    
    /// <summary>
    /// Tests the ConditionalJumpHandler for decoding JZ rel8 instruction
    /// </summary>
    [Fact]
    public void ConditionalJumpHandler_DecodesJzRel8_Correctly()
    {
        // Arrange
        // JZ +10 (74 0A) - Jump 10 bytes forward if zero/equal
        // Note: JZ and JE are equivalent in x86
        byte[] codeBuffer = new byte[] { 0x74, 0x0A };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("jz", instruction.Mnemonic);
        Assert.Equal("0x0000000C", instruction.Operands); // Current position (2) + offset (10) = 12 (0x0C)
    }
    
    /// <summary>
    /// Tests the TwoByteConditionalJumpHandler for decoding JNZ rel32 instruction
    /// </summary>
    [Fact]
    public void TwoByteConditionalJumpHandler_DecodesJnzRel32_Correctly()
    {
        // Arrange
        // JNZ +0x12345678 (0F 85 78 56 34 12) - Jump 0x12345678 bytes forward if not zero/not equal
        // Note: JNZ and JNE are equivalent in x86
        byte[] codeBuffer = new byte[] { 0x0F, 0x85, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("jnz", instruction.Mnemonic);
        Assert.Equal("0x1234567E", instruction.Operands); // Current position (6) + offset (0x12345678) = 0x1234567E
    }
}
