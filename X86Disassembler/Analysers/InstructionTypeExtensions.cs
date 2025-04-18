using X86Disassembler.X86;

namespace X86Disassembler.Analysers;

public static class InstructionTypeExtensions
{
    public static bool IsConditionalJump(this InstructionType type)
    {
        return type switch
        {
            InstructionType.Jg => true,
            InstructionType.Jge => true,
            InstructionType.Jl => true,
            InstructionType.Jle => true,
            InstructionType.Ja => true,
            InstructionType.Jae => true,
            InstructionType.Jb => true,
            InstructionType.Jbe => true,
            InstructionType.Jz => true,
            InstructionType.Jnz => true,
            InstructionType.Jo => true,
            InstructionType.Jno => true,
            InstructionType.Js => true,
            InstructionType.Jns => true,
            InstructionType.Jp => true,
            InstructionType.Jnp => true,
            _ => false
        };
    }

    public static bool IsRegularJump(this InstructionType type)
    {
        return type == InstructionType.Jmp;
    }

    public static bool IsRet(this InstructionType type)
    {
        return type is InstructionType.Ret or InstructionType.Retf;
    }
}