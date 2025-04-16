using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for CMP instruction handlers
/// </summary>
public class CmpInstructionHandlerTests
{
    /// <summary>
    /// Tests the CmpAlImmHandler for decoding CMP AL, imm8 instructions
    /// </summary>
    [Fact]
    public void CmpAlImmHandler_DecodesCmpAlImm8_Correctly()
    {
        // Arrange
        // CMP AL, 0x03 (3C 03)
        byte[] codeBuffer = new byte[] { 0x3C, 0x03 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Cmp, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (AL)
        var alOperand = instruction.StructuredOperands[0];
        Assert.IsType<Register8Operand>(alOperand);
        var registerOperand = (Register8Operand)alOperand;
        Assert.Equal(RegisterIndex8.AL, registerOperand.Register);
        Assert.Equal(8, registerOperand.Size); // Validate that it's an 8-bit register (AL)
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x03U, immediateOperand.Value);
    }
    
    /// <summary>
    /// Tests the CmpAlImmHandler with a different immediate value
    /// </summary>
    [Fact]
    public void CmpAlImmHandler_DecodesCmpAlImm8_WithDifferentValue()
    {
        // Arrange
        // CMP AL, 0xFF (3C FF)
        byte[] codeBuffer = new byte[] { 0x3C, 0xFF };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Cmp, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (AL)
        var alOperand = instruction.StructuredOperands[0];
        Assert.IsType<Register8Operand>(alOperand);
        var registerOperand = (Register8Operand)alOperand;
        Assert.Equal(RegisterIndex8.AL, registerOperand.Register);
        Assert.Equal(8, registerOperand.Size); // Validate that it's an 8-bit register (AL)
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0xFFU, immediateOperand.Value);
    }
    
    /// <summary>
    /// Tests the CmpRm32R32Handler for decoding CMP r32, r32 instructions
    /// </summary>
    [Fact]
    public void CmpRm32R32Handler_DecodesCmpR32R32_Correctly()
    {
        // Arrange
        // CMP ECX, EAX (39 C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        // mod=3 means direct register addressing, reg=0 is EAX, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0x39, 0xC1 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Cmp, instruction.Type);
        
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
    /// Tests the CmpRm32R32Handler for decoding CMP m32, r32 instructions
    /// </summary>
    [Fact]
    public void CmpRm32R32Handler_DecodesCmpM32R32_Correctly()
    {
        // Arrange
        // CMP [EBX+0x10], EDX (39 53 10) - ModR/M byte 53 = 01 010 011 (mod=1, reg=2, rm=3)
        // mod=1 means memory addressing with 8-bit displacement, reg=2 is EDX, rm=3 is EBX
        byte[] codeBuffer = new byte[] { 0x39, 0x53, 0x10 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Cmp, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand ([EBX+0x10])
        var memOperand = instruction.StructuredOperands[0];
        Assert.IsType<DisplacementMemoryOperand>(memOperand);
        var memoryOperand = (DisplacementMemoryOperand)memOperand;
        Assert.Equal(RegisterIndex.B, memoryOperand.BaseRegister);
        Assert.Equal(0x10, memoryOperand.Displacement);
        Assert.Equal(32, memoryOperand.Size); // Memory size is 32 bits (DWORD)
        
        // Check the second operand (EDX)
        var edxOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(edxOperand);
        var edxRegisterOperand = (RegisterOperand)edxOperand;
        Assert.Equal(RegisterIndex.D, edxRegisterOperand.Register);
        Assert.Equal(32, edxRegisterOperand.Size); // Validate that it's a 32-bit register (EDX)
    }
}
