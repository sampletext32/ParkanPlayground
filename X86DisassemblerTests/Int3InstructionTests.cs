namespace X86DisassemblerTests;

using Xunit;
using X86Disassembler.X86;

/// <summary>
/// Tests for INT3 instruction handler
/// </summary>
public class Int3InstructionTests
{
    /// <summary>
    /// Tests the Int3Handler for decoding INT3 instruction
    /// </summary>
    [Fact]
    public void Int3Handler_DecodesInt3_Correctly()
    {
        // Arrange
        // INT3 (CC)
        byte[] codeBuffer = new byte[] { 0xCC };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("int3", instruction.Mnemonic);
        Assert.Equal("", instruction.Operands);
    }
}
