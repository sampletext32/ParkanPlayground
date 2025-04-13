namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;
using X86Disassembler.X86.Handlers;
using X86Disassembler.X86.Handlers.Inc;

/// <summary>
/// Tests for handler selection in the InstructionHandlerFactory
/// </summary>
public class HandlerSelectionTests
{
    /// <summary>
    /// Tests that the IncRegHandler is NOT selected for the 0x83 opcode
    /// </summary>
    [Fact]
    public void InstructionHandlerFactory_DoesNotSelectIncRegHandler_For0x83Opcode()
    {
        // Arrange
        byte[] codeBuffer = new byte[] { 0x83, 0xC1, 0x04 }; // ADD ecx, 0x04
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        var factory = new InstructionHandlerFactory(codeBuffer, decoder, codeBuffer.Length);
        
        // Act
        var handler = factory.GetHandler(0x83);
        
        // Assert
        Assert.NotNull(handler);
        Assert.IsNotType<IncRegHandler>(handler);
    }
    
    /// <summary>
    /// Tests the specific problematic sequence
    /// </summary>
    [Fact]
    public void InstructionHandlerFactory_HandlesProblematicSequence_Correctly()
    {
        // Arrange - This is the sequence from the problematic example
        byte[] codeBuffer = new byte[] { 0x08, 0x83, 0xC1, 0x04, 0x50, 0xE8, 0x42, 0x01, 0x00, 0x00 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act - Disassemble the entire sequence
        var instructions = disassembler.Disassemble();
        
        // Assert - We should have at least 3 instructions
        Assert.True(instructions.Count >= 3, $"Expected at least 3 instructions, but got {instructions.Count}");
        
        // First instruction should be OR
        Assert.Equal("or", instructions[0].Mnemonic);
        
        // Second instruction should be ADD ecx, imm8
        Assert.Equal("add", instructions[1].Mnemonic);
        Assert.Equal("ecx, 0x00000004", instructions[1].Operands);
        
        // Third instruction should be PUSH eax
        Assert.Equal("push", instructions[2].Mnemonic);
        Assert.Equal("eax", instructions[2].Operands);
    }
}
