using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for MOV r/m32, imm32 instruction (0xC7)
/// </summary>
public class MovRm32Imm32Tests
{
    /// <summary>
    /// Tests the MOV r32, imm32 instruction (0xC7) with register operand
    /// </summary>
    [Fact]
    public void TestMovR32Imm32()
    {
        // Arrange
        byte[] code = { 0xC7, 0xC0, 0x78, 0x56, 0x34, 0x12 }; // MOV EAX, 0x12345678
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("mov", instructions[0].Mnemonic);
        Assert.Equal("eax, 0x12345678", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the MOV m32, imm32 instruction (0xC7) with memory operand
    /// </summary>
    [Fact]
    public void TestMovM32Imm32()
    {
        // Arrange
        byte[] code = { 0xC7, 0x00, 0x78, 0x56, 0x34, 0x12 }; // MOV DWORD PTR [EAX], 0x12345678
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("mov", instructions[0].Mnemonic);
        Assert.Equal("dword ptr [eax], 0x12345678", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the MOV m32, imm32 instruction (0xC7) with SIB byte addressing
    /// </summary>
    [Fact]
    public void TestMovM32Imm32_WithSIB()
    {
        // Arrange
        // MOV DWORD PTR [ESP+0x10], 0x00000000
        byte[] code = { 0xC7, 0x44, 0x24, 0x10, 0x00, 0x00, 0x00, 0x00 };
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("mov", instructions[0].Mnemonic);
        Assert.Equal("dword ptr [esp+0x10], 0x00000000", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the MOV m32, imm32 instruction (0xC7) with complex SIB byte addressing
    /// </summary>
    [Fact]
    public void TestMovM32Imm32_WithComplexSIB()
    {
        // Arrange
        // MOV DWORD PTR [EAX+ECX*4+0x12345678], 0xAABBCCDD
        byte[] code = { 0xC7, 0x84, 0x88, 0x78, 0x56, 0x34, 0x12, 0xDD, 0xCC, 0xBB, 0xAA };
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Single(instructions);
        Assert.Equal("mov", instructions[0].Mnemonic);
        Assert.Equal("dword ptr [eax+ecx*4+0x12345678], 0xAABBCCDD", instructions[0].Operands);
    }
    
    /// <summary>
    /// Tests the MOV m32, imm32 instruction (0xC7) with consecutive instructions
    /// </summary>
    [Fact]
    public void TestMovM32Imm32_ConsecutiveInstructions()
    {
        // Arrange
        // MOV DWORD PTR [ESP+0x10], 0x00000000
        // MOV DWORD PTR [ESP+0x14], 0x00000000
        byte[] code = { 
            0xC7, 0x44, 0x24, 0x10, 0x00, 0x00, 0x00, 0x00,
            0xC7, 0x44, 0x24, 0x14, 0x00, 0x00, 0x00, 0x00
        };
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x1000);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Equal(2, instructions.Count);
        
        // First instruction
        Assert.Equal("mov", instructions[0].Mnemonic);
        Assert.Equal("dword ptr [esp+0x10], 0x00000000", instructions[0].Operands);
        
        // Second instruction
        Assert.Equal("mov", instructions[1].Mnemonic);
        Assert.Equal("dword ptr [esp+0x14], 0x00000000", instructions[1].Operands);
    }

    /// <summary>
    /// Tests the MOV m32, imm32 instruction (0xC7) with instruction boundary detection
    /// </summary>
    [Fact]
    public void TestMovM32Imm32_InstructionBoundaryDetection()
    {
        // Arrange
        // This is the sequence from address 0x00002441 that was problematic
        // MOV DWORD PTR [ESP+0x10], 0x00000000
        // MOV DWORD PTR [ESP+0x14], 0x00000000
        byte[] code = { 
            0xC7, 0x44, 0x24, 0x10, 0x00, 0x00, 0x00, 0x00,
            0xC7, 0x44, 0x24, 0x14, 0x00, 0x00, 0x00, 0x00
        };
        
        // Act
        Disassembler disassembler = new Disassembler(code, 0x2441);
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Equal(2, instructions.Count);
        
        // First instruction
        Assert.Equal("mov", instructions[0].Mnemonic);
        Assert.Equal("dword ptr [esp+0x10], 0x00000000", instructions[0].Operands);
        
        // Second instruction
        Assert.Equal("mov", instructions[1].Mnemonic);
        Assert.Equal("dword ptr [esp+0x14], 0x00000000", instructions[1].Operands);
    }
}
