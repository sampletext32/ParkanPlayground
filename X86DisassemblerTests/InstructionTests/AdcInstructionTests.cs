using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for ADC (Add with Carry) instruction handlers
/// </summary>
public class AdcInstructionTests
{
    /// <summary>
    /// Tests the AdcImmToRm32Handler for decoding ADC r/m32, imm32 instruction
    /// </summary>
    [Fact]
    public void AdcImmToRm32Handler_DecodesAdcRm32Imm32_Correctly()
    {
        // Arrange
        // ADC EAX, 0x12345678 (81 D0 78 56 34 12) - ModR/M byte D0 = 11 010 000 (mod=3, reg=2, rm=0)
        // mod=3 means direct register addressing, reg=2 is the ADC opcode extension, rm=0 is EAX
        byte[] codeBuffer = new byte[] { 0x81, 0xD0, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Adc, instruction.Type);
        
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
    }
    
    /// <summary>
    /// Tests the AdcImmToRm32SignExtendedHandler for decoding ADC r/m32, imm8 instruction
    /// </summary>
    [Fact]
    public void AdcImmToRm32SignExtendedHandler_DecodesAdcRm32Imm8_Correctly()
    {
        // Arrange
        // ADC EAX, 0x42 (83 D0 42) - ModR/M byte D0 = 11 010 000 (mod=3, reg=2, rm=0)
        // mod=3 means direct register addressing, reg=2 is the ADC opcode extension, rm=0 is EAX
        byte[] codeBuffer = new byte[] { 0x83, 0xD0, 0x42 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Adc, instruction.Type);
        
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
    }
}
