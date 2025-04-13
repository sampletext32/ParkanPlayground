using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for ADD instruction handlers
/// </summary>
public class AddInstructionTests
{
    /// <summary>
    /// Tests the ADD r32, r/m32 instruction (0x03) with register operand
    /// </summary>
    [Fact]
    public void TestAddR32Rm32_Register()
    {
        // Arrange
        byte[] code = { 0x03, 0xF5 }; // ADD ESI, EBP
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("add", instructions[0].Mnemonic);
        Assert.Equal("esi, ebp", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the ADD r32, m32 instruction (0x03) with memory operand
    /// </summary>
    [Fact]
    public void TestAddR32M32()
    {
        // Arrange
        byte[] code = { 0x03, 0x00 }; // ADD EAX, DWORD PTR [EAX]
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("add", instructions[0].Mnemonic);
        Assert.Equal("eax, dword ptr [eax]", instructions[0].Operands);
    }
}
