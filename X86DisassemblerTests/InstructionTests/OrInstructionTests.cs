using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for OR instruction handlers
/// </summary>
public class OrInstructionTests
{
    /// <summary>
    /// Tests the OR r8, r/m8 instruction (0x0A)
    /// </summary>
    [Fact]
    public void TestOrR8Rm8()
    {
        // Arrange
        byte[] code = { 0x0A, 0xC8 }; // OR CL, AL
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Or, instruction.Type);
        
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
        Assert.IsType<Register8Operand>(alOperand);
        var registerOperand2 = (Register8Operand)alOperand;
        Assert.Equal(RegisterIndex8.AL, registerOperand2.Register);
        Assert.Equal(8, registerOperand2.Size); // Validate that it's an 8-bit register (AL)
    }
    
    /// <summary>
    /// Tests the OR r8, m8 instruction (0x0A) with memory operand
    /// </summary>
    [Fact]
    public void TestOrR8M8()
    {
        // Arrange
        byte[] code = { 0x0A, 0x00 }; // OR AL, BYTE PTR [EAX]
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Or, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (AL)
        var alOperand = instruction.StructuredOperands[0];
        Assert.IsType<Register8Operand>(alOperand);
        var registerOperand = (Register8Operand)alOperand;
        Assert.Equal(RegisterIndex8.AL, registerOperand.Register);
        Assert.Equal(8, registerOperand.Size); // Validate that it's an 8-bit register (AL)
        
        // Check the second operand (memory operand)
        var memOperand = instruction.StructuredOperands[1];
        Assert.IsType<BaseRegisterMemoryOperand>(memOperand);
        var memoryOperand = (BaseRegisterMemoryOperand)memOperand;
        Assert.Equal(8, memoryOperand.Size); // Validate that it's an 8-bit memory reference
        Assert.Equal(RegisterIndex.A, memoryOperand.BaseRegister);
    }
    
    /// <summary>
    /// Tests the OR r32, r/m32 instruction (0x0B)
    /// </summary>
    [Fact]
    public void TestOrR32Rm32()
    {
        // Arrange
        byte[] code = { 0x0B, 0xC8 }; // OR ECX, EAX
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Or, instruction.Type);
        
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
    /// Tests the OR r32, m32 instruction (0x0B) with memory operand
    /// </summary>
    [Fact]
    public void TestOrR32M32()
    {
        // Arrange
        byte[] code = { 0x0B, 0x00 }; // OR EAX, DWORD PTR [EAX]
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Or, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (memory operand)
        var memOperand = instruction.StructuredOperands[1];
        Assert.IsType<BaseRegisterMemoryOperand>(memOperand);
        var memoryOperand = (BaseRegisterMemoryOperand)memOperand;
        Assert.Equal(32, memoryOperand.Size); // Validate that it's a 32-bit memory reference
        Assert.Equal(RegisterIndex.A, memoryOperand.BaseRegister);
    }
    
    /// <summary>
    /// Tests the OR AL, imm8 instruction (0x0C)
    /// </summary>
    [Fact]
    public void TestOrAlImm8()
    {
        // Arrange
        byte[] code = { 0x0C, 0x42 }; // OR AL, 0x42
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Or, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (AL)
        var alOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(alOperand);
        var registerOperand = (RegisterOperand)alOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(8, registerOperand.Size); // Validate that it's an 8-bit register (AL)
        
        // Check the second operand (immediate)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x42U, immediateOperand.Value);
    }
    
    /// <summary>
    /// Tests the OR EAX, imm32 instruction (0x0D)
    /// </summary>
    [Fact]
    public void TestOrEaxImm32()
    {
        // Arrange
        byte[] code = { 0x0D, 0x78, 0x56, 0x34, 0x12 }; // OR EAX, 0x12345678
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Or, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (immediate)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x12345678U, immediateOperand.Value);
    }
    
    /// <summary>
    /// Tests the OR r/m32, imm32 instruction (0x81 /1)
    /// </summary>
    [Fact]
    public void TestOrRm32Imm32()
    {
        // Arrange
        byte[] code = { 0x81, 0xC8, 0x78, 0x56, 0x34, 0x12 }; // OR EAX, 0x12345678
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Or, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (immediate)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x12345678U, immediateOperand.Value);
    }
    
    /// <summary>
    /// Tests the OR r/m32, imm8 sign-extended instruction (0x83 /1)
    /// </summary>
    [Fact]
    public void TestOrRm32Imm8SignExtended()
    {
        // Arrange
        byte[] code = { 0x83, 0xC8, 0x42 }; // OR EAX, 0x42
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Or, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (immediate)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x00000042U, immediateOperand.Value);
    }
}
