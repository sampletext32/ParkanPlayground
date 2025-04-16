namespace X86Disassembler.PE;

/// <summary>
/// Represents the DOS header of a PE file
/// </summary>
public class DOSHeader
{
    public ushort e_magic;       // Magic number (MZ)
    public ushort e_cblp;        // Bytes on last page of file
    public ushort e_cp;          // Pages in file
    public ushort e_crlc;        // Relocations
    public ushort e_cparhdr;     // Size of header in paragraphs
    public ushort e_minalloc;    // Minimum extra paragraphs needed
    public ushort e_maxalloc;    // Maximum extra paragraphs needed
    public ushort e_ss;          // Initial (relative) SS value
    public ushort e_sp;          // Initial SP value
    public ushort e_csum;        // Checksum
    public ushort e_ip;          // Initial IP value
    public ushort e_cs;          // Initial (relative) CS value
    public ushort e_lfarlc;      // File address of relocation table
    public ushort e_ovno;        // Overlay number
    public ushort[] e_res;       // Reserved words
    public ushort e_oemid;       // OEM identifier (for e_oeminfo)
    public ushort e_oeminfo;     // OEM information; e_oemid specific
    public ushort[] e_res2;      // Reserved words
    public uint e_lfanew;        // File address of new exe header
        
    /// <summary>
    /// Initializes a new instance of the DOSHeader class
    /// </summary>
    public DOSHeader()
    {
        // Initialize arrays to avoid nullability warnings
        e_res = new ushort[4];
        e_res2 = new ushort[10];
    }
}