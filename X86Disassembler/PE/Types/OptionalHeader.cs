namespace X86Disassembler.PE.Types;

/// <summary>
/// Represents the Optional header of a PE file
/// </summary>
public class OptionalHeader
{
    // Optional Header Magic values
    private const ushort PE32_MAGIC = 0x10B; // 32-bit executable
    private const ushort PE32PLUS_MAGIC = 0x20B; // 64-bit executable

    // Standard fields
    public ushort Magic; // Magic number (PE32 or PE32+)
    public byte MajorLinkerVersion; // Major linker version
    public byte MinorLinkerVersion; // Minor linker version
    public uint SizeOfCode; // Size of code section
    public uint SizeOfInitializedData; // Size of initialized data section
    public uint SizeOfUninitializedData; // Size of uninitialized data section
    public uint AddressOfEntryPoint; // Address of entry point
    public uint BaseOfCode; // Base of code section
    public uint BaseOfData; // Base of data section (PE32 only)

    // Windows-specific fields
    public ulong ImageBase; // Image base address (uint for PE32, ulong for PE32+)
    public uint SectionAlignment; // Section alignment
    public uint FileAlignment; // File alignment
    public ushort MajorOperatingSystemVersion; // Major OS version
    public ushort MinorOperatingSystemVersion; // Minor OS version
    public ushort MajorImageVersion; // Major image version
    public ushort MinorImageVersion; // Minor image version
    public ushort MajorSubsystemVersion; // Major subsystem version
    public ushort MinorSubsystemVersion; // Minor subsystem version
    public uint Win32VersionValue; // Win32 version value
    public uint SizeOfImage; // Size of image
    public uint SizeOfHeaders; // Size of headers
    public uint CheckSum; // Checksum
    public ushort Subsystem; // Subsystem
    public ushort DllCharacteristics; // DLL characteristics
    public ulong SizeOfStackReserve; // Size of stack reserve (uint for PE32, ulong for PE32+)
    public ulong SizeOfStackCommit; // Size of stack commit (uint for PE32, ulong for PE32+)
    public ulong SizeOfHeapReserve; // Size of heap reserve (uint for PE32, ulong for PE32+)
    public ulong SizeOfHeapCommit; // Size of heap commit (uint for PE32, ulong for PE32+)
    public uint LoaderFlags; // Loader flags
    public uint NumberOfRvaAndSizes; // Number of RVA and sizes

    public DataDirectory[] DataDirectories = []; // Data directories

    /// <summary>
    /// Determines if the PE file is 64-bit based on the Magic value
    /// </summary>
    /// <returns>True if the PE file is 64-bit, false otherwise</returns>
    public bool Is64Bit()
    {
        return Magic == PE32PLUS_MAGIC;
    }
}