namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;
using X86Disassembler.X86.Handlers.Push;
using X86Disassembler.X86.Handlers.Pop;

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
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("push", instruction.Mnemonic);
        Assert.Equal("eax", instruction.Operands);
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
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("push", instruction.Mnemonic);
        Assert.Equal("ebp", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the PushImm8Handler for decoding PUSH imm8 instruction
    /// </summary>
    [Fact]
    public void PushImm8Handler_DecodesPushImm8_Correctly()
    {
        // Arrange
        // PUSH 0x42 (6A 42) - Push 8-bit immediate value onto the stack
        byte[] codeBuffer = new byte[] { 0x6A, 0x42 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("push", instruction.Mnemonic);
        Assert.Equal("0x42", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the PushImm32Handler for decoding PUSH imm32 instruction
    /// </summary>
    [Fact]
    public void PushImm32Handler_DecodesPushImm32_Correctly()
    {
        // Arrange
        // PUSH 0x12345678 (68 78 56 34 12) - Push 32-bit immediate value onto the stack
        byte[] codeBuffer = new byte[] { 0x68, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("push", instruction.Mnemonic);
        Assert.Equal("0x12345678", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the PopRegHandler for decoding POP r32 instruction
    /// </summary>
    [Fact]
    public void PopRegHandler_DecodesPopReg_Correctly()
    {
        // Arrange
        // POP EAX (58) - Pop value from stack into EAX
        byte[] codeBuffer = new byte[] { 0x58 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("pop", instruction.Mnemonic);
        Assert.Equal("eax", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the PopRegHandler for decoding POP r32 instruction with different register
    /// </summary>
    [Fact]
    public void PopRegHandler_DecodesPopEbp_Correctly()
    {
        // Arrange
        // POP EBP (5D) - Pop value from stack into EBP
        byte[] codeBuffer = new byte[] { 0x5D };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("pop", instruction.Mnemonic);
        Assert.Equal("ebp", instruction.Operands);
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
        Assert.Equal("push", instructions[0].Mnemonic);
        Assert.Equal("ebp", instructions[0].Operands);
        
        // Second instruction: MOV EBP, ESP
        Assert.Equal("mov", instructions[1].Mnemonic);
        Assert.Equal("ebp, esp", instructions[1].Operands);
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
        Assert.Equal("pop", instructions[0].Mnemonic);
        Assert.Equal("ebp", instructions[0].Operands);
        
        // Second instruction: RET
        Assert.Equal("ret", instructions[1].Mnemonic);
        Assert.Equal("", instructions[1].Operands);
    }
}
