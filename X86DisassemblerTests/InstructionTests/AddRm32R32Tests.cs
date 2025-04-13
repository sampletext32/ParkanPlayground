using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for ADD r/m32, r32 instruction (0x01)
/// </summary>
public class AddRm32R32Tests
{
    /// <summary>
    /// Tests the ADD r32, r32 instruction (0x01) with register operand
    /// </summary>
    [Fact]
    public void TestAddR32R32()
    {
        // Arrange
        byte[] code = { 0x01, 0xC1 }; // ADD ECX, EAX
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("add", instructions[0].Mnemonic);
        Assert.Equal("ecx, eax", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the ADD m32, r32 instruction (0x01) with memory operand
    /// </summary>
    [Fact]
    public void TestAddM32R32()
    {
        // Arrange
        byte[] code = { 0x01, 0x01 }; // ADD DWORD PTR [ECX], EAX
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("add", instructions[0].Mnemonic);
        Assert.Equal("dword ptr [ecx], eax", instructions[0].Operands);
    }
}
