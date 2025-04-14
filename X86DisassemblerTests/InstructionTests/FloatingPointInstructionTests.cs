using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for floating-point instruction handlers
/// </summary>
public class FloatingPointInstructionTests
{
    /// <summary>
    /// Tests the FnstswHandler for decoding FNSTSW AX instruction
    /// </summary>
    [Fact]
    public void FnstswHandler_DecodesFnstswAx_Correctly()
    {
        // Arrange
        // FNSTSW AX (DF E0)
        byte[] codeBuffer = new byte[] { 0xDF, 0xE0 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Fnstsw, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (AX)
        var axOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(axOperand);
        var registerOperand = (RegisterOperand)axOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(16, registerOperand.Size); // Validate that it's a 16-bit register (AX)
    }
    
    /// <summary>
    /// Tests the Float32OperationHandler for decoding FADD ST(0), ST(1) instruction
    /// </summary>
    [Fact]
    public void Float32OperationHandler_DecodesAddSt0St1_Correctly()
    {
        // Arrange
        // FADD ST(0), ST(1) (D8 C1)
        byte[] codeBuffer = new byte[] { 0xD8, 0xC1 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Fadd, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (ST(0))
        var st0Operand = instruction.StructuredOperands[0];
        Assert.IsType<FPURegisterOperand>(st0Operand);
        var fpuRegisterOperand1 = (FPURegisterOperand)st0Operand;
        Assert.Equal(FpuRegisterIndex.ST0, fpuRegisterOperand1.RegisterIndex);
        
        // Check the second operand (ST(1))
        var st1Operand = instruction.StructuredOperands[1];
        Assert.IsType<FPURegisterOperand>(st1Operand);
        var fpuRegisterOperand2 = (FPURegisterOperand)st1Operand;
        Assert.Equal(FpuRegisterIndex.ST1, fpuRegisterOperand2.RegisterIndex);
    }
    
    /// <summary>
    /// Tests the Float32OperationHandler for decoding FADD dword ptr [eax] instruction
    /// </summary>
    [Fact]
    public void Float32OperationHandler_DecodesAddMemory_Correctly()
    {
        // Arrange
        // FADD dword ptr [eax] (D8 00)
        byte[] codeBuffer = new byte[] { 0xD8, 0x00 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Fadd, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (dword ptr [eax])
        var memoryOperand = instruction.StructuredOperands[0];
        Assert.IsType<BaseRegisterMemoryOperand>(memoryOperand);
        var baseRegisterMemoryOperand = (BaseRegisterMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.A, baseRegisterMemoryOperand.BaseRegister);
        Assert.Equal(32, baseRegisterMemoryOperand.Size); // Validate that it's a 32-bit memory reference
    }
    
    /// <summary>
    /// Tests the LoadStoreControlHandler for decoding FLD dword ptr [eax] instruction
    /// </summary>
    [Fact]
    public void LoadStoreControlHandler_DecodesLoadMemory_Correctly()
    {
        // Arrange
        // FLD dword ptr [eax] (D9 00)
        byte[] codeBuffer = new byte[] { 0xD9, 0x00 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Fld, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (dword ptr [eax])
        var memoryOperand = instruction.StructuredOperands[0];
        Assert.IsType<BaseRegisterMemoryOperand>(memoryOperand);
        var baseRegisterMemoryOperand = (BaseRegisterMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.A, baseRegisterMemoryOperand.BaseRegister);
        Assert.Equal(32, baseRegisterMemoryOperand.Size); // Validate that it's a 32-bit memory reference
    }
    
    /// <summary>
    /// Tests the LoadStoreControlHandler for decoding FLDCW [eax] instruction
    /// </summary>
    [Fact]
    public void LoadStoreControlHandler_DecodesLoadControlWord_Correctly()
    {
        // Arrange
        // FLDCW [eax] (D9 28)
        byte[] codeBuffer = new byte[] { 0xD9, 0x28 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Fldcw, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (word ptr [eax])
        var memoryOperand = instruction.StructuredOperands[0];
        Assert.IsType<BaseRegisterMemoryOperand>(memoryOperand);
        var memoryOperandCast = (BaseRegisterMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.A, memoryOperandCast.BaseRegister);
        Assert.Equal(16, memoryOperandCast.Size);
    }
    
    /// <summary>
    /// Tests the Int32OperationHandler for decoding FIADD dword ptr [eax] instruction
    /// </summary>
    [Fact]
    public void Int32OperationHandler_DecodesIntegerAdd_Correctly()
    {
        // Arrange
        // FIADD dword ptr [eax] (DA 00)
        byte[] codeBuffer = new byte[] { 0xDA, 0x00 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Fiadd, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (dword ptr [eax])
        var memoryOperand = instruction.StructuredOperands[0];
        Assert.IsType<BaseRegisterMemoryOperand>(memoryOperand);
        var baseRegisterMemoryOperand = (BaseRegisterMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.A, baseRegisterMemoryOperand.BaseRegister);
        Assert.Equal(32, baseRegisterMemoryOperand.Size); // Validate that it's a 32-bit memory reference
    }
    
    /// <summary>
    /// Tests the LoadStoreInt32Handler for decoding FILD dword ptr [eax] instruction
    /// </summary>
    [Fact]
    public void LoadStoreInt32Handler_DecodesIntegerLoad_Correctly()
    {
        // Arrange
        // FILD dword ptr [eax] (DB 00)
        byte[] codeBuffer = new byte[] { 0xDB, 0x00 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Fild, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (dword ptr [eax])
        var memoryOperand = instruction.StructuredOperands[0];
        Assert.IsType<BaseRegisterMemoryOperand>(memoryOperand);
        var baseRegisterMemoryOperand = (BaseRegisterMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.A, baseRegisterMemoryOperand.BaseRegister);
        Assert.Equal(32, baseRegisterMemoryOperand.Size); // Validate that it's a 32-bit memory reference
    }
    
    /// <summary>
    /// Tests the Float64OperationHandler for decoding FADD qword ptr [eax] instruction
    /// </summary>
    [Fact]
    public void Float64OperationHandler_DecodesDoubleAdd_Correctly()
    {
        // Arrange
        // FADD qword ptr [eax] (DC 00)
        byte[] codeBuffer = new byte[] { 0xDC, 0x00 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Fadd, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (qword ptr [eax])
        var memoryOperand = instruction.StructuredOperands[0];
        Assert.IsType<BaseRegisterMemoryOperand>(memoryOperand);
        var baseRegisterMemoryOperand = (BaseRegisterMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.A, baseRegisterMemoryOperand.BaseRegister);
        Assert.Equal(64, baseRegisterMemoryOperand.Size); // Validate that it's a 64-bit memory reference
    }
    
    /// <summary>
    /// Tests the Float64OperationHandler for decoding FADD ST(1), ST(0) instruction
    /// </summary>
    [Fact]
    public void Float64OperationHandler_DecodesAddSt1St0_Correctly()
    {
        // Arrange
        // FADD ST(1), ST(0) (DC C1)
        byte[] codeBuffer = new byte[] { 0xDC, 0xC1 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Fadd, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (ST(1))
        var st1Operand = instruction.StructuredOperands[0];
        Assert.IsType<FPURegisterOperand>(st1Operand);
        var fpuRegisterOperand1 = (FPURegisterOperand)st1Operand;
        Assert.Equal(FpuRegisterIndex.ST1, fpuRegisterOperand1.RegisterIndex);
        
        // Check the second operand (ST(0))
        var st0Operand = instruction.StructuredOperands[1];
        Assert.IsType<FPURegisterOperand>(st0Operand);
        var fpuRegisterOperand2 = (FPURegisterOperand)st0Operand;
        Assert.Equal(FpuRegisterIndex.ST0, fpuRegisterOperand2.RegisterIndex);
    }
    
    /// <summary>
    /// Tests the LoadStoreFloat64Handler for decoding FLD qword ptr [eax] instruction
    /// </summary>
    [Fact]
    public void LoadStoreFloat64Handler_DecodesDoubleLoad_Correctly()
    {
        // Arrange
        // FLD qword ptr [eax] (DD 00)
        byte[] codeBuffer = new byte[] { 0xDD, 0x00 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Fld, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (qword ptr [eax])
        var memoryOperand = instruction.StructuredOperands[0];
        Assert.IsType<BaseRegisterMemoryOperand>(memoryOperand);
        var baseRegisterMemoryOperand = (BaseRegisterMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.A, baseRegisterMemoryOperand.BaseRegister);
        Assert.Equal(64, baseRegisterMemoryOperand.Size); // Validate that it's a 64-bit memory reference
    }
    
    /// <summary>
    /// Tests the Int16OperationHandler for decoding FIADD word ptr [eax] instruction
    /// </summary>
    [Fact]
    public void Int16OperationHandler_DecodesShortAdd_Correctly()
    {
        // Arrange
        // FIADD word ptr [eax] (DE 00)
        byte[] codeBuffer = new byte[] { 0xDE, 0x00 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Fiadd, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (word ptr [eax])
        var memoryOperand = instruction.StructuredOperands[0];
        Assert.IsType<BaseRegisterMemoryOperand>(memoryOperand);
        var baseRegisterMemoryOperand = (BaseRegisterMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.A, baseRegisterMemoryOperand.BaseRegister);
        Assert.Equal(16, baseRegisterMemoryOperand.Size); // Validate that it's a 16-bit memory reference
    }
    
    /// <summary>
    /// Tests the LoadStoreInt16Handler for decoding FILD word ptr [eax] instruction
    /// </summary>
    [Fact]
    public void LoadStoreInt16Handler_DecodesShortLoad_Correctly()
    {
        // Arrange
        // FILD word ptr [eax] (DF 00)
        byte[] codeBuffer = new byte[] { 0xDF, 0x00 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Fild, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (word ptr [eax])
        var memoryOperand = instruction.StructuredOperands[0];
        Assert.IsType<BaseRegisterMemoryOperand>(memoryOperand);
        var baseRegisterMemoryOperand = (BaseRegisterMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.A, baseRegisterMemoryOperand.BaseRegister);
        Assert.Equal(16, baseRegisterMemoryOperand.Size); // Validate that it's a 16-bit memory reference
    }
}
