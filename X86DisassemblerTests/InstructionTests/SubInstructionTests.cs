using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for SUB instruction handlers
/// </summary>
public class SubInstructionTests
{
    /// <summary>
    /// Tests the SubImmFromRm32Handler for decoding SUB r/m32, imm32 instruction
    /// </summary>
    [Fact]
    public void SubImmFromRm32Handler_DecodesSubRm32Imm32_Correctly()
    {
        // Arrange
        // SUB EAX, 0x12345678 (81 E8 78 56 34 12) - Subtract 0x12345678 from EAX
        byte[] codeBuffer = new byte[] { 0x81, 0xE8, 0x78, 0x56, 0x34, 0x12 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Sub, instruction.Type);
        
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
    /// Tests the SubImmFromRm32Handler for decoding SUB memory, imm32 instruction
    /// </summary>
    [Fact]
    public void SubImmFromRm32Handler_DecodesSubMemImm32_Correctly()
    {
        // Arrange
        // SUB [EBX+0x10], 0x12345678 (81 6B 10 78 56 34 12) - Subtract 0x12345678 from memory at [EBX+0x10]
        byte[] codeBuffer = new byte[] { 0x81, 0x6B, 0x10, 0x78, 0x56, 0x34, 0x12 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Sub, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (memory operand)
        var memoryOperand = instruction.StructuredOperands[0];
        Assert.IsType<DisplacementMemoryOperand>(memoryOperand);
        var memory = (DisplacementMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.B, memory.BaseRegister); // Base register is EBX
        Assert.Equal(0x10, memory.Displacement); // Displacement is 0x10
        Assert.Equal(32, memory.Size); // Memory size is 32 bits (DWORD)
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x12345678U, immediateOperand.Value);
    }
    
    /// <summary>
    /// Tests the SubImmFromRm32SignExtendedHandler for decoding SUB r/m32, imm8 instruction (sign-extended)
    /// </summary>
    [Fact]
    public void SubImmFromRm32SignExtendedHandler_DecodesSubRm32Imm8_Correctly()
    {
        // Arrange
        // SUB EAX, 0x42 (83 E8 42) - Subtract sign-extended 0x42 from EAX
        byte[] codeBuffer = new byte[] { 0x83, 0xE8, 0x42 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Sub, instruction.Type);
        
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
        Assert.Equal(0x42, immediateOperand.Value);
    }
    
    /// <summary>
    /// Tests the SubImmFromRm32SignExtendedHandler for decoding SUB r/m32, imm8 instruction with negative value
    /// </summary>
    [Fact]
    public void SubImmFromRm32SignExtendedHandler_DecodesSubRm32NegativeImm8_Correctly()
    {
        // Arrange
        // SUB EAX, -0x10 (83 E8 F0) - Subtract sign-extended -0x10 from EAX
        byte[] codeBuffer = new byte[] { 0x83, 0xE8, 0xF0 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Sub, instruction.Type);
        
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
        Assert.Equal(0xFFFFFFF0U, immediateOperand.Value);
    }
    
    /// <summary>
    /// Tests the SubImmFromRm32SignExtendedHandler for decoding SUB memory, imm8 instruction
    /// </summary>
    [Fact]
    public void SubImmFromRm32SignExtendedHandler_DecodesSubMemImm8_Correctly()
    {
        // Arrange
        // SUB [EBX+0x10], 0x42 (83 6B 10 42) - Subtract sign-extended 0x42 from memory at [EBX+0x10]
        byte[] codeBuffer = new byte[] { 0x83, 0x6B, 0x10, 0x42 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Sub, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (memory operand)
        var memoryOperand = instruction.StructuredOperands[0];
        Assert.IsType<DisplacementMemoryOperand>(memoryOperand);
        var memory = (DisplacementMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.B, memory.BaseRegister); // Base register is EBX
        Assert.Equal(0x10, memory.Displacement); // Displacement is 0x10
        Assert.Equal(32, memory.Size); // Memory size is 32 bits (DWORD)
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x42, immediateOperand.Value);
    }
    
    /// <summary>
    /// Tests a sequence of SUB instructions in a common pattern
    /// </summary>
    [Fact]
    public void SubInstruction_DecodesSubSequence_Correctly()
    {
        // Arrange
        // SUB ESP, 0x10 (83 EC 10) - Create stack space
        byte[] codeBuffer = new byte[] { 0x83, 0xEC, 0x10 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        
        // Instruction: SUB ESP, 0x10
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Sub, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (ESP)
        var espOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(espOperand);
        var registerOperand = (RegisterOperand)espOperand;
        Assert.Equal(RegisterIndex.Sp, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (ESP)
        
        // Check the second operand (immediate value)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x10, immediateOperand.Value);
    }
    
    /// <summary>
    /// Tests the SubRm32R32Handler for decoding SUB r/m32, r32 instruction
    /// </summary>
    [Fact]
    public void SubRm32R32Handler_DecodesSubRm32R32_Correctly()
    {
        // Arrange
        // SUB EAX, EBX (29 D8) - Subtract EBX from EAX
        byte[] codeBuffer = new byte[] { 0x29, 0xD8 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Sub, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (EBX)
        var ebxOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(ebxOperand);
        var registerOperand2 = (RegisterOperand)ebxOperand;
        Assert.Equal(RegisterIndex.B, registerOperand2.Register);
        Assert.Equal(32, registerOperand2.Size); // Validate that it's a 32-bit register (EBX)
    }
    
    /// <summary>
    /// Tests the SubRm32R32Handler for decoding SUB memory, r32 instruction
    /// </summary>
    [Fact]
    public void SubRm32R32Handler_DecodesSubMemR32_Correctly()
    {
        // Arrange
        // SUB [EBX+0x10], ECX (29 4B 10) - Subtract ECX from memory at [EBX+0x10]
        byte[] codeBuffer = new byte[] { 0x29, 0x4B, 0x10 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Sub, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (memory operand)
        var memoryOperand = instruction.StructuredOperands[0];
        Assert.IsType<DisplacementMemoryOperand>(memoryOperand);
        var memory = (DisplacementMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.B, memory.BaseRegister); // Base register is EBX
        Assert.Equal(0x10, memory.Displacement); // Displacement is 0x10
        Assert.Equal(32, memory.Size); // Memory size is 32 bits (DWORD)
        
        // Check the second operand (ECX)
        var ecxOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(ecxOperand);
        var registerOperand = (RegisterOperand)ecxOperand;
        Assert.Equal(RegisterIndex.C, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (ECX)
    }
    
    /// <summary>
    /// Tests the SubR32Rm32Handler for decoding SUB r32, r/m32 instruction
    /// </summary>
    [Fact]
    public void SubR32Rm32Handler_DecodesSubR32Rm32_Correctly()
    {
        // Arrange
        // SUB EBX, EAX (2B D8) - Subtract EAX from EBX
        byte[] codeBuffer = new byte[] { 0x2B, 0xD8 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Sub, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EBX)
        var ebxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ebxOperand);
        var registerOperand = (RegisterOperand)ebxOperand;
        Assert.Equal(RegisterIndex.B, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EBX)
        
        // Check the second operand (EAX)
        var eaxOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand2 = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand2.Register);
        Assert.Equal(32, registerOperand2.Size); // Validate that it's a 32-bit register (EAX)
    }
    
    /// <summary>
    /// Tests the SubR32Rm32Handler for decoding SUB r32, memory instruction
    /// </summary>
    [Fact]
    public void SubR32Rm32Handler_DecodesSubR32Mem_Correctly()
    {
        // Arrange
        // SUB ECX, [EBX+0x10] (2B 4B 10) - Subtract memory at [EBX+0x10] from ECX
        byte[] codeBuffer = new byte[] { 0x2B, 0x4B, 0x10 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Sub, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (ECX)
        var ecxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ecxOperand);
        var registerOperand = (RegisterOperand)ecxOperand;
        Assert.Equal(RegisterIndex.C, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (ECX)
        
        // Check the second operand (memory operand)
        var memoryOperand = instruction.StructuredOperands[1];
        Assert.IsType<DisplacementMemoryOperand>(memoryOperand);
        var memory = (DisplacementMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.B, memory.BaseRegister); // Base register is EBX
        Assert.Equal(0x10, memory.Displacement); // Displacement is 0x10
        Assert.Equal(32, memory.Size); // Memory size is 32 bits (DWORD)
    }
    
    /// <summary>
    /// Tests a sequence of SUB instructions with different encoding
    /// </summary>
    [Fact]
    public void SubInstruction_DecodesComplexSubSequence_Correctly()
    {
        // Arrange
        // SUB ESP, 0x10 (83 EC 10) - Create stack space
        // SUB EAX, EBX (29 D8) - Subtract EBX from EAX
        // SUB ECX, [EBP-4] (2B 4D FC) - Subtract memory at [EBP-4] from ECX
        byte[] codeBuffer = new byte[] { 0x83, 0xEC, 0x10, 0x29, 0xD8, 0x2B, 0x4D, 0xFC };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Equal(3, instructions.Count);
        
        // First instruction: SUB ESP, 0x10
        var instruction1 = instructions[0];
        Assert.NotNull(instruction1);
        Assert.Equal(InstructionType.Sub, instruction1.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction1.StructuredOperands.Count);
        
        // Check the first operand (ESP)
        var espOperand = instruction1.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(espOperand);
        var registerOperand = (RegisterOperand)espOperand;
        Assert.Equal(RegisterIndex.Sp, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (ESP)
        
        // Check the second operand (immediate value)
        var immOperand = instruction1.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x10, immediateOperand.Value);
        
        // Second instruction: SUB EAX, EBX
        var instruction2 = instructions[1];
        Assert.NotNull(instruction2);
        Assert.Equal(InstructionType.Sub, instruction2.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction2.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction2.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand2 = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand2.Register);
        Assert.Equal(32, registerOperand2.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (EBX)
        var ebxOperand = instruction2.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(ebxOperand);
        var registerOperand3 = (RegisterOperand)ebxOperand;
        Assert.Equal(RegisterIndex.B, registerOperand3.Register);
        Assert.Equal(32, registerOperand3.Size); // Validate that it's a 32-bit register (EBX)
        
        // Third instruction: SUB ECX, [EBP-4]
        var instruction3 = instructions[2];
        Assert.NotNull(instruction3);
        Assert.Equal(InstructionType.Sub, instruction3.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction3.StructuredOperands.Count);
        
        // Check the first operand (ECX)
        var ecxOperand = instruction3.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ecxOperand);
        var registerOperand4 = (RegisterOperand)ecxOperand;
        Assert.Equal(RegisterIndex.C, registerOperand4.Register);
        Assert.Equal(32, registerOperand4.Size); // Validate that it's a 32-bit register (ECX)
        
        // Check the second operand (memory operand)
        var memoryOperand = instruction3.StructuredOperands[1];
        Assert.IsType<DisplacementMemoryOperand>(memoryOperand);
        var memory = (DisplacementMemoryOperand)memoryOperand;
        Assert.Equal(RegisterIndex.Bp, memory.BaseRegister); // Base register is EBP
        Assert.Equal(-4, memory.Displacement); // Displacement is -4
        Assert.Equal(32, memory.Size); // Memory size is 32 bits (DWORD)
    }
}
