using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for data transfer instruction handlers
/// </summary>
public class DataTransferInstructionTests
{
    /// <summary>
    /// Tests the DataTransferHandler for decoding MOV r32, r/m32 instruction
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesMovR32Rm32_Correctly()
    {
        // Arrange
        // MOV EAX, ECX (8B C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        // mod=3 means direct register addressing, reg=0 is EAX, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0x8B, 0xC1 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Mov, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var eaxRegisterOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, eaxRegisterOperand.Register);
        Assert.Equal(32, eaxRegisterOperand.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (ECX)
        var ecxOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(ecxOperand);
        var ecxRegisterOperand = (RegisterOperand)ecxOperand;
        Assert.Equal(RegisterIndex.C, ecxRegisterOperand.Register);
        Assert.Equal(32, ecxRegisterOperand.Size); // Validate that it's a 32-bit register (ECX)
    }
    
    /// <summary>
    /// Tests the DataTransferHandler for decoding MOV r/m32, r32 instruction
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesMovRm32R32_Correctly()
    {
        // Arrange
        // MOV ECX, EAX (89 C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        // mod=3 means direct register addressing, reg=0 is EAX, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0x89, 0xC1 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Mov, instruction.Type);
        
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
    /// Tests the DataTransferHandler for decoding MOV r32, imm32 instruction
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesMovR32Imm32_Correctly()
    {
        // Arrange
        // MOV EAX, 0x12345678 (B8 78 56 34 12)
        byte[] codeBuffer = new byte[] { 0xB8, 0x78, 0x56, 0x34, 0x12 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Mov, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var eaxRegisterOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, eaxRegisterOperand.Register);
        Assert.Equal(32, eaxRegisterOperand.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (Immediate)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immImmediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x12345678, immImmediateOperand.Value);
    }
    
    /// <summary>
    /// Tests the DataTransferHandler for decoding MOV r8, imm8 instruction (DecodeMOVRegImm8)
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesMovR8Imm8_Correctly()
    {
        // Arrange
        // MOV AL, 0x42 (B0 42) - Register is encoded in the low 3 bits of the opcode
        byte[] codeBuffer = new byte[] { 0xB0, 0x42 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Mov, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (AL)
        var alOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(alOperand);
        var alRegisterOperand = (RegisterOperand)alOperand;
        Assert.Equal(RegisterIndex.A, alRegisterOperand.Register);
        Assert.Equal(8, alRegisterOperand.Size); // Validate that it's an 8-bit register (AL)
        
        // Check the second operand (Immediate)
        var immOperand = instruction.StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immImmediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x42, immImmediateOperand.Value);
    }
    
    /// <summary>
    /// Tests the DataTransferHandler for decoding MOV EAX, moffs32 instruction
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesMovEaxMoffs32_Correctly()
    {
        // Arrange
        // MOV EAX, [0x12345678] (A1 78 56 34 12)
        byte[] codeBuffer = new byte[] { 0xA1, 0x78, 0x56, 0x34, 0x12 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Mov, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var eaxRegisterOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, eaxRegisterOperand.Register);
        Assert.Equal(32, eaxRegisterOperand.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (Memory)
        var memOperand = instruction.StructuredOperands[1];
        Assert.IsType<DirectMemoryOperand>(memOperand);
        var directMemoryOperand = (DirectMemoryOperand)memOperand;
        Assert.Equal(0x12345678, directMemoryOperand.Address);
    }
    
    /// <summary>
    /// Tests the DataTransferHandler for decoding MOV moffs32, EAX instruction
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesMovMoffs32Eax_Correctly()
    {
        // Arrange
        // MOV [0x12345678], EAX (A3 78 56 34 12)
        byte[] codeBuffer = new byte[] { 0xA3, 0x78, 0x56, 0x34, 0x12 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Mov, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (Memory)
        var memOperand = instruction.StructuredOperands[0];
        Assert.IsType<DirectMemoryOperand>(memOperand);
        var directMemoryOperand = (DirectMemoryOperand)memOperand;
        Assert.Equal(0x12345678, directMemoryOperand.Address);
        
        // Check the second operand (EAX)
        var eaxOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var eaxRegisterOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, eaxRegisterOperand.Register);
        Assert.Equal(32, eaxRegisterOperand.Size); // Validate that it's a 32-bit register (EAX)
    }
    
    /// <summary>
    /// Tests the DataTransferHandler for decoding MOV with memory addressing
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesMovWithMemoryAddressing_Correctly()
    {
        // Arrange
        // MOV EAX, [ECX+0x12345678] (8B 81 78 56 34 12) - ModR/M byte 81 = 10 000 001 (mod=2, reg=0, rm=1)
        // mod=2 means memory addressing with 32-bit displacement, reg=0 is EAX, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0x8B, 0x81, 0x78, 0x56, 0x34, 0x12 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Mov, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var eaxRegisterOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, eaxRegisterOperand.Register);
        Assert.Equal(32, eaxRegisterOperand.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (Memory)
        var memOperand = instruction.StructuredOperands[1];
        Assert.IsType<DisplacementMemoryOperand>(memOperand);
        var displacementMemoryOperand = (DisplacementMemoryOperand)memOperand;
        Assert.Equal(0x12345678, displacementMemoryOperand.Displacement);
        Assert.Equal(RegisterIndex.C, displacementMemoryOperand.BaseRegister);
    }
    
    /// <summary>
    /// Tests the DataTransferHandler for decoding PUSH r32 instruction
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesPushR32_Correctly()
    {
        // Arrange
        // PUSH EAX (50)
        byte[] codeBuffer = new byte[] { 0x50 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Push, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var eaxRegisterOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, eaxRegisterOperand.Register);
        Assert.Equal(32, eaxRegisterOperand.Size); // Validate that it's a 32-bit register (EAX)
    }
    
    /// <summary>
    /// Tests the DataTransferHandler for decoding PUSH imm32 instruction (DecodePUSHImm32)
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesPushImm32_Correctly()
    {
        // Arrange
        // PUSH 0x12345678 (68 78 56 34 12)
        byte[] codeBuffer = new byte[] { 0x68, 0x78, 0x56, 0x34, 0x12 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Push, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (Immediate)
        var immOperand = instruction.StructuredOperands[0];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immImmediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x12345678, immImmediateOperand.Value);
    }
    
    /// <summary>
    /// Tests the DataTransferHandler for decoding PUSH imm8 instruction (DecodePUSHImm8)
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesPushImm8_Correctly()
    {
        // Arrange
        // PUSH 0x42 (6A 42)
        byte[] codeBuffer = new byte[] { 0x6A, 0x42 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Push, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (Immediate)
        var immOperand = instruction.StructuredOperands[0];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immImmediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x42, immImmediateOperand.Value);
    }
    
    /// <summary>
    /// Tests the DataTransferHandler for decoding POP r32 instruction
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesPopR32_Correctly()
    {
        // Arrange
        // POP ECX (59)
        byte[] codeBuffer = new byte[] { 0x59 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Pop, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (ECX)
        var ecxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ecxOperand);
        var ecxRegisterOperand = (RegisterOperand)ecxOperand;
        Assert.Equal(RegisterIndex.C, ecxRegisterOperand.Register);
        Assert.Equal(32, ecxRegisterOperand.Size); // Validate that it's a 32-bit register (ECX)
    }
    
    /// <summary>
    /// Tests the DataTransferHandler for decoding XCHG EAX, r32 instruction (DecodeXCHGEAXReg)
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesXchgEaxReg_Correctly()
    {
        // Arrange
        // XCHG EAX, ECX (91) - Register is encoded in the low 3 bits of the opcode
        byte[] codeBuffer = new byte[] { 0x91 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Xchg, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var eaxRegisterOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, eaxRegisterOperand.Register);
        Assert.Equal(32, eaxRegisterOperand.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (ECX)
        var ecxOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(ecxOperand);
        var ecxRegisterOperand = (RegisterOperand)ecxOperand;
        Assert.Equal(RegisterIndex.C, ecxRegisterOperand.Register);
        Assert.Equal(32, ecxRegisterOperand.Size); // Validate that it's a 32-bit register (ECX)
    }
    
    /// <summary>
    /// Tests the DataTransferHandler for decoding NOP instruction (special case of XCHG EAX, EAX)
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesNop_Correctly()
    {
        // Arrange
        // NOP (90) - This is actually XCHG EAX, EAX which is treated as NOP
        byte[] codeBuffer = new byte[] { 0x90 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Nop, instruction.Type);
        
        // Check that we have no operands
        Assert.Empty(instruction.StructuredOperands);
    }
}
