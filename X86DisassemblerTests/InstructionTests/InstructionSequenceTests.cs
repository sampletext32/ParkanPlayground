using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86DisassemblerTests.InstructionTests;

/// <summary>
/// Tests for specific instruction sequences that were problematic
/// </summary>
public class InstructionSequenceTests
{
    /// <summary>
    /// Tests that the disassembler correctly handles the sequence at address 0x10001C4B
    /// </summary>
    [Fact]
    public void Disassembler_HandlesJmpSequence_Correctly()
    {
        // Arrange - This is the sequence from address 0x10001C4B
        byte[] codeBuffer = new byte[] { 0x7D, 0x05, 0x83, 0xC5, 0x18, 0xEB, 0x03, 0x83, 0xC5, 0xB8, 0x8B, 0x56, 0x04 };
        var disassembler = new Disassembler(codeBuffer, 0x10001C4A);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.True(instructions.Count >= 5, $"Expected at least 5 instructions, but got {instructions.Count}");
        
        // First instruction: JGE LAB_10001c51 (JNL is an alternative mnemonic for JGE)
        Assert.True(instructions[0].Type == InstructionType.Jge, 
            $"Expected 'Jge', but got '{instructions[0].Type}'");
        
        // Check the operand (relative offset for jump target)
        var jgeOperand = instructions[0].StructuredOperands[0];
        Assert.IsType<RelativeOffsetOperand>(jgeOperand);
        
        // Second instruction: ADD EBP, 0x18
        Assert.Equal(InstructionType.Add, instructions[1].Type);
        
        // Check the operands
        Assert.Equal(2, instructions[1].StructuredOperands.Count);
        
        // Check the first operand (EBP)
        var ebpOperand = instructions[1].StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ebpOperand);
        var registerOperand = (RegisterOperand)ebpOperand;
        Assert.Equal(RegisterIndex.Bp, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EBP)
        
        // Check the second operand (immediate value)
        var immOperand = instructions[1].StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x18U, immediateOperand.Value);
        
        // Third instruction: JMP LAB_10001c54
        Assert.Equal(InstructionType.Jmp, instructions[2].Type);
        
        // Check the operand (relative offset for jump target)
        var jmpOperand = instructions[2].StructuredOperands[0];
        Assert.IsType<RelativeOffsetOperand>(jmpOperand);
        
        // Fourth instruction: ADD EBP, -0x48
        Assert.Equal(InstructionType.Add, instructions[3].Type);
        
        // Check the operands
        Assert.Equal(2, instructions[3].StructuredOperands.Count);
        
        // Check the first operand (EBP)
        ebpOperand = instructions[3].StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ebpOperand);
        registerOperand = (RegisterOperand)ebpOperand;
        Assert.Equal(RegisterIndex.Bp, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EBP)
        
        // Check the second operand (immediate value)
        immOperand = instructions[3].StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0xFFFFFFB8UL, immediateOperand.Value); // -0x48 sign-extended to 32-bit
        
        // Fifth instruction: MOV EDX, dword ptr [ESI + 0x4]
        Assert.Equal(InstructionType.Mov, instructions[4].Type);
        
        // Check the operands
        Assert.Equal(2, instructions[4].StructuredOperands.Count);
        
        // Check the first operand (EDX)
        var edxOperand = instructions[4].StructuredOperands[0];
        Assert.IsType<RegisterOperand>(edxOperand);
        registerOperand = (RegisterOperand)edxOperand;
        Assert.Equal(RegisterIndex.D, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EDX)
        
        // Check the second operand (memory operand)
        var memOperand = instructions[4].StructuredOperands[1];
        Assert.IsType<DisplacementMemoryOperand>(memOperand);
        var displacementMemoryOperand = (DisplacementMemoryOperand)memOperand;
        Assert.Equal(RegisterIndex.Si, displacementMemoryOperand.BaseRegister);
        Assert.Equal(4, displacementMemoryOperand.Displacement);
        Assert.Equal(32, displacementMemoryOperand.Size); // Validate that it's a 32-bit memory reference
    }
    
    /// <summary>
    /// Tests that the disassembler correctly handles the sequence at address 0x00001C4B
    /// </summary>
    [Fact]
    public void Disassembler_HandlesAddSequence_Correctly()
    {
        // Arrange - This is the sequence from address 0x00001C4B
        byte[] codeBuffer = new byte[] { 0x7d, 0x05, 0x83, 0xC5, 0x18, 0xEB, 0x03, 0x83, 0xC5, 0xB8, 0x8B, 0x56, 0x04, 0x8A, 0x02, 0x8D, 0x4A, 0x18 };
        var disassembler = new Disassembler(codeBuffer, 0x00001C4B);
        
        // Act
        var instructions = disassembler.Disassemble();
        
        // Assert
        Assert.True(instructions.Count >= 7, $"Expected at least 7 instructions, but got {instructions.Count}");
        
        // First instruction should be JGE with relative offset
        Assert.Equal(InstructionType.Jge, instructions[0].Type);
        
        // Check the operand (relative offset for jump target)
        var jgeOperand = instructions[0].StructuredOperands[0];
        Assert.IsType<RelativeOffsetOperand>(jgeOperand);
        
        // Second instruction should be ADD EBP, 0x18
        Assert.Equal(InstructionType.Add, instructions[1].Type);
        
        // Check the operands
        Assert.Equal(2, instructions[1].StructuredOperands.Count);
        
        // Check the first operand (EBP)
        var ebpOperand = instructions[1].StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ebpOperand);
        var registerOperand = (RegisterOperand)ebpOperand;
        Assert.Equal(RegisterIndex.Bp, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EBP)
        
        // Check the second operand (immediate value)
        var immOperand = instructions[1].StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        var immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0x18U, immediateOperand.Value);
        
        // Third instruction should be JMP
        Assert.Equal(InstructionType.Jmp, instructions[2].Type);
        
        // Check the operand (relative offset for jump target)
        var jmpOperand = instructions[2].StructuredOperands[0];
        Assert.IsType<RelativeOffsetOperand>(jmpOperand);
        
        // Fourth instruction should be ADD EBP, -0x48
        Assert.Equal(InstructionType.Add, instructions[3].Type);
        
        // Check the operands
        Assert.Equal(2, instructions[3].StructuredOperands.Count);
        
        // Check the first operand (EBP)
        ebpOperand = instructions[3].StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ebpOperand);
        registerOperand = (RegisterOperand)ebpOperand;
        Assert.Equal(RegisterIndex.Bp, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EBP)
        
        // Check the second operand (immediate value)
        immOperand = instructions[3].StructuredOperands[1];
        Assert.IsType<ImmediateOperand>(immOperand);
        immediateOperand = (ImmediateOperand)immOperand;
        Assert.Equal(0xFFFFFFB8U, immediateOperand.Value); // -0x48 sign-extended to 32-bit
        
        // Fifth instruction should be MOV EDX, dword ptr [ESI+0x4]
        Assert.Equal(InstructionType.Mov, instructions[4].Type);
        
        // Check the operands
        Assert.Equal(2, instructions[4].StructuredOperands.Count);
        
        // Check the first operand (EDX)
        var edxOperand = instructions[4].StructuredOperands[0];
        Assert.IsType<RegisterOperand>(edxOperand);
        registerOperand = (RegisterOperand)edxOperand;
        Assert.Equal(RegisterIndex.D, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (EDX)
        
        // Check the second operand (memory operand)
        var memOperand = instructions[4].StructuredOperands[1];
        Assert.IsType<DisplacementMemoryOperand>(memOperand);
        var displacementMemoryOperand = (DisplacementMemoryOperand)memOperand;
        Assert.Equal(RegisterIndex.Si, displacementMemoryOperand.BaseRegister);
        Assert.Equal(4, displacementMemoryOperand.Displacement);
        Assert.Equal(32, displacementMemoryOperand.Size); // Validate that it's a 32-bit memory reference
        
        // Sixth instruction should be MOV AL, byte ptr [EDX]
        Assert.Equal(InstructionType.Mov, instructions[5].Type);
        
        // Check the operands
        Assert.Equal(2, instructions[5].StructuredOperands.Count);
        
        // Check the first operand (AL)
        var alOperand = instructions[5].StructuredOperands[0];
        Assert.IsType<RegisterOperand>(alOperand);
        registerOperand = (RegisterOperand)alOperand;
        Assert.Equal(RegisterIndex.A, registerOperand.Register);
        Assert.Equal(8, registerOperand.Size); // Validate that it's an 8-bit register (AL)
        
        // Check the second operand (memory operand)
        memOperand = instructions[5].StructuredOperands[1];
        Assert.IsType<BaseRegisterMemoryOperand>(memOperand);
        var baseRegisterMemoryOperand = (BaseRegisterMemoryOperand)memOperand;
        Assert.Equal(RegisterIndex.D, baseRegisterMemoryOperand.BaseRegister);
        Assert.Equal(8, baseRegisterMemoryOperand.Size); // Validate that it's an 8-bit memory reference
        
        // Seventh instruction should be LEA ECX, [EDX+0x18]
        Assert.Equal(InstructionType.Lea, instructions[6].Type);
        
        // Check the operands
        Assert.Equal(2, instructions[6].StructuredOperands.Count);
        
        // Check the first operand (ECX)
        var ecxOperand = instructions[6].StructuredOperands[0];
        Assert.IsType<RegisterOperand>(ecxOperand);
        registerOperand = (RegisterOperand)ecxOperand;
        Assert.Equal(RegisterIndex.C, registerOperand.Register);
        Assert.Equal(32, registerOperand.Size); // Validate that it's a 32-bit register (ECX)
        
        // Check the second operand (memory operand)
        memOperand = instructions[6].StructuredOperands[1];
        Assert.IsType<DisplacementMemoryOperand>(memOperand);
        displacementMemoryOperand = (DisplacementMemoryOperand)memOperand;
        Assert.Equal(RegisterIndex.D, displacementMemoryOperand.BaseRegister);
        Assert.Equal(0x18, displacementMemoryOperand.Displacement);
        Assert.Equal(32, displacementMemoryOperand.Size); // Validate that it's a 32-bit memory reference
    }
}
