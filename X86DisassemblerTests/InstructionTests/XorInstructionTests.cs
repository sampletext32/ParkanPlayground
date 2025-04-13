using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for XOR instruction handlers
/// </summary>
public class XorInstructionTests
{
    /// <summary>
    /// Tests the XorRegMemHandler for decoding XOR r32, r/m32 instruction
    /// </summary>
    [Fact]
    public void XorRegMemHandler_DecodesXorR32Rm32_Correctly()
    {
        // Arrange
        // XOR EAX, ECX (33 C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        // mod=3 means direct register addressing, reg=0 is EAX, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0x33, 0xC1 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("xor", instruction.Mnemonic);
        Assert.Equal("eax, ecx", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the XorMemRegHandler for decoding XOR r/m32, r32 instruction
    /// </summary>
    [Fact]
    public void XorMemRegHandler_DecodesXorRm32R32_Correctly()
    {
        // Arrange
        // XOR ECX, EAX (31 C1) - ModR/M byte C1 = 11 000 001 (mod=3, reg=0, rm=1)
        // mod=3 means direct register addressing, reg=0 is EAX, rm=1 is ECX
        byte[] codeBuffer = new byte[] { 0x31, 0xC1 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("xor", instruction.Mnemonic);
        Assert.Equal("ecx, eax", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the XorAlImmHandler for decoding XOR AL, imm8 instruction
    /// </summary>
    [Fact]
    public void XorAlImmHandler_DecodesXorAlImm8_Correctly()
    {
        // Arrange
        // XOR AL, 0x42 (34 42)
        byte[] codeBuffer = new byte[] { 0x34, 0x42 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("xor", instruction.Mnemonic);
        Assert.Equal("al, 0x42", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the XorEaxImmHandler for decoding XOR EAX, imm32 instruction
    /// </summary>
    [Fact]
    public void XorEaxImmHandler_DecodesXorEaxImm32_Correctly()
    {
        // Arrange
        // XOR EAX, 0x12345678 (35 78 56 34 12)
        byte[] codeBuffer = new byte[] { 0x35, 0x78, 0x56, 0x34, 0x12 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("xor", instruction.Mnemonic);
        Assert.Equal("eax, 0x12345678", instruction.Operands);
    }
}
