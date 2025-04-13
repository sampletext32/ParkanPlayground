using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for INC instruction handlers
/// </summary>
public class IncInstructionTests
{
    /// <summary>
    /// Tests the INC EAX instruction (0x40)
    /// </summary>
    [Fact]
    public void TestIncEax()
    {
        // Arrange
        byte[] code = { 0x40 }; // INC EAX
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("inc", instructions[0].Mnemonic);
        Assert.Equal("eax", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the INC ECX instruction (0x41)
    /// </summary>
    [Fact]
    public void TestIncEcx()
    {
        // Arrange
        byte[] code = { 0x41 }; // INC ECX
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("inc", instructions[0].Mnemonic);
        Assert.Equal("ecx", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the INC EDI instruction (0x47)
    /// </summary>
    [Fact]
    public void TestIncEdi()
    {
        // Arrange
        byte[] code = { 0x47 }; // INC EDI
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("inc", instructions[0].Mnemonic);
        Assert.Equal("edi", instructions[0].Operands);
    }
}
