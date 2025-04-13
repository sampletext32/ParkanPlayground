namespace X86Disassembler.Decompiler;

using System.Collections.Generic;
using X86Disassembler.X86;

/// <summary>
/// Represents a control flow graph for decompilation
/// </summary>
 public class ControlFlowGraph
{
    /// <summary>
    /// Represents a basic block in the control flow graph
    /// </summary>
    public class BasicBlock
    {
        /// <summary>
        /// Gets or sets the starting address of the basic block
        /// </summary>
        public ulong StartAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the ending address of the basic block
        /// </summary>
        public ulong EndAddress { get; set; }
        
        /// <summary>
        /// Gets the list of instructions in this basic block
        /// </summary>
        public List<Instruction> Instructions { get; } = [];
        
        /// <summary>
        /// Gets the list of successor blocks (blocks that can be executed after this one)
        /// </summary>
        public List<BasicBlock> Successors { get; } = [];
        
        /// <summary>
        /// Gets the list of predecessor blocks (blocks that can execute before this one)
        /// </summary>
        public List<BasicBlock> Predecessors { get; } = [];
        
        /// <summary>
        /// Returns a string representation of the basic block
        /// </summary>
        /// <returns>A string representation of the basic block</returns>
        public override string ToString()
        {
            return $"Block {StartAddress:X8}-{EndAddress:X8} with {Instructions.Count} instructions";
        }
    }
    
    // Dictionary mapping addresses to basic blocks
    private readonly Dictionary<ulong, BasicBlock> _blocks = [];
    
    // Entry point of the control flow graph
    private BasicBlock? _entryBlock;
    
    /// <summary>
    /// Gets the entry block of the control flow graph
    /// </summary>
    public BasicBlock? EntryBlock => _entryBlock;
    
    /// <summary>
    /// Gets all basic blocks in the control flow graph
    /// </summary>
    public IReadOnlyDictionary<ulong, BasicBlock> Blocks => _blocks;
    
    /// <summary>
    /// Builds a control flow graph from a list of instructions
    /// </summary>
    /// <param name="instructions">The list of instructions</param>
    /// <param name="entryPoint">The entry point address</param>
    /// <returns>A control flow graph</returns>
    public static ControlFlowGraph Build(List<Instruction> instructions, ulong entryPoint)
    {
        ControlFlowGraph cfg = new ControlFlowGraph();
        
        // First pass: identify basic block boundaries
        HashSet<ulong> leaders = new HashSet<ulong>();
        
        // The entry point is always a leader
        leaders.Add(entryPoint);
        
        // Identify other leaders
        for (int i = 0; i < instructions.Count; i++)
        {
            Instruction inst = instructions[i];
            
            // Check if this instruction is a branch or jump
            if (IsControlTransfer(inst))
            {
                // The target of a jump/branch is a leader
                ulong? targetAddress = GetTargetAddress(inst);
                if (targetAddress.HasValue)
                {
                    leaders.Add(targetAddress.Value);
                }
                
                // The instruction following a jump/branch is also a leader (if it exists)
                if (i + 1 < instructions.Count)
                {
                    leaders.Add(instructions[i + 1].Address);
                }
            }
        }
        
        // Second pass: create basic blocks
        BasicBlock? currentBlock = null;
        
        foreach (Instruction inst in instructions)
        {
            // If this instruction is a leader, start a new basic block
            if (leaders.Contains(inst.Address))
            {
                // Finalize the previous block if it exists
                if (currentBlock != null)
                {
                    currentBlock.EndAddress = inst.Address - 1;
                    cfg._blocks[currentBlock.StartAddress] = currentBlock;
                }
                
                // Create a new block
                currentBlock = new BasicBlock
                {
                    StartAddress = inst.Address
                };
                
                // If this is the entry point, set it as the entry block
                if (inst.Address == entryPoint)
                {
                    cfg._entryBlock = currentBlock;
                }
            }
            
            // Add the instruction to the current block
            if (currentBlock != null)
            {
                currentBlock.Instructions.Add(inst);
            }
            
            // If this instruction is a control transfer, finalize the current block
            if (IsControlTransfer(inst) && currentBlock != null)
            {
                currentBlock.EndAddress = inst.Address;
                cfg._blocks[currentBlock.StartAddress] = currentBlock;
                currentBlock = null;
            }
        }
        
        // Finalize the last block if it exists
        if (currentBlock != null)
        {
            currentBlock.EndAddress = instructions[^1].Address;
            cfg._blocks[currentBlock.StartAddress] = currentBlock;
        }
        
        // Third pass: connect basic blocks
        foreach (var block in cfg._blocks.Values)
        {
            // Get the last instruction in the block
            Instruction lastInst = block.Instructions[^1];
            
            // If the last instruction is a jump, add the target as a successor
            if (IsControlTransfer(lastInst))
            {
                ulong? targetAddress = GetTargetAddress(lastInst);
                if (targetAddress.HasValue && cfg._blocks.TryGetValue(targetAddress.Value, out BasicBlock? targetBlock))
                {
                    block.Successors.Add(targetBlock);
                    targetBlock.Predecessors.Add(block);
                }
                
                // If the instruction is a conditional jump, the next block is also a successor
                if (IsConditionalJump(lastInst))
                {
                    ulong nextAddress = lastInst.Address + (ulong)lastInst.RawBytes.Length;
                    if (cfg._blocks.TryGetValue(nextAddress, out BasicBlock? nextBlock))
                    {
                        block.Successors.Add(nextBlock);
                        nextBlock.Predecessors.Add(block);
                    }
                }
            }
            // If the last instruction is not a jump, the next block is the successor
            else
            {
                ulong nextAddress = lastInst.Address + (ulong)lastInst.RawBytes.Length;
                if (cfg._blocks.TryGetValue(nextAddress, out BasicBlock? nextBlock))
                {
                    block.Successors.Add(nextBlock);
                    nextBlock.Predecessors.Add(block);
                }
            }
        }
        
        return cfg;
    }
    
    /// <summary>
    /// Checks if an instruction is a control transfer instruction (jump, call, ret)
    /// </summary>
    /// <param name="instruction">The instruction to check</param>
    /// <returns>True if the instruction is a control transfer</returns>
    private static bool IsControlTransfer(Instruction instruction)
    {
        string mnemonic = instruction.Mnemonic.ToLower();
        return mnemonic.StartsWith("j") || // All jumps (jmp, je, jne, etc.)
               mnemonic == "call" ||
               mnemonic == "ret";
    }
    
    /// <summary>
    /// Checks if an instruction is a conditional jump
    /// </summary>
    /// <param name="instruction">The instruction to check</param>
    /// <returns>True if the instruction is a conditional jump</returns>
    private static bool IsConditionalJump(Instruction instruction)
    {
        string mnemonic = instruction.Mnemonic.ToLower();
        return mnemonic.StartsWith("j") && mnemonic != "jmp"; // All jumps except jmp
    }
    
    /// <summary>
    /// Gets the target address of a control transfer instruction
    /// </summary>
    /// <param name="instruction">The instruction</param>
    /// <returns>The target address, or null if it cannot be determined</returns>
    private static ulong? GetTargetAddress(Instruction instruction)
    {
        string operands = instruction.Operands;
        
        // Check if the operand is a direct address (e.g., "0x12345678")
        if (operands.StartsWith("0x") && ulong.TryParse(operands.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out ulong address))
        {
            return address;
        }
        
        // For relative jumps, calculate the target address
        if (instruction.Mnemonic.ToLower().StartsWith("j") && int.TryParse(operands, out int offset))
        {
            return instruction.Address + (ulong)instruction.RawBytes.Length + (ulong)offset;
        }
        
        // For now, we cannot determine the target for indirect jumps
        return null;
    }
}
