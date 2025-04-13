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
}
