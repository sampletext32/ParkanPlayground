using System.Text;

namespace ControlLib;

public sealed class ControlReadException : Exception
{
    public ControlReadException(string message) : base(message) { }
}

public readonly record struct Vec3(float X, float Y, float Z);

public readonly record struct ResourceNamedRef(string ArchiveName, string ResourceName)
{
    public bool IsEmpty => string.IsNullOrEmpty(ArchiveName) || string.IsNullOrEmpty(ResourceName);
    public override string ToString() => IsEmpty ? "<empty>" : $"{ArchiveName} / {ResourceName}";
}

public static class ControlResourceReader
{
    public static CptFile ReadCptFile(string path)
    {
        using var fs = File.OpenRead(path);
        return ReadCptFile(fs, path);
    }

    public static CptFile ReadCptFile(Stream stream, string? sourceName = null)
    {
        var r = new LeReader(stream);
        long start = stream.Position;
        uint count = r.ReadUInt32();
        var records = new List<CptRecordRaw>(checked((int)count));

        for (int i = 0; i < count; i++)
        {
            long off = stream.Position - start;
            records.Add(new CptRecordRaw(
                Offset: off,
                Unknown0: r.ReadInt32(),
                OwnerPieceIndex: r.ReadInt32(),
                TargetPieceIndex: r.ReadInt32(),
                Position: r.ReadVec3(),
                Direction: r.ReadVec3()
            ));
        }

        var names = new List<string>(checked((int)count));
        for (int i = 0; i < count; i++)
        {
            names.Add(r.ReadFixedString(32));
        }

        r.ExpectEofOrThrow(sourceName ?? "CPT");
        return new CptFile(sourceName, count, records, names);
    }

    public static NdpFile ReadNdpFile(string path)
    {
        using var fs = File.OpenRead(path);
        return ReadNdpFile(fs, path);
    }

    public static NdpFile ReadNdpFile(Stream stream, string? sourceName = null)
    {
        var r = new LeReader(stream);
        long start = stream.Position;
        uint count = r.ReadUInt32();
        var records = new List<NdpRecordRaw>(checked((int)count));

        for (int i = 0; i < count; i++)
        {
            long off = stream.Position - start;
            records.Add(new NdpRecordRaw(
                Offset: off,
                Unknown0: r.ReadUInt32(),
                MaxVolume: r.ReadSingle(),
                MobilityContributionPerVolume: r.ReadSingle(),
                Resource: r.ReadResourceNamedRef()
            ));
        }

        r.ExpectEofOrThrow(sourceName ?? "NDP");
        return new NdpFile(sourceName, count, records);
    }

    public static CtlFile ReadCtlFile(string path)
    {
        using var fs = File.OpenRead(path);
        return ReadCtlFile(fs, path);
    }

    public static CtlFile ReadCtlFile(Stream stream, string? sourceName = null)
    {
        var r = new LeReader(stream);
        long start = stream.Position;

        var header = new CtlHeader(
            ControlStateCount: r.ReadUInt32(),
            ControlStateLinkSlotCount: r.ReadUInt32(),
            PieceBindingCount: r.ReadUInt32(),
            ItemRecordCount: r.ReadUInt32(),
            CommandListCount: r.ReadUInt32()
        );

        var rootParams = ReadCtlRootControlParams(r, stream.Position - start);

        var states = new List<CtlStateRecordRaw>(checked((int)header.ControlStateCount));
        for (int i = 0; i < header.ControlStateCount; i++)
        {
            long off = stream.Position - start;
            byte[] fixedBody = r.ReadBytesExact(0x9C);
            var links = new List<NavNodeLinkSlotRaw>(checked((int)header.ControlStateLinkSlotCount));
            for (int j = 0; j < header.ControlStateLinkSlotCount; j++)
            {
                links.Add(new NavNodeLinkSlotRaw(
                    Word0: r.ReadUInt32(),
                    Word1: r.ReadUInt32(),
                    Word2: r.ReadUInt32(),
                    Word3: r.ReadUInt32()
                ));
            }
            states.Add(new CtlStateRecordRaw(off, fixedBody, links));
        }

        int edgeCount = checked((int)(header.ControlStateCount * header.ControlStateCount));
        var edgeWeightMatrix = new float[edgeCount];
        for (int i = 0; i < edgeWeightMatrix.Length; i++)
            edgeWeightMatrix[i] = r.ReadSingle();

        var pieceBindings = new List<CtlPieceBindingRecordRaw>(checked((int)header.PieceBindingCount));
        for (int i = 0; i < header.PieceBindingCount; i++)
        {
            long off = stream.Position - start;
            pieceBindings.Add(ReadPieceBinding(r, off));
        }

        var items = new List<CtlItemRecordRaw>(checked((int)header.ItemRecordCount));
        for (int i = 0; i < header.ItemRecordCount; i++)
        {
            long off = stream.Position - start;
            items.Add(ReadCtlItemRecord(r, start, off));
        }

        long commandTableOffset = stream.Position - start;
        var commandTable = new ControlCommandListIndexTable(commandTableOffset, r.ReadInt32Array(21));

        var commandLists = new List<ControlCommandListRaw>(checked((int)header.CommandListCount));
        for (int i = 0; i < header.CommandListCount; i++)
        {
            long listOffset = stream.Position - start;
            uint count = r.ReadUInt32();
            var records = new List<ControlCommandRecordRaw>(checked((int)count));
            for (int j = 0; j < count; j++)
            {
                long recordOffset = stream.Position - start;
                records.Add(ReadCommandRecord(r, recordOffset));
            }
            commandLists.Add(new ControlCommandListRaw(listOffset, count, records));
        }

        r.ExpectEofOrThrow(sourceName ?? "CTL");

        return new CtlFile(
            SourceName: sourceName,
            Header: header,
            RootParams: rootParams,
            StateRecords: states,
            EdgeWeightMatrix: edgeWeightMatrix,
            PieceBindings: pieceBindings,
            ItemRecords: items,
            CommandTable: commandTable,
            CommandLists: commandLists
        );
    }

    private static CtlRootControlParams ReadCtlRootControlParams(LeReader r, long offset)
    {
        return new CtlRootControlParams(
            Offset: offset,
            MaxTangentialAccel: r.ReadVec3(),
            Unknown0C: r.ReadUInt32(),
            Unknown10: r.ReadUInt32(),
            Unknown14: r.ReadUInt32(),
            MaxTangentialSpeed: r.ReadVec3(),
            MaxAngularSpeed: r.ReadVec3(),
            ExternalTorqueResponseScale: r.ReadVec3(),
            VisualEulerAngleLimit: r.ReadVec3(),
            DetachCleanupDelayMs: r.ReadInt32(),
            Unknown4C: r.ReadSingle(),
            Unknown50: r.ReadSingle(),
            MovementPhysicsMode: r.ReadInt32(),
            Unknown58: r.ReadSingle(),
            SlopeLimitAngle: r.ReadSingle(),
            Unknown60: r.ReadUInt32(),
            Unknown64: r.ReadSingle(),
            MobilityIntegrityAmount: r.ReadSingle()
        );
    }

    private static CtlPieceBindingRecordRaw ReadPieceBinding(LeReader r, long offset)
    {
        return new CtlPieceBindingRecordRaw(
            Offset: offset,
            LocalPieceIndex: r.ReadInt32(),
            Anim1Start: r.ReadSingle(),
            Anim1EndOrDuration: r.ReadSingle(),
            Anim2Start: r.ReadSingle(),
            Anim2EndOrDuration: r.ReadSingle(),
            AnimBlendWeight: r.ReadSingle(),
            DefaultValue: r.ReadSingle(),
            PrimaryCptRefIndex: r.ReadInt32(),
            SecondaryCptRefIndex: r.ReadInt32(),
            Param24: r.ReadSingle(),
            Param28: r.ReadSingle(),
            Flags: r.ReadUInt32()
        );
    }

    private static CtlItemRecordRaw ReadCtlItemRecord(LeReader r, long fileStart, long offset)
    {
        int itemType = r.ReadInt32();
        int localPieceIndex = r.ReadInt32();
        uint flags = r.ReadUInt32();
        int unknown0C = r.ReadInt32();
        int unknown10 = r.ReadInt32();
        int unknown14 = r.ReadInt32();
        int optionalLinkIndex = r.ReadInt32();
        uint mobilityContribution = r.ReadUInt32();
        float unknown20 = r.ReadSingle();
        float unknown24 = r.ReadSingle();
        float unknown28 = r.ReadSingle();
        float unknown2C = r.ReadSingle();

        var commonParams = new float[16];
        for (int i = 0; i < commonParams.Length; i++)
            commonParams[i] = r.ReadSingle();

        string resourceArchiveName = r.ReadFixedString(32);
        string resourceResourceName = r.ReadFixedString(32);
        uint bindingCount = r.ReadUInt32();

        var bindingIndices = new int[checked((int)bindingCount)];
        for (int i = 0; i < bindingIndices.Length; i++)
            bindingIndices[i] = r.ReadInt32();

        int itemNameLength = r.ReadInt32();
        string itemName = string.Empty;
        if (itemNameLength < 0)
            throw new ControlReadException($"Negative CTL item name length {itemNameLength} at 0x{offset:X}");

        if (itemNameLength != 0)
        {
            // The engine allocates length+1 and copies length+1 bytes from the file.
            byte[] raw = r.ReadBytesExact(itemNameLength + 1);
            int nul = Array.IndexOf(raw, (byte)0);
            int len = nul >= 0 ? nul : raw.Length;
            itemName = Encoding.ASCII.GetString(raw, 0, len);
        }

        long endOffset = r.BaseStream.Position - fileStart;

        return new CtlItemRecordRaw(
            Offset: offset,
            EndOffset: endOffset,
            ItemType: itemType,
            LocalPieceIndex: localPieceIndex,
            Flags: flags,
            Unknown0C: unknown0C,
            Unknown10: unknown10,
            Unknown14: unknown14,
            OptionalLinkIndex: optionalLinkIndex,
            MobilityContribution: mobilityContribution,
            Param20: unknown20,
            Param24: unknown24,
            Param28: unknown28,
            Param2C: unknown2C,
            CommonParams: commonParams,
            Resource: new ResourceNamedRef(resourceArchiveName, resourceResourceName),
            PieceBindingIndices: bindingIndices,
            ItemNameLength: itemNameLength,
            ItemName: itemName
        );
    }

    private static ControlCommandRecordRaw ReadCommandRecord(LeReader r, long offset)
    {
        var condition = new ControlCommandConditionRaw(
            ConditionFlags: r.ReadUInt32(),
            ConditionMask: r.ReadUInt32(),
            ConditionInvertMask: r.ReadUInt32()
        );

        return new ControlCommandRecordRaw(
            Offset: offset,
            Condition: condition,
            Opcode: (EControlCommandOpcode)r.ReadUInt32(),
            Arg0: r.ReadUInt32(),
            Arg1: r.ReadUInt32(),
            Arg2: r.ReadUInt32(),
            Arg3: r.ReadUInt32(),
            Arg4: r.ReadUInt32(),
            Resource: r.ReadResourceNamedRef()
        );
    }
}

public sealed record CptFile(
    string? SourceName,
    uint Count,
    IReadOnlyList<CptRecordRaw> Records,
    IReadOnlyList<string> Names);

public sealed record CptRecordRaw(
    long Offset,
    int Unknown0,
    int OwnerPieceIndex,
    int TargetPieceIndex,
    Vec3 Position,
    Vec3 Direction);

public sealed record NdpFile(
    string? SourceName,
    uint Count,
    IReadOnlyList<NdpRecordRaw> Records);

public sealed record NdpRecordRaw(
    long Offset,
    uint Unknown0,
    float MaxVolume,
    float MobilityContributionPerVolume,
    ResourceNamedRef Resource);

public sealed record CtlFile(
    string? SourceName,
    CtlHeader Header,
    CtlRootControlParams RootParams,
    IReadOnlyList<CtlStateRecordRaw> StateRecords,
    IReadOnlyList<float> EdgeWeightMatrix,
    IReadOnlyList<CtlPieceBindingRecordRaw> PieceBindings,
    IReadOnlyList<CtlItemRecordRaw> ItemRecords,
    ControlCommandListIndexTable CommandTable,
    IReadOnlyList<ControlCommandListRaw> CommandLists);

public readonly record struct CtlHeader(
    uint ControlStateCount,
    uint ControlStateLinkSlotCount,
    uint PieceBindingCount,
    uint ItemRecordCount,
    uint CommandListCount);

public sealed record CtlRootControlParams(
    long Offset,
    Vec3 MaxTangentialAccel,
    uint Unknown0C,
    uint Unknown10,
    uint Unknown14,
    Vec3 MaxTangentialSpeed,
    Vec3 MaxAngularSpeed,
    Vec3 ExternalTorqueResponseScale,
    Vec3 VisualEulerAngleLimit,
    int DetachCleanupDelayMs,
    float Unknown4C,
    float Unknown50,
    int MovementPhysicsMode,
    float Unknown58,
    float SlopeLimitAngle,
    uint Unknown60,
    float Unknown64,
    float MobilityIntegrityAmount);

public sealed record CtlStateRecordRaw(
    long Offset,
    byte[] FixedBody,
    IReadOnlyList<NavNodeLinkSlotRaw> Links);

public readonly record struct NavNodeLinkSlotRaw(uint Word0, uint Word1, uint Word2, uint Word3);

public sealed record CtlPieceBindingRecordRaw(
    long Offset,
    int LocalPieceIndex,
    float Anim1Start,
    float Anim1EndOrDuration,
    float Anim2Start,
    float Anim2EndOrDuration,
    float AnimBlendWeight,
    float DefaultValue,
    int PrimaryCptRefIndex,
    int SecondaryCptRefIndex,
    float Param24,
    float Param28,
    uint Flags);

public sealed record CtlItemRecordRaw(
    long Offset,
    long EndOffset,
    int ItemType,
    int LocalPieceIndex,
    uint Flags,
    int Unknown0C,
    int Unknown10,
    int Unknown14,
    int OptionalLinkIndex,
    uint MobilityContribution,
    float Param20,
    float Param24,
    float Param28,
    float Param2C,
    IReadOnlyList<float> CommonParams,
    ResourceNamedRef Resource,
    IReadOnlyList<int> PieceBindingIndices,
    int ItemNameLength,
    string ItemName);

public sealed record ControlCommandListIndexTable(long Offset, IReadOnlyList<int> Indices)
{
    public int OnLoad => Indices.Count > 0 ? Indices[0] : -1;
    public int OnEnterCriticalDecay => Indices.Count > 6 ? Indices[6] : -1;
    public int OnExitCriticalDecay => Indices.Count > 7 ? Indices[7] : -1;
}

public sealed record ControlCommandListRaw(
    long Offset,
    uint Count,
    IReadOnlyList<ControlCommandRecordRaw> Records);

public sealed record ControlCommandRecordRaw(
    long Offset,
    ControlCommandConditionRaw Condition,
    EControlCommandOpcode Opcode,
    uint Arg0,
    uint Arg1,
    uint Arg2,
    uint Arg3,
    uint Arg4,
    ResourceNamedRef Resource);

public enum EControlCommandOpcode : uint {
    CONTROL_CMD_RESET_MOTION_COMMANDS = 0,
    CONTROL_CMD_MARK_ROOT_CENTRAL = 1,
    CONTROL_CMD_UNMARK_ROOT_CENTRAL = 2,
    CONTROL_CMD_SPAWN_EFFECT_ON_JOINT_BOUNDS = 3,
    CONTROL_CMD_SPAWN_ORIENTED_EFFECT_FROM_CPTS = 4,
    CONTROL_CMD_SPAWN_EFFECT_ON_OBJECT_BOUNDS = 5,
    CONTROL_CMD_KILL_PIECE = 7,
    CONTROL_CMD_REMOVE_EFFECT = 8,
    CONTROL_CMD_ITEM_LOAD_RESOURCE = 9,
    CONTROL_CMD_START_EFFECT = 0xA,
    CONTROL_CMD_REQUEST_STOP_EFFECT = 0xB,
    CONTROL_CMD_SET_TANG_ACCEL = 0xC,
    CONTROL_CMD_SET_NORM_SPEED = 0xD,
    CONTROL_CMD_ATTACH_EFFECT_TO_PIECE = 0xE,
    CONTROL_CMD_SCHEDULE_DETACH_OR_KILL = 0xF,
    CONTROL_CMD_FORCE_KILL_IGNORE_INVULNERABILITY = 0x11,
    CONTROL_CMD_ACTIVATE_EFFECT = 0x12,
    CONTROL_CMD_DEACTIVATE_EFFECT = 0x13,
    CONTROL_CMD_PLACE_BUILDING_ON_TERRAIN = 0x14,
    CONTROL_CMD_DAMAGE_OBJECTS_IN_BOUNDS = 0x15,
    CONTROL_CMD_SPAWN_OBJECT_FROM_3_CPTS = 0x16,
    CONTROL_CMD_EXECUTE_ITEM_ACTION_FALSE = 0x17,
    CONTROL_CMD_SET_ITEM_FLAG_0x100 = 0x18,
    CONTROL_CMD_CLEAR_ITEM_FLAG_0x100 = 0x19,
    CONTROL_CMD_SWAP_NDP_RESOURCE = 0x1B,
    CONTROL_CMD_QUERY_PARENT_CEILING = 0x1D,
    CONTROL_CMD_EXECUTE_ITEM_ACTION_TRUE = 0x1E,
    CONTROL_CMD_SET_OBJECT_TRANSFORMATION = 0x1F,
    CONTROL_CMD_DISTRIBUTE_DAMAGE = 0x23,
    CONTROL_CMD_QUEUE_PATH_TO_CONTROL_STATE = 0x28,
    CONTROL_CMD_QUEUE_PATH_TO_RANDOM_CONTROL_STATE = 0x29,
    CONTROL_CMD_SET_NET_SYNC_FLAG_0x10000000 = 0x2A,
    CONTROL_CMD_CLEAR_NET_SYNC_FLAG_0x10000000 = 0x2B,
    CONTROL_CMD_UNKNOWN_0x2c = 0x2C,
    CONTROL_CMD_UNKNOWN_0x2d = 0x2D,
    CONTROL_CMD_UNKNOWN_0x2e = 0x2E,
    CONTROL_CMD_SET_NET_SYNCHRO_MODE = 0x2F,
}

public readonly record struct ControlCommandConditionRaw(
    uint ConditionFlags,
    uint ConditionMask,
    uint ConditionInvertMask);

internal sealed class LeReader
{
    private readonly Stream _s;
    private readonly byte[] _buf = new byte[8];

    public LeReader(Stream stream) => _s = stream;
    public Stream BaseStream => _s;

    public byte[] ReadBytesExact(int count)
    {
        byte[] bytes = new byte[count];
        _s.ReadExactly(bytes, 0, count);
        return bytes;
    }

    public int ReadInt32()
    {
        _s.ReadExactly(_buf, 0, 4);
        return BitConverter.ToInt32(_buf, 0);
    }

    public uint ReadUInt32()
    {
        _s.ReadExactly(_buf, 0, 4);
        return BitConverter.ToUInt32(_buf, 0);
    }

    public float ReadSingle()
    {
        _s.ReadExactly(_buf, 0, 4);
        return BitConverter.ToSingle(_buf, 0);
    }

    public Vec3 ReadVec3() => new(ReadSingle(), ReadSingle(), ReadSingle());

    public string ReadFixedString(int byteCount)
    {
        byte[] raw = ReadBytesExact(byteCount);
        int nul = Array.IndexOf(raw, (byte)0);
        int len = nul >= 0 ? nul : raw.Length;
        return Encoding.ASCII.GetString(raw, 0, len);
    }

    public ResourceNamedRef ReadResourceNamedRef()
    {
        return new ResourceNamedRef(
            ArchiveName: ReadFixedString(32),
            ResourceName: ReadFixedString(32));
    }

    public int[] ReadInt32Array(int count)
    {
        var arr = new int[count];
        for (int i = 0; i < count; i++)
            arr[i] = ReadInt32();
        return arr;
    }

    public void ExpectEofOrThrow(string sourceName)
    {
        if (_s.CanSeek && _s.Position != _s.Length)
            throw new ControlReadException($"{sourceName}: parser stopped at 0x{_s.Position:X}, file length is 0x{_s.Length:X}");
    }
}
