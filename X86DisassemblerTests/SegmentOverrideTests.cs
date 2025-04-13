namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;

/// <summary>
/// Tests for segment override prefixes
/// </summary>
public class SegmentOverrideTests
{
    /// <summary>
    /// Tests that the CS segment override prefix (0x2E) is correctly recognized
    /// </summary>
    [Fact]
    public void CsSegmentOverride_IsRecognized()
    {
        // Arrange
        // CS segment override prefix (0x2E) followed by MOV EAX, [0] (8B 05 00 00 00 00)
        byte[] codeBuffer = new byte[] { 0x2E, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("mov", instruction.Mnemonic);
        Assert.Equal("eax, dword ptr cs:[0x00000000]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests that the DS segment override prefix (0x3E) is correctly recognized
    /// </summary>
    [Fact]
    public void DsSegmentOverride_IsRecognized()
    {
        // Arrange
        // DS segment override prefix (0x3E) followed by MOV EAX, [0] (8B 05 00 00 00 00)
        byte[] codeBuffer = new byte[] { 0x3E, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("mov", instruction.Mnemonic);
        Assert.Equal("eax, dword ptr ds:[0x00000000]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests that the ES segment override prefix (0x26) is correctly recognized
    /// </summary>
    [Fact]
    public void EsSegmentOverride_IsRecognized()
    {
        // Arrange
        // ES segment override prefix (0x26) followed by MOV EAX, [0] (8B 05 00 00 00 00)
        byte[] codeBuffer = new byte[] { 0x26, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("mov", instruction.Mnemonic);
        Assert.Equal("eax, dword ptr es:[0x00000000]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests that the FS segment override prefix (0x64) is correctly recognized
    /// </summary>
    [Fact]
    public void FsSegmentOverride_IsRecognized()
    {
        // Arrange
        // FS segment override prefix (0x64) followed by MOV [0], ESP (89 25 00 00 00 00)
        byte[] codeBuffer = new byte[] { 0x64, 0x89, 0x25, 0x00, 0x00, 0x00, 0x00 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("mov", instruction.Mnemonic);
        Assert.Equal("dword ptr fs:[0x00000000], esp", instruction.Operands);
    }
    
    /// <summary>
    /// Tests that the GS segment override prefix (0x65) is correctly recognized
    /// </summary>
    [Fact]
    public void GsSegmentOverride_IsRecognized()
    {
        // Arrange
        // GS segment override prefix (0x65) followed by MOV EAX, [0] (8B 05 00 00 00 00)
        byte[] codeBuffer = new byte[] { 0x65, 0x8B, 0x05, 0x00, 0x00, 0x00, 0x00 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("mov", instruction.Mnemonic);
        Assert.Equal("eax, dword ptr gs:[0x00000000]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests that the SS segment override prefix (0x36) is correctly recognized
    /// </summary>
    [Fact]
    public void SsSegmentOverride_IsRecognized()
    {
        // Arrange
        // SS segment override prefix (0x36) followed by MOV EAX, [EBP-4] (8B 45 FC)
        byte[] codeBuffer = new byte[] { 0x36, 0x8B, 0x45, 0xFC };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("mov", instruction.Mnemonic);
        Assert.Equal("eax, dword ptr ss:[ebp-0x04]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests that the FS segment override prefix (0x64) is correctly recognized when it's the only byte
    /// </summary>
    [Fact]
    public void FsSegmentOverride_Alone_IsRecognized()
    {
        // Arrange
        // Just the FS segment override prefix (0x64)
        byte[] codeBuffer = new byte[] { 0x64 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("fs", instruction.Mnemonic);
        Assert.Equal("", instruction.Operands);
    }
    
    /// <summary>
    /// Tests segment override with a complex addressing mode
    /// </summary>
    [Fact]
    public void SegmentOverride_WithComplexAddressing_IsRecognized()
    {
        // Arrange
        // FS segment override prefix (0x64) followed by MOV EAX, [EBX+ECX*4+0x10] (8B 84 8B 10 00 00 00)
        byte[] codeBuffer = new byte[] { 0x64, 0x8B, 0x84, 0x8B, 0x10, 0x00, 0x00, 0x00 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("mov", instruction.Mnemonic);
        Assert.Equal("eax, dword ptr fs:[ebx+ecx*4+0x10]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests segment override with a string instruction
    /// </summary>
    [Fact]
    public void SegmentOverride_WithStringInstruction_IsRecognized()
    {
        // Arrange
        // ES segment override prefix (0x26) followed by LODS DWORD PTR DS:[ESI] (AD)
        byte[] codeBuffer = new byte[] { 0x26, 0xAD };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("lods", instruction.Mnemonic);
        // The string instruction uses DS:ESI by default, but with ES override it becomes ES:ESI
        Assert.Equal("eax, dword ptr es:[esi]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests segment override with a REP prefix
    /// </summary>
    [Fact]
    public void SegmentOverride_WithRepPrefix_IsRecognized()
    {
        // Arrange
        // REP prefix (F3) followed by FS segment override prefix (0x64) followed by MOVS (A4)
        byte[] codeBuffer = new byte[] { 0xF3, 0x64, 0xA4 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("rep movs", instruction.Mnemonic);
        Assert.Equal("byte ptr fs:[edi], byte ptr fs:[esi]", instruction.Operands);
    }
}
