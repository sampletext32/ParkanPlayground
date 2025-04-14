using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for push and pop instruction handlers
/// </summary>
public class PushPopInstructionTests
{
    /// <summary>
    /// Tests the PushRegHandler for decoding PUSH r32 instruction
    /// </summary>
    [Fact]
    public void PushRegHandler_DecodesPushReg_Correctly()
    {
        // Arrange
        // PUSH EAX (50) - Push EAX onto the stack
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
        var registerOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EAX)
    }
    
    /// <summary>
    /// Tests the PushRegHandler for decoding PUSH r32 instruction with different register
    /// </summary>
    [Fact]
    public void PushRegHandler_DecodesPushEbp_Correctly()
    {
        // Arrange
        // PUSH EBP (55) - Push EBP onto the stack
        byte[] codeBuffer = new byte[] { 0x55 };
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
        
        // Check the operand (EBP)
        var ebpOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ebpOperand);
        var registerOperand = (RegisterOperand)ebpOperand;
        Assert.Equal(RegisterIndex.Bp, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EBP)
    }
    
    /// <summary>
    /// Tests the PushImm8Handler for decoding PUSH imm8 instruction
    /// </summary>
    [Fact]
    public void PushImm8Handler_DecodesPushImm8_Correctly()
    {
        // Arrange
        // PUSH 0x42 (6A 42) - Push immediate byte value 0x42 onto the stack
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
        
        // Check the operand (immediate value)
        var immOperand = instruction.StructuredOperands[0];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x42U, immediateOperand.Value);
    }
    
    /// <summary>
    /// Tests the PushImm32Handler for decoding PUSH imm32 instruction
    /// </summary>
    [Fact]
    public void PushImm32Handler_DecodesPushImm32_Correctly()
    {
        // Arrange
        // PUSH 0x12345678 (68 78 56 34 12) - Push immediate dword value 0x12345678 onto the stack
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
        
        // Check the operand (immediate value)
        var immOperand = instruction.StructuredOperands[0];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x12345678U, immediateOperand.Value);
    }
    
    /// <summary>
    /// Tests the PopRegHandler for decoding POP r32 instruction
    /// </summary>
    [Fact]
    public void PopRegHandler_DecodesPopReg_Correctly()
    {
        // Arrange
        // POP EAX (58) - Pop a value from the stack into EAX
        byte[] codeBuffer = new byte[] { 0x58 };
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
        
        // Check the operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EAX)
    }
    
    /// <summary>
    /// Tests the PopRegHandler for decoding POP r32 instruction with different register
    /// </summary>
    [Fact]
    public void PopRegHandler_DecodesPopEbp_Correctly()
    {
        // Arrange
        // POP EBP (5D) - Pop a value from the stack into EBP
        byte[] codeBuffer = new byte[] { 0x5D };
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
        
        // Check the operand (EBP)
        var ebpOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ebpOperand);
        var registerOperand = (RegisterOperand)ebpOperand;
        Assert.Equal(RegisterIndex.Bp, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EBP)
    }
    
    /// <summary>
    /// Tests a common function prologue sequence (PUSH EBP, MOV EBP, ESP)
    /// </summary>
    [Fact]
    public void PushPop_DecodesFunctionPrologue_Correctly()
    {
        // Arrange
        // PUSH EBP (55)
        // MOV EBP, ESP (89 E5)
        byte[] codeBuffer = new byte[] { 0x55, 0x89, 0xE5 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Equal(2, instructions.Count);
        
        // First instruction: PUSH EBP
        var pushInstruction = instructions[0];
        Assert.NotNull(pushInstruction);
        Assert.Equal(InstructionType.Push, pushInstruction.Type);
        
        // Check that we have one operand
        Assert.Single(pushInstruction.StructuredOperands);
        
        // Check the operand (EBP)
        var ebpOperand = pushInstruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ebpOperand);
        var registerOperand = (RegisterOperand)ebpOperand;
        Assert.Equal(RegisterIndex.Bp, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EBP)
        
        // Second instruction: MOV EBP, ESP
        var movInstruction = instructions[1];
        Assert.NotNull(movInstruction);
        Assert.Equal(InstructionType.Move, movInstruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, movInstruction.StructuredOperands.Count);
        
        // Check the operands (EBP and ESP)
        var ebpDestOperand = movInstruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ebpDestOperand);
        var ebpDestRegisterOperand = (RegisterOperand)ebpDestOperand;
        Assert.Equal(RegisterIndex.Bp, ebpDestRegisterOperand.Register);
        Assert.Equal(32, ebpDestRegisterOperand.Size); // Validate that it's a 32-bit register (EBP)
        
        var espSrcOperand = movInstruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(espSrcOperand);
        var espSrcRegisterOperand = (RegisterOperand)espSrcOperand;
        Assert.Equal(RegisterIndex.Sp, espSrcRegisterOperand.Register);
        Assert.Equal(32, espSrcRegisterOperand.Size); // Validate that it's a 32-bit register (ESP)
    }
    
    /// <summary>
    /// Tests a common function epilogue sequence (POP EBP, RET)
    /// </summary>
    [Fact]
    public void PushPop_DecodesFunctionEpilogue_Correctly()
    {
        // Arrange
        // POP EBP (5D)
        // RET (C3)
        byte[] codeBuffer = new byte[] { 0x5D, 0xC3 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Equal(2, instructions.Count);
        
        // First instruction: POP EBP
        var popInstruction = instructions[0];
        Assert.NotNull(popInstruction);
        Assert.Equal(InstructionType.Pop, popInstruction.Type);
        
        // Check that we have one operand
        Assert.Single(popInstruction.StructuredOperands);
        
        // Check the operand (EBP)
        var ebpOperand = popInstruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ebpOperand);
        var registerOperand = (RegisterOperand)ebpOperand;
        Assert.Equal(RegisterIndex.Bp, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EBP)
        
        // Second instruction: RET
        var retInstruction = instructions[1];
        Assert.NotNull(retInstruction);
        Assert.Equal(InstructionType.Ret, retInstruction.Type);
        
        // Check that we have no operands
        Assert.Empty(retInstruction.StructuredOperands);
    }
}
