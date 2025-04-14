using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for MOV r/m32, imm32 instruction (0xC7)
/// </summary>
public class MovRm32Imm32Tests
{
    /// <summary>
    /// Tests the MOV r32, imm32 instruction (0xC7) with register operand
    /// </summary>
    [Fact]
    public void TestMovR32Imm32()
    {
        // Arrange
        byte[] code = { 0xC7, 0xC0, 0x78, 0x56, 0x34, 0x12 }; // MOV EAX, 0x12345678
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Mov, instruction.Type);
        
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
    /// Tests the MOV m32, imm32 instruction (0xC7) with memory operand
    /// </summary>
    [Fact]
    public void TestMovM32Imm32()
    {
        // Arrange
        byte[] code = { 0xC7, 0x00, 0x78, 0x56, 0x34, 0x12 }; // MOV DWORD PTR [EAX], 0x12345678
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Mov, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (memory operand)
        var memOperand = instruction.StructuredOperands[0];
        Assert.IsType<BaseRegisterMemoryOperand>(memOperand);
        var memoryOperand = (BaseRegisterMemoryOperand)memOperand;
        Assert.Equal(RegisterIndex.A, memoryOperand.BaseRegister);
        Assert.Equal(32, memoryOperand.Size); // Validate that it's a 32-bit memory reference
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x12345678U, immediateOperand.Value);
        Assert.Equal(32, immediateOperand.Size); // Validate that it's a 32-bit immediate
    }
    
    /// <summary>
    /// Tests the MOV m32, imm32 instruction (0xC7) with SIB byte addressing
    /// </summary>
    [Fact]
    public void TestMovM32Imm32_WithSIB()
    {
        // Arrange
        // MOV DWORD PTR [ESP+0x10], 0x00000000
        byte[] code = { 0xC7, 0x44, 0x24, 0x10, 0x00, 0x00, 0x00, 0x00 };
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Mov, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (memory operand with SIB)
        var memOperand = instruction.StructuredOperands[0];
        Assert.IsType<DisplacementMemoryOperand>(memOperand);
        var displacementMemoryOperand = (DisplacementMemoryOperand)memOperand;
        Assert.Equal(RegisterIndex.Sp, displacementMemoryOperand.BaseRegister);
        Assert.Equal(32, displacementMemoryOperand.Size); // Validate that it's a 32-bit memory reference
        Assert.Equal(0x10, displacementMemoryOperand.Displacement);
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x00000000U, immediateOperand.Value);
        Assert.Equal(32, immediateOperand.Size); // Validate that it's a 32-bit immediate
    }
    
    /// <summary>
    /// Tests the MOV m32, imm32 instruction (0xC7) with complex SIB byte addressing
    /// </summary>
    [Fact]
    public void TestMovM32Imm32_WithComplexSIB()
    {
        // Arrange
        // MOV DWORD PTR [EAX+ECX*4+0x12345678], 0xAABBCCDD
        byte[] code = { 0xC7, 0x84, 0x88, 0x78, 0x56, 0x34, 0x12, 0xDD, 0xCC, 0xBB, 0xAA };
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.Equal(InstructionType.Mov, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (memory operand with SIB)
        var memOperand = instruction.StructuredOperands[0];
        Assert.IsType<ScaledIndexMemoryOperand>(memOperand);
        var sibMemoryOperand = (ScaledIndexMemoryOperand)memOperand;
        Assert.Equal(RegisterIndex.A, sibMemoryOperand.BaseRegister);
        Assert.Equal(RegisterIndex.C, sibMemoryOperand.IndexRegister);
        Assert.Equal(4, sibMemoryOperand.Scale);
        Assert.Equal(32, sibMemoryOperand.Size); // Validate that it's a 32-bit memory reference
        Assert.Equal(0x12345678, sibMemoryOperand.Displacement);
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0xAABBCCDDU, immediateOperand.Value);
        Assert.Equal(32, immediateOperand.Size); // Validate that it's a 32-bit immediate
    }
    
    /// <summary>
    /// Tests the MOV m32, imm32 instruction (0xC7) with consecutive instructions
    /// </summary>
    [Fact]
    public void TestMovM32Imm32_ConsecutiveInstructions()
    {
        // Arrange
        // MOV DWORD PTR [ESP+0x10], 0x00000000
        // MOV DWORD PTR [ESP+0x14], 0x00000000
        byte[] code = { 
            0xC7, 0x44, 0x24, 0x10, 0x00, 0x00, 0x00, 0x00,
            0xC7, 0x44, 0x24, 0x14, 0x00, 0x00, 0x00, 0x00
        };
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Equal(2, instructions.Count);
        
        // First instruction
        var firstInstruction = instructions[0];
        Assert.Equal(InstructionType.Mov, firstInstruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, firstInstruction.StructuredOperands.Count);
        
        // Check the first operand (memory operand with SIB)
        var memOperand = firstInstruction.StructuredOperands[0];
        Assert.IsType<DisplacementMemoryOperand>(memOperand);
        var displacementMemoryOperand = (DisplacementMemoryOperand)memOperand;
        Assert.Equal(RegisterIndex.Sp, displacementMemoryOperand.BaseRegister);
        Assert.Equal(32, displacementMemoryOperand.Size); // Validate that it's a 32-bit memory reference
        Assert.Equal(0x10, displacementMemoryOperand.Displacement);
        
        // Check the second operand (immediate value)
        var immOperand = firstInstruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x00000000U, immediateOperand.Value);
        Assert.Equal(32, immediateOperand.Size); // Validate that it's a 32-bit immediate
        
        // Second instruction
        var secondInstruction = instructions[1];
        Assert.Equal(InstructionType.Mov, secondInstruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, secondInstruction.StructuredOperands.Count);
        
        // Check the first operand (memory operand with SIB)
        memOperand = secondInstruction.StructuredOperands[0];
        Assert.IsType<DisplacementMemoryOperand>(memOperand);
        displacementMemoryOperand = (DisplacementMemoryOperand)memOperand;
        Assert.Equal(RegisterIndex.Sp, displacementMemoryOperand.BaseRegister);
        Assert.Equal(32, displacementMemoryOperand.Size); // Validate that it's a 32-bit memory reference
        Assert.Equal(0x14, displacementMemoryOperand.Displacement);
        
        // Check the second operand (immediate value)
        immOperand = secondInstruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x00000000U, immediateOperand.Value);
        Assert.Equal(32, immediateOperand.Size); // Validate that it's a 32-bit immediate
    }
    
    /// <summary>
    /// Tests the MOV m32, imm32 instruction (0xC7) with instruction boundary detection
    /// </summary>
    [Fact]
    public void TestMovM32Imm32_InstructionBoundaryDetection()
    {
        // Arrange
        // This is the sequence from address 0x00002441 that was problematic
        // MOV DWORD PTR [ESP+0x10], 0x00000000
        // MOV DWORD PTR [ESP+0x14], 0x00000000
        byte[] code = { 
            0xC7, 0x44, 0x24, 0x10, 0x00, 0x00, 0x00, 0x00,
            0xC7, 0x44, 0x24, 0x14, 0x00, 0x00, 0x00, 0x00
        };
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x2441);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Equal(2, instructions.Count);
        
        // First instruction
        var firstInstruction = instructions[0];
        Assert.Equal(InstructionType.Mov, firstInstruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, firstInstruction.StructuredOperands.Count);
        
        // Check the first operand (memory operand with SIB)
        var memOperand = firstInstruction.StructuredOperands[0];
        Assert.IsType<DisplacementMemoryOperand>(memOperand);
        var displacementMemoryOperand = (DisplacementMemoryOperand)memOperand;
        Assert.Equal(RegisterIndex.Sp, displacementMemoryOperand.BaseRegister);
        Assert.Equal(32, displacementMemoryOperand.Size); // Validate that it's a 32-bit memory reference
        Assert.Equal(0x10, displacementMemoryOperand.Displacement);
        
        // Check the second operand (immediate value)
        var immOperand = firstInstruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x00000000U, immediateOperand.Value);
        Assert.Equal(32, immediateOperand.Size); // Validate that it's a 32-bit immediate
        
        // Second instruction
        var secondInstruction = instructions[1];
        Assert.Equal(InstructionType.Mov, secondInstruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, secondInstruction.StructuredOperands.Count);
        
        // Check the first operand (memory operand with SIB)
        memOperand = secondInstruction.StructuredOperands[0];
        Assert.IsType<DisplacementMemoryOperand>(memOperand);
        displacementMemoryOperand = (DisplacementMemoryOperand)memOperand;
        Assert.Equal(RegisterIndex.Sp, displacementMemoryOperand.BaseRegister);
        Assert.Equal(32, displacementMemoryOperand.Size); // Validate that it's a 32-bit memory reference
        Assert.Equal(0x14, displacementMemoryOperand.Displacement);
        
        // Check the second operand (immediate value)
        immOperand = secondInstruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x00000000U, immediateOperand.Value);
        Assert.Equal(32, immediateOperand.Size); // Validate that it's a 32-bit immediate
    }
}
