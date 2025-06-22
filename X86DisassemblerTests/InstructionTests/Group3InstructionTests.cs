using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for Group3 instruction handlers
/// </summary>
public class Group3InstructionTests
{
    /// <summary>
    /// Tests the NotRm32Handler for decoding NOT r/m32 instruction
    /// </summary>
    [Fact]
    public void NotRm32Handler_DecodesNotRm32_Correctly()
    {
        // Arrange
        // NOT EAX (F7 D0) - ModR/M byte D0 = 11 010 000 (mod=3, reg=2, rm=0)
        // mod=3 means direct register addressing, reg=2 indicates NOT operation, rm=0 is EAX
        byte[] codeBuffer = new byte[] { 0xF7, 0xD0 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Not, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EAX)
    }
    
    /// <summary>
    /// Tests the NegRm32Handler for decoding NEG r/m32 instruction
    /// </summary>
    [Fact]
    public void NegRm32Handler_DecodesNegRm32_Correctly()
    {
        // Arrange
        // NEG ECX (F7 D9) - ModR/M byte D9 = 11 011 001 (mod=3, reg=3, rm=1)
        // mod=3 means direct register addressing, reg=3 indicates NEG operation, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0xF7, 0xD9 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Neg, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (ECX)
        var ecxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ecxOperand);
        var registerOperand = (RegisterOperand)ecxOperand;
        Assert.Equal(RegisterIndex.C, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (ECX)
    }
    
    /// <summary>
    /// Tests the MulRm32Handler for decoding MUL r/m32 instruction
    /// </summary>
    [Fact]
    public void MulRm32Handler_DecodesMulRm32_Correctly()
    {
        // Arrange
        // MUL EDX (F7 E2) - ModR/M byte E2 = 11 100 010 (mod=3, reg=4, rm=2)
        // mod=3 means direct register addressing, reg=4 indicates MUL operation, rm=2 is EDX
        byte[] codeBuffer = new byte[] { 0xF7, 0xE2 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Mul, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (EDX)
        var edxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(edxOperand);
        var registerOperand = (RegisterOperand)edxOperand;
        Assert.Equal(RegisterIndex.D, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EDX)
    }
    
    /// <summary>
    /// Tests the ImulRm32Handler for decoding IMUL r/m32 instruction
    /// </summary>
    [Fact]
    public void ImulRm32Handler_DecodesImulRm32_Correctly()
    {
        // Arrange
        // IMUL EBX (F7 EB) - ModR/M byte EB = 11 101 011 (mod=3, reg=5, rm=3)
        // mod=3 means direct register addressing, reg=5 indicates IMUL operation, rm=3 is EBX
        byte[] codeBuffer = new byte[] { 0xF7, 0xEB };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.IMul, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (EBX)
        var ebxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ebxOperand);
        var registerOperand = (RegisterOperand)ebxOperand;
        Assert.Equal(RegisterIndex.B, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EBX)
    }
    
    /// <summary>
    /// Tests the DivRm32Handler for decoding DIV r/m32 instruction
    /// </summary>
    [Fact]
    public void DivRm32Handler_DecodesDivRm32_Correctly()
    {
        // Arrange
        // DIV ESP (F7 F4) - ModR/M byte F4 = 11 110 100 (mod=3, reg=6, rm=4)
        // mod=3 means direct register addressing, reg=6 indicates DIV operation, rm=4 is ESP
        byte[] codeBuffer = new byte[] { 0xF7, 0xF4 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Div, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (ESP)
        var espOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(espOperand);
        var registerOperand = (RegisterOperand)espOperand;
        Assert.Equal(RegisterIndex.Sp, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (ESP)
    }
    
    /// <summary>
    /// Tests the IdivRm32Handler for decoding IDIV r/m32 instruction
    /// </summary>
    [Fact]
    public void IdivRm32Handler_DecodesIdivRm32_Correctly()
    {
        // Arrange
        // IDIV EBP (F7 FD) - ModR/M byte FD = 11 111 101 (mod=3, reg=7, rm=5)
        // mod=3 means direct register addressing, reg=7 indicates IDIV operation, rm=5 is EBP
        byte[] codeBuffer = new byte[] { 0xF7, 0xFD };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        var instruction = instructions[0];
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.IDiv, instruction.Type);
        
        // Check that we have one operand
        Assert.Single(instruction.StructuredOperands);
        
        // Check the operand (EBP)
        var ebpOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ebpOperand);
        var registerOperand = (RegisterOperand)ebpOperand;
        Assert.Equal(RegisterIndex.Bp, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EBP)
    }
}
