using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for DEC instruction handlers
/// </summary>
public class DecInstructionTests
{
    /// <summary>
    /// Tests the DEC EAX instruction (0x48)
    /// </summary>
    [Fact]
    public void TestDecEax()
    {
        // Arrange
        byte[] code = { 0x48 }; // DEC EAX
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Dec, instruction.Type);
        
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
    /// Tests the DEC ECX instruction (0x49)
    /// </summary>
    [Fact]
    public void TestDecEcx()
    {
        // Arrange
        byte[] code = { 0x49 }; // DEC ECX
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Dec, instruction.Type);
        
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
    /// Tests the DEC EDI instruction (0x4F)
    /// </summary>
    [Fact]
    public void TestDecEdi()
    {
        // Arrange
        byte[] code = { 0x4F }; // DEC EDI
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Dec, instruction.Type);
        
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
