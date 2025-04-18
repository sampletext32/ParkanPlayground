using X86Disassembler.X86;

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
        addressQueue.Enqueue(rvaAddress - _baseAddress);

        // List to store discovered basic blocks
        List<InstructionBlock> blocks = [];
        while (addressQueue.Count > 0)
        {
            // Get the next address to process
            var address = addressQueue.Dequeue();

            // Skip if we've already visited this address
            if (!visitedAddresses.Add(address))
            {
                Console.WriteLine($"Already visited address {address}");
                continue;
            }
            
            // Position the decoder at the current address
            decoder.SetPosition((int) address);

            // Collect instructions for this block
            List<Instruction> instructions = [];

            // Process instructions until we hit a control flow change
            while (true)
            {
                // If we've stepped onto an existing block, create a new block up to this point
                // and stop processing this path (to avoid duplicating instructions)
                if (blocks.Any(x => x.Address == (ulong) decoder.GetPosition()))
                {
                    Console.WriteLine("Stepped on to existing block. Creating in the middle");
                    RegisterBlock(blocks, address, instructions);
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
                    // Register this block (it ends with a conditional jump)
                    RegisterBlock(blocks, address, instructions);
                    
                    // Queue the jump target address for processing
                    addressQueue.Enqueue(
                        instruction.StructuredOperands[0]
                            .GetValue()
                    );
                    
                    // Queue the fall-through address (next instruction after this jump)
                    addressQueue.Enqueue((uint) decoder.GetPosition());
                    break;
                }

                // Check for unconditional jump (e.g., JMP)
                // For unconditional jumps, we only follow the jump target
                if (instruction.Type.IsRegularJump())
                {
                    // Register this block (it ends with an unconditional jump)
                    RegisterBlock(blocks, address, instructions);
                    
                    // Queue the jump target address for processing
                    addressQueue.Enqueue(
                        instruction.StructuredOperands[0]
                            .GetValue()
                    );
                    break;
                }

                // Check for return instruction (e.g., RET, RETF)
                // Returns end a block without any successors
                if (instruction.Type.IsRet())
                {
                    // Register this block (it ends with a return)
                    RegisterBlock(blocks, address, instructions);
                    break;
                }
            }
        }

        // Since blocks aren't necessarily ordered (ASM can jump anywhere it likes)
        // we need to sort the blocks ourselves
        blocks.Sort((b1, b2) => b1.Address.CompareTo(b2.Address));

        return new AsmFunction()
        {
            Address = rvaAddress,
            Blocks = blocks,
        };
    }

    /// <summary>
    /// Creates and registers a new instruction block in the blocks collection
    /// </summary>
    /// <param name="blocks">The list of blocks to add to</param>
    /// <param name="address">The starting address of the block</param>
    /// <param name="instructions">The instructions contained in the block</param>
    public void RegisterBlock(List<InstructionBlock> blocks, ulong address, List<Instruction> instructions)
    {
        // Create a new block with the provided address and instructions
        var block = new InstructionBlock()
        {
            Address = address,
            Instructions = instructions
        };
        
        // Add the block to the collection
        blocks.Add(block);

        // Log the created block for debugging
        Console.WriteLine($"Created block:\n{block}");
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
    public List<Instruction> Instructions { get; set; }

    /// <summary>
    /// Returns a string representation of the block, including its address and instructions
    /// </summary>
    public override string ToString()
    {
        return $"Address: {Address:X8}\n{string.Join("\n", Instructions)}";
    }
}