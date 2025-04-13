using System.Reflection;
using System.Text.Json;
using Xunit;
using X86Disassembler.X86;

namespace X86DisassemblerTests;

/// <summary>
/// General tests for the X86 disassembler using a JSON test file
/// </summary>
public class GeneralDisassemblerInstructionTests
{
    /// <summary>
    /// Runs tests on all instructions defined in the JSON file
    /// </summary>
    [Fact]
    public void RunTestsOnJson()
    {
        // Load the JSON test file from embedded resources
        using var jsonStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("X86DisassemblerTests.instruction_test.json");

        if (jsonStream == null)
        {
            throw new InvalidOperationException("Could not find instruction_test.json embedded resource");
        }

        // Deserialize the JSON file
        var instructions = JsonSerializer.Deserialize<List<JsonInstructionEntry>>(jsonStream)!;

        // Run tests for each instruction
        for (var index = 0; index < instructions.Count; index++)
        {
            var test = instructions[index];
            // Convert hex string to byte array
            byte[] code = HexStringToByteArray(test.RawBytes);

            // Create a disassembler with the code
            Disassembler disassembler = new Disassembler(code, 0x1000);

            // Disassemble the code
            var disassembledInstructions = disassembler.Disassemble();

            // Verify the number of instructions
            if (test.Disassembled.Count != disassembledInstructions.Count)
            {
                Assert.Fail(
                    $"Failed verifying test {index}: {test.RawBytes}. Instruction count mismatch.\n" +
                    $"Expected \"{test.Disassembled.Count}\", but got \"{disassembledInstructions.Count}\".\n" +
                    $"Disassembled instructions:\n" +
                    $"{string.Join("\n", disassembledInstructions)}"
                );
            }

            // Verify each instruction
            for (int i = 0; i < test.Disassembled.Count; i++)
            {
                var expected = test.Disassembled[i];
                var actual = disassembledInstructions[i];

                if (expected.Mnemonic != actual.Mnemonic)
                {
                    Assert.Fail(
                        $"Failed verifying test {index}: {test.RawBytes}. Instruction {i}. Mnemonic mismatch. " +
                        $"Expected \"{expected.Mnemonic}\", but got {actual.Mnemonic}\""
                    );
                }
                if (expected.Operands != actual.Operands)
                {
                    Assert.Fail(
                        $"Failed verifying test {index}: {test.RawBytes}. Instruction {i}. Operands mismatch. " +
                        $"Expected \"{expected.Operands}\", but got \"{actual.Operands}\""
                    );
                }
            }
        }
    }

    /// <summary>
    /// Converts a hex string to a byte array
    /// </summary>
    /// <param name="hex">The hex string to convert</param>
    /// <returns>The byte array</returns>
    private static byte[] HexStringToByteArray(string hex)
    {
        // Remove any non-hex characters
        hex = hex.Replace(" ", "")
            .Replace("-", "");

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