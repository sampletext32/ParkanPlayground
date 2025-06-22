using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for PUSH instruction handlers
/// </summary>
public class PushInstructionTests
{
    /// <summary>
    /// Tests the PUSH imm32 instruction (0x68)
    /// </summary>
    [Fact]
    public void TestPushImm32()
    {
        // Arrange
        byte[] code = { 0x68, 0x78, 0x56, 0x34, 0x12 }; // PUSH 0x12345678
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Push, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (immediate value)
        var operand = instruction.StructuredOperands[0];
        Assert.IsType<ImmediateOperand>(operand);
        var immOperand = (ImmediateOperand)operand;
        Assert.Equal(0x12345678u, immOperand.Value);
        Assert.Equal(32, immOperand.Size);
    }
    
    /// <summary>
    /// Tests the PUSH imm16 instruction with operand size prefix (0x66 0x68)
    /// </summary>
    [Fact]
    public void TestPushImm16WithOperandSizePrefix()
    {
        // Arrange
        byte[] code = { 0x66, 0x68, 0x78, 0x56 }; // PUSH 0x5678 (with operand size prefix)
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Push, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (immediate value)
        var operand = instruction.StructuredOperands[0];
        Assert.IsType<ImmediateOperand>(operand);
        var immOperand = (ImmediateOperand)operand;
        Assert.Equal(0x5678u, immOperand.Value);
        Assert.Equal(16, immOperand.Size);
    }
    
    /// <summary>
    /// Tests the PUSH imm8 instruction (0x6A)
    /// </summary>
    [Fact]
    public void TestPushImm8()
    {
        // Arrange
        byte[] code = { 0x6A, 0x42 }; // PUSH 0x42
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Push, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (immediate value)
        var operand = instruction.StructuredOperands[0];
        Assert.IsType<ImmediateOperand>(operand);
        var immOperand = (ImmediateOperand)operand;
        Assert.Equal(0x42u, immOperand.Value);
        Assert.Equal(8, immOperand.Size);
    }
}
