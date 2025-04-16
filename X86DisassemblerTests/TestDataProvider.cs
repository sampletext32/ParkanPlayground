using System.Collections;
using System.Globalization;
using System.IO;
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
    /// <returns>An enumerable of test file paths</returns>
    private IEnumerable<string> GetTestFiles()
    {
        // Get the directory where the test assembly is located
        var assemblyLocation = typeof(TestDataProvider).Assembly.Location;
        var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
        
        // Navigate to the TestData directory
        // First try to find it in the project structure (for development)
        string testDataDirectory = Path.Combine(assemblyDirectory!, "..", "..", "..", "TestData");
        
        // If the directory doesn't exist (e.g., in a published build), try the output directory
        if (!Directory.Exists(testDataDirectory))
        {
            testDataDirectory = Path.Combine(assemblyDirectory!, "TestData");
            
            // If still not found, throw an exception
            if (!Directory.Exists(testDataDirectory))
            {
                throw new DirectoryNotFoundException($"Could not find TestData directory at {testDataDirectory}");
            }
        }
        
        // Get the absolute path
        testDataDirectory = Path.GetFullPath(testDataDirectory);
        
        // Return all CSV files from the TestData directory
        return Directory.GetFiles(testDataDirectory, "*.csv");
    }

    /// <summary>
    /// Loads test entries from a CSV file
    /// </summary>
    /// <param name="filePath">The full path to the CSV file</param>
    /// <returns>An enumerable of TestFromFileEntry objects</returns>
    private IEnumerable<TestFromFileEntry> LoadTestEntries(string filePath)
    {
        // Check if the file exists
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Could not find test file at {filePath}");
        }
        
        // Open the file directly from the file system
        using var stream = File.OpenRead(filePath);

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
        foreach (var filePath in GetTestFiles())
        {
            // Extract just the filename part for display purposes
            string fileName = Path.GetFileName(filePath);
            int testIndex = 0;

            foreach (var entry in LoadTestEntries(filePath))
            {
                // Yield each test entry as a separate test case
                // Include the file name and index for better test identification
                yield return [fileName, testIndex++, entry];
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
