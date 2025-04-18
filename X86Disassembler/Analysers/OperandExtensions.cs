using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86Disassembler.Analysers;

public static class OperandExtensions
{
    public static uint GetValue(this Operand operand)
    {
        return operand switch
        {
            RelativeOffsetOperand roo => roo.TargetAddress,
            _ => 0
        };
    }
}