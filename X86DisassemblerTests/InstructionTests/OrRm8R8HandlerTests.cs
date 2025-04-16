using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for OR r/m8, r8 instruction handler
/// </summary>
public class OrRm8R8HandlerTests
{
    /// <summary>
    /// Tests the OrRm8R8Handler for decoding OR [mem], reg8 instruction
    /// </summary>
    [Fact]
    public void OrRm8R8Handler_DecodesOrMemReg8_Correctly()
    {
        // Arrange
        // OR [ebx+ecx*4+0x41], al (08 44 8B 41)
        byte[] codeBuffer = new byte[] { 0x08, 0x44, 0x8B, 0x41 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Or, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (memory operand with SIB)
        var memOperand = instruction.StructuredOperands[0];
        Assert.IsType<ScaledIndexMemoryOperand>(memOperand);
        var scaledIndexMemoryOperand = (ScaledIndexMemoryOperand)memOperand;
        Assert.Equal(RegisterIndex.B, scaledIndexMemoryOperand.BaseRegister);
        Assert.Equal(RegisterIndex.C, scaledIndexMemoryOperand.IndexRegister);
        Assert.Equal(4, scaledIndexMemoryOperand.Scale);
        Assert.Equal(0x41, scaledIndexMemoryOperand.Displacement);
        Assert.Equal(8, scaledIndexMemoryOperand.Size); // Validate that it's an 8-bit memory reference
        
        // Check the second operand (AL)
        var alOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(alOperand);
        var registerOperand = (RegisterOperand)alOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(8, registerOperand.Size); // Validate that it's an 8-bit register (AL)
    }
    
    /// <summary>
    /// Tests the OrRm8R8Handler for decoding OR reg8, reg8 instruction
    /// </summary>
    [Fact]
    public void OrRm8R8Handler_DecodesOrRegReg8_Correctly()
    {
        // Arrange
        // OR bl, ch (08 EB)
        byte[] codeBuffer = new byte[] { 0x08, 0xEB };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Or, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (BL)
        var blOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(blOperand);
        var registerOperand1 = (RegisterOperand)blOperand;
        Assert.Equal(RegisterIndex.B, registerOperand1.Register);
        Assert.Equal(8, registerOperand1.Size); // Validate that it's an 8-bit register (BL)
        
        // Check the second operand (CH)
        var chOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(chOperand);
        var registerOperand2 = (RegisterOperand)chOperand;
        Assert.Equal(RegisterIndex.C, registerOperand2.Register);
        Assert.Equal(8, registerOperand2.Size); // Validate that it's an 8-bit register (CH)
    }
}
