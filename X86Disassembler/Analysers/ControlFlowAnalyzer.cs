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
        // For each block in the function
        foreach (var block in function.AsmFunction.Blocks)
        {
            // Skip blocks that don't end with a conditional jump
            if (block.Instructions.Count == 0)
            {
                continue;
            }
            
            var lastInstruction = block.Instructions[^1];
            
            // Look for conditional jumps (Jcc instructions)
            if (IsConditionalJump(lastInstruction.Type))
            {
                // This is a potential if-then-else structure
                // The true branch is the target of the jump
                // The false branch is the fallthrough block
                
                // Get the jump target address
                ulong targetAddress = GetJumpTargetAddress(lastInstruction);
                
                // Find the target block
                if (_context.BlocksByAddress.TryGetValue(targetAddress, out var targetBlock))
                {
                    // Find the fallthrough block (the block that follows this one in memory)
                    var fallthroughBlock = FindFallthroughBlock(block);
                    
                    if (fallthroughBlock != null)
                    {                        
                        // Store the if-else structure in the context
                        var ifElseStructure = new IfElseStructure
                        {
                            ConditionBlock = block,
                            ThenBlock = targetBlock,
                            ElseBlock = fallthroughBlock
                        };
                        
                        _context.StoreAnalysisData(block.Address, "IfElseStructure", ifElseStructure);
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
    /// Checks if the given instruction type is a conditional jump
    /// </summary>
    /// <param name="type">The instruction type</param>
    /// <returns>True if the instruction is a conditional jump, false otherwise</returns>
    private bool IsConditionalJump(InstructionType type)
    {
        // Check for common conditional jumps
        return type == InstructionType.Jz || 
               type == InstructionType.Jnz || 
               type == InstructionType.Jg || 
               type == InstructionType.Jge || 
               type == InstructionType.Jl || 
               type == InstructionType.Jle || 
               type == InstructionType.Ja || 
               type == InstructionType.Jae || 
               type == InstructionType.Jb || 
               type == InstructionType.Jbe || 
               type == InstructionType.Jo || 
               type == InstructionType.Jno || 
               type == InstructionType.Js || 
               type == InstructionType.Jns || 
               type == InstructionType.Jp || 
               type == InstructionType.Jnp;
    }
    
    /// <summary>
    /// Gets the target address of a jump instruction
    /// </summary>
    /// <param name="instruction">The jump instruction</param>
    /// <returns>The target address of the jump</returns>
    private ulong GetJumpTargetAddress(Instruction instruction)
    {
        // The target address is usually the first operand of the jump instruction
        if (instruction.StructuredOperands.Count > 0 && 
            instruction.StructuredOperands[0] is ImmediateOperand immOp)
        {
            return (ulong)immOp.Value;
        }
        
        // If we can't determine the target, return 0
        return 0;
    }
    
    /// <summary>
    /// Finds the fallthrough block for a given block
    /// </summary>
    /// <param name="block">The block to find the fallthrough for</param>
    /// <returns>The fallthrough block, or null if none found</returns>
    private InstructionBlock? FindFallthroughBlock(InstructionBlock block)
    {
        // The fallthrough block is the one that follows this one in memory
        // It should be a successor of this block
        foreach (var successor in block.Successors)
        {
            // Check if this successor is the fallthrough block
            // (its address should be immediately after this block)
            if (successor.Address > block.Address)
            {
                return successor;
            }
        }
        
        return null;
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
        /// The block containing the 'then' branch
        /// </summary>
        public InstructionBlock ThenBlock { get; set; } = null!;
        
        /// <summary>
        /// The block containing the 'else' branch (may be null for if-then structures)
        /// </summary>
        public InstructionBlock ElseBlock { get; set; } = null!;
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
