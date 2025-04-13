using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for CMP r/m8, imm8 instruction (0x80 /7)
/// </summary>
public class CmpImmWithRm8Tests
{
    /// <summary>
    /// Tests the CMP r8, imm8 instruction (0x80 /7) with register operand
    /// </summary>
    [Fact]
    public void TestCmpR8Imm8()
    {
        // Arrange
        byte[] code = { 0x80, 0xF9, 0x02 }; // CMP CL, 0x02
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("cmp", instructions[0].Mnemonic);
        Assert.Equal("cl, 0x02", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the CMP m8, imm8 instruction (0x80 /7) with memory operand
    /// </summary>
    [Fact]
    public void TestCmpM8Imm8()
    {
        // Arrange
        byte[] code = { 0x80, 0x39, 0x05 }; // CMP BYTE PTR [ECX], 0x05
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("cmp", instructions[0].Mnemonic);
        Assert.Equal("byte ptr [ecx], 0x05", instructions[0].Operands);
    }
}
