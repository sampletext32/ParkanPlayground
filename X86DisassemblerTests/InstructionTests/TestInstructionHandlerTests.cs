using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for TEST instruction handlers
/// </summary>
public class TestInstructionHandlerTests
{
    /// <summary>
    /// Tests the TestRegMemHandler for decoding TEST r/m32, r32 instructions
    /// </summary>
    [Fact]
    public void TestRegMemHandler_DecodesTestR32R32_Correctly()
    {
        // Arrange
        // TEST ECX, EAX (85 C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        // mod=3 means direct register addressing, reg=0 is EAX, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0x85, 0xC1 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Test, instruction.Type);
        
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
    /// Tests the TestRegMem8Handler for decoding TEST r/m8, r8 instructions
    /// </summary>
    [Fact]
    public void TestRegMem8Handler_DecodesTestR8R8_Correctly()
    {
        // Arrange
        // TEST CL, AL (84 C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        // mod=3 means direct register addressing, reg=0 is AL, rm=1 is CL
        byte[] codeBuffer = new byte[] { 0x84, 0xC1 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Test, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (CL)
        var clOperand = instruction.StructuredOperands[0];
        Assert.IsType<Register8Operand>(clOperand);
        var registerOperand1 = (Register8Operand)clOperand;
        Assert.Equal(RegisterIndex8.CL, registerOperand1.Register);
        Assert.Equal(8, registerOperand1.Size); // Validate that it's an 8-bit register (CL)
        
        // Check the second operand (AL)
        var alOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(alOperand);
        var registerOperand2 = (RegisterOperand)alOperand;
        Assert.Equal(RegisterIndex.A, registerOperand2.Register);
        Assert.Equal(8, registerOperand2.Size); // Validate that it's an 8-bit register (AL)
    }
    
    /// <summary>
    /// Tests the TestAlImmHandler for decoding TEST AL, imm8 instructions
    /// </summary>
    [Fact]
    public void TestAlImmHandler_DecodesTestAlImm8_Correctly()
    {
        // Arrange
        // TEST AL, 0x42 (A8 42)
        byte[] codeBuffer = new byte[] { 0xA8, 0x42 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Test, instruction.Type);
        
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
    /// Tests the TestEaxImmHandler for decoding TEST EAX, imm32 instructions
    /// </summary>
    [Fact]
    public void TestEaxImmHandler_DecodesTestEaxImm32_Correctly()
    {
        // Arrange
        // TEST EAX, 0x12345678 (A9 78 56 34 12)
        byte[] codeBuffer = new byte[] { 0xA9, 0x78, 0x56, 0x34, 0x12 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Test, instruction.Type);
        
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
    /// Tests the TestImmWithRm8Handler for decoding TEST r/m8, imm8 instructions
    /// </summary>
    [Fact]
    public void TestImmWithRm8Handler_DecodesTestRm8Imm8_Correctly()
    {
        // Arrange
        // TEST AH, 0x01 (F6 C4 01) - ModR/M byte C4 = 11 000 100 (mod=3, reg=0, rm=4)
        // mod=3 means direct register addressing, reg=0 indicates TEST operation, rm=4 is AH
        byte[] codeBuffer = new byte[] { 0xF6, 0xC4, 0x01 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Test, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (AH)
        var ahOperand = instruction.StructuredOperands[0];
        Assert.IsType<Register8Operand>(ahOperand);
        var registerOperand = (Register8Operand)ahOperand;
        Assert.Equal(RegisterIndex8.AH, registerOperand.Register);
        Assert.Equal(8, registerOperand.Size); // Validate that it's an 8-bit register (AH)
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x01U, immediateOperand.Value);
        Assert.Equal(8, immediateOperand.Size); // Validate that it's an 8-bit immediate
    }
    
    /// <summary>
    /// Tests the TestImmWithRm32Handler for decoding TEST r/m32, imm32 instructions
    /// </summary>
    [Fact]
    public void TestImmWithRm32Handler_DecodesTestRm32Imm32_Correctly()
    {
        // Arrange
        // TEST EDI, 0x12345678 (F7 C7 78 56 34 12) - ModR/M byte C7 = 11 000 111 (mod=3, reg=0, rm=7)
        // mod=3 means direct register addressing, reg=0 indicates TEST operation, rm=7 is EDI
        byte[] codeBuffer = new byte[] { 0xF7, 0xC7, 0x78, 0x56, 0x34, 0x12 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Test, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EDI)
        var ediOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ediOperand);
        var registerOperand = (RegisterOperand)ediOperand;
        Assert.Equal(RegisterIndex.Di, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EDI)
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x12345678U, immediateOperand.Value);
        Assert.Equal(32, immediateOperand.Size); // Validate that it's a 32-bit immediate
    }
}
