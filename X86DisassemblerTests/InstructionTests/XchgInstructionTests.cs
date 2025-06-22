using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for exchange instruction handlers
/// </summary>
public class XchgInstructionTests
{
    /// <summary>
    /// Tests the XchgEaxRegHandler for decoding NOP instruction (XCHG EAX, EAX)
    /// </summary>
    [Fact]
    public void XchgEaxRegHandler_DecodesNop_Correctly()
    {
        // Arrange
        // NOP (90) - No operation (XCHG EAX, EAX)
        byte[] codeBuffer = new byte[] { 0x90 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Nop, instruction.Type);
        
        // Check that we have no operands for NOP
        Assert.Empty(instruction.StructuredOperands);
    }
    
    /// <summary>
    /// Tests the XchgEaxRegHandler for decoding XCHG EAX, ECX instruction
    /// </summary>
    [Fact]
    public void XchgEaxRegHandler_DecodesXchgEaxEcx_Correctly()
    {
        // Arrange
        // XCHG EAX, ECX (91) - Exchange EAX and ECX
        byte[] codeBuffer = new byte[] { 0x91 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Xchg, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand1 = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand1.Register);
        Assert.Equal(32, registerOperand1.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (ECX)
        var ecxOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(ecxOperand);
        var registerOperand2 = (RegisterOperand)ecxOperand;
        Assert.Equal(RegisterIndex.C, registerOperand2.Register);
        Assert.Equal(32, registerOperand2.Size); // Validate that it's a 32-bit register (ECX)
    }
    
    /// <summary>
    /// Tests the XchgEaxRegHandler for decoding XCHG EAX, EDX instruction
    /// </summary>
    [Fact]
    public void XchgEaxRegHandler_DecodesXchgEaxEdx_Correctly()
    {
        // Arrange
        // XCHG EAX, EDX (92) - Exchange EAX and EDX
        byte[] codeBuffer = new byte[] { 0x92 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Xchg, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand1 = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand1.Register);
        Assert.Equal(32, registerOperand1.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (EDX)
        var edxOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(edxOperand);
        var registerOperand2 = (RegisterOperand)edxOperand;
        Assert.Equal(RegisterIndex.D, registerOperand2.Register);
        Assert.Equal(32, registerOperand2.Size); // Validate that it's a 32-bit register (EDX)
    }
    
    /// <summary>
    /// Tests the XchgEaxRegHandler for decoding XCHG EAX, EBX instruction
    /// </summary>
    [Fact]
    public void XchgEaxRegHandler_DecodesXchgEaxEbx_Correctly()
    {
        // Arrange
        // XCHG EAX, EBX (93) - Exchange EAX and EBX
        byte[] codeBuffer = new byte[] { 0x93 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Xchg, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand1 = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand1.Register);
        Assert.Equal(32, registerOperand1.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (EBX)
        var ebxOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(ebxOperand);
        var registerOperand2 = (RegisterOperand)ebxOperand;
        Assert.Equal(RegisterIndex.B, registerOperand2.Register);
        Assert.Equal(32, registerOperand2.Size); // Validate that it's a 32-bit register (EBX)
    }
    
    /// <summary>
    /// Tests the XchgEaxRegHandler for decoding XCHG EAX, ESP instruction
    /// </summary>
    [Fact]
    public void XchgEaxRegHandler_DecodesXchgEaxEsp_Correctly()
    {
        // Arrange
        // XCHG EAX, ESP (94) - Exchange EAX and ESP
        byte[] codeBuffer = new byte[] { 0x94 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Xchg, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand1 = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand1.Register);
        Assert.Equal(32, registerOperand1.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (ESP)
        var espOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(espOperand);
        var registerOperand2 = (RegisterOperand)espOperand;
        Assert.Equal(RegisterIndex.Sp, registerOperand2.Register);
        Assert.Equal(32, registerOperand2.Size); // Validate that it's a 32-bit register (ESP)
    }
    
    /// <summary>
    /// Tests the XchgEaxRegHandler for decoding XCHG EAX, EBP instruction
    /// </summary>
    [Fact]
    public void XchgEaxRegHandler_DecodesXchgEaxEbp_Correctly()
    {
        // Arrange
        // XCHG EAX, EBP (95) - Exchange EAX and EBP
        byte[] codeBuffer = new byte[] { 0x95 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Xchg, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand1 = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand1.Register);
        Assert.Equal(32, registerOperand1.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (EBP)
        var ebpOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(ebpOperand);
        var registerOperand2 = (RegisterOperand)ebpOperand;
        Assert.Equal(RegisterIndex.Bp, registerOperand2.Register);
        Assert.Equal(32, registerOperand2.Size); // Validate that it's a 32-bit register (EBP)
    }
    
    /// <summary>
    /// Tests the XchgEaxRegHandler for decoding XCHG EAX, ESI instruction
    /// </summary>
    [Fact]
    public void XchgEaxRegHandler_DecodesXchgEaxEsi_Correctly()
    {
        // Arrange
        // XCHG EAX, ESI (96) - Exchange EAX and ESI
        byte[] codeBuffer = new byte[] { 0x96 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Xchg, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand1 = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand1.Register);
        Assert.Equal(32, registerOperand1.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (ESI)
        var esiOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(esiOperand);
        var registerOperand2 = (RegisterOperand)esiOperand;
        Assert.Equal(RegisterIndex.Si, registerOperand2.Register);
        Assert.Equal(32, registerOperand2.Size); // Validate that it's a 32-bit register (ESI)
    }
    
    /// <summary>
    /// Tests the XchgEaxRegHandler for decoding XCHG EAX, EDI instruction
    /// </summary>
    [Fact]
    public void XchgEaxRegHandler_DecodesXchgEaxEdi_Correctly()
    {
        // Arrange
        // XCHG EAX, EDI (97) - Exchange EAX and EDI
        byte[] codeBuffer = new byte[] { 0x97 };
        var decoder = new InstructionDecoder(codeBuffer, codeBuffer.Length);
        
        // Act
        var instruction = decoder.DecodeInstruction();
        
        // Assert
        Assert.NotNull(instruction);
        Assert.Equal(InstructionType.Xchg, instruction.Type);
        
        // Check that we have two operands
        Assert.Equal(2, instruction.StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instruction.StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand1 = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand1.Register);
        Assert.Equal(32, registerOperand1.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (EDI)
        var ediOperand = instruction.StructuredOperands[1];
        Assert.IsType<RegisterOperand>(ediOperand);
        var registerOperand2 = (RegisterOperand)ediOperand;
        Assert.Equal(RegisterIndex.Di, registerOperand2.Register);
        Assert.Equal(32, registerOperand2.Size); // Validate that it's a 32-bit register (EDI)
    }
    
    /// <summary>
    /// Tests a sequence with NOP instructions
    /// </summary>
    [Fact]
    public void XchgEaxRegHandler_DecodesNopSequence_Correctly()
    {
        // Arrange
        // Multiple NOPs followed by XCHG EAX, ECX
        byte[] codeBuffer = new byte[] { 0x90, 0x90, 0x90, 0x91 };
        var disassembler = new Disassembler(codeBuffer, 0);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.Equal(4, instructions.Count);
        
        // First three instructions should be NOPs
        for (int i = 0; i < 3; i++)
        {
            Assert.Equal(InstructionType.Nop, instructions[i].Type);
            Assert.Empty(instructions[i].StructuredOperands);
        }
        
        // Last instruction should be XCHG EAX, ECX
        Assert.Equal(InstructionType.Xchg, instructions[3].Type);
        
        // Check that we have two operands
        Assert.Equal(2, instructions[3].StructuredOperands.Count);
        
        // Check the first operand (EAX)
        var eaxOperand = instructions[3].StructuredOperands[0];
        Assert.IsType<RegisterOperand>(eaxOperand);
        var registerOperand1 = (RegisterOperand)eaxOperand;
        Assert.Equal(RegisterIndex.A, registerOperand1.Register);
        Assert.Equal(32, registerOperand1.Size); // Validate that it's a 32-bit register (EAX)
        
        // Check the second operand (ECX)
        var ecxOperand = instructions[3].StructuredOperands[1];
        Assert.IsType<RegisterOperand>(ecxOperand);
        var registerOperand2 = (RegisterOperand)ecxOperand;
        Assert.Equal(RegisterIndex.C, registerOperand2.Register);
        Assert.Equal(32, registerOperand2.Size); // Validate that it's a 32-bit register (ECX)
    }
}
