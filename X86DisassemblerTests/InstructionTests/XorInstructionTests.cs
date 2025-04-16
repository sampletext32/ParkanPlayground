using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for XOR instruction handlers
/// </summary>
public class XorInstructionTests
{
    /// <summary>
    /// Tests the XorRegMemHandler for decoding XOR r32, r/m32 instruction
    /// </summary>
    [Fact]
    public void XorRegMemHandler_DecodesXorR32Rm32_Correctly()
    {
        // Arrange
        // XOR EAX, ECX (33 C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        // mod=3 means direct register addressing, reg=0 is EAX, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0x33, 0xC1 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Xor, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand1 = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand1.Register);
        Assert.Equal(32, registerOperand1.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (ECX)
        var ecxOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(ecxOperand);
        var registerOperand2 = (RegisterOperand)ecxOperand;
        Assert.Equal(RegisterIndex.C, registerOperand2.Register);
        Assert.Equal(32, registerOperand2.Size); // Validate that it's a 32-bit register (ECX)
    }
    
    /// <summary>
    /// Tests the XorMemRegHandler for decoding XOR r/m32, r32 instruction
    /// </summary>
    [Fact]
    public void XorMemRegHandler_DecodesXorRm32R32_Correctly()
    {
        // Arrange
        // XOR ECX, EAX (31 C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        // mod=3 means direct register addressing, reg=0 is EAX, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0x31, 0xC1 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Xor, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (ECX)
        var ecxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ecxOperand);
        var registerOperand1 = (RegisterOperand)ecxOperand;
        Assert.Equal(RegisterIndex.C, registerOperand1.Register);
        Assert.Equal(32, registerOperand1.Size); // Validate that it's a 32-bit register (ECX)
        
        // Check the second operand (EAX)
        var eaxOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand2 = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand2.Register);
        Assert.Equal(32, registerOperand2.Size); // Validate that it's a 32-bit register (EAX)
    }
    
    /// <summary>
    /// Tests the XorAlImmHandler for decoding XOR AL, imm8 instruction
    /// </summary>
    [Fact]
    public void XorAlImmHandler_DecodesXorAlImm8_Correctly()
    {
        // Arrange
        // XOR AL, 0x42 (34 42)
        byte[] codeBuffer = new byte[] { 0x34, 0x42 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Xor, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (AL)
        var alOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(alOperand);
        var registerOperand = (RegisterOperand)alOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(8, registerOperand.Size); // Validate that it's an 8-bit register (AL)
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x42U, immediateOperand.Value);
        Assert.Equal(8, immediateOperand.Size); // Validate that it's an 8-bit immediate
    }
    
    /// <summary>
    /// Tests the XorEaxImmHandler for decoding XOR EAX, imm32 instruction
    /// </summary>
    [Fact]
    public void XorEaxImmHandler_DecodesXorEaxImm32_Correctly()
    {
        // Arrange
        // XOR EAX, 0x12345678 (35 78 56 34 12)
        byte[] codeBuffer = new byte[] { 0x35, 0x78, 0x56, 0x34, 0x12 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Xor, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand1 = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand1.Register);
        Assert.Equal(32, registerOperand1.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (0x12345678)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x12345678U, immediateOperand.Value);
        Assert.Equal(32, immediateOperand.Size); // Validate that it's a 32-bit immediate
    }
}
