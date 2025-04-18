using X86Disassembler.X86;
using X86Disassembler.X86.Operands;

namespace X86Disassembler.Analysers;

/// <summary>
/// Disassembles code into basic blocks by following control flow instructions.
/// A basic block is a sequence of instructions with a single entry point (the first instruction)
/// and a single exit point (the last instruction, typically a jump or return).
/// </summary>
public class BlockDisassembler
{
    // The buffer containing the code to disassemble
    private readonly byte[] _codeBuffer;

    // The length of the buffer
    private readonly int _length;

    // The base address of the code
    private readonly ulong _baseAddress;

    /// <summary>
    /// Initializes a new instance of the BlockDisassembler class
    /// </summary>
    /// <param name="codeBuffer">The raw code bytes to be disassembled</param>
    /// <param name="baseAddress">The base RVA (Relative Virtual Address) of the code section</param>
    public BlockDisassembler(byte[] codeBuffer, ulong baseAddress)
    {
        _codeBuffer = codeBuffer;
        _length = codeBuffer.Length;

        _baseAddress = baseAddress;
    }

    /// <summary>
    /// Disassembles code starting from the specified RVA address by following control flow.
    /// Creates blocks of instructions separated by jumps, branches, and returns.
    /// </summary>
    /// <param name="rvaAddress">The RVA (Relative Virtual Address) to start disassembly from</param>
    /// <returns>A list of instruction blocks representing the control flow of the code</returns>
    public AsmFunction DisassembleFromAddress(uint rvaAddress)
    {
        // Create instruction decoder for parsing the code buffer
        InstructionDecoder decoder = new InstructionDecoder(_codeBuffer, _length);

        // Track visited addresses to prevent infinite loops
        HashSet<ulong> visitedAddresses = [];

        // Queue of addresses to process (breadth-first approach)
        Queue<ulong> addressQueue = [];
        
        // Calculate the file offset from the RVA by subtracting the base address
        // Store the file offset for processing, but we'll convert back to RVA when creating blocks
        ulong fileOffset = rvaAddress - _baseAddress;
        addressQueue.Enqueue(fileOffset);
        
        // Keep track of the original entry point RVA for the function
        ulong entryPointRVA = rvaAddress;

        // List to store discovered basic blocks
        List<InstructionBlock> blocks = [];
        
        // Dictionary to track blocks by address for quick lookup
        Dictionary<ulong, InstructionBlock> blocksByAddress = new Dictionary<ulong, InstructionBlock>();
        
        while (addressQueue.Count > 0)
        {
            // Get the next address to process
            var address = addressQueue.Dequeue();

            // Skip if we've already visited this address
            if (!visitedAddresses.Add(address))
            {
                // Skip addresses we've already processed
                continue;
            }
            
            // Position the decoder at the current address
            decoder.SetPosition((int) address);

            // Collect instructions for this block
            List<Instruction> instructions = [];
            
            // Get the current block if it exists (for tracking predecessors)
            InstructionBlock? currentBlock = null;
            if (blocksByAddress.TryGetValue(address, out var existingBlock))
            {
                currentBlock = existingBlock;
            }

            // Process instructions until we hit a control flow change
            while (true)
            {
                // Get the current position
                ulong currentPosition = (ulong)decoder.GetPosition();
                
                // If we've stepped onto an existing block, create a new block up to this point
                // and stop processing this path (to avoid duplicating instructions)
                if (blocksByAddress.TryGetValue(currentPosition, out var targetBlock) && currentPosition != address)
                {
                    // We've stepped onto an existing block, create a new one up to this point
                    
                    // Register this block and establish the relationship with the target block
                    var newBlock = RegisterBlock(blocks, address, instructions, null, false, false);
                    blocksByAddress[address] = newBlock;
                    
                    // Add the target block as a successor to the new block
                    newBlock.Successors.Add(targetBlock);
                    
                    // Add the new block as a predecessor to the target block
                    targetBlock.Predecessors.Add(newBlock);
                    
                    break;
                }

                // Decode the next instruction
                var instruction = decoder.DecodeInstruction();

                // Handle decoding failures
                if (instruction is null)
                {
                    throw new InvalidOperationException($"Unexpectedly failed to decode instruction at {address}");
                }

                // Add the instruction to the current block
                instructions.Add(instruction);

                // Check for conditional jump (e.g., JZ, JNZ, JLE)
                // For conditional jumps, we need to follow both the jump target and the fall-through path
                if (instruction.Type.IsConditionalJump())
                {
                    // Get the jump target address
                    uint jumpTargetAddress = instruction.StructuredOperands[0].GetValue();
                    
                    // Get the fall-through address (next instruction after this jump)
                    uint fallThroughAddress = (uint)decoder.GetPosition();
                    
                    // Register this block (it ends with a conditional jump)
                    var newBlock = RegisterBlock(blocks, address, instructions, currentBlock, false, false);
                    blocksByAddress[address] = newBlock;
                    
                    // Register the target block if it doesn't exist yet
                    InstructionBlock? jumpTargetBlock = null;
                    if (blocksByAddress.TryGetValue(jumpTargetAddress, out var existingTargetBlock))
                    {
                        jumpTargetBlock = existingTargetBlock;
                    }
                    else
                    {
                        // We'll create this block later when we process the queue
                        // For now, just queue it for processing
                        addressQueue.Enqueue(jumpTargetAddress);
                    }
                    
                    // Register the fall-through block if it doesn't exist yet
                    InstructionBlock? fallThroughBlock = null;
                    if (blocksByAddress.TryGetValue(fallThroughAddress, out var existingFallThroughBlock))
                    {
                        fallThroughBlock = existingFallThroughBlock;
                    }
                    else
                    {
                        // We'll create this block later when we process the queue
                        // For now, just queue it for processing
                        addressQueue.Enqueue(fallThroughAddress);
                    }
                    
                    // If the jump target block exists, add it as a successor to the current block
                    if (jumpTargetBlock != null)
                    {
                        newBlock.Successors.Add(jumpTargetBlock);
                        jumpTargetBlock.Predecessors.Add(newBlock);
                    }
                    
                    // If the fall-through block exists, add it as a successor to the current block
                    if (fallThroughBlock != null)
                    {
                        newBlock.Successors.Add(fallThroughBlock);
                        fallThroughBlock.Predecessors.Add(newBlock);
                    }
                    
                    break;
                }

                // Check for unconditional jump (e.g., JMP)
                // For unconditional jumps, we only follow the jump target
                if (instruction.Type.IsRegularJump())
                {
                    // Get the jump target address
                    uint jumpTargetAddress = instruction.StructuredOperands[0].GetValue();
                    
                    // Register this block (it ends with an unconditional jump)
                    var newBlock = RegisterBlock(blocks, address, instructions, currentBlock, false, false);
                    blocksByAddress[address] = newBlock;
                    
                    // Register the target block if it doesn't exist yet
                    InstructionBlock? jumpTargetBlock = null;
                    if (blocksByAddress.TryGetValue(jumpTargetAddress, out var existingTargetBlock))
                    {
                        jumpTargetBlock = existingTargetBlock;
                    }
                    else
                    {
                        // We'll create this block later when we process the queue
                        // For now, just queue it for processing
                        addressQueue.Enqueue(jumpTargetAddress);
                    }
                    
                    // If the jump target block exists, add it as a successor to the current block
                    if (jumpTargetBlock != null)
                    {
                        newBlock.Successors.Add(jumpTargetBlock);
                        jumpTargetBlock.Predecessors.Add(newBlock);
                    }
                    
                    break;
                }

                // Check for return instruction (e.g., RET, RETF)
                // Returns end a block without any successors
                if (instruction.Type.IsRet())
                {
                    // Register this block (it ends with a return)
                    var newBlock = RegisterBlock(blocks, address, instructions, currentBlock, false, false);
                    blocksByAddress[address] = newBlock;
                    
                    break;
                }
            }
        }

        // Since blocks aren't necessarily ordered (ASM can jump anywhere it likes)
        // we need to sort the blocks ourselves
        blocks.Sort((b1, b2) => b1.Address.CompareTo(b2.Address));

        // First, establish the successor and predecessor relationships based on file offsets
        // This is done by analyzing the last instruction of each block
        foreach (var block in blocks)
        {
            if (block.Instructions.Count == 0) continue;
            
            var lastInstruction = block.Instructions[^1];
            
            // Check if the last instruction is a conditional jump
            if (lastInstruction.Type.IsConditionalJump())
            {
                // Get the jump target address (file offset)
                ulong targetAddress = 0;
                if (lastInstruction.StructuredOperands.Count > 0 && lastInstruction.StructuredOperands[0] is RelativeOffsetOperand relOp)
                {
                    targetAddress = relOp.TargetAddress;
                }
                
                // Find the target block
                var targetBlock = blocks.FirstOrDefault(b => b.Address == targetAddress);
                if (targetBlock != null)
                {
                    // Add the target block as a successor to this block
                    if (!block.Successors.Contains(targetBlock))
                    {
                        block.Successors.Add(targetBlock);
                    }
                    
                    // Add this block as a predecessor to the target block
                    if (!targetBlock.Predecessors.Contains(block))
                    {
                        targetBlock.Predecessors.Add(block);
                    }
                    
                    // For conditional jumps, also add the fall-through block as a successor
                    // The fall-through block is the one that immediately follows this block in memory
                    // Find the next block in address order
                    var nextBlock = blocks.OrderBy(b => b.Address).FirstOrDefault(b => b.Address > block.Address);
                    if (nextBlock != null)
                    {
                        // The fall-through block is the one that immediately follows this block in memory
                        var fallThroughBlock = nextBlock;
                        
                        // Add the fall-through block as a successor to this block
                        if (!block.Successors.Contains(fallThroughBlock))
                        {
                            block.Successors.Add(fallThroughBlock);
                        }
                        
                        // Add this block as a predecessor to the fall-through block
                        if (!fallThroughBlock.Predecessors.Contains(block))
                        {
                            fallThroughBlock.Predecessors.Add(block);
                        }
                    }
                }
            }
            // Check if the last instruction is an unconditional jump
            else if (lastInstruction.Type == InstructionType.Jmp)
            {
                // Get the jump target address (file offset)
                ulong targetAddress = 0;
                if (lastInstruction.StructuredOperands.Count > 0 && lastInstruction.StructuredOperands[0] is RelativeOffsetOperand relOp)
                {
                    targetAddress = relOp.TargetAddress;
                }
                
                // Find the target block
                var targetBlock = blocks.FirstOrDefault(b => b.Address == targetAddress);
                if (targetBlock != null)
                {
                    // Add the target block as a successor to this block
                    if (!block.Successors.Contains(targetBlock))
                    {
                        block.Successors.Add(targetBlock);
                    }
                    
                    // Add this block as a predecessor to the target block
                    if (!targetBlock.Predecessors.Contains(block))
                    {
                        targetBlock.Predecessors.Add(block);
                    }
                }
            }
            // For non-jump instructions that don't end the function (like Ret), add the fall-through block
            else if (!lastInstruction.Type.IsRet())
            {
                // The fall-through block is the one that immediately follows this block in memory
                // Find the next block in address order
                var nextBlock = blocks.OrderBy(b => b.Address).FirstOrDefault(b => b.Address > block.Address);
                if (nextBlock != null)
                {
                    // The fall-through block is the one that immediately follows this block in memory
                    var fallThroughBlock = nextBlock;
                    
                    // Add the fall-through block as a successor to this block
                    if (!block.Successors.Contains(fallThroughBlock))
                    {
                        block.Successors.Add(fallThroughBlock);
                    }
                    
                    // Add this block as a predecessor to the fall-through block
                    if (!fallThroughBlock.Predecessors.Contains(block))
                    {
                        fallThroughBlock.Predecessors.Add(block);
                    }
                }
            }
        }

        // Store the original file offset for each block in a dictionary
        Dictionary<InstructionBlock, ulong> blockToFileOffset = new Dictionary<InstructionBlock, ulong>();
        foreach (var block in blocks)
        {
            blockToFileOffset[block] = block.Address;
        }

        // Convert all block addresses from file offsets to RVA
        // and update the block dictionary for quick lookup
        Dictionary<ulong, InstructionBlock> rvaBlocksByAddress = new Dictionary<ulong, InstructionBlock>();
        Dictionary<ulong, ulong> fileOffsetToRvaMap = new Dictionary<ulong, ulong>();
        
        // First pass: create a mapping from file offset to RVA for each block
        foreach (var block in blocks)
        {
            // Get the original file offset address
            ulong blockFileOffset = block.Address;
            
            // Calculate the RVA address
            ulong blockRvaAddress = blockFileOffset + _baseAddress;
            
            // Store the mapping
            fileOffsetToRvaMap[blockFileOffset] = blockRvaAddress;
        }
        
        // Second pass: update all blocks to use RVA addresses
        foreach (var block in blocks)
        {
            // Get the original file offset address
            ulong blockFileOffset = block.Address;
            
            // Update the block's address to RVA
            ulong blockRvaAddress = fileOffsetToRvaMap[blockFileOffset];
            block.Address = blockRvaAddress;
            
            // Add to the dictionary for quick lookup
            rvaBlocksByAddress[blockRvaAddress] = block;
        }
        
        // Now update all successors and predecessors to use the correct RVA addresses
        foreach (var block in blocks)
        {
            // Create new lists for successors and predecessors with the correct RVA addresses
            List<InstructionBlock> updatedSuccessors = new List<InstructionBlock>();
            List<InstructionBlock> updatedPredecessors = new List<InstructionBlock>();
            
            // Update successors
            foreach (var successor in block.Successors)
            {
                // Get the original file offset of the successor
                if (blockToFileOffset.TryGetValue(successor, out ulong successorFileOffset))
                {
                    // Look up the RVA address in our mapping
                    if (fileOffsetToRvaMap.TryGetValue(successorFileOffset, out ulong successorRvaAddress))
                    {
                        // Find the block with this RVA address
                        if (rvaBlocksByAddress.TryGetValue(successorRvaAddress, out var rvaSuccessor))
                        {
                            updatedSuccessors.Add(rvaSuccessor);
                        }
                    }
                }
            }
            
            // Update predecessors
            foreach (var predecessor in block.Predecessors)
            {
                // Get the original file offset of the predecessor
                if (blockToFileOffset.TryGetValue(predecessor, out ulong predecessorFileOffset))
                {
                    // Look up the RVA address in our mapping
                    if (fileOffsetToRvaMap.TryGetValue(predecessorFileOffset, out ulong predecessorRvaAddress))
                    {
                        // Find the block with this RVA address
                        if (rvaBlocksByAddress.TryGetValue(predecessorRvaAddress, out var rvaPredecessor))
                        {
                            updatedPredecessors.Add(rvaPredecessor);
                        }
                    }
                }
            }
            
            // Replace the old lists with the updated ones
            block.Successors = updatedSuccessors;
            block.Predecessors = updatedPredecessors;
        }
        
        // Create a new AsmFunction with the RVA address
        var asmFunction = new AsmFunction()
        {
            Address = entryPointRVA,
            Blocks = blocks,
        };
        
        // Verify that the entry block exists (no need to log this information)
        
        return asmFunction;
    }

    /// <summary>
    /// Creates and registers a new instruction block in the blocks collection
    /// </summary>
    /// <param name="blocks">The list of blocks to add to</param>
    /// <param name="address">The starting address of the block</param>
    /// <param name="instructions">The instructions contained in the block</param>
    /// <param name="currentBlock">The current block being processed (null if this is the first block)</param>
    /// <param name="isJumpTarget">Whether this block is a jump target</param>
    /// <param name="isFallThrough">Whether this block is a fall-through from another block</param>
    /// <returns>The newly created block</returns>
    public InstructionBlock RegisterBlock(
        List<InstructionBlock> blocks, 
        ulong address, 
        List<Instruction> instructions, 
        InstructionBlock? currentBlock = null, 
        bool isJumpTarget = false, 
        bool isFallThrough = false)
    {
        // Check if a block already exists at this address
        var existingBlock = blocks.FirstOrDefault(b => b.Address == address);
        
        if (existingBlock != null)
        {
            // If the current block is not null, update the relationships
            if (currentBlock != null)
            {
                // Add the existing block as a successor to the current block if not already present
                if (!currentBlock.Successors.Contains(existingBlock))
                {
                    currentBlock.Successors.Add(existingBlock);
                }
                
                // Add the current block as a predecessor to the existing block if not already present
                if (!existingBlock.Predecessors.Contains(currentBlock))
                {
                    existingBlock.Predecessors.Add(currentBlock);
                }
            }
            
            return existingBlock;
        }
        
        // Create a new block with the provided address and instructions
        var block = new InstructionBlock()
        {
            Address = address,
            Instructions = new List<Instruction>(instructions) // Create a copy of the instructions list
        };
        
        // Add the block to the collection
        blocks.Add(block);
        
        // If the current block is not null, update the relationships
        if (currentBlock != null)
        {
            // Add the new block as a successor to the current block
            currentBlock.Successors.Add(block);
            
            // Add the current block as a predecessor to the new block
            block.Predecessors.Add(currentBlock);
        }
        
        return block;
    }
}

/// <summary>
/// Represents a basic block of instructions with a single entry and exit point
/// </summary>
public class InstructionBlock
{
    /// <summary>
    /// The starting address of the block
    /// </summary>
    public ulong Address { get; set; }
    
    /// <summary>
    /// The list of instructions contained in this block
    /// </summary>
    public List<Instruction> Instructions { get; set; } = [];

    /// <summary>
    /// The blocks that can transfer control to this block
    /// </summary>
    public List<InstructionBlock> Predecessors { get; set; } = [];

    /// <summary>
    /// The blocks that this block can transfer control to
    /// </summary>
    public List<InstructionBlock> Successors { get; set; } = [];

    /// <summary>
    /// Returns a string representation of the block, including its address, instructions, and control flow information
    /// </summary>
    public override string ToString()
    {
        // Create a string for predecessors
        string predecessorsStr = Predecessors.Count > 0 
            ? $"Predecessors: {string.Join(", ", Predecessors.Select(p => $"0x{p.Address:X8}"))}"
            : "No predecessors";
            
        // Create a string for successors
        string successorsStr = Successors.Count > 0 
            ? $"Successors: {string.Join(", ", Successors.Select(s => $"0x{s.Address:X8}"))}"
            : "No successors";
            
        // Return the complete string representation
        return $"Address: 0x{Address:X8}\n{predecessorsStr}\n{successorsStr}\n{string.Join("\n", Instructions)}";
    }
}