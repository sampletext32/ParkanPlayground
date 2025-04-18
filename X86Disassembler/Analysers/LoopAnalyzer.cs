namespace X86Disassembler.Analysers;

/// <summary>
/// Analyzes the control flow graph to identify loops
/// </summary>
public class LoopAnalyzer
{
    /// <summary>
    /// Identifies loops in the given function and stores them in the analyzer context
    /// </summary>
    /// <param name="context">The analyzer context to store results in</param>
    public void AnalyzeLoops(AnalyzerContext context)
    {
        // A back edge is an edge from a node to one of its dominators
        // For our simplified approach, we'll identify back edges as edges that point to blocks
        // with a lower address (potential loop headers)
        foreach (var block in context.Function.Blocks)
        {
            foreach (var successor in block.Successors)
            {
                // If the successor has a lower address than the current block,
                // it's potentially a back edge forming a loop
                if (successor.Address < block.Address)
                {
                    // Create a new loop with the identified back edge
                    var loop = new AnalyzerContext.Loop
                    {
                        Header = successor,
                        BackEdge = (block, successor)
                    };
                    
                    // Find all blocks in the loop using a breadth-first search
                    FindLoopBlocks(loop);
                    
                    // Find the exit blocks of the loop
                    FindLoopExits(loop);
                    
                    // Store the loop in the context
                    context.LoopsByHeaderAddress[successor.Address] = loop;
                    
                    // Update the blocks-to-loops mapping
                    foreach (var loopBlock in loop.Blocks)
                    {
                        if (!context.LoopsByBlockAddress.TryGetValue(loopBlock.Address, out var loops))
                        {
                            loops = [];
                            context.LoopsByBlockAddress[loopBlock.Address] = loops;
                        }
                        
                        loops.Add(loop);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Finds all blocks that are part of the loop
    /// </summary>
    /// <param name="loop">The loop to analyze</param>
    private void FindLoopBlocks(AnalyzerContext.Loop loop)
    {
        // Start with the header block
        loop.Blocks.Add(loop.Header);
        
        // Use a queue for breadth-first search
        Queue<InstructionBlock> queue = new Queue<InstructionBlock>();
        queue.Enqueue(loop.BackEdge.From); // Start from the back edge source
        
        // Keep track of visited blocks to avoid cycles
        HashSet<ulong> visited = new HashSet<ulong> { loop.Header.Address };
        
        while (queue.Count > 0)
        {
            var block = queue.Dequeue();
            
            // If we've already processed this block, skip it
            if (!visited.Add(block.Address))
            {
                continue;
            }
            
            // Add the block to the loop
            loop.Blocks.Add(block);
            
            // Add all predecessors to the queue (except those that would take us outside the loop)
            foreach (var predecessor in block.Predecessors)
            {
                // Skip the header's predecessors that aren't in the loop already
                // (to avoid including blocks outside the loop)
                if (block == loop.Header && !loop.Blocks.Contains(predecessor) && predecessor != loop.BackEdge.From)
                {
                    continue;
                }
                
                queue.Enqueue(predecessor);
            }
        }
    }
    
    /// <summary>
    /// Finds all exit blocks of the loop (blocks that have successors outside the loop)
    /// </summary>
    /// <param name="loop">The loop to analyze</param>
    private void FindLoopExits(AnalyzerContext.Loop loop)
    {
        foreach (var block in loop.Blocks)
        {
            foreach (var successor in block.Successors)
            {
                // If the successor is not part of the loop, this block is an exit
                if (!loop.Blocks.Contains(successor))
                {
                    loop.ExitBlocks.Add(block);
                    break; // Once we've identified this block as an exit, we can stop checking its successors
                }
            }
        }
    }
}
