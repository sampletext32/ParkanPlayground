using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for CMP instruction handlers
/// </summary>
public class CmpInstructionTests
{
    /// <summary>
    /// Tests the CMP r32, r/m32 instruction (0x3B) with register operand
    /// </summary>
    [Fact]
    public void TestCmpR32Rm32_Register()
    {
        // Arrange
        byte[] code = { 0x3B, 0xC7 }; // CMP EAX, EDI
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal(InstructionType.Cmp, instructions[0].Type);
        
        // Check that we have two operands
        Assert.Equal(2, instructions[0].StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instructions[0].StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand1 = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand1.Register);
        Assert.Equal(32, registerOperand1.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (EDI)
        var ediOperand = instructions[0].StructuredOperands[1];
        Assert.IsType<RegisterOperand>(ediOperand);
        var registerOperand2 = (RegisterOperand)ediOperand;
        Assert.Equal(RegisterIndex.Di, registerOperand2.Register);
        Assert.Equal(32, registerOperand2.Size); // Validate that it's a 32-bit register (EDI)
    }
    
    /// <summary>
    /// Tests the CMP r32, m32 instruction (0x3B) with memory operand
    /// </summary>
    [Fact]
    public void TestCmpR32M32()
    {
        // Arrange
        byte[] code = { 0x3B, 0x00 }; // CMP EAX, DWORD PTR [EAX]
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal(InstructionType.Cmp, instructions[0].Type);
        
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
