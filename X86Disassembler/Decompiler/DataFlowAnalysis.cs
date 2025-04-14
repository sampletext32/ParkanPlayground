namespace X86Disassembler.Decompiler;

using System.Collections.Generic;
using X86Disassembler.X86;

/// <summary>
/// Performs data flow analysis on x86 instructions
/// </summary>
public class DataFlowAnalysis
{
    /// <summary>
    /// Represents a variable in the decompiled code
    /// </summary>
    public class Variable
    {
        /// <summary>
        /// Gets or sets the name of the variable
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the type of the variable (if known)
        /// </summary>
        public string Type { get; set; } = "int"; // Default to int
        
        /// <summary>
        /// Gets or sets the storage location (register, memory, etc.)
        /// </summary>
        public string Location { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets whether this variable is a parameter
        /// </summary>
        public bool IsParameter { get; set; }
        
        /// <summary>
        /// Gets or sets whether this variable is a return value
        /// </summary>
        public bool IsReturnValue { get; set; }
    }
    
    /// <summary>
    /// Represents an operation in the decompiled code
    /// </summary>
    public class Operation
    {
        /// <summary>
        /// Gets or sets the operation type
        /// </summary>
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the destination variable
        /// </summary>
        public Variable? Destination { get; set; }
        
        /// <summary>
        /// Gets or sets the source variables or constants
        /// </summary>
        public List<object> Sources { get; } = []; // Can be Variable or constant value
        
        /// <summary>
        /// Gets or sets the original instruction
        /// </summary>
        public Instruction OriginalInstruction { get; set; } = null!;
        
        public ulong InstructionAddress { get; set; }
    }
    
    // Map of register names to variables
    private readonly Dictionary<string, Variable> _registerVariables = [];
    
    // Map of memory locations to variables
    private readonly Dictionary<string, Variable> _memoryVariables = [];
    
    // List of operations
    private readonly List<Operation> _operations = [];
    
    // Counter for generating variable names
    private int _variableCounter = 0;
    
    /// <summary>
    /// Gets the list of operations
    /// </summary>
    public IReadOnlyList<Operation> Operations => _operations;
    
    /// <summary>
    /// Gets the list of variables
    /// </summary>
    public IEnumerable<Variable> Variables
    {
        get
        {
            HashSet<Variable> uniqueVariables = [];
            foreach (var variable in _registerVariables.Values)
            {
                uniqueVariables.Add(variable);
            }
            foreach (var variable in _memoryVariables.Values)
            {
                uniqueVariables.Add(variable);
            }
            return uniqueVariables;
        }
    }
    
    /// <summary>
    /// Analyzes a list of instructions to identify variables and operations
    /// </summary>
    /// <param name="instructions">The list of instructions to analyze</param>
    public void Analyze(List<Instruction> instructions)
    {
        // Initialize common register variables
        InitializeRegisterVariables();
        
        // Process each instruction
        foreach (var instruction in instructions)
        {
            AnalyzeInstruction(instruction);
        }
    }
    
    /// <summary>
    /// Initializes common register variables
    /// </summary>
    private void InitializeRegisterVariables()
    {
        // 32-bit general purpose registers
        _registerVariables["eax"] = new Variable { Name = "eax", Location = "eax" };
        _registerVariables["ebx"] = new Variable { Name = "ebx", Location = "ebx" };
        _registerVariables["ecx"] = new Variable { Name = "ecx", Location = "ecx" };
        _registerVariables["edx"] = new Variable { Name = "edx", Location = "edx" };
        _registerVariables["esi"] = new Variable { Name = "esi", Location = "esi" };
        _registerVariables["edi"] = new Variable { Name = "edi", Location = "edi" };
        _registerVariables["ebp"] = new Variable { Name = "ebp", Location = "ebp" };
        _registerVariables["esp"] = new Variable { Name = "esp", Location = "esp" };
        
        // Mark EAX as the return value register
        _registerVariables["eax"].IsReturnValue = true;
        
        // 16-bit registers
        _registerVariables["ax"] = new Variable { Name = "ax", Location = "ax" };
        _registerVariables["bx"] = new Variable { Name = "bx", Location = "bx" };
        _registerVariables["cx"] = new Variable { Name = "cx", Location = "cx" };
        _registerVariables["dx"] = new Variable { Name = "dx", Location = "dx" };
        _registerVariables["si"] = new Variable { Name = "si", Location = "si" };
        _registerVariables["di"] = new Variable { Name = "di", Location = "di" };
        _registerVariables["bp"] = new Variable { Name = "bp", Location = "bp" };
        _registerVariables["sp"] = new Variable { Name = "sp", Location = "sp" };
        
        // 8-bit registers
        _registerVariables["al"] = new Variable { Name = "al", Location = "al" };
        _registerVariables["ah"] = new Variable { Name = "ah", Location = "ah" };
        _registerVariables["bl"] = new Variable { Name = "bl", Location = "bl" };
        _registerVariables["bh"] = new Variable { Name = "bh", Location = "bh" };
        _registerVariables["cl"] = new Variable { Name = "cl", Location = "cl" };
        _registerVariables["ch"] = new Variable { Name = "ch", Location = "ch" };
        _registerVariables["dl"] = new Variable { Name = "dl", Location = "dl" };
        _registerVariables["dh"] = new Variable { Name = "dh", Location = "dh" };
    }
    
    /// <summary>
    /// Analyzes a single instruction to identify variables and operations
    /// </summary>
    /// <param name="instruction">The instruction to analyze</param>
    private void AnalyzeInstruction(Instruction instruction)
    {
        // Use instruction.Type instead of instruction.Mnemonic
        InstructionType type = instruction.Type;
        
        // Use instruction.StructuredOperands instead of instruction.Operands
        var structuredOperands = instruction.StructuredOperands;
        
        // Skip instructions without operands
        if (structuredOperands == null || structuredOperands.Count == 0)
        {
            return;
        }
        
        // Create a new operation based on the instruction type
        Operation operation = new Operation
        {
            InstructionAddress = instruction.Address,
            Type = GetOperationType(type)
        };
        
        // Process the operation based on the instruction type
        // This would need to be updated to work with structured operands
        // For now, we'll just add a placeholder
        _operations.Add(operation);
    }
    
    private string GetOperationType(InstructionType type)
    {
        switch (type)
        {
            case InstructionType.Add:
                return "add";
            case InstructionType.Sub:
                return "sub";
            case InstructionType.Mul:
                return "mul";
            case InstructionType.Div:
                return "div";
            case InstructionType.And:
                return "and";
            case InstructionType.Or:
                return "or";
            case InstructionType.Xor:
                return "xor";
            case InstructionType.Push:
                return "push";
            case InstructionType.Pop:
                return "pop";
            case InstructionType.Call:
                return "call";
            case InstructionType.Ret:
                return "return";
            case InstructionType.Cmp:
                return "cmp";
            case InstructionType.Test:
                return "test";
            case InstructionType.Jmp:
                return "jmp";
            case InstructionType.Je:
                return "je";
            case InstructionType.Jne:
                return "jne";
            case InstructionType.Jg:
                return "jg";
            case InstructionType.Jge:
                return "jge";
            case InstructionType.Jl:
                return "jl";
            case InstructionType.Jle:
                return "jle";
            default:
                return type.ToString();
        }
    }
    
    /// <summary>
    /// Handles a MOV instruction
    /// </summary>
    /// <param name="operation">The operation to populate</param>
    /// <param name="operandParts">The operand parts</param>
    private void HandleMovInstruction(Operation operation, string[] operandParts)
    {
        if (operandParts.Length != 2)
        {
            return;
        }
        
        operation.Type = "assignment";
        
        // Get or create the destination variable
        Variable destination = GetOrCreateVariable(operandParts[0]);
        operation.Destination = destination;
        
        // Get the source (variable or constant)
        object source = GetOperandValue(operandParts[1]);
        operation.Sources.Add(source);
    }
    
    /// <summary>
    /// Handles an arithmetic instruction (ADD, SUB, MUL, DIV, AND, OR, XOR)
    /// </summary>
    /// <param name="operation">The operation to populate</param>
    /// <param name="mnemonic">The instruction mnemonic</param>
    /// <param name="operandParts">The operand parts</param>
    private void HandleArithmeticInstruction(Operation operation, string mnemonic, string[] operandParts)
    {
        if (operandParts.Length != 2)
        {
            return;
        }
        
        operation.Type = mnemonic;
        
        // Get or create the destination variable
        Variable destination = GetOrCreateVariable(operandParts[0]);
        operation.Destination = destination;
        
        // Get the source (variable or constant)
        object source = GetOperandValue(operandParts[1]);
        operation.Sources.Add(source);
        operation.Sources.Add(destination); // The destination is also a source in arithmetic operations
    }
    
    /// <summary>
    /// Handles a stack instruction (PUSH, POP)
    /// </summary>
    /// <param name="operation">The operation to populate</param>
    /// <param name="mnemonic">The instruction mnemonic</param>
    /// <param name="operandParts">The operand parts</param>
    private void HandleStackInstruction(Operation operation, string mnemonic, string[] operandParts)
    {
        if (operandParts.Length != 1)
        {
            return;
        }
        
        operation.Type = mnemonic;
        
        if (mnemonic == "push")
        {
            // For PUSH, the operand is the source
            object source = GetOperandValue(operandParts[0]);
            operation.Sources.Add(source);
        }
        else if (mnemonic == "pop")
        {
            // For POP, the operand is the destination
            Variable destination = GetOrCreateVariable(operandParts[0]);
            operation.Destination = destination;
        }
    }
    
    /// <summary>
    /// Handles a CALL instruction
    /// </summary>
    /// <param name="operation">The operation to populate</param>
    /// <param name="operandParts">The operand parts</param>
    private void HandleCallInstruction(Operation operation, string[] operandParts)
    {
        if (operandParts.Length != 1)
        {
            return;
        }
        
        operation.Type = "call";
        
        // The operand is the function name or address
        operation.Sources.Add(operandParts[0]);
    }
    
    /// <summary>
    /// Handles a RET instruction
    /// </summary>
    /// <param name="operation">The operation to populate</param>
    private void HandleReturnInstruction(Operation operation)
    {
        operation.Type = "return";
        
        // The return value is in EAX
        if (_registerVariables.TryGetValue("eax", out Variable? eax))
        {
            operation.Sources.Add(eax);
        }
    }
    
    /// <summary>
    /// Handles a comparison instruction (CMP, TEST)
    /// </summary>
    /// <param name="operation">The operation to populate</param>
    /// <param name="mnemonic">The instruction mnemonic</param>
    /// <param name="operandParts">The operand parts</param>
    private void HandleComparisonInstruction(Operation operation, string mnemonic, string[] operandParts)
    {
        if (operandParts.Length != 2)
        {
            return;
        }
        
        operation.Type = mnemonic;
        
        // Get the operands
        object left = GetOperandValue(operandParts[0]);
        object right = GetOperandValue(operandParts[1]);
        
        operation.Sources.Add(left);
        operation.Sources.Add(right);
    }
    
    /// <summary>
    /// Handles a jump instruction (JMP, JE, JNE, etc.)
    /// </summary>
    /// <param name="operation">The operation to populate</param>
    /// <param name="mnemonic">The instruction mnemonic</param>
    /// <param name="operandParts">The operand parts</param>
    private void HandleJumpInstruction(Operation operation, string mnemonic, string[] operandParts)
    {
        if (operandParts.Length != 1)
        {
            return;
        }
        
        operation.Type = mnemonic;
        
        // The operand is the jump target
        operation.Sources.Add(operandParts[0]);
    }
    
    /// <summary>
    /// Gets or creates a variable for an operand
    /// </summary>
    /// <param name="operand">The operand string</param>
    /// <returns>The variable</returns>
    private Variable GetOrCreateVariable(string operand)
    {
        // Check if it's a register
        if (IsRegister(operand))
        {
            string register = operand.ToLower();
            if (_registerVariables.TryGetValue(register, out Variable? variable))
            {
                return variable;
            }
        }
        
        // Check if it's a memory location
        if (IsMemoryLocation(operand))
        {
            string normalizedLocation = NormalizeMemoryLocation(operand);
            if (_memoryVariables.TryGetValue(normalizedLocation, out Variable? variable))
            {
                return variable;
            }
            
            // Create a new variable for this memory location
            variable = new Variable
            {
                Name = $"var_{_variableCounter++}",
                Location = normalizedLocation
            };
            
            _memoryVariables[normalizedLocation] = variable;
            return variable;
        }
        
        // If it's neither a register nor a memory location, create a temporary variable
        Variable tempVariable = new Variable
        {
            Name = $"temp_{_variableCounter++}",
            Location = operand
        };
        
        return tempVariable;
    }
    
    /// <summary>
    /// Gets the value of an operand (variable or constant)
    /// </summary>
    /// <param name="operand">The operand string</param>
    /// <returns>The operand value (Variable or constant)</returns>
    private object GetOperandValue(string operand)
    {
        // Check if it's a register or memory location
        if (IsRegister(operand) || IsMemoryLocation(operand))
        {
            return GetOrCreateVariable(operand);
        }
        
        // Check if it's a hexadecimal constant
        if (operand.StartsWith("0x") && operand.Length > 2)
        {
            if (int.TryParse(operand.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out int value))
            {
                return value;
            }
        }
        
        // Check if it's a decimal constant
        if (int.TryParse(operand, out int decimalValue))
        {
            return decimalValue;
        }
        
        // Otherwise, return the operand as a string
        return operand;
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
    
    /// <summary>
    /// Normalizes a memory location operand
    /// </summary>
    /// <param name="operand">The operand to normalize</param>
    /// <returns>The normalized memory location</returns>
    private string NormalizeMemoryLocation(string operand)
    {
        // Extract the part inside the brackets
        int startIndex = operand.IndexOf('[');
        int endIndex = operand.IndexOf(']');
        
        if (startIndex >= 0 && endIndex > startIndex)
        {
            string memoryReference = operand.Substring(startIndex + 1, endIndex - startIndex - 1).Trim();
            return memoryReference;
        }
        
        return operand;
    }
}
