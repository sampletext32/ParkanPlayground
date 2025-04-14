using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for Group 1 sign-extended immediate instructions (0x83 opcode)
/// </summary>
public class Group1SignExtendedHandlerTests
{
    /// <summary>
    /// Tests that the disassembler correctly handles ADD ecx, imm8 instruction (0x83 0xC1 0x04)
    /// </summary>
    [Fact]
    public void Disassembler_HandlesAddEcxImm8_Correctly()
    {
        // Arrange
        // ADD ecx, 0x04 (83 C1 04)
        byte[] codeBuffer = new byte[] { 0x83, 0xC1, 0x04 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Add, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (ECX)
        var ecxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ecxOperand);
        var registerOperand = (RegisterOperand)ecxOperand;
        Assert.Equal(RegisterIndex.C, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (ECX)
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x04U, immediateOperand.Value);
    }
    
    /// <summary>
    /// Tests that the disassembler correctly handles the specific sequence from address 0x00001874
    /// </summary>
    [Fact]
    public void Disassembler_HandlesSpecificSequence_Correctly()
    {
        // Arrange
        // This is the sequence from the problematic example:
        // 83 C1 04 50 E8 42 01 00 00
        byte[] codeBuffer = new byte[] { 0x83, 0xC1, 0x04, 0x50, 0xE8, 0x42, 0x01, 0x00, 0x00 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.True(instructions.Count >= 3, $"Expected at least 3 instructions, but got {instructions.Count}");
        
        // First instruction should be ADD ecx, 0x04
        var firstInstruction = instructions[0];
        Assert.Equal(InstructionType.Add, firstInstruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, firstInstruction.StructuredOperands.Count);
        
        // Check the first operand (ECX)
        var ecxOperand = firstInstruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ecxOperand);
        var registerOperand = (RegisterOperand)ecxOperand;
        Assert.Equal(RegisterIndex.C, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (ECX)
        
        // Check the second operand (immediate value)
        var immOperand = firstInstruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x04U, immediateOperand.Value);
    }
}
