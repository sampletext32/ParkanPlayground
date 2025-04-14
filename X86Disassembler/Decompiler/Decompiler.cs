namespace X86Disassembler.Decompiler;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

/// <summary>
/// Main decompiler class that translates assembly code into higher-level code
/// </summary>
public class Decompiler
{
    // The list of disassembled instructions
    private readonly List<Instruction> _instructions;
    
    // The control flow graph
    private ControlFlowGraph? _controlFlowGraph;
    
    // The data flow analysis
    private DataFlowAnalysis? _dataFlowAnalysis;
    
    // The entry point address
    private readonly ulong _entryPoint;
    
    /// <summary>
    /// Initializes a new instance of the Decompiler class
    /// </summary>
    /// <param name="instructions">The list of disassembled instructions</param>
    /// <param name="entryPoint">The entry point address</param>
    public Decompiler(List<Instruction> instructions, ulong entryPoint)
    {
        _instructions = instructions;
        _entryPoint = entryPoint;
    }
    
    /// <summary>
    /// Decompiles the instructions and returns the decompiled code
    /// </summary>
    /// <returns>The decompiled code</returns>
    public string Decompile()
    {
        // Build the control flow graph
        _controlFlowGraph = ControlFlowGraph.Build(_instructions, _entryPoint);
        
        // Perform data flow analysis
        _dataFlowAnalysis = new DataFlowAnalysis();
        _dataFlowAnalysis.Analyze(_instructions);
        
        // Generate pseudocode from the control flow graph and data flow analysis
        return GeneratePseudocode();
    }
    
    /// <summary>
    /// Generates pseudocode from the control flow graph and data flow analysis
    /// </summary>
    /// <returns>The generated pseudocode</returns>
    private string GeneratePseudocode()
    {
        if (_controlFlowGraph == null || _controlFlowGraph.EntryBlock == null)
        {
            return "// Could not build control flow graph";
        }
        
        StringBuilder code = new StringBuilder();
        
        // Add a function header
        code.AppendLine("// Decompiled function");
        code.AppendLine("int DecompiledFunction() {")
            .AppendLine();
        
        // Generate variable declarations
        if (_dataFlowAnalysis != null)
        {
            foreach (var variable in _dataFlowAnalysis.Variables)
            {
                // Skip register variables
                if (IsRegister(variable.Location))
                {
                    continue;
                }
                
                // Generate a variable declaration
                code.AppendLine($"    {variable.Type} {variable.Name}; // {variable.Location}");
            }
            
            if (_dataFlowAnalysis.Variables.Any(v => !IsRegister(v.Location)))
            {
                code.AppendLine();
            }
        }
        
        // Process the blocks in a depth-first order
        HashSet<ulong> visitedBlocks = new HashSet<ulong>();
        GenerateCodeForBlock(_controlFlowGraph.EntryBlock, code, visitedBlocks, 1);
        
        // Add a return statement if not already present
        if (!code.ToString().Contains("return"))
        {
            code.AppendLine("    return 0;");
        }
        
        // Close the function
        code.AppendLine("}");
        
        return code.ToString();
    }
    
    /// <summary>
    /// Generates code for a basic block and its successors
    /// </summary>
    /// <param name="block">The basic block</param>
    /// <param name="code">The code builder</param>
    /// <param name="visitedBlocks">The set of visited blocks</param>
    /// <param name="indentLevel">The indentation level</param>
    private void GenerateCodeForBlock(ControlFlowGraph.BasicBlock block, StringBuilder code, HashSet<ulong> visitedBlocks, int indentLevel)
    {
        // If we've already visited this block, add a goto statement
        if (visitedBlocks.Contains(block.StartAddress))
        {
            string indent = new string(' ', indentLevel * 4);
            code.AppendLine($"{indent}goto block_{block.StartAddress:X8};");
            return;
        }
        
        // Mark this block as visited
        visitedBlocks.Add(block.StartAddress);
        
        // Add a label for this block
        string blockIndent = new string(' ', (indentLevel - 1) * 4);
        code.AppendLine($"{blockIndent}block_{block.StartAddress:X8}:")
            .AppendLine();
        
        // Generate code for the instructions in this block
        foreach (var instruction in block.Instructions)
        {
            string instructionCode = TranslateInstruction(instruction, indentLevel);
            if (!string.IsNullOrEmpty(instructionCode))
            {
                code.AppendLine(instructionCode);
            }
        }
        
        // Handle successors based on the control flow
        if (block.Successors.Count == 1)
        {
            // Unconditional branch to the next block
            GenerateCodeForBlock(block.Successors[0], code, visitedBlocks, indentLevel);
        }
        else if (block.Successors.Count == 2)
        {
            // Conditional branch
            string indent = new string(' ', indentLevel * 4);
            
            // Get the last instruction in the block
            Instruction lastInstruction = block.Instructions[^1];
            string condition = GetConditionFromJump(lastInstruction);
            
            // Find the fall-through block and the jump target block
            ControlFlowGraph.BasicBlock? fallthroughBlock = null;
            ControlFlowGraph.BasicBlock? jumpTargetBlock = null;
            
            // Use a constant estimated instruction length since RawBytes is not available
            const int estimatedInstructionLength = 4; // Typical x86 instruction length
            ulong nextAddress = lastInstruction.Address + (ulong)estimatedInstructionLength;
            foreach (var successor in block.Successors)
            {
                if (successor.StartAddress == nextAddress)
                {
                    fallthroughBlock = successor;
                }
                else
                {
                    jumpTargetBlock = successor;
                }
            }
            
            if (fallthroughBlock != null && jumpTargetBlock != null)
            {
                // Generate an if statement
                code.AppendLine($"{indent}if ({condition}) {{")
                    .AppendLine();
                
                // Generate code for the jump target block
                GenerateCodeForBlock(jumpTargetBlock, code, visitedBlocks, indentLevel + 1);
                
                // Close the if statement
                code.AppendLine($"{indent}}}")
                    .AppendLine();
                
                // Generate code for the fall-through block
                GenerateCodeForBlock(fallthroughBlock, code, visitedBlocks, indentLevel);
            }
            else
            {
                // If we couldn't determine the fall-through and jump target blocks,
                // just generate code for both successors
                foreach (var successor in block.Successors)
                {
                    GenerateCodeForBlock(successor, code, visitedBlocks, indentLevel);
                }
            }
        }
    }
    
    /// <summary>
    /// Translates an instruction into a higher-level code statement
    /// </summary>
    /// <param name="instruction">The instruction to translate</param>
    /// <param name="indentLevel">The indentation level</param>
    /// <returns>The translated code statement</returns>
    private string TranslateInstruction(Instruction instruction, int indentLevel)
    {
        string indent = new string(' ', indentLevel * 4);
        string mnemonic = instruction.Type.ToString().ToLower();
        string operands = "";
        
        // Format operands if available
        if (instruction.StructuredOperands != null && instruction.StructuredOperands.Count > 0)
        {
            operands = string.Join(", ", instruction.StructuredOperands.Select(op => op.ToString()));
        }
        
        // Skip jumps (handled by control flow)
        if (mnemonic.StartsWith("j"))
        {
            return $"{indent}// {instruction}";
        }
        
        // Handle different instruction types
        switch (mnemonic)
        {
            case "mov":
                return TranslateMovInstruction(instruction, indent);
                
            case "add":
            case "sub":
            case "mul":
            case "div":
            case "and":
            case "or":
            case "xor":
                return TranslateArithmeticInstruction(instruction, indent);
                
            case "push":
            case "pop":
                return $"{indent}// {instruction}";
                
            case "call":
                return TranslateCallInstruction(instruction, indent);
                
            case "ret":
                return TranslateReturnInstruction(instruction, indent);
                
            case "cmp":
            case "test":
                return $"{indent}// {instruction}";
                
            default:
                // For other instructions, just add a comment
                return $"{indent}// {instruction}";
        }
    }
    
    /// <summary>
    /// Translates a MOV instruction
    /// </summary>
    /// <param name="instruction">The instruction to translate</param>
    /// <param name="indent">The indentation string</param>
    /// <returns>The translated code statement</returns>
    private string TranslateMovInstruction(Instruction instruction, string indent)
    {
        string[] operandParts = instruction.StructuredOperands.Select(op => op.ToString()).ToArray();
        if (operandParts.Length != 2)
        {
            return $"{indent}// {instruction}";
        }
        
        string destination = operandParts[0].Trim();
        string source = operandParts[1].Trim();
        
        // Skip register-to-register moves for registers we don't track
        if (IsRegister(destination) && IsRegister(source))
        {
            return $"{indent}// {instruction}";
        }
        
        // Translate memory access
        if (IsMemoryLocation(destination))
        {
            string variableName = GetVariableNameForMemory(destination);
            return $"{indent}{variableName} = {GetReadableOperand(source)}; // {instruction}";
        }
        else if (IsMemoryLocation(source))
        {
            string variableName = GetVariableNameForMemory(source);
            return $"{indent}{GetReadableOperand(destination)} = {variableName}; // {instruction}";
        }
        
        // Default case
        return $"{indent}{GetReadableOperand(destination)} = {GetReadableOperand(source)}; // {instruction}";
    }
    
    /// <summary>
    /// Translates an arithmetic instruction
    /// </summary>
    /// <param name="instruction">The instruction to translate</param>
    /// <param name="indent">The indentation string</param>
    /// <returns>The translated code statement</returns>
    private string TranslateArithmeticInstruction(Instruction instruction, string indent)
    {
        string[] operandParts = instruction.StructuredOperands.Select(op => op.ToString()).ToArray();
        if (operandParts.Length != 2)
        {
            return $"{indent}// {instruction}";
        }
        
        string destination = operandParts[0].Trim();
        string source = operandParts[1].Trim();
        string operatorSymbol = GetOperatorForMnemonic(instruction.Type.ToString().ToLower());
        
        // Skip register-to-register operations for registers we don't track
        if (IsRegister(destination) && IsRegister(source))
        {
            return $"{indent}// {instruction}";
        }
        
        // Translate the operation
        return $"{indent}{GetReadableOperand(destination)} {operatorSymbol}= {GetReadableOperand(source)}; // {instruction}";
    }
    
    /// <summary>
    /// Translates a CALL instruction
    /// </summary>
    /// <param name="instruction">The instruction to translate</param>
    /// <param name="indent">The indentation string</param>
    /// <returns>The translated code statement</returns>
    private string TranslateCallInstruction(Instruction instruction, string indent)
    {
        string target = instruction.StructuredOperands.FirstOrDefault()?.ToString() ?? "";
        
        // Try to get a function name from the target
        string functionName = GetFunctionNameFromTarget(target);
        
        return $"{indent}{functionName}(); // {instruction}";
    }
    
    /// <summary>
    /// Translates a RET instruction
    /// </summary>
    /// <param name="instruction">The instruction to translate</param>
    /// <param name="indent">The indentation string</param>
    /// <returns>The translated code statement</returns>
    private string TranslateReturnInstruction(Instruction instruction, string indent)
    {
        // Check if EAX is used as a return value
        if (_dataFlowAnalysis != null)
        {
            var eaxVariable = _dataFlowAnalysis.Variables.FirstOrDefault(v => v.Location == "eax" && v.IsReturnValue);
            if (eaxVariable != null)
            {
                return $"{indent}return {eaxVariable.Name}; // {instruction}";
            }
        }
        
        return $"{indent}return; // {instruction}";
    }
    
    /// <summary>
    /// Gets the condition from a conditional jump instruction
    /// </summary>
    /// <param name="instruction">The jump instruction</param>
    /// <returns>The condition expression</returns>
    private string GetConditionFromJump(Instruction instruction)
    {
        string mnemonic = instruction.Type.ToString().ToLower();
        
        // Map jump mnemonics to conditions
        return mnemonic switch
        {
            "je" => "a == b",
            "jne" => "a != b",
            "jz" => "a == 0",
            "jnz" => "a != 0",
            "jg" => "a > b",
            "jge" => "a >= b",
            "jl" => "a < b",
            "jle" => "a <= b",
            "ja" => "a > b (unsigned)",
            "jae" => "a >= b (unsigned)",
            "jb" => "a < b (unsigned)",
            "jbe" => "a <= b (unsigned)",
            _ => "condition"
        };
    }
    
    /// <summary>
    /// Gets the operator for an arithmetic mnemonic
    /// </summary>
    /// <param name="mnemonic">The instruction mnemonic</param>
    /// <returns>The operator</returns>
    private string GetOperatorForMnemonic(string mnemonic)
    {
        return mnemonic switch
        {
            "add" => "+",
            "sub" => "-",
            "mul" => "*",
            "div" => "/",
            "and" => "&",
            "or" => "|",
            "xor" => "^",
            _ => mnemonic
        };
    }
    
    /// <summary>
    /// Gets a readable representation of an operand
    /// </summary>
    /// <param name="operand">The operand</param>
    /// <returns>A readable representation</returns>
    private string GetReadableOperand(string operand)
    {
        // If it's a register, return it as is
        if (IsRegister(operand))
        {
            return operand;
        }
        
        // If it's a memory location, get a variable name
        if (IsMemoryLocation(operand))
        {
            return GetVariableNameForMemory(operand);
        }
        
        // If it's a hexadecimal constant, format it
        if (operand.StartsWith("0x") && operand.Length > 2)
        {
            return operand;
        }
        
        // Otherwise, return it as is
        return operand;
    }
    
    /// <summary>
    /// Gets a variable name for a memory location
    /// </summary>
    /// <param name="memoryLocation">The memory location</param>
    /// <returns>A variable name</returns>
    private string GetVariableNameForMemory(string memoryLocation)
    {
        if (_dataFlowAnalysis == null)
        {
            return "memory";
        }
        
        // Extract the part inside the brackets
        int startIndex = memoryLocation.IndexOf('[');
        int endIndex = memoryLocation.IndexOf(']');
        
        if (startIndex >= 0 && endIndex > startIndex)
        {
            string memoryReference = memoryLocation.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
            
            // Try to find a variable for this memory location
            var variable = _dataFlowAnalysis.Variables.FirstOrDefault(v => v.Location == memoryReference);
            if (variable != null)
            {
                return variable.Name;
            }
            
            // If it's a stack variable (relative to EBP), give it a meaningful name
            if (memoryReference.StartsWith("ebp+") || memoryReference.StartsWith("ebp-"))
            {
                string offset = memoryReference.Substring(4);
                return $"local_{offset.Replace("+", "plus_").Replace("-", "minus_")}";
            }
        }
        
        return "memory";
    }
    
    /// <summary>
    /// Gets a function name from a call target
    /// </summary>
    /// <param name="target">The call target</param>
    /// <returns>A function name</returns>
    private string GetFunctionNameFromTarget(string target)
    {
        // If it's a direct address, format it
        if (target.StartsWith("0x") && target.Length > 2)
        {
            return $"function_{target.Substring(2)}";
        }
        
        // If it's a memory location, extract the address
        if (IsMemoryLocation(target))
        {
            return $"function_ptr_{GetVariableNameForMemory(target)}";
        }
        
        // Otherwise, use the target as is
        return target;
    }
    
    /// <summary>
    /// Checks if an operand is a register
    /// </summary>
    /// <param name="operand">The operand to check</param>
    /// <returns>True if the operand is a register</returns>
    private bool IsRegister(string operand)
    {
        string[] registers = { "eax", "ebx", "ecx", "edx", "esi", "edi", "ebp", "esp",
                              "ax", "bx", "cx", "dx", "si", "di", "bp", "sp",
                              "al", "ah", "bl", "bh", "cl", "ch", "dl", "dh" };
        
        return Array.IndexOf(registers, operand.ToLower()) >= 0;
    }
    
    /// <summary>
    /// Checks if an operand is a memory location
    /// </summary>
    /// <param name="operand">The operand to check</param>
    /// <returns>True if the operand is a memory location</returns>
    private bool IsMemoryLocation(string operand)
    {
        return operand.Contains('[') && operand.Contains(']');
    }
}
