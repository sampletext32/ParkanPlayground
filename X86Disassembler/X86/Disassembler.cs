using X86Disassembler.X86.Operands;

namespace X86Disassembler.X86;

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
}