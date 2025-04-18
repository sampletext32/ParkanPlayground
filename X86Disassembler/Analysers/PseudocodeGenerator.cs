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
        result.AppendLine($"{new string(' ', indentLevel * 4)}// Block at 0x{block.Address:X8}");
        
        // Check if this block ends with a conditional jump
        bool hasConditionalJump = block.Instructions.Count > 0 && 
                                 IsConditionalJump(block.Instructions[^1].Type);
        
        // If this block has a conditional jump but wasn't detected as an if-else structure,
        // we'll create an inline if statement for better readability
        if (hasConditionalJump && block.Successors.Count == 2)
        {
            // Get the last instruction (conditional jump)
            var jumpInstruction = block.Instructions[^1];
            
            // Generate condition based on the jump type
            string condition = GenerateConditionFromJump(jumpInstruction);
            
            // Generate code for all instructions except the last one (the jump)
            for (int i = 0; i < block.Instructions.Count - 1; i++)
            {
                var instruction = block.Instructions[i];
                
                // Skip prologue/epilogue instructions
                if (IsPrologueOrEpilogueInstruction(instruction))
                {
                    continue;
                }
                
                // Generate pseudocode for this instruction
                var pseudocode = GenerateInstructionPseudocode(function, instruction);
                if (!string.IsNullOrEmpty(pseudocode))
                {
                    result.AppendLine($"{new string(' ', indentLevel * 4)}{pseudocode}");
                }
                else
                {
                    // If we couldn't generate pseudocode, add the instruction as a comment
                    result.AppendLine($"{new string(' ', indentLevel * 4)}/* {instruction} */;");
                }
            }
            
            // Generate the if statement
            result.AppendLine($"{new string(' ', indentLevel * 4)}if ({condition})");
            result.AppendLine($"{new string(' ', indentLevel * 4)}{{");
            
            // Find the target block (true branch)
            var targetAddress = GetJumpTargetAddress(jumpInstruction);
            var targetBlock = block.Successors.FirstOrDefault(s => s.Address == targetAddress);
            
            if (targetBlock != null)
            {
                // Generate code for the target block
                GenerateBlockCode(function, targetBlock, result, indentLevel + 1, processedBlocks);
            }
            
            result.AppendLine($"{new string(' ', indentLevel * 4)}}}");
            
            // Find the fallthrough block (false branch)
            var fallthroughBlock = block.Successors.FirstOrDefault(s => s.Address != targetAddress);
            
            if (fallthroughBlock != null && !processedBlocks.Contains(fallthroughBlock.Address))
            {
                // Generate code for the fallthrough block
                GenerateBlockCode(function, fallthroughBlock, result, indentLevel, processedBlocks);
            }
        }
        else
        {
            // Regular block processing
            // Generate code for each instruction in the block
            foreach (var instruction in block.Instructions)
            {
                // Skip prologue/epilogue instructions
                if (IsPrologueOrEpilogueInstruction(instruction))
                {
                    continue;
                }
                
                // Generate pseudocode for this instruction
                var pseudocode = GenerateInstructionPseudocode(function, instruction);
                if (!string.IsNullOrEmpty(pseudocode))
                {
                    result.AppendLine($"{new string(' ', indentLevel * 4)}{pseudocode}");
                }
                else
                {
                    // If we couldn't generate pseudocode, add the instruction as a comment
                    result.AppendLine($"{new string(' ', indentLevel * 4)}/* {instruction} */;");
                }
            }
            
            // Process successors in order
            foreach (var successor in block.Successors)
            {
                // Only process successors that haven't been processed yet
                if (!processedBlocks.Contains(successor.Address))
                {
                    GenerateBlockCode(function, successor, result, indentLevel, processedBlocks);
                }
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
        // Handle different instruction types
        switch (instruction.Type)
        {
            case InstructionType.Mov:
                // Handle MOV instruction
                if (instruction.StructuredOperands.Count >= 2)
                {
                    var dest = instruction.StructuredOperands[0];
                    var src = instruction.StructuredOperands[1];
                    
                    return $"{FormatOperand(dest)} = {FormatOperand(src)};";
                }
                break;
                
            case InstructionType.Add:
                // Handle ADD instruction
                if (instruction.StructuredOperands.Count >= 2)
                {
                    var dest = instruction.StructuredOperands[0];
                    var src = instruction.StructuredOperands[1];
                    
                    return $"{FormatOperand(dest)} += {FormatOperand(src)};";
                }
                break;
                
            case InstructionType.Sub:
                // Handle SUB instruction
                if (instruction.StructuredOperands.Count >= 2)
                {
                    var dest = instruction.StructuredOperands[0];
                    var src = instruction.StructuredOperands[1];
                    
                    return $"{FormatOperand(dest)} -= {FormatOperand(src)};";
                }
                break;
                
            case InstructionType.And:
                // Handle AND instruction
                if (instruction.StructuredOperands.Count >= 2)
                {
                    var dest = instruction.StructuredOperands[0];
                    var src = instruction.StructuredOperands[1];
                    
                    return $"{FormatOperand(dest)} &= {FormatOperand(src)};";
                }
                break;
                
            case InstructionType.Or:
                // Handle OR instruction
                if (instruction.StructuredOperands.Count >= 2)
                {
                    var dest = instruction.StructuredOperands[0];
                    var src = instruction.StructuredOperands[1];
                    
                    return $"{FormatOperand(dest)} |= {FormatOperand(src)};";
                }
                break;
                
            case InstructionType.Xor:
                // Handle XOR instruction
                if (instruction.StructuredOperands.Count >= 2)
                {
                    var dest = instruction.StructuredOperands[0];
                    var src = instruction.StructuredOperands[1];
                    
                    // Special case: xor eax, eax is used to zero a register
                    if (dest is RegisterOperand destReg && src is RegisterOperand srcReg && 
                        destReg.Register == srcReg.Register)
                    {
                        return $"{FormatOperand(dest)} = 0;";
                    }
                    
                    return $"{FormatOperand(dest)} ^= {FormatOperand(src)};";
                }
                break;
                
            case InstructionType.Test:
                // Handle TEST instruction (used for condition testing)
                if (instruction.StructuredOperands.Count >= 2)
                {
                    var op1 = instruction.StructuredOperands[0];
                    var op2 = instruction.StructuredOperands[1];
                    
                    return $"// Test {FormatOperand(op1)} & {FormatOperand(op2)}";
                }
                break;
                
            case InstructionType.Cmp:
                // Handle CMP instruction (used for condition testing)
                if (instruction.StructuredOperands.Count >= 2)
                {
                    var op1 = instruction.StructuredOperands[0];
                    var op2 = instruction.StructuredOperands[1];
                    
                    return $"// Compare {FormatOperand(op1)} with {FormatOperand(op2)}";
                }
                break;
                
            case InstructionType.Call:
                // Handle CALL instruction
                if (instruction.StructuredOperands.Count >= 1)
                {
                    var target = instruction.StructuredOperands[0];
                    
                    return $"call({FormatOperand(target)});";
                }
                break;
                
            case InstructionType.Push:
                // Handle PUSH instruction
                if (instruction.StructuredOperands.Count >= 1)
                {
                    var value = instruction.StructuredOperands[0];
                    
                    return $"push({FormatOperand(value)});";
                }
                break;
                
            case InstructionType.Pop:
                // Handle POP instruction
                if (instruction.StructuredOperands.Count >= 1)
                {
                    var dest = instruction.StructuredOperands[0];
                    
                    return $"{FormatOperand(dest)} = pop();";
                }
                break;
        }
        
        // If we couldn't generate pseudocode, return a comment with the instruction
        return $"/* {instruction} */";
    }
    
    /// <summary>
    /// Formats an operand for display in pseudocode
    /// </summary>
    /// <param name="operand">The operand to format</param>
    /// <returns>A string representation of the operand</returns>
    private string FormatOperand(Operand operand)
    {
        if (operand is RegisterOperand regOp)
        {
            // Format register operand
            return RegisterMapper.GetRegisterName(regOp.Register, 32);
        }
        else if (operand is ImmediateOperand immOp)
        {
            // Format immediate operand
            return $"0x{immOp.Value:X}";
        }
        else if (operand is DisplacementMemoryOperand dispOp)
        {
            // Format displacement memory operand
            string baseReg = RegisterMapper.GetRegisterName(dispOp.BaseRegister, 32);
            return $"*({baseReg} + 0x{dispOp.Displacement:X})";
        }
        else if (operand is BaseRegisterMemoryOperand baseOp)
        {
            // Format base register memory operand
            string baseReg = RegisterMapper.GetRegisterName(baseOp.BaseRegister, 32);
            return $"*({baseReg})";
        }
        
        // Default formatting
        return operand.ToString();
    }
    
    /// <summary>
    /// Checks if an instruction is part of the function prologue or epilogue
    /// </summary>
    /// <param name="instruction">The instruction to check</param>
    /// <returns>True if the instruction is part of the prologue or epilogue, false otherwise</returns>
    private bool IsPrologueOrEpilogueInstruction(Instruction instruction)
    {
        // Check for common prologue/epilogue instructions
        if (instruction.Type == InstructionType.Push && 
            instruction.StructuredOperands.Count > 0 && 
            instruction.StructuredOperands[0] is RegisterOperand reg && 
            reg.Register == RegisterIndex.Bp)
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
               type == InstructionType.Jns;
    }
    
    /// <summary>
    /// Gets the target address of a jump instruction
    /// </summary>
    /// <param name="instruction">The jump instruction</param>
    /// <returns>The target address of the jump</returns>
    private ulong GetJumpTargetAddress(Instruction instruction)
    {
        // Jump instructions have the target address as their first operand
        if (instruction.StructuredOperands.Count > 0)
        {
            return instruction.StructuredOperands[0].GetValue();
        }
        
        // If we can't determine the target address, return 0
        return 0;
    }
    
    /// <summary>
    /// Generates a condition expression based on a conditional jump instruction
    /// </summary>
    /// <param name="instruction">The conditional jump instruction</param>
    /// <returns>A string representing the condition expression</returns>
    private string GenerateConditionFromJump(Instruction instruction)
    {
        // Map jump types to their equivalent C-like conditions
        // Note: These are inverted because the jump is taken when the condition is true,
        // but in C-like code, the condition is for the 'if' statement
        switch (instruction.Type)
        {
            case InstructionType.Jz:  // Jump if Zero (ZF=1)
                return "condition == 0";
                
            case InstructionType.Jnz: // Jump if Not Zero (ZF=0)
                return "condition != 0";
                
            case InstructionType.Jg:  // Jump if Greater (ZF=0 and SF=OF)
                return "condition > 0";
                
            case InstructionType.Jge: // Jump if Greater or Equal (SF=OF)
                return "condition >= 0";
                
            case InstructionType.Jl:  // Jump if Less (SF!=OF)
                return "condition < 0";
                
            case InstructionType.Jle: // Jump if Less or Equal (ZF=1 or SF!=OF)
                return "condition <= 0";
                
            case InstructionType.Ja:  // Jump if Above (CF=0 and ZF=0)
                return "condition > 0 /* unsigned */";
                
            case InstructionType.Jae: // Jump if Above or Equal (CF=0)
                return "condition >= 0 /* unsigned */";
                
            case InstructionType.Jb:  // Jump if Below (CF=1)
                return "condition < 0 /* unsigned */";
                
            case InstructionType.Jbe: // Jump if Below or Equal (CF=1 or ZF=1)
                return "condition <= 0 /* unsigned */";
                
            // Add more cases for other conditional jumps as needed
                
            default:
                // For unknown jump types, use a generic condition
                return "/* unknown condition */";
        }
    }
}
