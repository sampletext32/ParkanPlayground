using X86Disassembler.X86;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for floating-point instruction handlers
/// </summary>
public class FloatingPointInstructionTests
{
    /// <summary>
    /// Tests the FnstswHandler for decoding FNSTSW AX instruction
    /// </summary>
    [Fact]
    public void FnstswHandler_DecodesFnstswAx_Correctly()
    {
        // Arrange
        // FNSTSW AX (DF E0)
        byte[] codeBuffer = new byte[] { 0xDF, 0xE0 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("fnstsw", instruction.Mnemonic);
        Assert.Equal("ax", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the Float32OperationHandler for decoding FADD ST(0), ST(1) instruction
    /// </summary>
    [Fact]
    public void Float32OperationHandler_DecodesAddSt0St1_Correctly()
    {
        // Arrange
        // FADD ST(0), ST(1) (D8 C1)
        byte[] codeBuffer = new byte[] { 0xD8, 0xC1 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("fadd", instruction.Mnemonic);
        Assert.Equal("st(0), st(1)", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the Float32OperationHandler for decoding FADD dword ptr [eax] instruction
    /// </summary>
    [Fact]
    public void Float32OperationHandler_DecodesAddMemory_Correctly()
    {
        // Arrange
        // FADD dword ptr [eax] (D8 00)
        byte[] codeBuffer = new byte[] { 0xD8, 0x00 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("fadd", instruction.Mnemonic);
        Assert.Equal("dword ptr [eax]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the LoadStoreControlHandler for decoding FLD dword ptr [eax] instruction
    /// </summary>
    [Fact]
    public void LoadStoreControlHandler_DecodesLoadMemory_Correctly()
    {
        // Arrange
        // FLD dword ptr [eax] (D9 00)
        byte[] codeBuffer = new byte[] { 0xD9, 0x00 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("fld", instruction.Mnemonic);
        Assert.Equal("dword ptr [eax]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the LoadStoreControlHandler for decoding FLDCW [eax] instruction
    /// </summary>
    [Fact]
    public void LoadStoreControlHandler_DecodesLoadControlWord_Correctly()
    {
        // Arrange
        // FLDCW [eax] (D9 28)
        byte[] codeBuffer = new byte[] { 0xD9, 0x28 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("fldcw", instruction.Mnemonic);
        Assert.Equal("word ptr [eax]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the Int32OperationHandler for decoding FIADD dword ptr [eax] instruction
    /// </summary>
    [Fact]
    public void Int32OperationHandler_DecodesIntegerAdd_Correctly()
    {
        // Arrange
        // FIADD dword ptr [eax] (DA 00)
        byte[] codeBuffer = new byte[] { 0xDA, 0x00 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("fiadd", instruction.Mnemonic);
        Assert.Equal("dword ptr [eax]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the LoadStoreInt32Handler for decoding FILD dword ptr [eax] instruction
    /// </summary>
    [Fact]
    public void LoadStoreInt32Handler_DecodesIntegerLoad_Correctly()
    {
        // Arrange
        // FILD dword ptr [eax] (DB 00)
        byte[] codeBuffer = new byte[] { 0xDB, 0x00 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("fild", instruction.Mnemonic);
        Assert.Equal("dword ptr [eax]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the Float64OperationHandler for decoding FADD qword ptr [eax] instruction
    /// </summary>
    [Fact]
    public void Float64OperationHandler_DecodesDoubleAdd_Correctly()
    {
        // Arrange
        // FADD qword ptr [eax] (DC 00)
        byte[] codeBuffer = new byte[] { 0xDC, 0x00 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("fadd", instruction.Mnemonic);
        Assert.Equal("qword ptr [eax]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the Float64OperationHandler for decoding FADD ST(1), ST(0) instruction
    /// </summary>
    [Fact]
    public void Float64OperationHandler_DecodesAddSt1St0_Correctly()
    {
        // Arrange
        // FADD ST(1), ST(0) (DC C1)
        byte[] codeBuffer = new byte[] { 0xDC, 0xC1 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("fadd", instruction.Mnemonic);
        Assert.Equal("st(1), st(0)", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the LoadStoreFloat64Handler for decoding FLD qword ptr [eax] instruction
    /// </summary>
    [Fact]
    public void LoadStoreFloat64Handler_DecodesDoubleLoad_Correctly()
    {
        // Arrange
        // FLD qword ptr [eax] (DD 00)
        byte[] codeBuffer = new byte[] { 0xDD, 0x00 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("fld", instruction.Mnemonic);
        Assert.Equal("qword ptr [eax]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the Int16OperationHandler for decoding FIADD word ptr [eax] instruction
    /// </summary>
    [Fact]
    public void Int16OperationHandler_DecodesShortAdd_Correctly()
    {
        // Arrange
        // FIADD word ptr [eax] (DE 00)
        byte[] codeBuffer = new byte[] { 0xDE, 0x00 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("fiadd", instruction.Mnemonic);
        Assert.Equal("word ptr [eax]", instruction.Operands);
    }
    
    /// <summary>
    /// Tests the LoadStoreInt16Handler for decoding FILD word ptr [eax] instruction
    /// </summary>
    [Fact]
    public void LoadStoreInt16Handler_DecodesShortLoad_Correctly()
    {
        // Arrange
        // FILD word ptr [eax] (DF 00)
        byte[] codeBuffer = new byte[] { 0xDF, 0x00 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal("fild", instruction.Mnemonic);
        Assert.Equal("word ptr [eax]", instruction.Operands);
    }
}
