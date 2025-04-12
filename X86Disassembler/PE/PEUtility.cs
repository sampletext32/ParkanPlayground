namespace X86Disassembler.PE;

/// <summary>
/// Utility class for PE format operations
/// </summary>
public class PEUtility
{
    private readonly List<SectionHeader> _sectionHeaders;
    private readonly uint _sizeOfHeaders;
        
    /// <summary>
    /// Initialize a new instance of the PEUtility class
    /// </summary>
    /// <param name="sectionHeaders">The section headers</param>
    /// <param name="sizeOfHeaders">The size of the headers</param>
    public PEUtility(List<SectionHeader> sectionHeaders, uint sizeOfHeaders)
    {
        _sectionHeaders = sectionHeaders;
        _sizeOfHeaders = sizeOfHeaders;
    }
        
    /// <summary>
    /// Converts a Relative Virtual Address (RVA) to a file offset
    /// </summary>
    /// <param name="rva">The RVA to convert</param>
    /// <returns>The corresponding file offset</returns>
    public uint RvaToOffset(uint rva)
    {
        if (rva == 0)
        {
            return 0;
        }
            
        foreach (var section in _sectionHeaders)
        {
            // Check if the RVA is within this section
            if (rva >= section.VirtualAddress && rva < section.VirtualAddress + section.VirtualSize)
            {
                // Calculate the offset within the section
                uint offsetInSection = rva - section.VirtualAddress;
                    
                // Make sure we don't exceed the raw data size
                if (offsetInSection < section.SizeOfRawData)
                {
                    return section.PointerToRawData + offsetInSection;
                }
            }
        }
            
        // If the RVA is not within any section, it might be in the headers
        if (rva < _sizeOfHeaders)
        {
            return rva;
        }
            
        throw new ArgumentException($"RVA {rva:X8} is not within any section");
    }
}