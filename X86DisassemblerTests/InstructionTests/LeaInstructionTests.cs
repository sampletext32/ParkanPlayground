using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for LEA instruction handlers
/// </summary>
public class LeaInstructionTests
{
    /// <summary>
    /// Tests the LEA r32, m instruction (0x8D) with simple memory operand
    /// </summary>
    [Fact]
    public void TestLeaR32M_Simple()
    {
        // Arrange
        byte[] code = { 0x8D, 0x00 }; // LEA EAX, [EAX]
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Lea, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (memory operand)
        var memOperand = instruction.StructuredOperands[1];
        Assert.IsType<BaseRegisterMemoryOperand>(memOperand);
        var memoryOperand = (BaseRegisterMemoryOperand)memOperand;
        Assert.Equal(RegisterIndex.A, memoryOperand.BaseRegister);
    }
    
    /// <summary>
    /// Tests the LEA r32, m instruction (0x8D) with displacement
    /// </summary>
    [Fact]
    public void TestLeaR32M_WithDisplacement()
    {
        // Arrange
        byte[] code = { 0x8D, 0x7E, 0xFC }; // LEA EDI, [ESI - 0x4]
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Lea, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EDI)
        var ediOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ediOperand);
        var registerOperand = (RegisterOperand)ediOperand;
        Assert.Equal(RegisterIndex.Di, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EDI)
        
        // Check the second operand (memory operand with displacement)
        var memOperand = instruction.StructuredOperands[1];
        Assert.IsType<DisplacementMemoryOperand>(memOperand);
        var displacementMemoryOperand = (DisplacementMemoryOperand)memOperand;
        Assert.Equal(RegisterIndex.Si, displacementMemoryOperand.BaseRegister);
        Assert.Equal(-4, displacementMemoryOperand.Displacement);
    }
    
    /// <summary>
    /// Tests the LEA r32, m instruction (0x8D) with SIB byte
    /// </summary>
    [Fact]
    public void TestLeaR32M_WithSib()
    {
        // Arrange
        byte[] code = { 0x8D, 0x04, 0x8D, 0x00, 0x00, 0x00, 0x00 }; // LEA EAX, [ECX*4]
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Lea, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (scaled index memory operand)
        var memOperand = instruction.StructuredOperands[1];
        Assert.IsType<ScaledIndexMemoryOperand>(memOperand);
        var scaledIndexMemoryOperand = (ScaledIndexMemoryOperand)memOperand;
        Assert.Equal(RegisterIndex.C, scaledIndexMemoryOperand.IndexRegister);
        Assert.Equal(4, scaledIndexMemoryOperand.Scale);
        Assert.Equal(0, scaledIndexMemoryOperand.Displacement);
    }
    
    /// <summary>
    /// Tests the LEA r32, m instruction (0x8D) with complex addressing
    /// </summary>
    [Fact]
    public void TestLeaR32M_Complex()
    {
        // Arrange
        byte[] code = { 0x8D, 0x84, 0x88, 0x78, 0x56, 0x34, 0x12 }; // LEA EAX, [EAX+ECX*4+0x12345678]
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Lea, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (complex memory operand)
        var memOperand = instruction.StructuredOperands[1];
        Assert.IsType<ScaledIndexMemoryOperand>(memOperand);
        var scaledIndexMemoryOperand = (ScaledIndexMemoryOperand)memOperand;
        Assert.Equal(RegisterIndex.A, scaledIndexMemoryOperand.BaseRegister);
        Assert.Equal(RegisterIndex.C, scaledIndexMemoryOperand.IndexRegister);
        Assert.Equal(4, scaledIndexMemoryOperand.Scale);
        Assert.Equal(0x12345678, scaledIndexMemoryOperand.Displacement);
    }
}
