using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for call instruction handlers
/// </summary>
public class CallInstructionTests
{
    /// <summary>
    /// Tests the CallRel32Handler for decoding CALL rel32 instruction
    /// </summary>
    [Fact]
    public void CallRel32Handler_DecodesCallRel32_Correctly()
    {
        // Arrange
        // CALL +0x12345678 (E8 78 56 34 12) - Call to address 0x12345678 bytes forward
        byte[] codeBuffer = new byte[] { 0xE8, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Call, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check that the operand is a relative offset operand
        var operand = instruction.StructuredOperands[0];
        Assert.IsType<RelativeOffsetOperand>(operand);
        
        // Check the target address
        var relativeOffsetOperand = (RelativeOffsetOperand)operand;
        Assert.Equal(0x1234567DUL, relativeOffsetOperand.TargetAddress); // Current position (5) + offset (0x12345678) = 0x1234567D
    }
}
