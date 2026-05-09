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
        // Register provider for Windows-1251 if not already done globally.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // 1. Locate the master nodes (TRF0).
        var trf0 = archive.Files.FirstOrDefault(f => f.FileType == "TRF0");
        if (trf0 == null)
        {
            return new List<ResearchNodeData>();
        }

        // 2. Load node states (TRF1): one byte per node.
        byte[] initialStates = LoadRawBuffer(stream, archive, "TRF1");

        var nodes = new List<Trf0Element>();
        stream.Position = trf0.OffsetInFile;
        for (int i = 0; i < trf0.ElementCount; i++)
        {
            nodes.Add(ReadTrf0Element(stream));
        }

        // 3. Load string pools.
        byte[] shortNamesPoolTrf7 = LoadRawBuffer(stream, archive, "TRF7");
        byte[] longNamesPoolTrf8 = LoadRawBuffer(stream, archive, "TRF8");
        byte[] helpTextPoolTrf9 = LoadRawBuffer(stream, archive, "TRF9");
        byte[] descriptionsPoolTrfa = LoadRawBuffer(stream, archive, "TRFA");

        // 4. Load relationship maps.
        var prereqMap = LoadRelationMap(stream, archive, "TRF2", "TRF3", strict: true);
        var unlockMap = LoadRelationMap(stream, archive, "TRF4", "TRF5", strict: true);

        // TRFB/TRF6 is auxiliary and currently unknown.
        // Real game files may have inconsistent counts here, so keep it non-strict.
        _ = LoadRelationMap(stream, archive, "TRFB", "TRF6", strict: false);

        // 5. Assemble DTOs.
        var result = new List<ResearchNodeData>();
        for (int i = 0; i < nodes.Count; i++)
        {
            var element = nodes[i];
            result.Add(new ResearchNodeData
            {
                Index = i,
                Node = element,
                State = initialStates.Length > i ? (NodeState)initialStates[i] : NodeState.Hidden,
                ShortName = GetStringFromPool(shortNamesPoolTrf7, element.OffsetShortName_TRF7),
                LongName = GetStringFromPool(longNamesPoolTrf8, element.OffsetLongName_TRF8),
                HelpText = GetStringFromPool(helpTextPoolTrf9, element.OffsetHelpText_TRF9),
                Description = GetStringFromPool(descriptionsPoolTrfa, element.OffsetDescription_TRFA),
                PrerequisiteIds = prereqMap.TryGetValue(i, out var pValue) ? pValue : [],
                UnlockIds = unlockMap.TryGetValue(i, out var uValue) ? uValue : []
            });
        }

        return result;
    }

    private static Trf0Element ReadTrf0Element(Stream s)
    {
        // TRF0 element size is 40 bytes (0x28).
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

    private static Dictionary<int, uint[]> LoadRelationMap(
        Stream s,
        NResArchive archive,
        string headerType,
        string dataType,
        bool strict)
    {
        var header = archive.Files.FirstOrDefault(f => f.FileType == headerType);
        var data = archive.Files.FirstOrDefault(f => f.FileType == dataType);
        var map = new Dictionary<int, uint[]>();

        if (header == null || data == null)
        {
            return map;
        }

        uint[] counts = new uint[header.ElementCount];
        s.Position = header.OffsetInFile;
        for (int i = 0; i < header.ElementCount; i++)
        {
            counts[i] = s.ReadUInt32LittleEndian();
        }

        long dataWordsRequired = 0;
        for (int i = 0; i < counts.Length; i++)
        {
            dataWordsRequired += counts[i];
        }

        if (dataWordsRequired * sizeof(uint) > data.FileLength)
        {
            if (strict)
            {
                throw new InvalidDataException(
                    $"Relation map {headerType}/{dataType} expects {dataWordsRequired * sizeof(uint)} bytes but section has {data.FileLength} bytes.");
            }

            return map;
        }

        long dataEnd = data.OffsetInFile + data.FileLength;
        s.Position = data.OffsetInFile;

        for (int i = 0; i < counts.Length; i++)
        {
            uint count = counts[i];
            if (s.Position + count * sizeof(uint) > dataEnd)
            {
                if (strict)
                {
                    throw new EndOfStreamException(
                        $"Relation map {headerType}/{dataType} overreads section at node {i} (count={count}).");
                }

                break;
            }

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
        if (item == null)
        {
            return [];
        }

        byte[] buffer = new byte[item.FileLength];
        s.Position = item.OffsetInFile;
        s.ReadExactly(buffer);
        return buffer;
    }

    private static string GetStringFromPool(byte[] pool, uint offset)
    {
        if (pool.Length == 0 || offset >= pool.Length)
        {
            return string.Empty;
        }

        int end = Array.IndexOf(pool, (byte)0, (int)offset);
        int length = end == -1 ? pool.Length - (int)offset : end - (int)offset;

        return CyrillicEncoding.GetString(pool, (int)offset, length);
    }
}
