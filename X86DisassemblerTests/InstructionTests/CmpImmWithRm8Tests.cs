using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for CMP r/m8, imm8 instruction (0x80 /7)
/// </summary>
public class CmpImmWithRm8Tests
{
    /// <summary>
    /// Tests the CMP r8, imm8 instruction (0x80 /7) with register operand
    /// </summary>
    [Fact]
    public void TestCmpR8Imm8()
    {
        // Arrange
        byte[] code = { 0x80, 0xF9, 0x02 }; // CMP CL, 0x02
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal(InstructionType.Cmp, instructions[0].Type);
        
        // Check that we have two operands
        Assert.Equal(2, instructions[0].StructuredOperands.Count);
        
        // Check the first operand (CL)
        var clOperand = instructions[0].StructuredOperands[0];
        Assert.IsType<RegisterOperand>(clOperand);
        var registerOperand = (RegisterOperand)clOperand;
        Assert.Equal(RegisterIndex.C, registerOperand.Register);
        Assert.Equal(8, registerOperand.Size); // Validate that it's an 8-bit register (CL)
        
        // Check the second operand (immediate value)
        var immOperand = instructions[0].StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x02U, immediateOperand.Value);
    }
    
    /// <summary>
    /// Tests the CMP m8, imm8 instruction (0x80 /7) with memory operand
    /// </summary>
    [Fact]
    public void TestCmpM8Imm8()
    {
        // Arrange
        byte[] code = { 0x80, 0x39, 0x05 }; // CMP BYTE PTR [ECX], 0x05
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal(InstructionType.Cmp, instructions[0].Type);
        
        // Check that we have two operands
        Assert.Equal(2, instructions[0].StructuredOperands.Count);
        
        // Check the first operand (memory operand)
        var memoryOperand = instructions[0].StructuredOperands[0];
        Assert.IsType<BaseRegisterMemoryOperand>(memoryOperand);
        var memory = (BaseRegisterMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.C, memory.BaseRegister); // Base register is ECX
        Assert.Equal(8, memory.Size); // Memory size is 8 bits (BYTE)
        
        // Check the second operand (immediate value)
        var immOperand = instructions[0].StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x05U, immediateOperand.Value);
    }
}
