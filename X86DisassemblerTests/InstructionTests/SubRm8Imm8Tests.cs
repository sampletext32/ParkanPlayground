using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for SUB r/m8, imm8 instruction
/// </summary>
public class SubRm8Imm8Tests
{
    /// <summary>
    /// Tests the SUB r8, imm8 instruction with register operand
    /// </summary>
    [Fact]
    public void SubRm8Imm8_Decodes_Correctly()
    {
        // Arrange
        // SUB BL, 0x42
        byte[] codeBuffer = new byte[] { 0x80, 0xeb, 0x42 };
        var disassembler = new Disassembler(codeBuffer, 0x1000);
        
        // Act
        var instructions = disassembler.Disassemble();

        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Sub, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (BL)
        var blOperand = instruction.StructuredOperands[0];
        Assert.IsType<Register8Operand>(blOperand);
        var registerOperand = (Register8Operand)blOperand;
        Assert.Equal(RegisterIndex8.BL, registerOperand.Register);
        Assert.Equal(8, registerOperand.Size); // Validate that it's an 8-bit register (BL)
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x42U, immediateOperand.Value);
        Assert.Equal(8, immediateOperand.Size); // Validate that it's an 8-bit immediate
    }
}