using X86Disassembler.Analysers.DecompilerTypes;
using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86Disassembler.Analysers;

/// <summary>
/// Analyzes disassembled code to identify and track variables
/// </summary>
public class VariableAnalyzer
{
    /// <summary>
    /// The analyzer context
    /// </summary>
    private readonly AnalyzerContext _context;
    
    /// <summary>
    /// Creates a new variable analyzer
    /// </summary>
    /// <param name="context">The analyzer context</param>
    public VariableAnalyzer(AnalyzerContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Analyzes the function to identify stack variables
    /// </summary>
    /// <param name="function">The function to analyze</param>
    public void AnalyzeStackVariables(Function function)
    {
        // Dictionary to track stack offsets and their corresponding variables
        var stackOffsets = new Dictionary<int, Variable>();
        
        // First, identify the function prologue to determine stack frame setup
        bool hasPushEbp = false;
        bool hasMovEbpEsp = false;
        int localSize = 0;
        
        // Look for the function prologue pattern: push ebp; mov ebp, esp; sub esp, X
        foreach (var block in function.AsmFunction.Blocks)
        {
            foreach (var instruction in block.Instructions)
            {
                // Look for push ebp
                if (instruction.Type == InstructionType.Push &&
                    instruction.StructuredOperands.Count > 0 &&
                    instruction.StructuredOperands[0] is RegisterOperand regOp &&
                    regOp.Register == RegisterIndex.Bp)
                {
                    hasPushEbp = true;
                    continue;
                }
                
                // Look for mov ebp, esp
                if (instruction.Type == InstructionType.Mov &&
                    instruction.StructuredOperands.Count > 1 &&
                    instruction.StructuredOperands[0] is RegisterOperand destReg &&
                    instruction.StructuredOperands[1] is RegisterOperand srcReg &&
                    destReg.Register == RegisterIndex.Bp &&
                    srcReg.Register == RegisterIndex.Sp)
                {
                    hasMovEbpEsp = true;
                    continue;
                }
                
                // Look for sub esp, X to determine local variable space
                if (instruction.Type == InstructionType.Sub &&
                    instruction.StructuredOperands.Count > 1 &&
                    instruction.StructuredOperands[0] is RegisterOperand subReg &&
                    instruction.StructuredOperands[1] is ImmediateOperand immOp &&
                    subReg.Register == RegisterIndex.Sp)
                {
                    localSize = (int)immOp.Value;
                    break;
                }
            }
            
            // If we found the complete prologue, no need to check more blocks
            if (hasPushEbp && hasMovEbpEsp && localSize > 0)
            {
                break;
            }
        }
        
        // If we didn't find a standard prologue, we can't reliably analyze stack variables
        if (!hasPushEbp || !hasMovEbpEsp)
        {
            return;
        }
        
        // Now scan for memory accesses relative to EBP
        foreach (var block in function.AsmFunction.Blocks)
        {
            foreach (var instruction in block.Instructions)
            {
                // Look for memory operands that reference [ebp+X] or [ebp-X]
                foreach (var operand in instruction.StructuredOperands)
                {
                    if (operand is DisplacementMemoryOperand memOp && 
                        memOp.BaseRegister == RegisterIndex.Bp)
                    {
                        // This is accessing memory relative to EBP
                        int offset = (int)memOp.Displacement;
                        
                        // Determine if this is a parameter or local variable
                        if (offset > 0 && offset < 1000) // Positive offset = parameter (with reasonable limit)
                        {
                            // Parameters start at [ebp+8] (return address at [ebp+4], saved ebp at [ebp+0])
                            int paramIndex = (offset - 8) / 4; // Assuming 4-byte parameters
                            
                            // Make sure we have enough parameters in the function
                            while (function.Parameters.Count <= paramIndex)
                            {
                                var param = new Variable($"param_{function.Parameters.Count + 1}", DataType.Unknown)
                                {
                                    Storage = Variable.StorageType.Parameter,
                                    StackOffset = 8 + (function.Parameters.Count * 4),
                                    IsParameter = true,
                                    ParameterIndex = function.Parameters.Count,
                                    Size = 4 // Assume 4 bytes (32-bit)
                                };
                                function.Parameters.Add(param);
                            }
                        }
                        else if (offset < 0 && offset > -1000) // Negative offset = local variable (with reasonable limit)
                        {
                            // Check if we've already seen this offset
                            if (!stackOffsets.TryGetValue(offset, out var variable))
                            {
                                // Create a new local variable
                                variable = new Variable($"local_{Math.Abs(offset)}", DataType.Unknown)
                                {
                                    Storage = Variable.StorageType.Stack,
                                    StackOffset = offset,
                                    Size = 4 // Assume 4 bytes (32-bit)
                                };
                                
                                // Add to our tracking dictionaries
                                stackOffsets[offset] = variable;
                                function.LocalVariables.Add(variable);
                            }
                            
                            // Track the usage of this variable
                            TrackVariableUsage(variable, instruction);
                        }
                    }
                }
            }
        }
        
        // Analyze register-based variables
        AnalyzeRegisterVariables(function);
    }
    
    /// <summary>
    /// Analyzes register usage to identify variables stored in registers
    /// </summary>
    /// <param name="function">The function to analyze</param>
    private void AnalyzeRegisterVariables(Function function)
    {
        // This is a more complex analysis that would track register values across blocks
        // For now, we'll focus on identifying registers that hold consistent values
        
        // Dictionary to track register variables
        var registerVariables = new Dictionary<RegisterIndex, Variable>();
        
        // For each block, analyze register usage
        foreach (var block in function.AsmFunction.Blocks)
        {
            // Check if we have register values for this block from data flow analysis
            var registerValuesKey = "RegisterValues";
            if (_context.GetAnalysisData<Dictionary<RegisterIndex, DataFlowAnalyzer.ValueInfo>>(block.Address, registerValuesKey) is Dictionary<RegisterIndex, DataFlowAnalyzer.ValueInfo> registerValues)
            {
                foreach (var kvp in registerValues)
                {
                    var register = kvp.Key;
                    var valueInfo = kvp.Value;
                    
                    // Skip special registers like ESP and EBP
                    if (register == RegisterIndex.Sp || register == RegisterIndex.Bp)
                    {
                        continue;
                    }
                    
                    // If the register holds a constant value, it might be a variable
                    if (valueInfo.Type == DataFlowAnalyzer.ValueInfo.ValueType.Constant)
                    {
                        // Check if we already have a variable for this register
                        if (!registerVariables.TryGetValue(register, out var variable))
                        {
                            // Create a new register variable
                            variable = new Variable($"reg_{RegisterMapper.GetRegisterName(register, 32)}", DataType.Unknown)
                            {
                                Storage = Variable.StorageType.Register,
                                Register = register,
                                Size = 4 // Assume 4 bytes (32-bit)
                            };
                            
                            // Add to our tracking dictionary
                            registerVariables[register] = variable;
                            function.RegisterVariables.Add(variable);
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Tracks how a variable is used in an instruction
    /// </summary>
    /// <param name="variable">The variable to track</param>
    /// <param name="instruction">The instruction using the variable</param>
    private void TrackVariableUsage(Variable variable, Instruction instruction)
    {
        // For now, we'll just try to infer the variable type based on its usage
        
        // If the variable is used in a comparison with 0, it might be a boolean
        if (instruction.Type == InstructionType.Cmp || instruction.Type == InstructionType.Test)
        {
            if (instruction.StructuredOperands.Count > 1 && 
                instruction.StructuredOperands[1] is ImmediateOperand immOp && 
                immOp.Value == 0)
            {
                // This might be a boolean check
                if (variable.Type == DataType.Unknown)
                {
                    // Set to int for now as we don't have a bool type
                    variable.Type = DataType.Int;
                }
            }
        }
        
        // If the variable is used with string instructions, it might be a string
        // Check for string operations - we don't have specific string instruction types yet
        // Skip string detection for now as we don't have the specific instruction types
        // We'll detect strings through other means later
        
        // If the variable is used with floating-point instructions, it might be a float
        // Check for floating-point operations
        if (instruction.Type == InstructionType.Fld || 
            instruction.Type == InstructionType.Fst || 
            instruction.Type == InstructionType.Fstp)
        {
            if (variable.Type == DataType.Unknown)
            {
                variable.Type = DataType.Float;
            }
        }
    }
}
