using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for the InstructionDecoder class
/// </summary>
public class InstructionDecoderTests
{
    /// <summary>
    /// Tests that the decoder correctly decodes a TEST AH, imm8 instruction
    /// </summary>
    [Fact]
    public void DecodeInstruction_DecodesTestAhImm8_Correctly()
    {
        // Arrange
        // TEST AH, 0x01 (F6 C4 01) - ModR/M byte C4 = 11 000 100 (mod=3, reg=0, rm=4)
        byte[] codeBuffer = new byte[] { 0xF6, 0xC4, 0x01 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Test, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (AH)
        var ahOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ahOperand);
        var ahRegisterOperand = (RegisterOperand)ahOperand;
        Assert.Equal(RegisterIndex.A, ahRegisterOperand.Register);
        Assert.Equal(8, ahRegisterOperand.Size); // Validate that it's an 8-bit register (AH)
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x01U, immediateOperand.Value);
    }
    
    /// <summary>
    /// Tests that the decoder correctly decodes a TEST r/m8, r8 instruction
    /// </summary>
    [Fact]
    public void DecodeInstruction_DecodesTestRm8R8_Correctly()
    {
        // Arrange
        // TEST CL, AL (84 C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        byte[] codeBuffer = new byte[] { 0x84, 0xC1 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Test, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (CL)
        var clOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(clOperand);
        var clRegisterOperand = (RegisterOperand)clOperand;
        Assert.Equal(RegisterIndex.C, clRegisterOperand.Register);
        Assert.Equal(8, clRegisterOperand.Size); // Validate that it's an 8-bit register (CL)
        
        // Check the second operand (AL)
        var alOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(alOperand);
        var alRegisterOperand = (RegisterOperand)alOperand;
        Assert.Equal(RegisterIndex.A, alRegisterOperand.Register);
        Assert.Equal(8, alRegisterOperand.Size); // Validate that it's an 8-bit register (AL)
    }
    
    /// <summary>
    /// Tests that the decoder correctly decodes a TEST r/m32, r32 instruction
    /// </summary>
    [Fact]
    public void DecodeInstruction_DecodesTestRm32R32_Correctly()
    {
        // Arrange
        // TEST ECX, EAX (85 C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        byte[] codeBuffer = new byte[] { 0x85, 0xC1 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Test, instruction.Type);
        
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
        var eaxRegisterOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, eaxRegisterOperand.Register);
        Assert.Equal(32, eaxRegisterOperand.Size); // Validate that it's a 32-bit register (EAX)
    }
    
    /// <summary>
    /// Tests that the decoder correctly decodes a TEST AL, imm8 instruction
    /// </summary>
    [Fact]
    public void DecodeInstruction_DecodesTestAlImm8_Correctly()
    {
        // Arrange
        // TEST AL, 0x42 (A8 42)
        byte[] codeBuffer = new byte[] { 0xA8, 0x42 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Test, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (AL)
        var alOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(alOperand);
        var alRegisterOperand = (RegisterOperand)alOperand;
        Assert.Equal(RegisterIndex.A, alRegisterOperand.Register);
        Assert.Equal(8, alRegisterOperand.Size); // Validate that it's an 8-bit register (AL)
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x42U, immediateOperand.Value);
        Assert.Equal(8, immediateOperand.Size); // Validate that it's an 8-bit immediate
    }
    
    /// <summary>
    /// Tests that the decoder correctly decodes a TEST EAX, imm32 instruction
    /// </summary>
    [Fact]
    public void DecodeInstruction_DecodesTestEaxImm32_Correctly()
    {
        // Arrange
        // TEST EAX, 0x12345678 (A9 78 56 34 12)
        byte[] codeBuffer = new byte[] { 0xA9, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Test, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var eaxRegisterOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, eaxRegisterOperand.Register);
        Assert.Equal(32, eaxRegisterOperand.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x12345678U, immediateOperand.Value);
        Assert.Equal(32, immediateOperand.Size); // Validate that it's a 32-bit immediate
    }
    
    /// <summary>
    /// Tests that the decoder correctly decodes a TEST r/m32, imm32 instruction
    /// </summary>
    [Fact]
    public void DecodeInstruction_DecodesTestRm32Imm32_Correctly()
    {
        // Arrange
        // TEST EDI, 0x12345678 (F7 C7 78 56 34 12) - ModR/M byte C7 = 11 000 111 (mod=3, reg=0, rm=7)
        byte[] codeBuffer = new byte[] { 0xF7, 0xC7, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Test, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EDI)
        var ediOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ediOperand);
        var ediRegisterOperand = (RegisterOperand)ediOperand;
        Assert.Equal(RegisterIndex.Di, ediRegisterOperand.Register);
        Assert.Equal(32, ediRegisterOperand.Size); // Validate that it's a 32-bit register (EDI)
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x12345678U, immediateOperand.Value);
        Assert.Equal(32, immediateOperand.Size); // Validate that it's a 32-bit immediate
    }
    
    /// <summary>
    /// Tests that the decoder correctly handles multiple instructions in sequence
    /// </summary>
    [Fact]
    public void DecodeInstruction_HandlesMultipleInstructions_Correctly()
    {
        // Arrange
        // TEST AH, 0x01 (F6 C4 01)
        // JZ +45 (74 2D)
        byte[] codeBuffer = new byte[] { 0xF6, 0xC4, 0x01, 0x74, 0x2D };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act - First instruction
        var instruction1 = decoder.DecodeInstruction();
        
        // Assert - First instruction
        Assert.NotNull(instruction1);
        Assert.Equal(InstructionType.Test, instruction1.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction1.StructuredOperands.Count);
        
        // Check the first operand (AH)
        var ahOperand = instruction1.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ahOperand);
        var ahRegisterOperand = (RegisterOperand)ahOperand;
        Assert.Equal(RegisterIndex.A, ahRegisterOperand.Register);
        Assert.Equal(8, ahRegisterOperand.Size); // Validate that it's an 8-bit register (AH)
        
        // Check the second operand (immediate value)
        var immOperand = instruction1.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x01U, immediateOperand.Value);
        
        // Act - Second instruction
        var instruction2 = decoder.DecodeInstruction();
        
        // Assert - Second instruction
        Assert.NotNull(instruction2);
        Assert.Equal(InstructionType.Jz, instruction2.Type);
        
        // Check that we have one operand
        Assert.Single(instruction2.StructuredOperands);
        
        // Check the operand (offset)
        var offsetOperand = instruction2.StructuredOperands[0];
        Assert.IsType<RelativeOffsetOperand>(offsetOperand);
        var relativeOffset = (RelativeOffsetOperand)offsetOperand;
        Assert.Equal(0x00000032UL, relativeOffset.TargetAddress);
    }
}
