using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for ADD r/m32, r32 instruction (0x01)
/// </summary>
public class AddRm32R32Tests
{
    /// <summary>
    /// Tests the ADD r32, r32 instruction (0x01) with register operand
    /// </summary>
    [Fact]
    public void TestAddR32R32()
    {
        // Arrange
        byte[] code = { 0x01, 0xC1 }; // ADD ECX, EAX
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal(InstructionType.Add, instructions[0].Type);
        
        // Check that we have two operands
        Assert.Equal(2, instructions[0].StructuredOperands.Count);
        
        // Check the first operand (ECX)
        var ecxOperand = instructions[0].StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ecxOperand);
        var registerOperand1 = (RegisterOperand)ecxOperand;
        Assert.Equal(RegisterIndex.C, registerOperand1.Register);
        Assert.Equal(32, registerOperand1.Size); // Validate that it's a 32-bit register (ECX)
        
        // Check the second operand (EAX)
        var eaxOperand = instructions[0].StructuredOperands[1];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand2 = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand2.Register);
        Assert.Equal(32, registerOperand2.Size); // Validate that it's a 32-bit register (EAX)
    }
    
    /// <summary>
    /// Tests the ADD m32, r32 instruction (0x01) with memory operand
    /// </summary>
    [Fact]
    public void TestAddM32R32()
    {
        // Arrange
        byte[] code = { 0x01, 0x01 }; // ADD DWORD PTR [ECX], EAX
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal(InstructionType.Add, instructions[0].Type);
        
        // Check that we have two operands
        Assert.Equal(2, instructions[0].StructuredOperands.Count);
        
        // Check the first operand (memory operand)
        var memoryOperand = instructions[0].StructuredOperands[0];
        Assert.IsType<BaseRegisterMemoryOperand>(memoryOperand);
        var memory = (BaseRegisterMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.C, memory.BaseRegister); // Base register is ECX
        Assert.Equal(32, memory.Size); // Memory size is 32 bits (DWORD)
        
        // Check the second operand (EAX)
        var eaxOperand = instructions[0].StructuredOperands[1];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EAX)
    }
}
