using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for ADD instruction handlers
/// </summary>
public class AddInstructionTests
{
    /// <summary>
    /// Tests the ADD r32, r/m32 instruction (0x03) with register operand
    /// </summary>
    [Fact]
    public void TestAddR32Rm32_Register()
    {
        // Arrange
        byte[] code = { 0x03, 0xF5 }; // ADD ESI, EBP
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal(InstructionType.Add, instructions[0].Type);
        
        // Check that we have two operands
        Assert.Equal(2, instructions[0].StructuredOperands.Count);
        
        // Check the first operand (ESI)
        var esiOperand = instructions[0].StructuredOperands[0];
        Assert.IsType<RegisterOperand>(esiOperand);
        var registerOperand1 = (RegisterOperand)esiOperand;
        Assert.Equal(RegisterIndex.Si, registerOperand1.Register);
        Assert.Equal(32, registerOperand1.Size); // Validate that it's a 32-bit register (ESI)
        
        // Check the second operand (EBP)
        var ebpOperand = instructions[0].StructuredOperands[1];
        Assert.IsType<RegisterOperand>(ebpOperand);
        var registerOperand2 = (RegisterOperand)ebpOperand;
        Assert.Equal(RegisterIndex.Bp, registerOperand2.Register);
        Assert.Equal(32, registerOperand2.Size); // Validate that it's a 32-bit register (EBP)
    }
    
    /// <summary>
    /// Tests the ADD r32, m32 instruction (0x03) with memory operand
    /// </summary>
    [Fact]
    public void TestAddR32M32()
    {
        // Arrange
        byte[] code = { 0x03, 0x00 }; // ADD EAX, DWORD PTR [EAX]
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal(InstructionType.Add, instructions[0].Type);
        
        // Check that we have two operands
        Assert.Equal(2, instructions[0].StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instructions[0].StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (memory operand)
        var memoryOperand = instructions[0].StructuredOperands[1];
        Assert.IsType<BaseRegisterMemoryOperand>(memoryOperand);
        var memory = (BaseRegisterMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.A, memory.BaseRegister); // Base register is EAX
        Assert.Equal(32, memory.Size); // Memory size is 32 bits (DWORD)
    }
}
