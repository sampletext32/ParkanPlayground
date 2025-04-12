namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;
using X86Disassembler.X86.Handlers;

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
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("fnstsw", instruction.Mnemonic);
        Assert.Equal("ax", instruction.Operands);
    }
}
