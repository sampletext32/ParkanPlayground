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
    /// Disassembles the code buffer and returns the disassembled instructions
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
}