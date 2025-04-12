namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;
using X86Disassembler.X86.Handlers;

/// <summary>
/// Tests for data transfer instruction handlers
/// </summary>
public class DataTransferInstructionTests
{
    /// <summary>
    /// Tests the DataTransferHandler for decoding MOV r32, r/m32 instruction
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesMovR32Rm32_Correctly()
    {
        // Arrange
        // MOV EAX, ECX (8B C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        // mod=3 means direct register addressing, reg=0 is EAX, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0x8B, 0xC1 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("mov", instruction.Mnemonic);
        Assert.Equal("ecx, eax", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the DataTransferHandler for decoding MOV r/m32, r32 instruction
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesMovRm32R32_Correctly()
    {
        // Arrange
        // MOV ECX, EAX (89 C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        // mod=3 means direct register addressing, reg=0 is EAX, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0x89, 0xC1 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("mov", instruction.Mnemonic);
        Assert.Equal("eax, ecx", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the DataTransferHandler for decoding MOV r32, imm32 instruction
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesMovR32Imm32_Correctly()
    {
        // Arrange
        // MOV EAX, 0x12345678 (B8 78 56 34 12)
        byte[] codeBuffer = new byte[] { 0xB8, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("mov", instruction.Mnemonic);
        Assert.Equal("eax, 0x12345678", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the DataTransferHandler for decoding MOV EAX, moffs32 instruction
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesMovEaxMoffs32_Correctly()
    {
        // Arrange
        // MOV EAX, [0x12345678] (A1 78 56 34 12)
        byte[] codeBuffer = new byte[] { 0xA1, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("mov", instruction.Mnemonic);
        Assert.Equal("eax, [0x12345678]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the DataTransferHandler for decoding MOV moffs32, EAX instruction
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesMovMoffs32Eax_Correctly()
    {
        // Arrange
        // MOV [0x12345678], EAX (A3 78 56 34 12)
        byte[] codeBuffer = new byte[] { 0xA3, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("mov", instruction.Mnemonic);
        Assert.Equal("[0x12345678], eax", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the DataTransferHandler for decoding MOV with memory addressing
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesMovWithMemoryAddressing_Correctly()
    {
        // Arrange
        // MOV EAX, [ECX+0x12345678] (8B 81 78 56 34 12) - ModR/M byte 81 = 10 000 001 (mod=2, reg=0, rm=1)
        // mod=2 means memory addressing with 32-bit displacement, reg=0 is EAX, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0x8B, 0x81, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("mov", instruction.Mnemonic);
        Assert.Equal("dword ptr [ecx+0x12345678], eax", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the DataTransferHandler for decoding PUSH r32 instruction
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesPushR32_Correctly()
    {
        // Arrange
        // PUSH EAX (50)
        byte[] codeBuffer = new byte[] { 0x50 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("push", instruction.Mnemonic);
        Assert.Equal("eax", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the DataTransferHandler for decoding POP r32 instruction
    /// </summary>
    [Fact]
    public void DataTransferHandler_DecodesPopR32_Correctly()
    {
        // Arrange
        // POP ECX (59)
        byte[] codeBuffer = new byte[] { 0x59 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("pop", instruction.Mnemonic);
        Assert.Equal("ecx", instruction.Operands);
    }
}
