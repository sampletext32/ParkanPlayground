using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for CALL r/m32 instruction (0xFF /2)
/// </summary>
public class CallRm32Tests
{
    /// <summary>
    /// Tests the CALL r32 instruction (0xFF /2) with register operand
    /// </summary>
    [Fact]
    public void TestCallReg()
    {
        // Arrange
        byte[] code = { 0xFF, 0xD3 }; // CALL EBX
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Call, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (EBX)
        var ebxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ebxOperand);
        var registerOperand = (RegisterOperand)ebxOperand;
        Assert.Equal(RegisterIndex.B, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EBX)
    }
    
    /// <summary>
    /// Tests the CALL m32 instruction (0xFF /2) with memory operand
    /// </summary>
    [Fact]
    public void TestCallMem()
    {
        // Arrange
        byte[] code = { 0xFF, 0x10 }; // CALL DWORD PTR [EAX]
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Call, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (memory operand)
        var memoryOperand = instruction.StructuredOperands[0];
        Assert.IsType<BaseRegisterMemoryOperand>(memoryOperand);
        var memory = (BaseRegisterMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.A, memory.BaseRegister); // Base register is EAX
        Assert.Equal(32, memory.Size); // Memory size is 32 bits (DWORD)
    }
}
