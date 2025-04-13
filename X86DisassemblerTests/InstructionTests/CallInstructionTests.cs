using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for call instruction handlers
/// </summary>
public class CallInstructionTests
{
    /// <summary>
    /// Tests the CallRel32Handler for decoding CALL rel32 instruction
    /// </summary>
    [Fact]
    public void CallRel32Handler_DecodesCallRel32_Correctly()
    {
        // Arrange
        // CALL +0x12345678 (E8 78 56 34 12) - Call to address 0x12345678 bytes forward
        byte[] codeBuffer = new byte[] { 0xE8, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("call", instruction.Mnemonic);
        Assert.Equal("0x1234567D", instruction.Operands); // Current position (5) + offset (0x12345678) = 0x1234567D
    }
}
