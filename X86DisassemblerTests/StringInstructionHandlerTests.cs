namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;
using X86Disassembler.X86.Handlers.String;

/// <summary>
/// Tests for string instruction handlers
/// </summary>
public class StringInstructionHandlerTests
{
    /// <summary>
    /// Tests the RepMovsHandler for decoding REP MOVS instruction
    /// </summary>
    [Fact]
    public void RepMovsHandler_DecodesRepMovs_Correctly()
    {
        // Arrange
        // REP MOVS (F3 A5)
        byte[] codeBuffer = new byte[] { 0xF3, 0xA5 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("rep movs", instruction.Mnemonic);
        Assert.Equal("dword ptr [edi], dword ptr [esi]", instruction.Operands);
    }
}
