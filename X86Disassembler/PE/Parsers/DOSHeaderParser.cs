using X86Disassembler.PE.Types;

namespace X86Disassembler.PE.Parsers;

/// <summary>
/// Parser for the DOS header of a PE file
/// </summary>
public class DOSHeaderParser : IParser<DOSHeader>
{
    // DOS Header constants
    private const ushort DOS_SIGNATURE = 0x5A4D; // 'MZ'

    public DOSHeader Parse(BinaryReader reader)
    {
        var header = new DOSHeader();

        header.e_magic = reader.ReadUInt16();
        if (header.e_magic != DOS_SIGNATURE)
        {
            throw new InvalidDataException("Invalid DOS signature (MZ)");
        }

        header.e_cblp = reader.ReadUInt16();
        header.e_cp = reader.ReadUInt16();
        header.e_crlc = reader.ReadUInt16();
        header.e_cparhdr = reader.ReadUInt16();
        header.e_minalloc = reader.ReadUInt16();
        header.e_maxalloc = reader.ReadUInt16();
        header.e_ss = reader.ReadUInt16();
        header.e_sp = reader.ReadUInt16();
        header.e_csum = reader.ReadUInt16();
        header.e_ip = reader.ReadUInt16();
        header.e_cs = reader.ReadUInt16();
        header.e_lfarlc = reader.ReadUInt16();
        header.e_ovno = reader.ReadUInt16();

        header.e_res = new ushort[4];
        for (int i = 0; i < 4; i++)
        {
            header.e_res[i] = reader.ReadUInt16();
        }

        header.e_oemid = reader.ReadUInt16();
        header.e_oeminfo = reader.ReadUInt16();

        header.e_res2 = new ushort[10];
        for (int i = 0; i < 10; i++)
        {
            header.e_res2[i] = reader.ReadUInt16();
        }

        header.e_lfanew = reader.ReadUInt32();

        return header;
    }
}