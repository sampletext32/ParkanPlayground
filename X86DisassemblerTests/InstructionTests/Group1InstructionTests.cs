using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for Group1 instruction handlers
/// </summary>
public class Group1InstructionTests
{
    /// <summary>
    /// Tests the AddImmToRm8Handler for decoding ADD r/m8, imm8 instruction
    /// </summary>
    [Fact]
    public void AddImmToRm8Handler_DecodesAddRm8Imm8_Correctly()
    {
        // Arrange
        // ADD AL, 0x42 (80 C0 42) - ModR/M byte C0 = 11 000 000 (mod=3, reg=0, rm=0)
        // mod=3 means direct register addressing, reg=0 indicates ADD operation, rm=0 is AL
        byte[] codeBuffer = new byte[] { 0x80, 0xC0, 0x42 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("add", instruction.Mnemonic);
        Assert.Equal("al, 0x42", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the AddImmToRm32Handler for decoding ADD r/m32, imm32 instruction
    /// </summary>
    [Fact]
    public void AddImmToRm32Handler_DecodesAddRm32Imm32_Correctly()
    {
        // Arrange
        // ADD ECX, 0x12345678 (81 C1 78 56 34 12) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        // mod=3 means direct register addressing, reg=0 indicates ADD operation, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0x81, 0xC1, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("add", instruction.Mnemonic);
        Assert.Equal("ecx, 0x12345678", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the OrImmToRm8Handler for decoding OR r/m8, imm8 instruction
    /// </summary>
    [Fact]
    public void OrImmToRm8Handler_DecodesOrRm8Imm8_Correctly()
    {
        // Arrange
        // OR BL, 0x42 (80 CB 42) - ModR/M byte CB = 11 001 011 (mod=3, reg=1, rm=3)
        // mod=3 means direct register addressing, reg=1 indicates OR operation, rm=3 is BL
        byte[] codeBuffer = new byte[] { 0x80, 0xCB, 0x42 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("or", instruction.Mnemonic);
        Assert.Equal("bl, 0x42", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the SubImmFromRm32Handler for decoding SUB r/m32, imm32 instruction
    /// </summary>
    [Fact]
    public void SubImmFromRm32Handler_DecodesSubRm32Imm32_Correctly()
    {
        // Arrange
        // SUB EDX, 0x12345678 (81 EA 78 56 34 12) - ModR/M byte EA = 11 101 010 (mod=3, reg=5, rm=2)
        // mod=3 means direct register addressing, reg=5 indicates SUB operation, rm=2 is EDX
        byte[] codeBuffer = new byte[] { 0x81, 0xEA, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("sub", instruction.Mnemonic);
        Assert.Equal("edx, 0x12345678", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the CmpImmWithRm32Handler for decoding CMP r/m32, imm32 instruction
    /// </summary>
    [Fact]
    public void CmpImmWithRm32Handler_DecodesCmpRm32Imm32_Correctly()
    {
        // Arrange
        // CMP EBX, 0x12345678 (81 FB 78 56 34 12) - ModR/M byte FB = 11 111 011 (mod=3, reg=7, rm=3)
        // mod=3 means direct register addressing, reg=7 indicates CMP operation, rm=3 is EBX
        byte[] codeBuffer = new byte[] { 0x81, 0xFB, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("cmp", instruction.Mnemonic);
        Assert.Equal("ebx, 0x12345678", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the AdcImmToRm32Handler for decoding ADC r/m32, imm32 instruction
    /// </summary>
    [Fact]
    public void AdcImmToRm32Handler_DecodesAdcRm32Imm32_Correctly()
    {
        // Arrange
        // ADC ECX, 0x12345678 (81 D1 78 56 34 12) - ModR/M byte D1 = 11 010 001 (mod=3, reg=2, rm=1)
        // mod=3 means direct register addressing, reg=2 indicates ADC operation, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0x81, 0xD1, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("adc", instruction.Mnemonic);
        Assert.Equal("ecx, 0x12345678", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the AdcImmToRm32SignExtendedHandler for decoding ADC r/m32, imm8 instruction (sign-extended)
    /// </summary>
    [Fact]
    public void AdcImmToRm32SignExtendedHandler_DecodesAdcRm32Imm8_Correctly()
    {
        // Arrange
        // ADC ECX, 0x42 (83 D1 42) - ModR/M byte D1 = 11 010 001 (mod=3, reg=2, rm=1)
        // mod=3 means direct register addressing, reg=2 indicates ADC operation, rm=1 is ECX
        // The immediate value 0x42 is sign-extended to 32 bits
        byte[] codeBuffer = new byte[] { 0x83, 0xD1, 0x42 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("adc", instruction.Mnemonic);
        Assert.Equal("ecx, 0x00000042", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the SbbImmFromRm32Handler for decoding SBB r/m32, imm32 instruction
    /// </summary>
    [Fact]
    public void SbbImmFromRm32Handler_DecodesSbbRm32Imm32_Correctly()
    {
        // Arrange
        // SBB EDX, 0x12345678 (81 DA 78 56 34 12) - ModR/M byte DA = 11 011 010 (mod=3, reg=3, rm=2)
        // mod=3 means direct register addressing, reg=3 indicates SBB operation, rm=2 is EDX
        byte[] codeBuffer = new byte[] { 0x81, 0xDA, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("sbb", instruction.Mnemonic);
        Assert.Equal("edx, 0x12345678", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the SbbImmFromRm32SignExtendedHandler for decoding SBB r/m32, imm8 instruction (sign-extended)
    /// </summary>
    [Fact]
    public void SbbImmFromRm32SignExtendedHandler_DecodesSbbRm32Imm8_Correctly()
    {
        // Arrange
        // SBB EDX, 0x42 (83 DA 42) - ModR/M byte DA = 11 011 010 (mod=3, reg=3, rm=2)
        // mod=3 means direct register addressing, reg=3 indicates SBB operation, rm=2 is EDX
        // The immediate value 0x42 is sign-extended to 32 bits
        byte[] codeBuffer = new byte[] { 0x83, 0xDA, 0x42 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("sbb", instruction.Mnemonic);
        Assert.Equal("edx, 0x00000042", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the AndImmWithRm32Handler for decoding AND r/m32, imm32 instruction
    /// </summary>
    [Fact]
    public void AndImmWithRm32Handler_DecodesAndRm32Imm32_Correctly()
    {
        // Arrange
        // AND EBX, 0x12345678 (81 E3 78 56 34 12) - ModR/M byte E3 = 11 100 011 (mod=3, reg=4, rm=3)
        // mod=3 means direct register addressing, reg=4 indicates AND operation, rm=3 is EBX
        byte[] codeBuffer = new byte[] { 0x81, 0xE3, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("and", instruction.Mnemonic);
        Assert.Equal("ebx, 0x12345678", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the AndImmWithRm32SignExtendedHandler for decoding AND r/m32, imm8 instruction (sign-extended)
    /// </summary>
    [Fact]
    public void AndImmWithRm32SignExtendedHandler_DecodesAndRm32Imm8_Correctly()
    {
        // Arrange
        // AND EBX, 0x42 (83 E3 42) - ModR/M byte E3 = 11 100 011 (mod=3, reg=4, rm=3)
        // mod=3 means direct register addressing, reg=4 indicates AND operation, rm=3 is EBX
        // The immediate value 0x42 is sign-extended to 32 bits
        byte[] codeBuffer = new byte[] { 0x83, 0xE3, 0x42 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("and", instruction.Mnemonic);
        Assert.Equal("ebx, 0x00000042", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the XorImmWithRm32Handler for decoding XOR r/m32, imm32 instruction
    /// </summary>
    [Fact]
    public void XorImmWithRm32Handler_DecodesXorRm32Imm32_Correctly()
    {
        // Arrange
        // XOR ESI, 0x12345678 (81 F6 78 56 34 12) - ModR/M byte F6 = 11 110 110 (mod=3, reg=6, rm=6)
        // mod=3 means direct register addressing, reg=6 indicates XOR operation, rm=6 is ESI
        byte[] codeBuffer = new byte[] { 0x81, 0xF6, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("xor", instruction.Mnemonic);
        Assert.Equal("esi, 0x12345678", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the XorImmWithRm32SignExtendedHandler for decoding XOR r/m32, imm8 instruction (sign-extended)
    /// </summary>
    [Fact]
    public void XorImmWithRm32SignExtendedHandler_DecodesXorRm32Imm8_Correctly()
    {
        // Arrange
        // XOR ESI, 0x42 (83 F6 42) - ModR/M byte F6 = 11 110 110 (mod=3, reg=6, rm=6)
        // mod=3 means direct register addressing, reg=6 indicates XOR operation, rm=6 is ESI
        // The immediate value 0x42 is sign-extended to 32 bits
        byte[] codeBuffer = new byte[] { 0x83, 0xF6, 0x42 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("xor", instruction.Mnemonic);
        Assert.Equal("esi, 0x00000042", instruction.Operands);
    }
}
