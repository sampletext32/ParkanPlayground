using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86Disassembler.Analysers;

/// <summary>
/// Analyzes data flow through instructions to track register values
/// </summary>
public class DataFlowAnalyzer
{
    // Constants for analysis data keys
    private const string REGISTER_VALUE_KEY = "RegisterValue";
    private const string MEMORY_VALUE_KEY = "MemoryValue";

    /// <summary>
    /// Represents a known value for a register or memory location
    /// </summary>
    public class ValueInfo
    {
        /// <summary>
        /// The type of value (constant, register, memory, unknown)
        /// </summary>
        public enum ValueType
        {
            Unknown,
            Constant,
            Register,
            Memory
        }

        /// <summary>
        /// The type of this value
        /// </summary>
        public ValueType Type { get; set; } = ValueType.Unknown;

        /// <summary>
        /// The constant value (if Type is Constant)
        /// </summary>
        public ulong? ConstantValue { get; set; }

        /// <summary>
        /// The source register (if Type is Register)
        /// </summary>
        public RegisterIndex? SourceRegister { get; set; }

        /// <summary>
        /// The memory address or expression (if Type is Memory)
        /// </summary>
        public string? MemoryExpression { get; set; }

        /// <summary>
        /// The instruction that defined this value
        /// </summary>
        public Instruction? DefiningInstruction { get; set; }

        /// <summary>
        /// Returns a string representation of the value
        /// </summary>
        public override string ToString()
        {
            return Type switch
            {
                ValueType.Constant => $"0x{ConstantValue:X8}",
                ValueType.Register => $"{SourceRegister}",
                ValueType.Memory => $"[{MemoryExpression}]",
                _ => "unknown"
            };
        }
    }

    /// <summary>
    /// Analyzes data flow in the function and stores results in the analyzer context
    /// </summary>
    /// <param name="context">The analyzer context to store results in</param>
    public void AnalyzeDataFlow(AnalyzerContext context)
    {
        // Process each block in order
        foreach (var block in context.Function.Blocks)
        {
            // Dictionary to track register values within this block
            Dictionary<RegisterIndex, ValueInfo> registerValues = new();

            // Process each instruction in the block
            foreach (var instruction in block.Instructions)
            {
                // Process the instruction based on its type
                ProcessInstruction(instruction, registerValues, context);

                // Store the current register state at this instruction's address
                StoreRegisterState(instruction.Address, registerValues, context);
            }
        }
    }

    /// <summary>
    /// Processes an instruction to update register values
    /// </summary>
    /// <param name="instruction">The instruction to process</param>
    /// <param name="registerValues">The current register values</param>
    /// <param name="context">The analyzer context</param>
    private void ProcessInstruction(Instruction instruction, Dictionary<RegisterIndex, ValueInfo> registerValues, AnalyzerContext context)
    {
        // Handle different instruction types
        switch (instruction.Type)
        {
            // MOV instructions
            case InstructionType.Mov:
                ProcessMovInstruction(instruction, registerValues);
                break;

            // XOR instructions
            case InstructionType.Xor:
                ProcessXorInstruction(instruction, registerValues);
                break;

            // ADD instructions
            case InstructionType.Add:
                ProcessAddInstruction(instruction, registerValues);
                break;

            // SUB instructions
            case InstructionType.Sub:
                ProcessSubInstruction(instruction, registerValues);
                break;

            // PUSH/POP instructions can affect register values
            case InstructionType.Pop:
                ProcessPopInstruction(instruction, registerValues);
                break;

            // Call instructions typically clobber certain registers
            case InstructionType.Call:
                ProcessCallInstruction(instruction, registerValues);
                break;

            // Other instructions that modify registers
            default:
                // For now, mark destination registers as unknown for unsupported instructions
                if (instruction.StructuredOperands.Count > 0 &&
                    instruction.StructuredOperands[0] is RegisterOperand regOp)
                {
                    registerValues[regOp.Register] = new ValueInfo
                    {
                        Type = ValueInfo.ValueType.Unknown,
                        DefiningInstruction = instruction
                    };
                }

                break;
        }
    }

    /// <summary>
    /// Processes a MOV instruction to update register values
    /// </summary>
    private void ProcessMovInstruction(Instruction instruction, Dictionary<RegisterIndex, ValueInfo> registerValues)
    {
        // Handle different MOV variants
        if (instruction.StructuredOperands.Count >= 2)
        {
            var dest = instruction.StructuredOperands[0];
            var src = instruction.StructuredOperands[1];

            // MOV reg, imm
            if (dest is RegisterOperand destReg && src is ImmediateOperand immSrc)
            {
                registerValues[destReg.Register] = new ValueInfo
                {
                    Type = ValueInfo.ValueType.Constant,
                    ConstantValue = immSrc.Value,
                    DefiningInstruction = instruction
                };
            }
            // MOV reg, reg
            else if (dest is RegisterOperand destReg2 && src is RegisterOperand srcReg)
            {
                if (registerValues.TryGetValue(srcReg.Register, out var srcValue))
                {
                    // Copy the source value
                    registerValues[destReg2.Register] = new ValueInfo
                    {
                        Type = srcValue.Type,
                        ConstantValue = srcValue.ConstantValue,
                        SourceRegister = srcValue.SourceRegister,
                        MemoryExpression = srcValue.MemoryExpression,
                        DefiningInstruction = instruction
                    };
                }
                else
                {
                    // Source register value is unknown
                    registerValues[destReg2.Register] = new ValueInfo
                    {
                        Type = ValueInfo.ValueType.Register,
                        SourceRegister = srcReg.Register,
                        DefiningInstruction = instruction
                    };
                }
            }
            // MOV reg, [mem]
            else if (dest is RegisterOperand destReg3 && src is MemoryOperand memSrc)
            {
                registerValues[destReg3.Register] = new ValueInfo
                {
                    Type = ValueInfo.ValueType.Memory,
                    MemoryExpression = memSrc.ToString(),
                    DefiningInstruction = instruction
                };
            }
            // MOV [mem], reg or MOV [mem], imm
            // These don't update register values, so we don't need to handle them here
        }
    }

    /// <summary>
    /// Processes an XOR instruction to update register values
    /// </summary>
    private void ProcessXorInstruction(Instruction instruction, Dictionary<RegisterIndex, ValueInfo> registerValues)
    {
        // Handle XOR reg, reg (often used for zeroing a register)
        if (instruction.StructuredOperands.Count >= 2)
        {
            var dest = instruction.StructuredOperands[0];
            var src = instruction.StructuredOperands[1];

            // XOR reg, same_reg (zeroing idiom)
            if (dest is RegisterOperand destReg && src is RegisterOperand srcReg &&
                destReg.Register == srcReg.Register)
            {
                registerValues[destReg.Register] = new ValueInfo
                {
                    Type = ValueInfo.ValueType.Constant,
                    ConstantValue = 0,
                    DefiningInstruction = instruction
                };
            }
            // Other XOR operations make the result unknown
            else if (dest is RegisterOperand destReg2)
            {
                registerValues[destReg2.Register] = new ValueInfo
                {
                    Type = ValueInfo.ValueType.Unknown,
                    DefiningInstruction = instruction
                };
            }
        }
    }

    /// <summary>
    /// Processes an ADD instruction to update register values
    /// </summary>
    private void ProcessAddInstruction(Instruction instruction, Dictionary<RegisterIndex, ValueInfo> registerValues)
    {
        // Handle ADD reg, imm where we know the register value
        if (instruction.StructuredOperands.Count >= 2)
        {
            var dest = instruction.StructuredOperands[0];
            var src = instruction.StructuredOperands[1];

            // ADD reg, imm where reg is a known constant
            if (dest is RegisterOperand destReg && src is ImmediateOperand immSrc &&
                registerValues.TryGetValue(destReg.Register, out var destValue) &&
                destValue.Type == ValueInfo.ValueType.Constant &&
                destValue.ConstantValue.HasValue)
            {
                // Calculate the new constant value
                registerValues[destReg.Register] = new ValueInfo
                {
                    Type = ValueInfo.ValueType.Constant,
                    ConstantValue = (uint?) (destValue.ConstantValue.Value + immSrc.Value),
                    DefiningInstruction = instruction
                };
            }
            // Other ADD operations make the result unknown
            else if (dest is RegisterOperand destReg2)
            {
                registerValues[destReg2.Register] = new ValueInfo
                {
                    Type = ValueInfo.ValueType.Unknown,
                    DefiningInstruction = instruction
                };
            }
        }
    }

    /// <summary>
    /// Processes a SUB instruction to update register values
    /// </summary>
    private void ProcessSubInstruction(Instruction instruction, Dictionary<RegisterIndex, ValueInfo> registerValues)
    {
        // Handle SUB reg, imm where we know the register value
        if (instruction.StructuredOperands.Count >= 2)
        {
            var dest = instruction.StructuredOperands[0];
            var src = instruction.StructuredOperands[1];

            // SUB reg, imm where reg is a known constant
            if (dest is RegisterOperand destReg && src is ImmediateOperand immSrc &&
                registerValues.TryGetValue(destReg.Register, out var destValue) &&
                destValue.Type == ValueInfo.ValueType.Constant &&
                destValue.ConstantValue.HasValue)
            {
                // Calculate the new constant value
                registerValues[destReg.Register] = new ValueInfo
                {
                    Type = ValueInfo.ValueType.Constant,
                    ConstantValue = (uint?) (destValue.ConstantValue.Value - immSrc.Value),
                    DefiningInstruction = instruction
                };
            }
            // Other SUB operations make the result unknown
            else if (dest is RegisterOperand destReg2)
            {
                registerValues[destReg2.Register] = new ValueInfo
                {
                    Type = ValueInfo.ValueType.Unknown,
                    DefiningInstruction = instruction
                };
            }
        }
    }

    /// <summary>
    /// Processes a POP instruction to update register values
    /// </summary>
    private void ProcessPopInstruction(Instruction instruction, Dictionary<RegisterIndex, ValueInfo> registerValues)
    {
        // POP reg makes the register value unknown (comes from stack)
        if (instruction.StructuredOperands.Count >= 1 &&
            instruction.StructuredOperands[0] is RegisterOperand destReg)
        {
            registerValues[destReg.Register] = new ValueInfo
            {
                Type = ValueInfo.ValueType.Unknown,
                DefiningInstruction = instruction
            };
        }
    }

    /// <summary>
    /// Processes a CALL instruction to update register values
    /// </summary>
    private void ProcessCallInstruction(Instruction instruction, Dictionary<RegisterIndex, ValueInfo> registerValues)
    {
        // CALL instructions typically clobber EAX, ECX, and EDX in x86 calling conventions
        registerValues[RegisterIndex.A] = new ValueInfo
        {
            Type = ValueInfo.ValueType.Unknown,
            DefiningInstruction = instruction
        };

        registerValues[RegisterIndex.C] = new ValueInfo
        {
            Type = ValueInfo.ValueType.Unknown,
            DefiningInstruction = instruction
        };

        registerValues[RegisterIndex.D] = new ValueInfo
        {
            Type = ValueInfo.ValueType.Unknown,
            DefiningInstruction = instruction
        };
    }

    /// <summary>
    /// Stores the current register state at the given address
    /// </summary>
    private void StoreRegisterState(ulong address, Dictionary<RegisterIndex, ValueInfo> registerValues, AnalyzerContext context)
    {
        // Create a copy of the register values to store
        var registerValuesCopy = new Dictionary<RegisterIndex, ValueInfo>(registerValues);

        // Store in the context
        context.StoreAnalysisData(address, REGISTER_VALUE_KEY, registerValuesCopy);
    }
    
    /// <summary>
    /// Gets the register values at the given address
    /// </summary>
    public static Dictionary<string, ValueInfo>? GetRegisterValues(ulong address, AnalyzerContext context)
    {
        return context.GetAnalysisData<Dictionary<string, ValueInfo>>(address, REGISTER_VALUE_KEY);
    }
}