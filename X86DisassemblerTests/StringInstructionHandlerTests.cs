namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;
using X86Disassembler.X86.Handlers.String;

/// <summary>
/// Tests for string instruction handlers
/// </summary>
public class StringInstructionHandlerTests
{
    /// <summary>
    /// Tests the StringInstructionHandler for decoding REP MOVS instruction
    /// </summary>
    [Fact]
    public void StringInstructionHandler_DecodesRepMovs_Correctly()
    {
        // Arrange
        // REP MOVS (F3 A5)
        byte[] codeBuffer = new byte[] { 0xF3, 0xA5 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("rep movs", instruction.Mnemonic);
        Assert.Equal("dword ptr [edi], dword ptr [esi]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the StringInstructionHandler for decoding REPNE SCAS instruction
    /// </summary>
    [Fact]
    public void StringInstructionHandler_DecodesRepneScas_Correctly()
    {
        // Arrange
        // REPNE SCAS (F2 AF)
        byte[] codeBuffer = new byte[] { 0xF2, 0xAF };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("repne scas", instruction.Mnemonic);
        Assert.Equal("eax, dword ptr [edi]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the StringInstructionHandler for decoding MOVS instruction without prefix
    /// </summary>
    [Fact]
    public void StringInstructionHandler_DecodesMovs_Correctly()
    {
        // Arrange
        // MOVS (A5)
        byte[] codeBuffer = new byte[] { 0xA5 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("movs", instruction.Mnemonic);
        Assert.Equal("dword ptr [edi], dword ptr [esi]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the StringInstructionHandler for decoding STOS instruction without prefix
    /// </summary>
    [Fact]
    public void StringInstructionHandler_DecodesStosb_Correctly()
    {
        // Arrange
        // STOSB (AA)
        byte[] codeBuffer = new byte[] { 0xAA };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("stos", instruction.Mnemonic);
        Assert.Equal("byte ptr [edi], al", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the StringInstructionHandler for decoding LODS instruction without prefix
    /// </summary>
    [Fact]
    public void StringInstructionHandler_DecodesLodsd_Correctly()
    {
        // Arrange
        // LODSD (AD)
        byte[] codeBuffer = new byte[] { 0xAD };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("lods", instruction.Mnemonic);
        Assert.Equal("eax, dword ptr [esi]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the StringInstructionHandler for decoding SCAS instruction without prefix
    /// </summary>
    [Fact]
    public void StringInstructionHandler_DecodesScasb_Correctly()
    {
        // Arrange
        // SCASB (AE)
        byte[] codeBuffer = new byte[] { 0xAE };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("scas", instruction.Mnemonic);
        Assert.Equal("al, byte ptr [edi]", instruction.Operands);
    }
}
