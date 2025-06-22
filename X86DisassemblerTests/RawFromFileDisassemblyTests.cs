using X86Disassembler.X86;
using Xunit.Abstractions;

namespace X86DisassemblerTests;

/// <summary>
/// Tests for disassembling raw bytes from CSV test files
/// </summary>
public class RawFromFileDisassemblyTests(ITestOutputHelper output)
{
    [Theory]
    [ClassData(typeof(TestDataProvider))]
    public void RunTests(string f, int idx, TestFromFileEntry test)
    {
        Printer.WriteLine = output.WriteLine;
        
        // Convert hex string to byte array
        byte[] code = HexStringToByteArray(test.RawBytes);

        // Create a disassembler with the code
        Disassembler disassembler = new Disassembler(code, 0x1000);

        // Disassemble the code
        var disassembledInstructions = disassembler.Disassemble();

        // Verify the number of instructions
        if (test.Instructions.Count != disassembledInstructions.Count)
        {
            AssertFailWithReason(
                idx,
                f,
                test,
                disassembledInstructions,
                "Instruction count mismatch"
            );
        }

        // Verify each instruction
        for (int i = 0; i < test.Instructions.Count; i++)
        {
            var expected = test.Instructions[i];
            var actual = disassembledInstructions[i];

            // Compare instruction type instead of mnemonic
            if (expected.Type != actual.Type)
            {
                AssertFailWithReason(
                    idx,
                    f,
                    test,
                    disassembledInstructions,
                    $"Type mismatch: Expected {expected.Type}, got {actual.Type}"
                );
            }

            // Compare operands
            if (!CompareOperands(expected.Operands, actual.StructuredOperands))
            {
                AssertFailWithReason(
                    idx,
                    f,
                    test,
                    disassembledInstructions,
                    $"Operands mismatch: \n" +
                    $"Expected: {string.Join(", ", expected.Operands)}.\n" +
                    $"Actual: {string.Join(", ", actual.StructuredOperands.Select(x => $"{x.GetType().Name}({x})"))}"
                );
            }
        }
    }

    /// <summary>
    /// Compare operands with some flexibility since the string representation might be slightly different
    /// </summary>
    private bool CompareOperands(string[] expectedOperands, List<Operand> actualOperands)
    {
        // Check if the number of operands matches
        if (expectedOperands.Length != actualOperands.Count)
        {
            return false;
        }

        // Initialize result to true and set to false if any operand doesn't match
        bool result = true;

        // Compare each operand
        for (var i = 0; i < expectedOperands.Length; i++)
        {
            var expected = expectedOperands[i];
            var actual = actualOperands[i];

            if (expected == actual.ToString()) continue;

            result = false;
            break;
        }

        return result;
    }

    private void AssertFailWithReason(int index, string file, TestFromFileEntry test, List<Instruction> disassembledInstructions, string reason)
    {
        output.WriteLine($"Test {index} in {file} failed: {reason}");
        output.WriteLine($"Raw bytes: {test.RawBytes}");
        output.WriteLine("Expected instructions:");
        foreach (var instruction in test.Instructions)
        {
            output.WriteLine($"  {instruction.Type:G} {string.Join(",", instruction.Operands)}");
        }
        output.WriteLine("Actual instructions:");
        foreach (var instruction in disassembledInstructions)
        {
            output.WriteLine($"  {instruction.Type} {string.Join(", ", instruction.StructuredOperands)}");
        }
        Assert.Fail(reason);
    }

    /// <summary>
    /// Converts a hexadecimal string to a byte array
    /// </summary>
    private static byte[] HexStringToByteArray(string hex)
    {
        // Remove any spaces or other formatting characters
        hex = hex.Replace(" ", "").Replace("-", "").Replace("0x", "");

        // Create a byte array that will hold the converted hex string
        byte[] bytes = new byte[hex.Length / 2];

        // Convert each pair of hex characters to a byte
        for (int i = 0; i < hex.Length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }

        return bytes;
    }
}