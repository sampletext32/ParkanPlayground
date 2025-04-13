using System.Globalization;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;
using X86Disassembler.X86;
using Xunit.Abstractions;

namespace X86DisassemblerTests;

public class RawFromFileDisassemblyTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData("pushreg_tests.csv")]
    [InlineData("popreg_tests.csv")]
    [InlineData("pushimm_tests.csv")]
    [InlineData("nop_tests.csv")]
    [InlineData("xchg_tests.csv")]
    [InlineData("sub_tests.csv")]
    [InlineData("segment_override_tests.csv")]
    public void RunTests(string file)
    {
        // Load the CSV test file from embedded resources
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream($"X86DisassemblerTests.TestData.{file}");

        if (stream == null)
        {
            throw new InvalidOperationException($"Could not find {file} embedded resource");
        }

        // Configure CSV reader with semicolon delimiter
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ";",
            BadDataFound = null, // Ignore bad data
            AllowComments = true, // Enable comments in CSV files
            Comment = '#', // Use # as the comment character
            IgnoreBlankLines = true // Skip empty lines
        };

        using var streamReader = new StreamReader(stream);
        using var csvReader = new CsvReader(streamReader, config);

        // Register class map for TestFromFileEntry
        csvReader.Context.RegisterClassMap<TestFromFileEntryMap>();

        // Read all records from CSV
        var tests = csvReader.GetRecords<TestFromFileEntry>()
            .ToList();

        // Run tests for each instruction
        for (var index = 0; index < tests.Count; index++)
        {
            var test = tests[index];

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
                    index,
                    file,
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

                if (expected.Mnemonic != actual.Mnemonic)
                {
                    AssertFailWithReason(
                        index,
                        file,
                        test,
                        disassembledInstructions,
                        "Mnemonic mismatch"
                    );
                }

                if (expected.Operands != actual.Operands)
                {
                    AssertFailWithReason(
                        index,
                        file,
                        test,
                        disassembledInstructions,
                        "Operands mismatch"
                    );
                }
            }

            output.WriteLine(
                $"Test succeeded {index} of file {file}: {test.RawBytes}.\n" +
                $"Instruction count \"{test.Instructions.Count}\".\n" +
                $"{string.Join("\n", test.Instructions.Select(x => $"{x.Mnemonic} {x.Operands}"))}\n"
            );
        }
    }

    private static void AssertFailWithReason(int index, string file, TestFromFileEntry test, List<Instruction> disassembledInstructions, string reason)
    {
        Assert.Fail(
            $"Failed verifying test {index} of file {file}: {test.RawBytes}. {reason}.\n" +
            $"Expected \"{test.Instructions.Count}\", but got \"{disassembledInstructions.Count}\".\n" +
            $"\n" +
            $"Expected instructions:\n" +
            $"{string.Join("\n", test.Instructions.Select(x => $"{x.Mnemonic} {x.Operands}"))}\n" +
            $"\n" +
            $"Disassembled instructions:\n" +
            $"{string.Join("\n", disassembledInstructions)}"
        );
    }

    /// <summary>
    /// Converts a hex string to a byte array
    /// </summary>
    /// <param name="hex">The hex string to convert</param>
    /// <returns>The byte array</returns>
    private static byte[] HexStringToByteArray(string hex)
    {
        // Remove any non-hex characters if present
        hex = hex.Replace(" ", "").Replace("-", "");

        // Create a byte array
        byte[] bytes = new byte[hex.Length / 2];

        // Convert each pair of hex characters to a byte using spans for better performance
        ReadOnlySpan<char> hexSpan = hex.AsSpan();
        
        for (int i = 0; i < hexSpan.Length; i += 2)
        {
            // Parse two characters at a time using spans
            if (!byte.TryParse(hexSpan.Slice(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bytes[i / 2]))
            {
                throw new FormatException($"Invalid hex string at position {i}: {hexSpan.Slice(i, 2).ToString()}");
            }
        }

        return bytes;
    }
}