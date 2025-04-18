using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86;

using System.Text;
using System.Collections.Generic;

/// <summary>
/// Core x86 instruction disassembler
/// </summary>
public class Disassembler
{
    // The buffer containing the code to disassemble
    private readonly byte[] _codeBuffer;

    // The length of the buffer
    private readonly int _length;

    // The base address of the code
    private readonly ulong _baseAddress;

    // Segment override prefixes
    private static readonly byte[] SegmentOverridePrefixes = {0x26, 0x2E, 0x36, 0x3E, 0x64, 0x65};

    /// <summary>
    /// Initializes a new instance of the Disassembler class
    /// </summary>
    /// <param name="codeBuffer">The buffer containing the code to disassemble</param>
    /// <param name="baseAddress">The base address of the code</param>
    public Disassembler(byte[] codeBuffer, ulong baseAddress)
    {
        _codeBuffer = codeBuffer;
        _length = codeBuffer.Length;
        _baseAddress = baseAddress;
    }

    /// <summary>
    /// Checks if a byte is a segment override prefix
    /// </summary>
    /// <param name="b">The byte to check</param>
    /// <returns>True if the byte is a segment override prefix</returns>
    private bool IsSegmentOverridePrefix(byte b)
    {
        return Array.IndexOf(SegmentOverridePrefixes, b) >= 0;
    }

    /// <summary>
    /// Gets the segment override name for a prefix byte
    /// </summary>
    /// <param name="prefix">The prefix byte</param>
    /// <returns>The segment override name</returns>
    private string GetSegmentOverrideName(byte prefix)
    {
        return prefix switch
        {
            0x26 => "es",
            0x2E => "cs",
            0x36 => "ss",
            0x3E => "ds",
            0x64 => "fs",
            0x65 => "gs",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Disassembles the code buffer sequentially and returns all disassembled instructions
    /// </summary>
    /// <returns>A list of disassembled instructions</returns>
    public List<Instruction> Disassemble()
    {
        List<Instruction> instructions = new List<Instruction>();

        // Create an instruction decoder
        InstructionDecoder decoder = new InstructionDecoder(_codeBuffer, _length);

        // Decode instructions until the end of the buffer is reached
        while (true)
        {
            int position = decoder.GetPosition();

            // Check if we've reached the end of the buffer
            if (!decoder.CanReadByte())
            {
                break;
            }

            // Store the position before decoding to handle prefixes properly
            int startPosition = position;

            // Decode the instruction
            Instruction? instruction = decoder.DecodeInstruction();

            if (instruction != null)
            {
                // Adjust the instruction address to include the base address
                instruction.Address = _baseAddress + (uint)startPosition;

                // Add the instruction to the list
                instructions.Add(instruction);
            }
            else
            {
                // If decoding failed, create a dummy instruction for the unknown byte
                byte unknownByte = decoder.ReadByte();

                Instruction dummyInstruction = new Instruction
                {
                    Address = _baseAddress + (uint)position,
                    Type = InstructionType.Unknown,
                    StructuredOperands = [OperandFactory.CreateImmediateOperand(unknownByte, 8),]
                };

                instructions.Add(dummyInstruction);
            }
        }

        return instructions;
    }

    /// <summary>
    /// Disassembles a function starting from a specific virtual address (RVA) and follows control flow
    /// </summary>
    /// <param name="startRva">The relative virtual address to start disassembly from</param>
    /// <returns>A list of disassembled instructions representing the function</returns>
    public List<Instruction> DisassembleFunction(uint startRva)
    {
        // The _baseAddress is the section's RVA (stored in Program.cs)
        // We need to calculate the offset within the section by subtracting the section's RVA from the start RVA
        int startOffset = (int)(startRva - _baseAddress);
        
        // Validate the offset is within bounds
        if (startOffset < 0 || startOffset >= _length)
        {
            throw new ArgumentOutOfRangeException(nameof(startRva), 
                $"Start address 0x{startRva:X8} is outside the bounds of the section at RVA 0x{_baseAddress:X8} with size {_length}");
        }

        return DisassembleFromOffset(startOffset);
    }

    /// <summary>
    /// Disassembles instructions starting from a specific offset using control flow analysis
    /// </summary>
    /// <param name="startOffset">The offset in the code buffer to start disassembly from</param>
    /// <returns>A list of disassembled instructions</returns>
    private List<Instruction> DisassembleFromOffset(int startOffset)
    {
        // Keep track of disassembled instructions
        List<Instruction> instructions = new List<Instruction>();
        
        // Track visited addresses to avoid infinite loops
        HashSet<int> visitedOffsets = new HashSet<int>();
        
        // Queue of offsets to process
        Queue<int> offsetQueue = new Queue<int>();
        offsetQueue.Enqueue(startOffset);
        
        while (offsetQueue.Count > 0)
        {
            int currentOffset = offsetQueue.Dequeue();
            
            // Skip if we've already processed this offset
            if (visitedOffsets.Contains(currentOffset))
            {
                continue;
            }
            
            // Create a new decoder positioned at the current offset
            InstructionDecoder decoder = new InstructionDecoder(_codeBuffer, _length);
            decoder.SetPosition(currentOffset);
            
            // Process instructions at this address until we hit a control flow change
            while (decoder.CanReadByte() && decoder.GetPosition() < _length)
            {
                int positionBeforeDecode = decoder.GetPosition();
                visitedOffsets.Add(positionBeforeDecode);
                
                // Decode the instruction
                Instruction? instruction = decoder.DecodeInstruction();
                if (instruction == null)
                {
                    // Invalid instruction, skip to next byte
                    decoder.SetPosition(positionBeforeDecode + 1);
                    continue;
                }
                
                // Set the instruction address
                instruction.Address = _baseAddress + (uint)positionBeforeDecode;
                
                // Add the instruction to our list
                instructions.Add(instruction);
                
                // Check for control flow instructions
                if (IsReturnInstruction(instruction))
                {
                    // End of function, don't follow any further from this branch
                    break;
                }
                else if (IsUnconditionalJump(instruction))
                {
                    // Follow the unconditional jump target
                    int? targetOffset = GetJumpTargetOffset(instruction, positionBeforeDecode);
                    if (targetOffset.HasValue && targetOffset.Value >= 0 && targetOffset.Value < _length)
                    {
                        offsetQueue.Enqueue(targetOffset.Value);
                    }
                    
                    // End this branch of execution
                    break;
                }
                else if (IsConditionalJump(instruction))
                {
                    // Follow both paths for conditional jumps (target and fall-through)
                    int? targetOffset = GetJumpTargetOffset(instruction, positionBeforeDecode);
                    if (targetOffset.HasValue && targetOffset.Value >= 0 && targetOffset.Value < _length)
                    {
                        offsetQueue.Enqueue(targetOffset.Value);
                    }
                    
                    // Continue with fall-through path in this loop
                }
                else if (IsCallInstruction(instruction))
                {
                    // For calls, we just continue with the next instruction (we don't follow the call)
                    // We could add separate functionality to follow calls if needed
                }
            }
        }
        
        // Sort instructions by address for readability
        instructions.Sort((a, b) => a.Address.CompareTo(b.Address));
        
        return instructions;
    }

    /// <summary>
    /// Checks if an instruction is a return instruction
    /// </summary>
    private bool IsReturnInstruction(Instruction instruction)
    {
        return instruction.Type == InstructionType.Ret || 
               instruction.Type == InstructionType.Retf;
    }

    /// <summary>
    /// Checks if an instruction is an unconditional jump
    /// </summary>
    private bool IsUnconditionalJump(Instruction instruction)
    {
        return instruction.Type == InstructionType.Jmp;
    }

    /// <summary>
    /// Checks if an instruction is a conditional jump
    /// </summary>
    private bool IsConditionalJump(Instruction instruction)
    {
        return instruction.Type == InstructionType.Je || 
               instruction.Type == InstructionType.Jne ||
               instruction.Type == InstructionType.Ja ||
               instruction.Type == InstructionType.Jae ||
               instruction.Type == InstructionType.Jb ||
               instruction.Type == InstructionType.Jbe ||
               instruction.Type == InstructionType.Jg ||
               instruction.Type == InstructionType.Jge ||
               instruction.Type == InstructionType.Jl ||
               instruction.Type == InstructionType.Jle ||
               instruction.Type == InstructionType.Jo ||
               instruction.Type == InstructionType.Jno ||
               instruction.Type == InstructionType.Jp ||
               instruction.Type == InstructionType.Jnp ||
               instruction.Type == InstructionType.Js ||
               instruction.Type == InstructionType.Jns ||
               instruction.Type == InstructionType.Jcxz;
    }

    /// <summary>
    /// Checks if an instruction is a call instruction
    /// </summary>
    private bool IsCallInstruction(Instruction instruction)
    {
        return instruction.Type == InstructionType.Call;
    }

    /// <summary>
    /// Gets the jump target offset from a jump instruction
    /// </summary>
    private int? GetJumpTargetOffset(Instruction instruction, int instructionOffset)
    {
        // Check if the instruction has at least one operand
        if (instruction.StructuredOperands == null || instruction.StructuredOperands.Count == 0)
        {
            return null;
        }
        
        // Look for an immediate operand which represents the offset
        var operand = instruction.StructuredOperands[0];
        if (operand is ImmediateOperand immediateOperand)
        {
            // Calculate the target address
            // For relative jumps, the target is IP (instruction pointer) + instruction length + offset
            int instructionLength = (int)(instruction.Address - _baseAddress) - instructionOffset + 1;
            int jumpOffset = Convert.ToInt32(immediateOperand.Value);
            
            return instructionOffset + instructionLength + jumpOffset;
        }
        
        // For now, we don't handle indirect jumps like JMP [eax] or JMP [ebx+4]
        return null;
    }
}