using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

public class SubRm8Imm8Tests
{
    [Fact]
    public void SubRm8Imm8_Decodes_Correctly()
    {
        // Arrange
        // SUB BL, 0x42
        byte[] codeBuffer = new byte[] { 0x80, 0xeb, 0x42 };
        var decoder = new Disassembler(codeBuffer, 0x1000);
        
        // Act
        var instructions = decoder.Disassemble();

        // Assert
        Assert.Single(instructions);
        Assert.NotNull(instructions[0]);
        Assert.Equal("sub", instructions[0].Mnemonic);
        Assert.Equal("bl, 0x42", instructions[0].Operands);
    }
}