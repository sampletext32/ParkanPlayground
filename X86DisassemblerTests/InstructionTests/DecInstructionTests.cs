using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for DEC instruction handlers
/// </summary>
public class DecInstructionTests
{
    /// <summary>
    /// Tests the DEC EAX instruction (0x48)
    /// </summary>
    [Fact]
    public void TestDecEax()
    {
        // Arrange
        byte[] code = { 0x48 }; // DEC EAX
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("dec", instructions[0].Mnemonic);
        Assert.Equal("eax", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the DEC ECX instruction (0x49)
    /// </summary>
    [Fact]
    public void TestDecEcx()
    {
        // Arrange
        byte[] code = { 0x49 }; // DEC ECX
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("dec", instructions[0].Mnemonic);
        Assert.Equal("ecx", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the DEC EDI instruction (0x4F)
    /// </summary>
    [Fact]
    public void TestDecEdi()
    {
        // Arrange
        byte[] code = { 0x4F }; // DEC EDI
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("dec", instructions[0].Mnemonic);
        Assert.Equal("edi", instructions[0].Operands);
    }
}
