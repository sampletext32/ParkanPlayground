namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;

/// <summary>
/// Tests for Group 1 sign-extended immediate instructions (0x83 opcode)
/// </summary>
public class Group1SignExtendedHandlerTests
{
    /// <summary>
    /// Tests that the disassembler correctly handles ADD ecx, imm8 instruction (0x83 0xC1 0x04)
    /// </summary>
    [Fact]
    public void Disassembler_HandlesAddEcxImm8_Correctly()
    {
        // Arrange
        // ADD ecx, 0x04 (83 C1 04)
        byte[] codeBuffer = new byte[] { 0x83, 0xC1, 0x04 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("add", instruction.Mnemonic);
        Assert.Contains("ecx", instruction.Operands);
        // Accept either format for the immediate value
        Assert.True(instruction.Operands.Contains("0x04") || instruction.Operands.Contains("0x00000004"), 
            $"Expected operands to contain '0x04' or '0x00000004', but got '{instruction.Operands}'");
    }
    
    /// <summary>
    /// Tests that the disassembler correctly handles the specific sequence from address 0x00001874
    /// </summary>
    [Fact]
    public void Disassembler_HandlesSpecificSequence_Correctly()
    {
        // Arrange
        // This is the sequence from the problematic example:
        // 08 83 C1 04 50 E8 42 01 00 00
        byte[] codeBuffer = new byte[] { 0x08, 0x83, 0xC1, 0x04, 0x50, 0xE8, 0x42, 0x01, 0x00, 0x00 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.True(instructions.Count >= 3, $"Expected at least 3 instructions, but got {instructions.Count}");
        
        // First instruction should be OR r/m8, r8 (but might be incomplete)
        Assert.Equal("or", instructions[0].Mnemonic);
        
        // Second instruction should be ADD ecx, 0x04
        Assert.Equal("add", instructions[1].Mnemonic);
        Assert.Contains("ecx", instructions[1].Operands);
        // Accept either format for the immediate value
        Assert.True(instructions[1].Operands.Contains("0x04") || instructions[1].Operands.Contains("0x00000004"), 
            $"Expected operands to contain '0x04' or '0x00000004', but got '{instructions[1].Operands}'");
        
        // Third instruction should be PUSH eax
        Assert.Equal("push", instructions[2].Mnemonic);
        Assert.Equal("eax", instructions[2].Operands);
    }
}
