namespace X86DisassemblerTests;

using System;
using Xunit;
using X86Disassembler.X86;
using X86Disassembler.X86.Handlers;
using X86Disassembler.X86.Handlers.ArithmeticImmediate;
using X86Disassembler.X86.Handlers.Inc;

/// <summary>
/// Tests for debugging the specific problematic sequence
/// </summary>
public class SequenceDebuggingTests
{
    /// <summary>
    /// Tests each byte in the problematic sequence individually
    /// </summary>
    [Fact]
    public void Debug_ProblematicSequence_ByteByByte()
    {
        // The problematic sequence
        byte[] fullSequence = new byte[] { 0x08, 0x83, 0xC1, 0x04, 0x50, 0xE8, 0x42, 0x01, 0x00, 0x00 };
        
        // Test each byte individually
        for (int i = 0; i < fullSequence.Length; i++)
        {
            byte opcode = fullSequence[i];
            string expectedMnemonic = GetExpectedMnemonic(opcode);
            
            // Create a buffer with just this byte
            byte[] buffer = new byte[] { opcode };
            var decoder = new InstructionDecoder(buffer, buffer.Length);
            var factory = new InstructionHandlerFactory(buffer, decoder, buffer.Length);
            
            // Get the handler for this opcode
            var handler = factory.GetHandler(opcode);
            
            // Output debug information
            Console.WriteLine($"Byte 0x{opcode:X2} at position {i}: Handler = {(handler != null ? handler.GetType().Name : "null")}");
            
            // If we have a handler, decode the instruction
            if (handler != null)
            {
                var instruction = new Instruction();
                bool success = handler.Decode(opcode, instruction);
                Console.WriteLine($"  Decoded as: {instruction.Mnemonic} {instruction.Operands}");
            }
        }
        
        // Now test the specific sequence 0x83 0xC1 0x04 (ADD ecx, 0x04)
        byte[] addSequence = new byte[] { 0x83, 0xC1, 0x04 };
        var addDecoder = new InstructionDecoder(addSequence, addSequence.Length);
        var addInstruction = addDecoder.DecodeInstruction();
        
        Console.WriteLine($"\nDecoding 0x83 0xC1 0x04 directly: {addInstruction?.Mnemonic} {addInstruction?.Operands}");
        
        // Now test the sequence 0x08 0x83 0xC1 0x04
        byte[] orAddSequence = new byte[] { 0x08, 0x83, 0xC1, 0x04 };
        var orAddDecoder = new InstructionDecoder(orAddSequence, orAddSequence.Length);
        
        // Decode the first instruction (0x08)
        var orInstruction = orAddDecoder.DecodeInstruction();
        Console.WriteLine($"\nDecoding 0x08 in sequence 0x08 0x83 0xC1 0x04: {orInstruction?.Mnemonic} {orInstruction?.Operands}");
        
        // Decode the second instruction (0x83 0xC1 0x04)
        var secondInstruction = orAddDecoder.DecodeInstruction();
        Console.WriteLine($"Decoding 0x83 0xC1 0x04 after 0x08: {secondInstruction?.Mnemonic} {secondInstruction?.Operands}");
        
        // Assert that we get the expected mnemonic for the second instruction
        Assert.Equal("add", secondInstruction?.Mnemonic);
    }
    
    /// <summary>
    /// Gets the expected mnemonic for a given opcode
    /// </summary>
    private string GetExpectedMnemonic(byte opcode)
    {
        return opcode switch
        {
            0x08 => "or",
            0x83 => "add", // Assuming reg field is 0 (ADD)
            0x50 => "push",
            0xE8 => "call",
            0x40 => "inc",
            0x41 => "inc",
            0x42 => "inc",
            _ => "??"
        };
    }
}
