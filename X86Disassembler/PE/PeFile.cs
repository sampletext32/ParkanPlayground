using X86Disassembler.PE.Parsers;

namespace X86Disassembler.PE;

/// <summary>
/// Represents a Portable Executable (PE) file format parser
/// </summary>
public class PeFile
{
    // DOS Header constants
    private const ushort DOS_SIGNATURE = 0x5A4D; // 'MZ'
    private const uint PE_SIGNATURE = 0x00004550; // 'PE\0\0'

    // Optional Header Magic values
    private const ushort PE32_MAGIC = 0x10B; // 32-bit executable
    private const ushort PE32PLUS_MAGIC = 0x20B; // 64-bit executable

    // Section characteristics flags
    private const uint IMAGE_SCN_CNT_CODE = 0x00000020; // Section contains code
    private const uint IMAGE_SCN_MEM_EXECUTE = 0x20000000; // Section is executable
    private const uint IMAGE_SCN_MEM_READ = 0x40000000; // Section is readable
    private const uint IMAGE_SCN_MEM_WRITE = 0x80000000; // Section is writable

    // Data directories
    private const int IMAGE_DIRECTORY_ENTRY_EXPORT = 0; // Export Directory
    private const int IMAGE_DIRECTORY_ENTRY_IMPORT = 1; // Import Directory
    private const int IMAGE_DIRECTORY_ENTRY_RESOURCE = 2; // Resource Directory
    private const int IMAGE_DIRECTORY_ENTRY_EXCEPTION = 3; // Exception Directory
    private const int IMAGE_DIRECTORY_ENTRY_SECURITY = 4; // Security Directory
    private const int IMAGE_DIRECTORY_ENTRY_BASERELOC = 5; // Base Relocation Table
    private const int IMAGE_DIRECTORY_ENTRY_DEBUG = 6; // Debug Directory
    private const int IMAGE_DIRECTORY_ENTRY_ARCHITECTURE = 7; // Architecture Specific Data
    private const int IMAGE_DIRECTORY_ENTRY_GLOBALPTR = 8; // RVA of GP
    private const int IMAGE_DIRECTORY_ENTRY_TLS = 9; // TLS Directory
    private const int IMAGE_DIRECTORY_ENTRY_LOAD_CONFIG = 10; // Load Configuration Directory
    private const int IMAGE_DIRECTORY_ENTRY_BOUND_IMPORT = 11; // Bound Import Directory
    private const int IMAGE_DIRECTORY_ENTRY_IAT = 12; // Import Address Table
    private const int IMAGE_DIRECTORY_ENTRY_DELAY_IMPORT = 13; // Delay Load Import Descriptors
    private const int IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR = 14; // COM Runtime descriptor

    // PE file data
    private byte[] _fileData;

    // Parser instances
    private readonly DOSHeaderParser _dosHeaderParser;
    private readonly FileHeaderParser _fileHeaderParser;
    private readonly OptionalHeaderParser _optionalHeaderParser;
    private readonly SectionHeaderParser _sectionHeaderParser;
    private PEUtility _peUtility;
    private ExportDirectoryParser _exportDirectoryParser;
    private ImportDescriptorParser _importDescriptorParser;

    // Parsed headers
    public DOSHeader DosHeader { get; private set; }
    public FileHeader FileHeader { get; private set; }
    public OptionalHeader OptionalHeader { get; private set; }
    public List<SectionHeader> SectionHeaders { get; private set; }
    public bool Is64Bit { get; private set; }

    // Export and Import information
    public ExportDirectory ExportDirectory { get; private set; }
    public List<ExportedFunction> ExportedFunctions { get; private set; }
    public List<ImportDescriptor> ImportDescriptors { get; private set; }

    /// <summary>
    /// Initializes a new instance of the PEFormat class
    /// </summary>
    /// <param name="fileData">The raw file data</param>
    public PeFile(byte[] fileData)
    {
        _fileData = fileData;
        SectionHeaders = new List<SectionHeader>();
        ExportedFunctions = new List<ExportedFunction>();
        ImportDescriptors = new List<ImportDescriptor>();

        // Initialize parsers
        _dosHeaderParser = new DOSHeaderParser();
        _fileHeaderParser = new FileHeaderParser();
        _optionalHeaderParser = new OptionalHeaderParser();
        _sectionHeaderParser = new SectionHeaderParser();

        // Initialize properties to avoid nullability warnings
        DosHeader = new DOSHeader();
        FileHeader = new FileHeader();
        OptionalHeader = new OptionalHeader();
        ExportDirectory = new ExportDirectory();

        // These will be initialized during Parse()
        _peUtility = null!;
        _exportDirectoryParser = null!;
        _importDescriptorParser = null!;
    }

    /// <summary>
    /// Parses the PE file structure
    /// </summary>
    /// <returns>True if parsing was successful, false otherwise</returns>
    public bool Parse()
    {
        try
        {
            using (MemoryStream stream = new MemoryStream(_fileData))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                // Parse DOS header
                DosHeader = _dosHeaderParser.Parse(reader);

                // Move to PE header
                reader.BaseStream.Seek(DosHeader.e_lfanew, SeekOrigin.Begin);

                // Verify PE signature
                uint peSignature = reader.ReadUInt32();
                if (peSignature != PE_SIGNATURE)
                {
                    throw new InvalidDataException("Invalid PE signature");
                }

                // Parse File Header
                FileHeader = _fileHeaderParser.Parse(reader);

                // Parse Optional Header
                OptionalHeader = _optionalHeaderParser.Parse(reader);
                Is64Bit = OptionalHeader.Is64Bit();

                // Parse Section Headers
                for (int i = 0; i < FileHeader.NumberOfSections; i++)
                {
                    SectionHeaders.Add(_sectionHeaderParser.Parse(reader));
                }

                // Initialize utility after section headers are parsed
                _peUtility = new PEUtility(SectionHeaders, OptionalHeader.SizeOfHeaders);
                _exportDirectoryParser = new ExportDirectoryParser(_peUtility);
                _importDescriptorParser = new ImportDescriptorParser(_peUtility);

                // Parse Export Directory
                if (OptionalHeader.DataDirectories.Length > IMAGE_DIRECTORY_ENTRY_EXPORT &&
                    OptionalHeader.DataDirectories[IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress != 0)
                {
                    uint exportDirRva = OptionalHeader.DataDirectories[IMAGE_DIRECTORY_ENTRY_EXPORT].VirtualAddress;
                    uint exportDirSize = OptionalHeader.DataDirectories[IMAGE_DIRECTORY_ENTRY_EXPORT].Size;

                    ExportDirectory = _exportDirectoryParser.Parse(reader, exportDirRva);
                    ExportedFunctions = _exportDirectoryParser.ParseExportedFunctions(
                        reader,
                        ExportDirectory,
                        exportDirRva,
                        exportDirSize
                    );
                }

                // Parse Import Descriptors
                if (OptionalHeader.DataDirectories.Length > IMAGE_DIRECTORY_ENTRY_IMPORT &&
                    OptionalHeader.DataDirectories[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress != 0)
                {
                    uint importDirRva = OptionalHeader.DataDirectories[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress;
                    ImportDescriptors = _importDescriptorParser.Parse(reader, importDirRva);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing PE file: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the raw data for a specific section
    /// </summary>
    /// <param name="sectionIndex">Index of the section</param>
    /// <returns>Byte array containing the section data</returns>
    public byte[] GetSectionData(int sectionIndex)
    {
        if (sectionIndex < 0 || sectionIndex >= SectionHeaders.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(sectionIndex));
        }

        SectionHeader section = SectionHeaders[sectionIndex];
        byte[] sectionData = new byte[section.SizeOfRawData];

        Array.Copy(
            _fileData,
            section.PointerToRawData,
            sectionData,
            0,
            section.SizeOfRawData
        );

        return sectionData;
    }

    /// <summary>
    /// Gets the raw data for a section by name
    /// </summary>
    /// <param name="sectionName">Name of the section</param>
    /// <returns>Byte array containing the section data</returns>
    public byte[] GetSectionData(string sectionName)
    {
        for (int i = 0; i < SectionHeaders.Count; i++)
        {
            if (SectionHeaders[i].Name == sectionName)
            {
                return GetSectionData(i);
            }
        }

        throw new ArgumentException($"Section '{sectionName}' not found");
    }

    /// <summary>
    /// Gets all code sections
    /// </summary>
    /// <returns>List of section indices that contain code</returns>
    public List<int> GetCodeSections()
    {
        List<int> codeSections = new List<int>();

        for (int i = 0; i < SectionHeaders.Count; i++)
        {
            if (SectionHeaders[i]
                .ContainsCode())
            {
                codeSections.Add(i);
            }
        }

        return codeSections;
    }

    /// <summary>
    /// Checks if a section contains code
    /// </summary>
    /// <param name="section">The section to check</param>
    /// <returns>True if the section contains code, false otherwise</returns>
    public bool IsSectionContainsCode(SectionHeader section)
    {
        return section.ContainsCode();
    }
}