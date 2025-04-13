using X86Disassembler.X86;

namespace TestDisassembler;

public class Program
{
    public static void Main(string[] args)
    {
        // Test the specific byte sequence with segment override prefix that's causing issues
        byte[] codeBytes = HexStringToByteArray("26FF7510");
        
        // Create a disassembler with the code
        Disassembler disassembler = new Disassembler(codeBytes, 0x1000);
        
        // Disassemble the code
        var instructions = disassembler.Disassemble();
        
        // Print the number of instructions
        Console.WriteLine($"Number of instructions: {instructions.Count}");
        
        // Print each instruction
        for (int i = 0; i < instructions.Count; i++)
        {
            Console.WriteLine($"Instruction {i+1}: {instructions[i].Mnemonic} {instructions[i].Operands}");
        }
    }
    
    private static byte[] HexStringToByteArray(string hex)
    {
        // Remove any non-hex characters
        hex = hex.Replace(" ", "").Replace("-", "");
        
        // Create a byte array
        byte[] bytes = new byte[hex.Length / 2];
        
        // Convert each pair of hex characters to a byte
        for (int i = 0; i < hex.Length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        
        return bytes;
    }
}
