using System.Text;
using Common;
using NResLib;

namespace ResTreeLib;

public class ResTreeParser
{
    private static readonly Encoding CyrillicEncoding = CodePagesEncodingProvider.Instance.GetEncoding(1251) 
        ?? Encoding.GetEncoding("windows-1251");

    public static List<ResearchNodeData> Parse(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        var nres = NResParser.ReadFile(fs);

        fs.Seek(0, SeekOrigin.Begin);
        
        return Parse(nres.Archive!, fs);
    }
    
    public static List<ResearchNodeData> Parse(NResArchive archive, Stream stream)
    {
        // Register provider for Windows-1251 if not already done globally
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        if (archive.Files.Any(x => x.FileType == "TRFB"))
        {
            _ = 5;
        }

        // 1. Locate the Master Nodes (TRF0)
        var trf0 = archive.Files.FirstOrDefault(f => f.FileType == "TRF0");
        if (trf0 == null) return new List<ResearchNodeData>();

        // 2. Load the Node States (TRF1) - 1 byte per node
        byte[] initialStates = LoadRawBuffer(stream, archive, "TRF1");

        var nodes = new List<Trf0Element>();
        stream.Position = trf0.OffsetInFile;
        for (int i = 0; i < trf0.ElementCount; i++)
        {
            nodes.Add(ReadTrf0Element(stream));
        }

        // 3. Load String Pools
        byte[] shortNamesPool_trf7 = LoadRawBuffer(stream, archive, "TRF7");
        byte[] longNamesPool_trf8 = LoadRawBuffer(stream, archive, "TRF8");
        byte[] helpTextPool_trf9 = LoadRawBuffer(stream, archive, "TRF9");
        byte[] descriptionsPool_trfa = LoadRawBuffer(stream, archive, "TRFA");

        // 4. Load Relationship Maps
        // TRF2/3 = Prerequisites
        var prereqMap = LoadRelationMap(stream, archive, "TRF2", "TRF3");
        // TRF4/5 = Unlocks/Effects
        var unlockMap = LoadRelationMap(stream, archive, "TRF4", "TRF5");
        // TRFB/6 = unknown
        var aux_trf6 = LoadRelationMap(stream, archive, "TRFB", "TRF6");

        // 5. Assemble
        var result = new List<ResearchNodeData>();
        for (int i = 0; i < nodes.Count; i++)
        {
            var element = nodes[i];
            result.Add(new ResearchNodeData
            {
                Index = i,
                Node = element,
                // Cast the TRF1 byte to our NodeState Flags
                State = (initialStates.Length > i) ? (NodeState)initialStates[i] : NodeState.Hidden,
                ShortName = GetStringFromPool(shortNamesPool_trf7, element.OffsetShortName_TRF7),
                LongName = GetStringFromPool(longNamesPool_trf8, element.OffsetLongName_TRF8),
                HelpText = GetStringFromPool(helpTextPool_trf9, element.OffsetHelpText_TRF9),
                Description = GetStringFromPool(descriptionsPool_trfa, element.OffsetDescription_TRFA),
                PrerequisiteIds = prereqMap.TryGetValue(i, out var pValue) ? pValue : [],
                UnlockIds = unlockMap.TryGetValue(i, out var uValue) ? uValue : []
            });
        }

        return result;
    }

    private static Trf0Element ReadTrf0Element(Stream s)
    {
        // Total size must be 40 bytes (0x28)
        return new Trf0Element
        {
            ResearchCost = s.ReadFloatLittleEndian(),
            ResearchTime = s.ReadFloatLittleEndian(),
            ViewPosX = s.ReadFloatLittleEndian(),
            ViewPosY = s.ReadFloatLittleEndian(),
            OffsetShortName_TRF7 = s.ReadUInt32LittleEndian(),
            OffsetLongName_TRF8 = s.ReadUInt32LittleEndian(),
            OffsetHelpText_TRF9 = s.ReadUInt32LittleEndian(),
            OffsetDescription_TRFA = s.ReadUInt32LittleEndian(),
            OffsetAux_TRFB = s.ReadUInt16LittleEndian(),
            TurretType = (byte)s.ReadByte(),
            MainType = (byte)s.ReadByte(),
            SubType = (byte)s.ReadByte(),
            BuildSubSystem = (byte)s.ReadByte(),
            SizeOfType = (byte)s.ReadByte(),
            UpgradeLevel = (byte)s.ReadByte()
        };
    }

    // LoadRelationMap and LoadRawBuffer remain the same as your provided code
    private static Dictionary<int, uint[]> LoadRelationMap(Stream s, NResArchive archive, string headerType, string dataType)
    {
        var header = archive.Files.FirstOrDefault(f => f.FileType == headerType);
        var data = archive.Files.FirstOrDefault(f => f.FileType == dataType);
        var map = new Dictionary<int, uint[]>();

        if (header == null || data == null) return map;

        uint[] counts = new uint[header.ElementCount];
        s.Position = header.OffsetInFile;
        for (int i = 0; i < header.ElementCount; i++)
        {
            counts[i] = s.ReadUInt32LittleEndian();
        }

        s.Position = data.OffsetInFile;
        for (int i = 0; i < counts.Length; i++)
        {
            uint count = counts[i];
            uint[] ids = new uint[count];
            for (int j = 0; j < count; j++)
            {
                ids[j] = s.ReadUInt32LittleEndian();
            }
            map[i] = ids;
        }

        return map;
    }

    private static byte[] LoadRawBuffer(Stream s, NResArchive archive, string type)
    {
        var item = archive.Files.FirstOrDefault(f => f.FileType == type);
        if (item == null) return [];

        byte[] buffer = new byte[item.FileLength];
        s.Position = item.OffsetInFile;
        s.ReadExactly(buffer);
        return buffer;
    }

    private static string GetStringFromPool(byte[] pool, uint offset)
    {
        if (pool.Length == 0 || offset >= pool.Length) return string.Empty;
        
        int end = Array.IndexOf(pool, (byte)0, (int)offset);
        int length = (end == -1) ? pool.Length - (int)offset : end - (int)offset;
        
        return CyrillicEncoding.GetString(pool, (int)offset, length);
    }
}