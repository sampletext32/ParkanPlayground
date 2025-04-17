using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for string instruction handlers
/// </summary>
public class StringInstructionHandlerTests
{
    /// <summary>
    /// Tests the StringInstructionHandler for decoding REP MOVS instruction
    /// </summary>
    [Fact]
    public void StringInstructionHandler_DecodesRepMovs_Correctly()
    {
        // Arrange
        // REP MOVS (F3 A5)
        byte[] codeBuffer = new byte[] { 0xF3, 0xA5 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.RepMovsD, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (destination memory operand)
        var destOperand = instruction.StructuredOperands[0];
        Assert.IsType<BaseRegisterMemoryOperand>(destOperand);
        var destMemoryOperand = (BaseRegisterMemoryOperand)destOperand;
        Assert.Equal(RegisterIndex.Di, destMemoryOperand.BaseRegister);
        Assert.Equal(32, destMemoryOperand.Size); // Validate that it's a 32-bit memory reference
        
        // Check the second operand (source memory operand)
        var srcOperand = instruction.StructuredOperands[1];
        Assert.IsType<BaseRegisterMemoryOperand>(srcOperand);
        var srcMemoryOperand = (BaseRegisterMemoryOperand)srcOperand;
        Assert.Equal(RegisterIndex.Si, srcMemoryOperand.BaseRegister);
        Assert.Equal(32, srcMemoryOperand.Size); // Validate that it's a 32-bit memory reference
    }
    
    /// <summary>
    /// Tests the StringInstructionHandler for decoding REPNE SCAS instruction
    /// </summary>
    [Fact]
    public void StringInstructionHandler_DecodesRepneScas_Correctly()
    {
        // Arrange
        // REPNE SCAS (F2 AF)
        byte[] codeBuffer = new byte[] { 0xF2, 0xAF };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.RepneScasD, instruction.Type);
        
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
        Assert.Equal(RegisterIndex.Di, memoryOperand.BaseRegister);
        Assert.Equal(32, memoryOperand.Size); // Validate that it's a 32-bit memory reference
    }
    
    /// <summary>
    /// Tests the StringInstructionHandler for decoding MOVS instruction without prefix
    /// </summary>
    [Fact]
    public void StringInstructionHandler_DecodesMovs_Correctly()
    {
        // Arrange
        // MOVS (A5)
        byte[] codeBuffer = new byte[] { 0xA5 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.MovsD, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (destination memory operand)
        var destOperand = instruction.StructuredOperands[0];
        Assert.IsType<BaseRegisterMemoryOperand>(destOperand);
        var destMemoryOperand = (BaseRegisterMemoryOperand)destOperand;
        Assert.Equal(RegisterIndex.Di, destMemoryOperand.BaseRegister);
        Assert.Equal(32, destMemoryOperand.Size); // Validate that it's a 32-bit memory reference
        
        // Check the second operand (source memory operand)
        var srcOperand = instruction.StructuredOperands[1];
        Assert.IsType<BaseRegisterMemoryOperand>(srcOperand);
        var srcMemoryOperand = (BaseRegisterMemoryOperand)srcOperand;
        Assert.Equal(RegisterIndex.Si, srcMemoryOperand.BaseRegister);
        Assert.Equal(32, srcMemoryOperand.Size); // Validate that it's a 32-bit memory reference
    }
    
    /// <summary>
    /// Tests the StringInstructionHandler for decoding STOSB instruction
    /// </summary>
    [Fact]
    public void StringInstructionHandler_DecodesStosb_Correctly()
    {
        // Arrange
        // STOSB (AA)
        byte[] codeBuffer = new byte[] { 0xAA };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.StosB, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (memory operand)
        var memOperand = instruction.StructuredOperands[0];
        Assert.IsType<BaseRegisterMemoryOperand>(memOperand);
        var memoryOperand = (BaseRegisterMemoryOperand)memOperand;
        Assert.Equal(RegisterIndex.Di, memoryOperand.BaseRegister);
        Assert.Equal(8, memoryOperand.Size); // Validate that it's an 8-bit memory reference
        
        // Check the second operand (AL)
        var alOperand = instruction.StructuredOperands[1];
        Assert.IsType<Register8Operand>(alOperand);
        var registerOperand = (Register8Operand)alOperand;
        Assert.Equal(RegisterIndex8.AL, registerOperand.Register);
        Assert.Equal(8, registerOperand.Size); // Validate that it's an 8-bit register (AL)
    }
    
    /// <summary>
    /// Tests the StringInstructionHandler for decoding LODSD instruction
    /// </summary>
    [Fact]
    public void StringInstructionHandler_DecodesLodsd_Correctly()
    {
        // Arrange
        // LODSD (AD)
        byte[] codeBuffer = new byte[] { 0xAD };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.LodsD, instruction.Type);
        
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
        Assert.Equal(RegisterIndex.Si, memoryOperand.BaseRegister);
        Assert.Equal(32, memoryOperand.Size); // Validate that it's a 32-bit memory reference
    }
    
    /// <summary>
    /// Tests the StringInstructionHandler for decoding SCASB instruction
    /// </summary>
    [Fact]
    public void StringInstructionHandler_DecodesScasb_Correctly()
    {
        // Arrange
        // SCASB (AE)
        byte[] codeBuffer = new byte[] { 0xAE };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.ScasB, instruction.Type);
        
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
        Assert.Equal(RegisterIndex.Di, memoryOperand.BaseRegister);
        Assert.Equal(8, memoryOperand.Size); // Validate that it's an 8-bit memory reference
    }
}
