using X86Disassembler.Analysers.DecompilerTypes;
using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86Disassembler.Analysers;

/// <summary>
/// Analyzes control flow structures in disassembled code
/// </summary>
public class ControlFlowAnalyzer
{
    /// <summary>
    /// The analyzer context
    /// </summary>
    private readonly AnalyzerContext _context;
    
    /// <summary>
    /// Creates a new control flow analyzer
    /// </summary>
    /// <param name="context">The analyzer context</param>
    public ControlFlowAnalyzer(AnalyzerContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Analyzes the control flow of a function to identify high-level structures
    /// </summary>
    /// <param name="function">The function to analyze</param>
    public void AnalyzeControlFlow(Function function)
    {
        // First, identify if-else structures
        IdentifyIfElseStructures(function);
        
        // Then, identify switch statements
        IdentifySwitchStatements(function);
    }
    
    /// <summary>
    /// Identifies if-else structures in the control flow graph
    /// </summary>
    /// <param name="function">The function to analyze</param>
    private void IdentifyIfElseStructures(Function function)
    {
        // Now analyze each block for conditional jumps
        foreach (var block in function.AsmFunction.Blocks)
        {
            // Get the last instruction in the block
            var lastInstruction = block.Instructions.LastOrDefault();
            if (lastInstruction == null) continue;
            
            // Check if the last instruction is a conditional jump
            if (lastInstruction.Type.IsConditionalJump())
            {
                // Get the jump target address
                ulong targetAddress = GetJumpTargetAddress(lastInstruction);
                
                // Find the target block
                InstructionBlock? targetBlock = null;
                foreach (var b in function.AsmFunction.Blocks)
                {
                    if (b.Address == targetAddress)
                    {
                        targetBlock = b;
                        break;
                    }
                }
                
                if (targetBlock == null)
                {
                    continue;
                }
                
                // Find the fall-through block (should be in the successors)
                InstructionBlock? fallThroughBlock = null;
                foreach (var successor in block.Successors)
                {
                    if (successor != targetBlock)
                    {
                        fallThroughBlock = successor;
                        break;
                    }
                }
                
                if (fallThroughBlock == null)
                {
                    continue;
                }
                    
                // Create an if-else structure
                var ifElseStructure = new IfElseStructure
                {
                    ConditionBlock = block,
                    ThenBlock = targetBlock,
                    ElseBlock = fallThroughBlock
                };
                
                // Store the if-else structure in the analysis context
                function.AsmFunction.Context.StoreAnalysisData(block.Address, "IfElseStructure", ifElseStructure);
            }
        }
        
        // Second pass: identify nested if-else structures
        foreach (var block in function.AsmFunction.Blocks)
        {
            var ifElseStructure = _context.GetAnalysisData<IfElseStructure>(block.Address, "IfElseStructure");
            if (ifElseStructure != null)
            {
                // Check if the 'then' block contains another if-else structure
                var nestedThenIf = _context.GetAnalysisData<IfElseStructure>(ifElseStructure.ThenBlock.Address, "IfElseStructure");
                if (nestedThenIf != null)
                {
                    ifElseStructure.NestedThenStructure = nestedThenIf;
                }
                
                // Check if the 'else' block contains another if-else structure
                if (ifElseStructure.ElseBlock != null)
                {
                    var nestedElseIf = _context.GetAnalysisData<IfElseStructure>(ifElseStructure.ElseBlock.Address, "IfElseStructure");
                    if (nestedElseIf != null)
                    {
                        ifElseStructure.NestedElseStructure = nestedElseIf;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Identifies switch statements in the control flow graph
    /// </summary>
    /// <param name="function">The function to analyze</param>
    private void IdentifySwitchStatements(Function function)
    {
        // For each block in the function
        foreach (var block in function.AsmFunction.Blocks)
        {
            // Look for patterns that indicate a switch statement
            // Common patterns include:
            // 1. A series of compare and jump instructions
            // 2. An indirect jump through a jump table
            
            // For now, we'll focus on the first pattern (series of compares)
            if (IsPotentialSwitchHeader(block))
            {
                // This is a potential switch statement
                var switchStructure = new SwitchStructure
                {
                    HeaderBlock = block,
                    Cases = []
                };
                
                // Find the cases by analyzing the successors
                foreach (var successor in block.Successors)
                {
                    // Each successor is a potential case
                    switchStructure.Cases.Add(new SwitchCase
                    {
                        CaseBlock = successor,
                        Value = 0 // We'd need more analysis to determine the actual value
                    });
                }
                
                // Store the switch structure in the context
                _context.StoreAnalysisData(block.Address, "SwitchStructure", switchStructure);
            }
        }
    }
    
    /// <summary>
    /// Gets the target address of a jump instruction
    /// </summary>
    /// <param name="instruction">The jump instruction</param>
    /// <returns>The target address of the jump</returns>
    private ulong GetJumpTargetAddress(Instruction instruction)
    {
        // Add debug output to see the instruction and its operands
        
        // For conditional jumps, the target address is the first operand
        if (instruction.StructuredOperands.Count > 0)
        {
            var operand = instruction.StructuredOperands[0];
            
            if (operand is ImmediateOperand immOp)
            {
                return (ulong)immOp.Value;
            }
            else if (operand is RelativeOffsetOperand relOp)
            {
                // For relative jumps, the target address is directly available in the operand
                // We need to convert from file offset to RVA by adding 0x1000 (the section offset)
                // This matches how the blocks are converted in BlockDisassembler.cs
                ulong rvaTargetAddress = relOp.TargetAddress + 0x1000;
                return rvaTargetAddress;
            }
        }
        
        // If we can't determine the target, return 0
        return 0;
    }
    
    /// <summary>
    /// Checks if the given block is a potential switch statement header
    /// </summary>
    /// <param name="block">The block to check</param>
    /// <returns>True if the block is a potential switch header, false otherwise</returns>
    private bool IsPotentialSwitchHeader(InstructionBlock block)
    {
        // A switch header typically has multiple successors
        if (block.Successors.Count <= 2)
        {
            return false;
        }
        
        // Look for patterns that indicate a switch statement
        // For now, we'll just check if the block ends with an indirect jump
        if (block.Instructions.Count > 0)
        {
            var lastInstruction = block.Instructions[^1];
            if (lastInstruction.Type == InstructionType.Jmp && 
                lastInstruction.StructuredOperands.Count > 0 && 
                !(lastInstruction.StructuredOperands[0] is ImmediateOperand))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Represents an if-else structure in the control flow graph
    /// </summary>
    public class IfElseStructure
    {
        /// <summary>
        /// The block containing the condition
        /// </summary>
        public InstructionBlock ConditionBlock { get; set; } = null!;
        
        /// <summary>
        /// The block representing the 'then' branch (taken when condition is true)
        /// </summary>
        public InstructionBlock ThenBlock { get; set; } = null!;
        
        /// <summary>
        /// The block representing the 'else' branch (taken when condition is false)
        /// </summary>
        public InstructionBlock? ElseBlock { get; set; }
        
        /// <summary>
        /// The block where both branches merge back together (if applicable)
        /// </summary>
        public InstructionBlock? MergeBlock { get; set; }
        
        /// <summary>
        /// Whether this is a complete if-else structure with a merge point
        /// </summary>
        public bool IsComplete { get; set; }
        
        /// <summary>
        /// Nested if-else structure in the 'then' branch (if any)
        /// </summary>
        public IfElseStructure? NestedThenStructure { get; set; }
        
        /// <summary>
        /// Nested if-else structure in the 'else' branch (if any)
        /// </summary>
        public IfElseStructure? NestedElseStructure { get; set; }
    }
    
    /// <summary>
    /// Represents a switch statement in the control flow graph
    /// </summary>
    public class SwitchStructure
    {
        /// <summary>
        /// The block containing the switch header
        /// </summary>
        public InstructionBlock HeaderBlock { get; set; } = null!;
        
        /// <summary>
        /// The cases of the switch statement
        /// </summary>
        public List<SwitchCase> Cases { get; set; } = [];
    }
    
    /// <summary>
    /// Represents a case in a switch statement
    /// </summary>
    public class SwitchCase
    {
        /// <summary>
        /// The value of the case
        /// </summary>
        public int Value { get; set; }
        
        /// <summary>
        /// The block containing the case code
        /// </summary>
        public InstructionBlock CaseBlock { get; set; } = null!;
    }
}
