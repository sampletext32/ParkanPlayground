using X86Disassembler.Analysers.DecompilerTypes;
using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86Disassembler.Analysers;

/// <summary>
/// Analyzes disassembled functions to identify variables, parameters, and control flow structures
/// </summary>
public class FunctionAnalyzer
{
    /// <summary>
    /// The analyzer context
    /// </summary>
    private readonly AnalyzerContext _context;
    
    /// <summary>
    /// Creates a new function analyzer
    /// </summary>
    /// <param name="context">The analyzer context</param>
    public FunctionAnalyzer(AnalyzerContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Analyzes a function at the specified address
    /// </summary>
    /// <param name="address">The address of the function</param>
    /// <param name="name">The name of the function (if known)</param>
    /// <returns>The analyzed function</returns>
    public Function AnalyzeFunction(ulong address, string name = "")
    {
        // If no name is provided, generate one based on the address
        if (string.IsNullOrEmpty(name))
        {
            name = $"func_{address:X8}";
        }
        
        // Create a function object
        var function = new Function(name, address, _context.Function)
        {
            ReturnType = DataType.Unknown // Default to unknown return type
        };
        
        // Create a variable analyzer and analyze variables
        var variableAnalyzer = new VariableAnalyzer(_context);
        variableAnalyzer.AnalyzeStackVariables(function);
        
        // Determine the calling convention
        DetermineCallingConvention(function);
        
        // Infer parameter and return types
        InferTypes(function);
        
        return function;
    }
    
    /// <summary>
    /// Determines the calling convention of a function based on its behavior
    /// </summary>
    /// <param name="function">The function to analyze</param>
    private void DetermineCallingConvention(Function function)
    {
        // By default, we'll assume cdecl
        function.CallingConvention = CallingConvention.Cdecl;
        
        // Get the exit blocks (blocks with ret instructions)
        var exitBlocks = function.AsmFunction.Blocks.Where(b => 
            b.Instructions.Count > 0 && 
            b.Instructions.Last().Type == InstructionType.Ret).ToList();
        
        // Check if the function cleans up its own stack
        bool cleansOwnStack = false;
        
        // Look for ret instructions with an immediate operand
        foreach (var block in function.AsmFunction.Blocks)
        {
            var lastInstruction = block.Instructions.LastOrDefault();
            if (lastInstruction != null && lastInstruction.Type == InstructionType.Ret)
            {
                // If the ret instruction has an immediate operand, it's cleaning its own stack
                if (lastInstruction.StructuredOperands.Count > 0 && 
                    lastInstruction.StructuredOperands[0] is ImmediateOperand immOp && 
                    immOp.Value > 0)
                {
                    cleansOwnStack = true;
                    break;
                }
            }
        }
        
        // If the function cleans its own stack, it's likely stdcall
        if (cleansOwnStack)
        {
            function.CallingConvention = CallingConvention.Stdcall;
            
            // Check for thiscall (ECX used for this pointer)
            // This would require more sophisticated analysis of register usage
        }
        
        // Check for fastcall (first two parameters in ECX and EDX)
        // This would require more sophisticated analysis of register usage
    }
    
    /// <summary>
    /// Infers types for parameters and local variables based on their usage
    /// </summary>
    /// <param name="function">The function to analyze</param>
    private void InferTypes(Function function)
    {
        // This is a complex analysis that would require tracking how variables are used
        // For now, we'll just set default types
        
        // Set return type based on register usage
        function.ReturnType = DataType.Int; // Default to int
        
        // For each parameter, try to infer its type
        foreach (var param in function.Parameters)
        {
            // Default to int for now
            param.Type = DataType.Int;
        }
        
        // For each local variable, try to infer its type
        foreach (var localVar in function.LocalVariables)
        {
            // Default to int for now
            localVar.Type = DataType.Int;
        }
    }
}
