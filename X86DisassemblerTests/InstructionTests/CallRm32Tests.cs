using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for CALL r/m32 instruction (0xFF /2)
/// </summary>
public class CallRm32Tests
{
    /// <summary>
    /// Tests the CALL r32 instruction (0xFF /2) with register operand
    /// </summary>
    [Fact]
    public void TestCallReg()
    {
        // Arrange
        byte[] code = { 0xFF, 0xD3 }; // CALL EBX
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("call", instructions[0].Mnemonic);
        Assert.Equal("ebx", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the CALL m32 instruction (0xFF /2) with memory operand
    /// </summary>
    [Fact]
    public void TestCallMem()
    {
        // Arrange
        byte[] code = { 0xFF, 0x10 }; // CALL DWORD PTR [EAX]
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("call", instructions[0].Mnemonic);
        Assert.Equal("dword ptr [eax]", instructions[0].Operands);
    }
}
