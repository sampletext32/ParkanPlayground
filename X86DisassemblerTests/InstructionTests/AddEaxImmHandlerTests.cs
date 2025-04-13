using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for ADD EAX, imm32 instruction handler
/// </summary>
public class AddEaxImmHandlerTests
{
    /// <summary>
    /// Tests the AddEaxImmHandler for decoding ADD EAX, imm32 instruction
    /// </summary>
    [Fact]
    public void AddEaxImmHandler_DecodesAddEaxImm32_Correctly()
    {
        // Arrange
        // ADD EAX, 0x12345678 (05 78 56 34 12)
        byte[] codeBuffer = new byte[] { 0x05, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("add", instruction.Mnemonic);
        Assert.Equal("eax, 0x12345678", instruction.Operands);
    }
}
