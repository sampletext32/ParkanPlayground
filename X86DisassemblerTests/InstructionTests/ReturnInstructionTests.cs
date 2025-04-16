using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for return instruction handlers
/// </summary>
public class ReturnInstructionTests
{
    /// <summary>
    /// Tests the RetHandler for decoding RET instruction
    /// </summary>
    [Fact]
    public void RetHandler_DecodesRet_Correctly()
    {
        // Arrange
        // RET (C3) - Return from procedure
        byte[] codeBuffer = new byte[] { 0xC3 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Ret, instruction.Type);
        
        // Check that we have no operands
        Assert.Empty(instruction.StructuredOperands);
    }
    
    /// <summary>
    /// Tests the RetImmHandler for decoding RET imm16 instruction
    /// </summary>
    [Fact]
    public void RetImmHandler_DecodesRetImm16_Correctly()
    {
        // Arrange
        // RET 0x1234 (C2 34 12) - Return from procedure and pop 0x1234 bytes
        byte[] codeBuffer = new byte[] { 0xC2, 0x34, 0x12 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Ret, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (immediate value)
        var immOperand = instruction.StructuredOperands[0];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x1234U, immediateOperand.Value);
        Assert.Equal(16, immediateOperand.Size); // Validate that it's a 16-bit immediate
    }
}
