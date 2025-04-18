using System.Text;
using X86Disassembler.Analysers.DecompilerTypes;
using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86Disassembler.Analysers;

/// <summary>
/// Generates C-like pseudocode from decompiled functions
/// </summary>
public class PseudocodeGenerator
{
    /// <summary>
    /// Generates pseudocode for a decompiled function
    /// </summary>
    /// <param name="function">The function to generate pseudocode for</param>
    /// <returns>The generated pseudocode</returns>
    public string GeneratePseudocode(Function function)
    {
        var result = new StringBuilder();
        
        // Add function signature
        result.AppendLine($"{function.ReturnType} {function.Name}({string.Join(", ", function.Parameters.Select(p => $"{p.Type} {p.Name}"))})")
              .AppendLine("{");
        
        // Add local variable declarations
        foreach (var localVar in function.LocalVariables)
        {
            result.AppendLine($"    {localVar.Type} {localVar.Name}; // Stack offset: {localVar.StackOffset}");
        }
        
        // Add register variable declarations
        foreach (var regVar in function.RegisterVariables)
        {
            result.AppendLine($"    {regVar.Type} {regVar.Name}; // Register: {RegisterMapper.GetRegisterName(regVar.Register!.Value, 32)}");
        }
        
        if (function.LocalVariables.Count > 0 || function.RegisterVariables.Count > 0)
        {
            result.AppendLine();
        }
        
        // Generate the function body using control flow analysis
        GenerateFunctionBody(function, result, 1);
        
        // Add a return statement
        result.AppendLine()
              .AppendLine("    return 0; // Placeholder return value")
              .AppendLine("}");
        
        return result.ToString();
    }
    
    /// <summary>
    /// Generates the body of the function using control flow analysis
    /// </summary>
    /// <param name="function">The function to generate code for</param>
    /// <param name="result">The string builder to append to</param>
    /// <param name="indentLevel">The current indentation level</param>
    private void GenerateFunctionBody(Function function, StringBuilder result, int indentLevel)
    {
        // Try to find the entry block
        var entryBlock = function.AsmFunction.EntryBlock;
        
        // If the entry block is not found, try to find a block with an address that matches the function address minus the base address
        if (entryBlock == null && function.AsmFunction.Blocks.Count > 0)
        {
            // Get the first block as a fallback
            entryBlock = function.AsmFunction.Blocks[0];
            
            // Log a warning but continue with the first block
            result.AppendLine($"{new string(' ', indentLevel * 4)}// Warning: Entry block not found at address 0x{function.Address:X8}, using first block at 0x{entryBlock.Address:X8}");
        }
        else if (entryBlock == null)
        {
            result.AppendLine($"{new string(' ', indentLevel * 4)}// Function body could not be decompiled - no blocks found");
            return;
        }
        
        // Process blocks in order, starting from the entry block
        var processedBlocks = new HashSet<ulong>();
        GenerateBlockCode(function, entryBlock, result, indentLevel, processedBlocks);
    }
    
    /// <summary>
    /// Generates code for a basic block and its successors
    /// </summary>
    /// <param name="function">The function containing the block</param>
    /// <param name="block">The block to generate code for</param>
    /// <param name="result">The string builder to append to</param>
    /// <param name="indentLevel">The current indentation level</param>
    /// <param name="processedBlocks">Set of blocks that have already been processed</param>
    private void GenerateBlockCode(Function function, InstructionBlock block, StringBuilder result, int indentLevel, HashSet<ulong> processedBlocks)
    {
        // Check if we've already processed this block
        if (processedBlocks.Contains(block.Address))
        {
            return;
        }
        
        // Mark this block as processed
        processedBlocks.Add(block.Address);
        
        // Check if this block is part of a control flow structure
        var context = function.AsmFunction.Context;
        
        // Check for if-else structure
        var ifElseStructure = context.GetAnalysisData<ControlFlowAnalyzer.IfElseStructure>(block.Address, "IfElseStructure");
        if (ifElseStructure != null && ifElseStructure.ConditionBlock.Address == block.Address)
        {
            // This block is the condition of an if-else structure
            GenerateIfElseCode(function, ifElseStructure, result, indentLevel, processedBlocks);
            return;
        }
        
        // Check for switch structure
        var switchStructure = context.GetAnalysisData<ControlFlowAnalyzer.SwitchStructure>(block.Address, "SwitchStructure");
        if (switchStructure != null && switchStructure.HeaderBlock.Address == block.Address)
        {
            // This block is the header of a switch structure
            GenerateSwitchCode(function, switchStructure, result, indentLevel, processedBlocks);
            return;
        }
        
        // Check if this block is part of a loop
        var loops = context.LoopsByBlockAddress.TryGetValue(block.Address, out var blockLoops) ? blockLoops : null;
        if (loops != null && loops.Count > 0)
        {
            // Get the innermost loop
            var loop = loops[0];
            
            // Check if this is the loop header
            if (loop.Header.Address == block.Address)
            {
                // This block is the header of a loop
                GenerateLoopCode(function, loop, result, indentLevel, processedBlocks);
                return;
            }
        }
        
        // If we get here, this is a regular block
        GenerateRegularBlockCode(function, block, result, indentLevel, processedBlocks);
    }
    
    /// <summary>
    /// Generates code for a regular basic block
    /// </summary>
    /// <param name="function">The function containing the block</param>
    /// <param name="block">The block to generate code for</param>
    /// <param name="result">The string builder to append to</param>
    /// <param name="indentLevel">The current indentation level</param>
    /// <param name="processedBlocks">Set of blocks that have already been processed</param>
    private void GenerateRegularBlockCode(Function function, InstructionBlock block, StringBuilder result, int indentLevel, HashSet<ulong> processedBlocks)
    {
        // Add a comment with the block address
        string indent = new string(' ', indentLevel * 4);
        result.AppendLine($"{indent}// Block at 0x{block.Address:X8}");
        
        // Generate pseudocode for the instructions in this block
        foreach (var instruction in block.Instructions)
        {
            // Skip function prologue/epilogue instructions
            if (IsPrologueOrEpilogueInstruction(instruction))
            {
                continue;
            }
            
            // Generate pseudocode for this instruction
            string pseudocode = GenerateInstructionPseudocode(function, instruction);
            if (!string.IsNullOrEmpty(pseudocode))
            {
                result.AppendLine($"{indent}{pseudocode};");
            }
        }
        
        // Process successors
        foreach (var successor in block.Successors)
        {
            if (!processedBlocks.Contains(successor.Address))
            {
                GenerateBlockCode(function, successor, result, indentLevel, processedBlocks);
            }
        }
    }
    
    /// <summary>
    /// Generates code for an if-else structure
    /// </summary>
    /// <param name="function">The function containing the structure</param>
    /// <param name="ifElseStructure">The if-else structure to generate code for</param>
    /// <param name="result">The string builder to append to</param>
    /// <param name="indentLevel">The current indentation level</param>
    /// <param name="processedBlocks">Set of blocks that have already been processed</param>
    private void GenerateIfElseCode(Function function, ControlFlowAnalyzer.IfElseStructure ifElseStructure, StringBuilder result, int indentLevel, HashSet<ulong> processedBlocks)
    {
        // Mark the condition block as processed
        processedBlocks.Add(ifElseStructure.ConditionBlock.Address);
        
        // Generate the condition expression
        string condition = GenerateConditionExpression(function, ifElseStructure.ConditionBlock);
        
        // Add the if statement
        string indent = new string(' ', indentLevel * 4);
        result.AppendLine($"{indent}// If-else structure at 0x{ifElseStructure.ConditionBlock.Address:X8}")
              .AppendLine($"{indent}if ({condition})");
        
        // Add the then branch
        result.AppendLine($"{indent}{{")
              .AppendLine($"{indent}    // Then branch at 0x{ifElseStructure.ThenBlock.Address:X8}");
        
        // Generate code for the then branch
        GenerateBlockCode(function, ifElseStructure.ThenBlock, result, indentLevel + 1, processedBlocks);
        
        // Close the then branch
        result.AppendLine($"{indent}}}");
        
        // Add the else branch if it exists and is not already processed
        if (ifElseStructure.ElseBlock != null && !processedBlocks.Contains(ifElseStructure.ElseBlock.Address))
        {
            result.AppendLine($"{indent}else")
                  .AppendLine($"{indent}{{")
                  .AppendLine($"{indent}    // Else branch at 0x{ifElseStructure.ElseBlock.Address:X8}");
            
            // Generate code for the else branch
            GenerateBlockCode(function, ifElseStructure.ElseBlock, result, indentLevel + 1, processedBlocks);
            
            // Close the else branch
            result.AppendLine($"{indent}}}");
        }
    }
    
    /// <summary>
    /// Generates code for a switch structure
    /// </summary>
    /// <param name="function">The function containing the structure</param>
    /// <param name="switchStructure">The switch structure to generate code for</param>
    /// <param name="result">The string builder to append to</param>
    /// <param name="indentLevel">The current indentation level</param>
    /// <param name="processedBlocks">Set of blocks that have already been processed</param>
    private void GenerateSwitchCode(Function function, ControlFlowAnalyzer.SwitchStructure switchStructure, StringBuilder result, int indentLevel, HashSet<ulong> processedBlocks)
    {
        // Mark the header block as processed
        processedBlocks.Add(switchStructure.HeaderBlock.Address);
        
        // Generate the switch expression
        string switchExpr = "/* switch expression */";
        
        // Add the switch statement
        string indent = new string(' ', indentLevel * 4);
        result.AppendLine($"{indent}// Switch structure at 0x{switchStructure.HeaderBlock.Address:X8}")
              .AppendLine($"{indent}switch ({switchExpr})");
        
        // Add the switch body
        result.AppendLine($"{indent}{{")
              .AppendLine();
        
        // Generate code for each case
        foreach (var switchCase in switchStructure.Cases)
        {
            // Add the case label
            result.AppendLine($"{indent}    case {switchCase.Value}:")
                  .AppendLine($"{indent}        // Case block at 0x{switchCase.CaseBlock.Address:X8}");
            
            // Generate code for the case block
            GenerateBlockCode(function, switchCase.CaseBlock, result, indentLevel + 2, processedBlocks);
            
            // Add a break statement
            result.AppendLine($"{indent}        break;")
                  .AppendLine();
        }
        
        // Add a default case
        result.AppendLine($"{indent}    default:")
              .AppendLine($"{indent}        // Default case")
              .AppendLine($"{indent}        break;");
        
        // Close the switch body
        result.AppendLine($"{indent}}}");
    }
    
    /// <summary>
    /// Generates code for a loop structure
    /// </summary>
    /// <param name="function">The function containing the structure</param>
    /// <param name="loop">The loop to generate code for</param>
    /// <param name="result">The string builder to append to</param>
    /// <param name="indentLevel">The current indentation level</param>
    /// <param name="processedBlocks">Set of blocks that have already been processed</param>
    private void GenerateLoopCode(Function function, AnalyzerContext.Loop loop, StringBuilder result, int indentLevel, HashSet<ulong> processedBlocks)
    {
        // Mark the header block as processed
        processedBlocks.Add(loop.Header.Address);
        
        // Add the loop header
        string indent = new string(' ', indentLevel * 4);
        result.AppendLine($"{indent}// Loop at 0x{loop.Header.Address:X8}")
              .AppendLine($"{indent}while (true) // Simplified loop condition");
        
        // Add the loop body
        result.AppendLine($"{indent}{{")
              .AppendLine($"{indent}    // Loop body");
        
        // Generate code for the loop body (starting with the header)
        GenerateBlockCode(function, loop.Header, result, indentLevel + 1, processedBlocks);
        
        // Close the loop body
        result.AppendLine($"{indent}}}");
    }
    
    /// <summary>
    /// Generates a condition expression for an if statement
    /// </summary>
    /// <param name="function">The function containing the block</param>
    /// <param name="conditionBlock">The block containing the condition</param>
    /// <returns>A string representing the condition expression</returns>
    private string GenerateConditionExpression(Function function, InstructionBlock conditionBlock)
    {
        // For now, we'll just return a placeholder
        // In a real implementation, we would analyze the instructions to determine the condition
        return "/* condition */";
    }
    
    /// <summary>
    /// Generates pseudocode for a single instruction
    /// </summary>
    /// <param name="function">The function containing the instruction</param>
    /// <param name="instruction">The instruction to generate pseudocode for</param>
    /// <returns>The generated pseudocode</returns>
    private string GenerateInstructionPseudocode(Function function, Instruction instruction)
    {
        // For now, we'll just return a comment with the instruction
        return $"/* {instruction} */";
    }
    
    /// <summary>
    /// Checks if an instruction is part of the function prologue or epilogue
    /// </summary>
    /// <param name="instruction">The instruction to check</param>
    /// <returns>True if the instruction is part of the prologue or epilogue, false otherwise</returns>
    private bool IsPrologueOrEpilogueInstruction(Instruction instruction)
    {
        // Check for common prologue instructions
        if (instruction.Type == InstructionType.Push && 
            instruction.StructuredOperands.Count > 0 && 
            instruction.StructuredOperands[0] is RegisterOperand regOp && 
            regOp.Register == RegisterIndex.Bp)
        {
            return true; // push ebp
        }
        
        if (instruction.Type == InstructionType.Mov && 
            instruction.StructuredOperands.Count > 1 && 
            instruction.StructuredOperands[0] is RegisterOperand destReg && 
            instruction.StructuredOperands[1] is RegisterOperand srcReg && 
            destReg.Register == RegisterIndex.Bp && 
            srcReg.Register == RegisterIndex.Sp)
        {
            return true; // mov ebp, esp
        }
        
        if (instruction.Type == InstructionType.Sub && 
            instruction.StructuredOperands.Count > 1 && 
            instruction.StructuredOperands[0] is RegisterOperand subReg && 
            subReg.Register == RegisterIndex.Sp)
        {
            return true; // sub esp, X
        }
        
        // Check for common epilogue instructions
        if (instruction.Type == InstructionType.Pop && 
            instruction.StructuredOperands.Count > 0 && 
            instruction.StructuredOperands[0] is RegisterOperand popReg && 
            popReg.Register == RegisterIndex.Bp)
        {
            return true; // pop ebp
        }
        
        if (instruction.Type == InstructionType.Ret)
        {
            return true; // ret
        }
        
        return false;
    }
}
