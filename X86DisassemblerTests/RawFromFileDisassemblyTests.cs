using System.Globalization;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;
using X86Disassembler.X86;
using X86Disassembler.X86.Operands;
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
    [InlineData("xor_tests.csv")]
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

                // Compare instruction type instead of mnemonic
                if (expected.Type != actual.Type)
                {
                    AssertFailWithReason(
                        index,
                        file,
                        test,
                        disassembledInstructions,
                        $"Type mismatch: Expected {expected.Type}, got {actual.Type}"
                    );
                }

                // For operands, we need to do a string comparison since the CSV contains string operands
                // and we now have structured operands in the actual instruction
                string actualOperandsString = string.Join(", ", actual.StructuredOperands);
                if (!CompareOperands(expected.Operands, actualOperandsString))
                {
                    AssertFailWithReason(
                        index,
                        file,
                        test,
                        disassembledInstructions,
                        $"Operands mismatch: Expected '{expected.Operands}', got '{actualOperandsString}'"
                    );
                }
            }
        }
    }

    // Compare operands with some flexibility since the string representation might be slightly different
    private bool CompareOperands(string expected, string actual)
    {
        // Normalize strings for comparison
        expected = NormalizeOperandString(expected);
        actual = NormalizeOperandString(actual);
        
        return expected == actual;
    }
    
    // Normalize operand strings to handle slight formatting differences
    private string NormalizeOperandString(string operands)
    {
        if (string.IsNullOrEmpty(operands))
            return string.Empty;
            
        // Remove all spaces
        operands = operands.Replace(" ", "");
        
        // Convert to lowercase
        operands = operands.ToLowerInvariant();
        
        // Normalize hex values (remove 0x prefix if present)
        operands = operands.Replace("0x", "");
        
        return operands;
    }

    private void AssertFailWithReason(int index, string file, TestFromFileEntry test, List<Instruction> disassembledInstructions, string reason)
    {
        output.WriteLine($"Test {index} in {file} failed: {reason}");
        output.WriteLine($"Raw bytes: {test.RawBytes}");
        output.WriteLine("Expected instructions:");
        foreach (var instruction in test.Instructions)
        {
            output.WriteLine($"  {instruction.Mnemonic} {instruction.Operands}");
        }
        output.WriteLine("Actual instructions:");
        foreach (var instruction in disassembledInstructions)
        {
            output.WriteLine($"  {instruction.Type} {string.Join(", ", instruction.StructuredOperands)}");
        }
        Assert.True(false, reason);
    }

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