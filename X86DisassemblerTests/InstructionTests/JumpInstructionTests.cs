using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

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
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instruction = disassembler.Disassemble().First();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Jmp, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check that the operand is a relative offset operand
        var operand = instruction.StructuredOperands[0];
        Assert.IsType<RelativeOffsetOperand>(operand);
        
        // Check the target address
        var relativeOffsetOperand = (RelativeOffsetOperand)operand;
        Assert.Equal(0x00000007UL, relativeOffsetOperand.TargetAddress); // Current position (2) + offset (5) = 7
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
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instruction = disassembler.Disassemble().First();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Jmp, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check that the operand is a relative offset operand
        var operand = instruction.StructuredOperands[0];
        Assert.IsType<RelativeOffsetOperand>(operand);
        
        // Check the target address
        var relativeOffsetOperand = (RelativeOffsetOperand)operand;
        Assert.Equal(0x1234567DUL, relativeOffsetOperand.TargetAddress); // Current position (5) + offset (0x12345678) = 0x1234567D
    }
    
    /// <summary>
    /// Tests the ConditionalJumpHandler for decoding JZ rel8 instruction
    /// </summary>
    [Fact]
    public void ConditionalJumpHandler_DecodesJzRel8_Correctly()
    {
        // Arrange
        // JZ +10 (74 0A) - Jump 10 bytes forward if zero flag is set
        byte[] codeBuffer = new byte[] { 0x74, 0x0A };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instruction = disassembler.Disassemble().First();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Jz, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check that the operand is a relative offset operand
        var operand = instruction.StructuredOperands[0];
        Assert.IsType<RelativeOffsetOperand>(operand);
        
        // Check the target address
        var relativeOffsetOperand = (RelativeOffsetOperand)operand;
        Assert.Equal(0x0000000CUL, relativeOffsetOperand.TargetAddress); // Current position (2) + offset (10) = 12 (0x0C)
    }
    
    /// <summary>
    /// Tests the TwoByteConditionalJumpHandler for decoding JNZ rel32 instruction
    /// </summary>
    [Fact]
    public void TwoByteConditionalJumpHandler_DecodesJnzRel32_Correctly()
    {
        // Arrange
        // JNZ +0x12345678 (0F 85 78 56 34 12) - Jump 0x12345678 bytes forward if zero flag is not set
        byte[] codeBuffer = new byte[] { 0x0F, 0x85, 0x78, 0x56, 0x34, 0x12 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instruction = disassembler.Disassemble().First();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Jnz, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check that the operand is a relative offset operand
        var operand = instruction.StructuredOperands[0];
        Assert.IsType<RelativeOffsetOperand>(operand);
        
        // Check the target address
        var relativeOffsetOperand = (RelativeOffsetOperand)operand;
        Assert.Equal(0x1234567EUL, relativeOffsetOperand.TargetAddress); // Current position (6) + offset (0x12345678) = 0x1234567E
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
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instruction = disassembler.Disassemble().First();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Jge, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check that the operand is a relative offset operand
        var operand = instruction.StructuredOperands[0];
        Assert.IsType<RelativeOffsetOperand>(operand);
        
        // Check the target address
        var relativeOffsetOperand = (RelativeOffsetOperand)operand;
        Assert.Equal(0x00000007UL, relativeOffsetOperand.TargetAddress); // Current position (2) + offset (5) = 7
    }
    
    /// <summary>
    /// Tests the JgeRel8Handler for decoding JGE rel8 instruction with negative offset
    /// </summary>
    [Fact]
    public void JgeRel8Handler_DecodesJgeRel8_WithNegativeOffset_Correctly()
    {
        // Arrange
        // JGE -5 (7D FB) - Jump 5 bytes backward if greater than or equal
        // 0xFB is -5 in two's complement
        byte[] codeBuffer = new byte[] { 0x7D, 0xFB };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal(InstructionType.Jge, instructions[0].Type);
        
        // Check that we have one operand
        Assert.Single(instructions[0].StructuredOperands);
        
        // Check that the operand is a relative offset operand
        var operand = instructions[0].StructuredOperands[0];
        Assert.IsType<RelativeOffsetOperand>(operand);
        
        // Check the target address
        var relativeOffsetOperand = (RelativeOffsetOperand)operand;
        Assert.Equal(0xFFFFFFFDUL, relativeOffsetOperand.TargetAddress); // 0 + 2 - 5 = 0xFFFFFFFD (sign-extended)
    }
    
    /// <summary>
    /// Tests the JgeRel8Handler for decoding JGE rel8 instruction in a sequence
    /// </summary>
    [Fact]
    public void JgeRel8Handler_DecodesJgeRel8_InSequence_Correctly()
    {
        // Arrange
        // Sequence of instructions:
        // 1. JGE +5 (7D 05) - Jump 5 bytes forward if greater than or equal
        // 2. ADD EBP, 0x18 (83 C5 18) - Add 0x18 to EBP
        // 3. JMP +3 (EB 03) - Jump 3 bytes forward
        // 4. ADD EBP, -0x48 (83 C5 B8) - Add -0x48 to EBP (0xB8 is -0x48 in two's complement)
        byte[] codeBuffer = new byte[] { 0x7D, 0x05, 0x83, 0xC5, 0x18, 0xEB, 0x03, 0x83, 0xC5, 0xB8 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Equal(4, instructions.Count);
        
        // First instruction: JGE +5
        Assert.Equal(InstructionType.Jge, instructions[0].Type);
        
        // Check that we have one operand
        Assert.Single(instructions[0].StructuredOperands);
        
        // Check that the operand is a relative offset operand
        var operand = instructions[0].StructuredOperands[0];
        Assert.IsType<RelativeOffsetOperand>(operand);
        
        // Check the target address
        var relativeOffsetOperand = (RelativeOffsetOperand)operand;
        Assert.Equal(7UL, relativeOffsetOperand.TargetAddress); // Base address is ignored, only relative offset matters
        
        // Second instruction: ADD EBP, 0x18
        Assert.Equal(InstructionType.Add, instructions[1].Type);
        
        // Check that we have two operands
        Assert.Equal(2, instructions[1].StructuredOperands.Count);
        
        // Check that the first operand is a register operand
        var firstOperand = instructions[1].StructuredOperands[0];
        Assert.IsType<RegisterOperand>(firstOperand);
        
        // Check that the second operand is an immediate operand
        var secondOperand = instructions[1].StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(secondOperand);
        
        // Check the values of the operands
        var registerOperand = (RegisterOperand)firstOperand;
        var immediateOperand = (ImmediateOperand)secondOperand;
        Assert.Equal(RegisterIndex.Bp, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EBP)
        Assert.Equal(0x18U, immediateOperand.Value);
        
        // Third instruction: JMP +3
        Assert.Equal(InstructionType.Jmp, instructions[2].Type);
        
        // Check that we have one operand
        Assert.Single(instructions[2].StructuredOperands);
        
        // Check that the operand is a relative offset operand
        operand = instructions[2].StructuredOperands[0];
        Assert.IsType<RelativeOffsetOperand>(operand);
        
        // Check the target address
        relativeOffsetOperand = (RelativeOffsetOperand)operand;
        Assert.Equal(10UL, relativeOffsetOperand.TargetAddress); // Base address is ignored, only relative offset matters
        
        // Fourth instruction: ADD EBP, -0x48 (0xB8 sign-extended to 32-bit is 0xFFFFFFB8)
        Assert.Equal(InstructionType.Add, instructions[3].Type);
        
        // Check that we have two operands
        Assert.Equal(2, instructions[3].StructuredOperands.Count);
        
        // Check that the first operand is a register operand
        firstOperand = instructions[3].StructuredOperands[0];
        Assert.IsType<RegisterOperand>(firstOperand);
        
        // Check that the second operand is an immediate operand
        secondOperand = instructions[3].StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(secondOperand);
        
        // Check the values of the operands
        registerOperand = (RegisterOperand)firstOperand;
        immediateOperand = (ImmediateOperand)secondOperand;
        Assert.Equal(RegisterIndex.Bp, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EBP)
        Assert.Equal(0xFFFFFFB8L, immediateOperand.Value);
    }
}
