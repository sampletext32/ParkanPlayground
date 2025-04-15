using System.Collections;
using System.Globalization;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;

namespace X86DisassemblerTests;

/// <summary>
/// Provides test data from CSV files in the TestData directory
/// </summary>
public class TestDataProvider : IEnumerable<object[]>
{
    /// <summary>
    /// Gets all CSV test files from the TestData directory
    /// </summary>
    /// <returns>An enumerable of test file names</returns>
    private IEnumerable<string> GetTestFiles()
    {
        // Get all CSV files from the TestData directory in the assembly
        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith("X86DisassemblerTests.TestData.") && name.EndsWith(".csv"));
            
        // Return all CSV files from the TestData directory
        // All files have been converted to the new format
        foreach (var resourceName in resourceNames)
        {
            // Return the full resource name
            yield return resourceName;
        }
    }

    /// <summary>
    /// Loads test entries from a CSV file
    /// </summary>
    /// <param name="resourceName">The full resource name of the CSV file</param>
    /// <returns>An enumerable of TestFromFileEntry objects</returns>
    private IEnumerable<TestFromFileEntry> LoadTestEntries(string resourceName)
    {
        // Load the CSV test file from embedded resources
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            throw new InvalidOperationException($"Could not find {resourceName} embedded resource");
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
        var entries = csvReader.GetRecords<TestFromFileEntry>().ToList();
        
        // Return each entry with its file name
        foreach (var entry in entries)
        {
            yield return entry;
        }
    }

    /// <summary>
    /// Returns an enumerator that provides test data for each test entry
    /// </summary>
    public IEnumerator<object[]> GetEnumerator()
    {
        foreach (var resourceName in GetTestFiles())
        {
            // Extract just the filename part for display purposes
            string fileName = resourceName.Replace("X86DisassemblerTests.TestData.", "");
            int testIndex = 0;

            foreach (var entry in LoadTestEntries(resourceName))
            {
                // Yield each test entry as a separate test case
                // Include the file name and index for better test identification
                yield return [fileName, testIndex++, entry];
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
