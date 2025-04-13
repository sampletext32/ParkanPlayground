using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

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
    
    /// <summary>
    /// Tests the JgeRel8Handler for decoding JGE rel8 instruction with positive offset
    /// </summary>
    [Fact]
    public void JgeRel8Handler_DecodesJgeRel8_WithPositiveOffset_Correctly()
    {
        // Arrange
        // JGE +5 (7D 05) - Jump 5 bytes forward if greater than or equal
        byte[] codeBuffer = new byte[] { 0x7D, 0x05 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("jge", instruction.Mnemonic);
        Assert.Equal("0x00000007", instruction.Operands); // Current position (2) + offset (5) = 7
    }
    
    /// <summary>
    /// Tests the JgeRel8Handler for decoding JGE rel8 instruction with negative offset
    /// </summary>
    [Fact]
    public void JgeRel8Handler_DecodesJgeRel8_WithNegativeOffset_Correctly()
    {
        // Arrange
        // JGE -5 (7D FB) - Jump 5 bytes backward if greater than or equal
        byte[] codeBuffer = new byte[] { 0x7D, 0xFB };
        var disassembler = new Disassembler(codeBuffer, 0x1000); // Set a base address for easier verification
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("jge", instructions[0].Mnemonic);
        Assert.Equal("0xFFFFFFFD", instructions[0].Operands); // 0x1000 + 2 - 5 = 0xFFFFFFFD (sign-extended)
    }
    
    /// <summary>
    /// Tests the JgeRel8Handler for decoding JGE rel8 instruction in a sequence
    /// </summary>
    [Fact]
    public void JgeRel8Handler_DecodesJgeRel8_InSequence_Correctly()
    {
        // Arrange
        // This is a common pattern: JGE/JL followed by different code paths
        // JGE +5 (7D 05) - Jump 5 bytes forward if greater than or equal
        // ADD EBP, 0x18 (83 C5 18)
        // JMP +3 (EB 03) - Jump past the next instruction
        // ADD EBP, -0x48 (83 C5 B8)
        byte[] codeBuffer = new byte[] { 0x7D, 0x05, 0x83, 0xC5, 0x18, 0xEB, 0x03, 0x83, 0xC5, 0xB8 };
        var disassembler = new Disassembler(codeBuffer, 0x1000);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.True(instructions.Count >= 4, $"Expected at least 4 instructions, but got {instructions.Count}");
        
        // First instruction: JGE +5
        Assert.Equal("jge", instructions[0].Mnemonic);
        Assert.Equal("0x00000007", instructions[0].Operands); // Base address is ignored, only relative offset matters
        
        // Second instruction: ADD EBP, 0x18
        Assert.Equal("add", instructions[1].Mnemonic);
        Assert.Equal("ebp, 0x00000018", instructions[1].Operands);
        
        // Third instruction: JMP +3
        Assert.Equal("jmp", instructions[2].Mnemonic);
        Assert.Equal("0x0000000A", instructions[2].Operands); // Base address is ignored, only relative offset matters
        
        // Fourth instruction: ADD EBP, -0x48 (0xB8 sign-extended to 32-bit is 0xFFFFFFB8)
        Assert.Equal("add", instructions[3].Mnemonic);
        Assert.Equal("ebp, 0xFFFFFFB8", instructions[3].Operands);
    }
}
