namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;

/// <summary>
/// Tests for CMP instruction sequences
/// </summary>
public class CmpInstructionSequenceTests
{
    /// <summary>
    /// Tests the CMP instruction with a complex memory operand
    /// </summary>
    [Fact]
    public void CmpImmWithRm8_ComplexMemoryOperand_Correctly()
    {
        // Arrange
        // CMP BYTE PTR [EBP], 0x03 (80 7D 00 03)
        byte[] codeBuffer = new byte[] { 0x80, 0x7D, 0x00, 0x03 };
        var disassembler = new Disassembler(codeBuffer, 0x1C46);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("cmp", instructions[0].Mnemonic);
        Assert.Equal("byte ptr [ebp+0x00], 0x03", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the CMP instruction followed by a JGE instruction
    /// </summary>
    [Fact]
    public void CmpImmWithRm8_FollowedByJge_Correctly()
    {
        // Arrange
        // CMP BYTE PTR [EBP], 0x03 (80 7D 00 03)
        // JGE +5 (7D 05)
        byte[] codeBuffer = new byte[] { 0x80, 0x7D, 0x00, 0x03, 0x7D, 0x05 };
        var disassembler = new Disassembler(codeBuffer, 0x1C46);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Equal(2, instructions.Count);
        
        // First instruction: CMP BYTE PTR [EBP], 0x03
        Assert.Equal("cmp", instructions[0].Mnemonic);
        Assert.Equal("byte ptr [ebp+0x00], 0x03", instructions[0].Operands);
        
        // Second instruction: JGE +5
        Assert.Equal("jge", instructions[1].Mnemonic);
        Assert.Equal("0x0000000B", instructions[1].Operands); // Base address is ignored, only relative offset matters
    }
    
    /// <summary>
    /// Tests the full sequence of instructions from address 0x00001C46
    /// </summary>
    [Fact]
    public void CmpJgeSequence_DecodesCorrectly()
    {
        // Arrange
        // This is the sequence from address 0x00001C46
        // CMP BYTE PTR [EBP], 0x03 (80 7D 00 03)
        // JGE +5 (7D 05)
        // ADD EBP, 0x18 (83 C5 18)
        // JMP +3 (EB 03)
        // ADD EBP, -0x48 (83 C5 B8)
        byte[] codeBuffer = new byte[] { 
            0x80, 0x7D, 0x00, 0x03, 0x7D, 0x05, 0x83, 0xC5, 
            0x18, 0xEB, 0x03, 0x83, 0xC5, 0xB8, 0x8B, 0x56, 0x04 
        };
        var disassembler = new Disassembler(codeBuffer, 0x1C46);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.True(instructions.Count >= 5, $"Expected at least 5 instructions, but got {instructions.Count}");
        
        // First instruction: CMP BYTE PTR [EBP], 0x03
        Assert.Equal("cmp", instructions[0].Mnemonic);
        Assert.Equal("byte ptr [ebp+0x00], 0x03", instructions[0].Operands);
        
        // Second instruction: JGE +5
        Assert.Equal("jge", instructions[1].Mnemonic);
        Assert.Equal("0x0000000B", instructions[1].Operands); // Base address is ignored, only relative offset matters
        
        // Third instruction: ADD EBP, 0x18
        Assert.Equal("add", instructions[2].Mnemonic);
        Assert.Equal("ebp, 0x00000018", instructions[2].Operands);
        
        // Fourth instruction: JMP +3
        Assert.Equal("jmp", instructions[3].Mnemonic);
        Assert.Equal("0x0000000E", instructions[3].Operands); // Base address is ignored, only relative offset matters
        
        // Fifth instruction: ADD EBP, -0x48 (0xB8 sign-extended to 32-bit is 0xFFFFFFB8)
        Assert.Equal("add", instructions[4].Mnemonic);
        Assert.Equal("ebp, 0xFFFFFFB8", instructions[4].Operands);
    }
}
