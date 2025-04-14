using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

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
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Cmp, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (memory operand)
        var memoryOperand = instruction.StructuredOperands[0];
        Assert.IsType<DisplacementMemoryOperand>(memoryOperand);
        var memory = (DisplacementMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.Bp, memory.BaseRegister); // Base register is EBP
        Assert.Equal(0, memory.Displacement); // Displacement is 0
        Assert.Equal(8, memory.Size); // Memory size is 8 bits (BYTE)
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x03U, immediateOperand.Value);
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
        var cmpInstruction = instructions[0];
        Assert.Equal(InstructionType.Cmp, cmpInstruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, cmpInstruction.StructuredOperands.Count);
        
        // Check the first operand (memory operand)
        var memoryOperand = cmpInstruction.StructuredOperands[0];
        Assert.IsType<DisplacementMemoryOperand>(memoryOperand);
        var memory = (DisplacementMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.Bp, memory.BaseRegister); // Base register is EBP
        Assert.Equal(0, memory.Displacement); // Displacement is 0
        Assert.Equal(8, memory.Size); // Memory size is 8 bits (BYTE)
        
        // Check the second operand (immediate value)
        var immOperand = cmpInstruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x03U, immediateOperand.Value);
        
        // Second instruction: JGE +5
        var jgeInstruction = instructions[1];
        Assert.Equal(InstructionType.Jge, jgeInstruction.Type);
        
        // Check that we have one operand
        Assert.Single(jgeInstruction.StructuredOperands);
        
        // Check the operand (relative offset)
        var relativeOffsetOperand = jgeInstruction.StructuredOperands[0];
        Assert.IsType<RelativeOffsetOperand>(relativeOffsetOperand);
        var offsetOperand = (RelativeOffsetOperand)relativeOffsetOperand;
        // The target address should be 0xB (11 decimal) which is the relative offset from the start of the buffer
        // This is because the JGE instruction is at offset 4 in the buffer, its length is 2 bytes,
        // and the offset value is 5, so 4 + 2 + 5 = 11 (0xB)
        Assert.Equal(0xBUL, offsetOperand.TargetAddress);
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
        var cmpInstruction = instructions[0];
        Assert.Equal(InstructionType.Cmp, cmpInstruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, cmpInstruction.StructuredOperands.Count);
        
        // Check the first operand (memory operand)
        var memoryOperand = cmpInstruction.StructuredOperands[0];
        Assert.IsType<DisplacementMemoryOperand>(memoryOperand);
        var memory = (DisplacementMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.Bp, memory.BaseRegister); // Base register is EBP
        Assert.Equal(0, memory.Displacement); // Displacement is 0
        Assert.Equal(8, memory.Size); // Memory size is 8 bits (BYTE)
        
        // Check the second operand (immediate value)
        var immOperand = cmpInstruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x03U, immediateOperand.Value);
        
        // Second instruction: JGE +5
        var jgeInstruction = instructions[1];
        Assert.Equal(InstructionType.Jge, jgeInstruction.Type);
        
        // Check that we have one operand
        Assert.Single(jgeInstruction.StructuredOperands);
        
        // Check the operand (relative offset)
        var relativeOffsetOperand = jgeInstruction.StructuredOperands[0];
        Assert.IsType<RelativeOffsetOperand>(relativeOffsetOperand);
        var offsetOperand = (RelativeOffsetOperand)relativeOffsetOperand;
        // The target address should be 0xB (11 decimal) which is the relative offset from the start of the buffer
        // This is because the JGE instruction is at offset 4 in the buffer, its length is 2 bytes,
        // and the offset value is 5, so 4 + 2 + 5 = 11 (0xB)
        Assert.Equal(0xBUL, offsetOperand.TargetAddress); // Target address is 4 + 2 + 5 = 11 (0xB)
        
        // Third instruction: ADD EBP, 0x18
        var addInstruction = instructions[2];
        Assert.Equal(InstructionType.Add, addInstruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, addInstruction.StructuredOperands.Count);
        
        // Check the first operand (register operand)
        var registerOperand = addInstruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(registerOperand);
        var register = (RegisterOperand)registerOperand;
        Assert.Equal(RegisterIndex.Bp, register.Register); // Register is EBP
        
        // Check the second operand (immediate value)
        var immOperand2 = addInstruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand2);
        var immediateOperand2 = (ImmediateOperand)immOperand2;
        Assert.Equal(0x18U, immediateOperand2.Value);
        
        // Fourth instruction: JMP +3
        var jmpInstruction = instructions[3];
        Assert.Equal(InstructionType.Jmp, jmpInstruction.Type);
        
        // Check that we have one operand
        Assert.Single(jmpInstruction.StructuredOperands);
        
        // Check the operand (relative offset)
        var relativeOffsetOperand2 = jmpInstruction.StructuredOperands[0];
        Assert.IsType<RelativeOffsetOperand>(relativeOffsetOperand2);
        var offsetOperand2 = (RelativeOffsetOperand)relativeOffsetOperand2;
        Assert.Equal(0xEUL, offsetOperand2.TargetAddress); // Target address is 9 + 2 + 3 = 14 (0xE)
        
        // Fifth instruction: ADD EBP, -0x48 (0xB8 sign-extended to 32-bit is -72 decimal)
        var addInstruction2 = instructions[4];
        Assert.Equal(InstructionType.Add, addInstruction2.Type);
        
        // Check that we have two operands
        Assert.Equal(2, addInstruction2.StructuredOperands.Count);
        
        // Check the first operand (register operand)
        var registerOperand2 = addInstruction2.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(registerOperand2);
        var register2 = (RegisterOperand)registerOperand2;
        Assert.Equal(RegisterIndex.Bp, register2.Register); // Register is EBP
        
        // Check the second operand (immediate value)
        var immOperand3 = addInstruction2.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand3);
        var immediateOperand3 = (ImmediateOperand)immOperand3;
        
        // The immediate value 0xB8 is sign-extended to 32-bit as a negative value (-72 decimal)
        // This is because 0xB8 with the high bit set is treated as a negative number in two's complement
        Assert.Equal(-72L, (long)immediateOperand3.Value);
        
        // Sixth instruction: MOV EDX, DWORD PTR [ESI+0x4]
        var movInstruction = instructions[5];
        Assert.Equal(InstructionType.Mov, movInstruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, movInstruction.StructuredOperands.Count);
        
        // Check the first operand (register operand)
        var registerOperand3 = movInstruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(registerOperand3);
        var register3 = (RegisterOperand)registerOperand3;
        Assert.Equal(RegisterIndex.D, register3.Register); // Register is EDX
        Assert.Equal(32, register3.Size); // Validate that it's a 32-bit register (EDX)
        
        // Check the second operand (memory operand)
        var memoryOperand2 = movInstruction.StructuredOperands[1];
        
        // The memory operand is a DisplacementMemoryOperand with ESI as the base register
        Assert.IsType<DisplacementMemoryOperand>(memoryOperand2);
        var memory2 = (DisplacementMemoryOperand)memoryOperand2;
        
        // Check the base register and displacement
        Assert.Equal(RegisterIndex.Si, memory2.BaseRegister); // Base register is ESI
        Assert.Equal(4, memory2.Displacement); // Displacement is 4
        Assert.Equal(32, memory2.Size); // Memory size is 32 bits (DWORD)
    }
}
