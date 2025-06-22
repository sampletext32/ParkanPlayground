using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for SBB (Subtract with Borrow) instruction handlers
/// </summary>
public class SbbInstructionTests
{
    /// <summary>
    /// Tests the SbbImmFromRm32Handler for decoding SBB r/m32, imm32 instruction
    /// </summary>
    [Fact]
    public void SbbImmFromRm32Handler_DecodesSbbRm32Imm32_Correctly()
    {
        // Arrange
        // SBB EAX, 0x12345678 (81 D8 78 56 34 12) - ModR/M byte D8 = 11 011 000 (mod=3, reg=3, rm=0)
        // mod=3 means direct register addressing, reg=3 is the SBB opcode extension, rm=0 is EAX
        byte[] codeBuffer = new byte[] { 0x81, 0xD8, 0x78, 0x56, 0x34, 0x12 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Sbb, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x12345678U, immediateOperand.Value);
        Assert.Equal(32, immediateOperand.Size); // Validate that it's a 32-bit immediate
    }
    
    /// <summary>
    /// Tests the SbbImmFromRm32SignExtendedHandler for decoding SBB r/m32, imm8 instruction
    /// </summary>
    [Fact]
    public void SbbImmFromRm32SignExtendedHandler_DecodesSbbRm32Imm8_Correctly()
    {
        // Arrange
        // SBB EAX, 0x42 (83 D8 42) - ModR/M byte D8 = 11 011 000 (mod=3, reg=3, rm=0)
        // mod=3 means direct register addressing, reg=3 is the SBB opcode extension, rm=0 is EAX
        byte[] codeBuffer = new byte[] { 0x83, 0xD8, 0x42 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Sbb, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x00000042U, immediateOperand.Value);
        Assert.Equal(32, immediateOperand.Size); // Validate that it's a 32-bit immediate
    }
}
