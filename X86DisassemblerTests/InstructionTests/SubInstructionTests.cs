using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for SUB instruction handlers
/// </summary>
public class SubInstructionTests
{
    /// <summary>
    /// Tests the SubImmFromRm32Handler for decoding SUB r/m32, imm32 instruction
    /// </summary>
    [Fact]
    public void SubImmFromRm32Handler_DecodesSubRm32Imm32_Correctly()
    {
        // Arrange
        // SUB EAX, 0x12345678 (81 E8 78 56 34 12) - Subtract 0x12345678 from EAX
        byte[] codeBuffer = new byte[] { 0x81, 0xE8, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("sub", instruction.Mnemonic);
        Assert.Equal("eax, 0x12345678", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the SubImmFromRm32Handler for decoding SUB memory, imm32 instruction
    /// </summary>
    [Fact]
    public void SubImmFromRm32Handler_DecodesSubMemImm32_Correctly()
    {
        // Arrange
        // SUB [EBX+0x10], 0x12345678 (81 6B 10 78 56 34 12) - Subtract 0x12345678 from memory at [EBX+0x10]
        byte[] codeBuffer = new byte[] { 0x81, 0x6B, 0x10, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("sub", instruction.Mnemonic);
        // The actual output from the disassembler for this instruction
        Assert.Equal("dword ptr [ebx+0x10], 0x12345678", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the SubImmFromRm32SignExtendedHandler for decoding SUB r/m32, imm8 instruction (sign-extended)
    /// </summary>
    [Fact]
    public void SubImmFromRm32SignExtendedHandler_DecodesSubRm32Imm8_Correctly()
    {
        // Arrange
        // SUB EAX, 0x42 (83 E8 42) - Subtract sign-extended 0x42 from EAX
        byte[] codeBuffer = new byte[] { 0x83, 0xE8, 0x42 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("sub", instruction.Mnemonic);
        Assert.Equal("eax, 0x42", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the SubImmFromRm32SignExtendedHandler for decoding SUB r/m32, negative imm8 instruction (sign-extended)
    /// </summary>
    [Fact]
    public void SubImmFromRm32SignExtendedHandler_DecodesSubRm32NegativeImm8_Correctly()
    {
        // Arrange
        // SUB EAX, 0xF0 (83 E8 F0) - Subtract sign-extended 0xF0 from EAX
        byte[] codeBuffer = new byte[] { 0x83, 0xE8, 0xF0 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("sub", instruction.Mnemonic);
        // F0 is -16 in two's complement, should be sign-extended to 0xFFFFFFF0
        Assert.Equal("eax, 0xFFFFFFF0", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the SubImmFromRm32SignExtendedHandler for decoding SUB memory, imm8 instruction
    /// </summary>
    [Fact]
    public void SubImmFromRm32SignExtendedHandler_DecodesSubMemImm8_Correctly()
    {
        // Arrange
        // SUB [EBX+0x10], 0x42 (83 6B 10 42) - Subtract sign-extended 0x42 from memory at [EBX+0x10]
        byte[] codeBuffer = new byte[] { 0x83, 0x6B, 0x10, 0x42 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("sub", instruction.Mnemonic);
        // The actual output from the disassembler for this instruction
        Assert.Equal("dword ptr [ebx+0x10], 0x42", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the SubR32Rm32Handler for decoding SUB r32, r/m32 instruction
    /// </summary>
    [Fact]
    public void SubR32Rm32Handler_DecodesSubR32Rm32_Correctly()
    {
        // Arrange
        // SUB EBX, EAX (2B D8) - Subtract EAX from EBX
        byte[] codeBuffer = new byte[] { 0x2B, 0xD8 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("sub", instruction.Mnemonic);
        Assert.Equal("ebx, eax", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the SubRm32R32Handler for decoding SUB r/m32, r32 instruction
    /// </summary>
    [Fact]
    public void SubRm32R32Handler_DecodesSubRm32R32_Correctly()
    {
        // Arrange
        // SUB EAX, EBX (29 D8) - Subtract EBX from EAX
        byte[] codeBuffer = new byte[] { 0x29, 0xD8 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("sub", instruction.Mnemonic);
        Assert.Equal("eax, ebx", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the SubRm32R32Handler for decoding SUB memory, r32 instruction
    /// </summary>
    [Fact]
    public void SubRm32R32Handler_DecodesSubMemR32_Correctly()
    {
        // Arrange
        // SUB [EBX+0x10], ECX (29 4B 10) - Subtract ECX from memory at [EBX+0x10]
        byte[] codeBuffer = new byte[] { 0x29, 0x4B, 0x10 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("sub", instruction.Mnemonic);
        Assert.Equal("dword ptr [ebx+0x10], ecx", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the SubR32Rm32Handler for decoding SUB r32, memory instruction
    /// </summary>
    [Fact]
    public void SubR32Rm32Handler_DecodesSubR32Mem_Correctly()
    {
        // Arrange
        // SUB ECX, [EBX+0x10] (2B 4B 10) - Subtract memory at [EBX+0x10] from ECX
        byte[] codeBuffer = new byte[] { 0x2B, 0x4B, 0x10 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("sub", instruction.Mnemonic);
        Assert.Equal("ecx, dword ptr [ebx+0x10]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests a sequence of SUB instructions with different encoding
    /// </summary>
    [Fact]
    public void SubInstruction_DecodesComplexSubSequence_Correctly()
    {
        // Arrange
        // SUB ESP, 0x10 (83 EC 10) - Create stack space
        // SUB EAX, EBX (29 D8) - Subtract EBX from EAX
        // SUB ECX, [EBP-4] (2B 4D FC) - Subtract memory at [EBP-4] from ECX
        byte[] codeBuffer = new byte[] { 0x83, 0xEC, 0x10, 0x29, 0xD8, 0x2B, 0x4D, 0xFC };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Equal(3, instructions.Count);
        
        // First instruction: SUB ESP, 0x10
        Assert.Equal("sub", instructions[0].Mnemonic);
        Assert.Equal("esp, 0x10", instructions[0].Operands);
        
        // Second instruction: SUB EAX, EBX
        Assert.Equal("sub", instructions[1].Mnemonic);
        Assert.Equal("eax, ebx", instructions[1].Operands);
        
        // Third instruction: SUB ECX, [EBP-4]
        Assert.Equal("sub", instructions[2].Mnemonic);
        Assert.Equal("ecx, dword ptr [ebp-0x04]", instructions[2].Operands);
    }
}
