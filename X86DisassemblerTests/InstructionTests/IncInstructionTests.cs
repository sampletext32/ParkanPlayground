using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for INC instruction handlers
/// </summary>
public class IncInstructionTests
{
    /// <summary>
    /// Tests the INC EAX instruction (0x40)
    /// </summary>
    [Fact]
    public void TestIncEax()
    {
        // Arrange
        byte[] code = { 0x40 }; // INC EAX
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Inc, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EAX)
    }
    
    /// <summary>
    /// Tests the INC ECX instruction (0x41)
    /// </summary>
    [Fact]
    public void TestIncEcx()
    {
        // Arrange
        byte[] code = { 0x41 }; // INC ECX
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Inc, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (ECX)
        var ecxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ecxOperand);
        var registerOperand = (RegisterOperand)ecxOperand;
        Assert.Equal(RegisterIndex.C, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (ECX)
    }
    
    /// <summary>
    /// Tests the INC EDI instruction (0x47)
    /// </summary>
    [Fact]
    public void TestIncEdi()
    {
        // Arrange
        byte[] code = { 0x47 }; // INC EDI
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Inc, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (EDI)
        var ediOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ediOperand);
        var registerOperand = (RegisterOperand)ediOperand;
        Assert.Equal(RegisterIndex.Di, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EDI)
    }
}
