using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for AND instruction handlers
/// </summary>
public class AndInstructionTests
{
    /// <summary>
    /// Tests the AndImmWithRm32Handler for decoding AND r/m32, imm32 instruction
    /// </summary>
    [Fact]
    public void AndImmWithRm32Handler_DecodesAndRm32Imm32_Correctly()
    {
        // Arrange
        // AND EAX, 0x12345678 (81 E0 78 56 34 12) - ModR/M byte E0 = 11 100 000 (mod=3, reg=4, rm=0)
        // mod=3 means direct register addressing, reg=4 is the AND opcode extension, rm=0 is EAX
        byte[] codeBuffer = new byte[] { 0x81, 0xE0, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.And, instruction.Type);
        
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
    /// Tests the AndImmWithRm32SignExtendedHandler for decoding AND r/m32, imm8 instruction
    /// </summary>
    [Fact]
    public void AndImmWithRm32SignExtendedHandler_DecodesAndRm32Imm8_Correctly()
    {
        // Arrange
        // AND EAX, 0x42 (83 E0 42) - ModR/M byte E0 = 11 100 000 (mod=3, reg=4, rm=0)
        // mod=3 means direct register addressing, reg=4 is the AND opcode extension, rm=0 is EAX
        byte[] codeBuffer = new byte[] { 0x83, 0xE0, 0x42 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.And, instruction.Type);
        
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
    
    /// <summary>
    /// Tests the AndRegMemHandler for decoding AND r32, r/m32 instruction
    /// </summary>
    [Fact]
    public void AndRegMemHandler_DecodesAndR32Rm32_Correctly()
    {
        // Arrange
        // AND EAX, ECX (23 C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        // mod=3 means direct register addressing, reg=0 is EAX, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0x23, 0xC1 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.And, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (ECX)
        var ecxOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(ecxOperand);
        var ecxRegisterOperand = (RegisterOperand)ecxOperand;
        Assert.Equal(RegisterIndex.C, ecxRegisterOperand.Register);
        Assert.Equal(32, ecxRegisterOperand.Size); // Validate that it's a 32-bit register (ECX)
    }
    
    /// <summary>
    /// Tests the AndMemRegHandler for decoding AND r/m32, r32 instruction
    /// </summary>
    [Fact]
    public void AndMemRegHandler_DecodesAndRm32R32_Correctly()
    {
        // Arrange
        // AND ECX, EAX (21 C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        // mod=3 means direct register addressing, reg=0 is EAX, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0x21, 0xC1 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.And, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (ECX)
        var ecxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ecxOperand);
        var ecxRegisterOperand = (RegisterOperand)ecxOperand;
        Assert.Equal(RegisterIndex.C, ecxRegisterOperand.Register);
        Assert.Equal(32, ecxRegisterOperand.Size); // Validate that it's a 32-bit register (ECX)
        
        // Check the second operand (EAX)
        var eaxOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EAX)
    }
}
