using System.Text;
using Common;

namespace ControlLib;

public sealed class ControlReadException : Exception
{
    public ControlReadException(string message) : base(message) { }
}

public static class ControlResourceReader
{
    private static void ExpectEofOrThrow(Stream stream, string sourceName)
    {
        if (stream.CanSeek && stream.Position != stream.Length)
            throw new ControlReadException($"{sourceName}: parser stopped at 0x{stream.Position:X}, file length is 0x{stream.Length:X}");
    }

    public static CptFile ReadCptFile(string path)
    {
        using var fs = File.OpenRead(path);
        return ReadCptFile(fs, path);
    }

    public static CptFile ReadCptFile(Stream stream, string? sourceName = null)
    {
        var r = stream;
        long start = stream.Position;
        uint count = r.ReadUInt32LittleEndian();
        var records = new List<CptRecordRaw>(checked((int)count));

        for (int i = 0; i < count; i++)
        {
            long off = stream.Position - start;
            records.Add(new CptRecordRaw(
                Offset: off,
                Unknown0: r.ReadInt32LittleEndian(),
                OwnerPieceIndex: r.ReadInt32LittleEndian(),
                TargetPieceIndex: r.ReadInt32LittleEndian(),
                Position: r.ReadVector3LittleEndian(),
                Direction: r.ReadVector3LittleEndian()
            ));
        }

        var names = new List<string>(checked((int)count));
        for (int i = 0; i < count; i++)
        {
            names.Add(r.ReadFixedString(32));
        }

        ExpectEofOrThrow(stream, sourceName ?? "CPT");
        return new CptFile(sourceName, count, records, names);
    }

    public static NdpFile ReadNdpFile(string path)
    {
        using var fs = File.OpenRead(path);
        return ReadNdpFile(fs, path);
    }

    public static NdpFile ReadNdpFile(Stream stream, string? sourceName = null)
    {
        var r = stream;
        long start = stream.Position;
        uint count = r.ReadUInt32LittleEndian();
        var records = new List<NdpRecordRaw>(checked((int)count));

        for (int i = 0; i < count; i++)
        {
            long off = stream.Position - start;
            records.Add(new NdpRecordRaw(
                Offset: off,
                Unknown0: r.ReadUInt32LittleEndian(),
                MaxVolume: r.ReadFloatLittleEndian(),
                MobilityContributionPerVolume: r.ReadFloatLittleEndian(),
                Resource: r.ReadResourceNamedRef()
            ));
        }

        ExpectEofOrThrow(stream, sourceName ?? "NDP");
        return new NdpFile(sourceName, count, records);
    }

    public static CtlFile ReadCtlFile(string path)
    {
        using var fs = File.OpenRead(path);
        return ReadCtlFile(fs, path);
    }

    public static CtlFile ReadCtlFile(Stream stream, string? sourceName = null)
    {
        var r = stream;
        long start = stream.Position;

        var header = new CtlHeader(
            ControlStateCount: r.ReadUInt32LittleEndian(),
            ControlStateLinkSlotCount: r.ReadUInt32LittleEndian(),
            PieceBindingCount: r.ReadUInt32LittleEndian(),
            ItemRecordCount: r.ReadUInt32LittleEndian(),
            CommandListCount: r.ReadUInt32LittleEndian()
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
                    Word0: r.ReadUInt32LittleEndian(),
                    Word1: r.ReadUInt32LittleEndian(),
                    Word2: r.ReadUInt32LittleEndian(),
                    Word3: r.ReadUInt32LittleEndian()
                ));
            }
            states.Add(new CtlStateRecordRaw(off, fixedBody, links));
        }

        int edgeCount = checked((int)(header.ControlStateCount * header.ControlStateCount));
        var edgeWeightMatrix = new float[edgeCount];
        for (int i = 0; i < edgeWeightMatrix.Length; i++)
            edgeWeightMatrix[i] = r.ReadFloatLittleEndian();

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
        var commandTable = new ControlCommandListIndexTable(commandTableOffset, r.ReadInt32ArrayLittleEndian(21));

        var commandLists = new List<ControlCommandListRaw>(checked((int)header.CommandListCount));
        for (int i = 0; i < header.CommandListCount; i++)
        {
            long listOffset = stream.Position - start;
            uint count = r.ReadUInt32LittleEndian();
            var records = new List<ControlCommandRecordRaw>(checked((int)count));
            for (int j = 0; j < count; j++)
            {
                long recordOffset = stream.Position - start;
                records.Add(ReadCommandRecord(r, recordOffset));
            }
            commandLists.Add(new ControlCommandListRaw(listOffset, count, records));
        }

        ExpectEofOrThrow(stream, sourceName ?? "CTL");

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

    private static CtlRootControlParams ReadCtlRootControlParams(Stream r, long offset)
    {
        return new CtlRootControlParams(
            Offset: offset,
            MaxTangentialAccel: r.ReadVector3LittleEndian(),
            Unknown0C: r.ReadUInt32LittleEndian(),
            Unknown10: r.ReadUInt32LittleEndian(),
            Unknown14: r.ReadUInt32LittleEndian(),
            MaxTangentialSpeed: r.ReadVector3LittleEndian(),
            MaxAngularSpeed: r.ReadVector3LittleEndian(),
            ExternalTorqueResponseScale: r.ReadVector3LittleEndian(),
            VisualEulerAngleLimit: r.ReadVector3LittleEndian(),
            DetachCleanupDelayMs: r.ReadInt32LittleEndian(),
            Unknown4C: r.ReadFloatLittleEndian(),
            Unknown50: r.ReadFloatLittleEndian(),
            MovementPhysicsMode: r.ReadInt32LittleEndian(),
            Unknown58: r.ReadFloatLittleEndian(),
            SlopeLimitAngle: r.ReadFloatLittleEndian(),
            Unknown60: r.ReadUInt32LittleEndian(),
            Unknown64: r.ReadFloatLittleEndian(),
            MobilityIntegrityAmount: r.ReadFloatLittleEndian()
        );
    }

    private static CtlPieceBindingRecordRaw ReadPieceBinding(Stream r, long offset)
    {
        return new CtlPieceBindingRecordRaw(
            Offset: offset,
            LocalPieceIndex: r.ReadInt32LittleEndian(),
            Anim1Start: r.ReadFloatLittleEndian(),
            Anim1EndOrDuration: r.ReadFloatLittleEndian(),
            Anim2Start: r.ReadFloatLittleEndian(),
            Anim2EndOrDuration: r.ReadFloatLittleEndian(),
            AnimBlendWeight: r.ReadFloatLittleEndian(),
            DefaultValue: r.ReadFloatLittleEndian(),
            PrimaryCptRefIndex: r.ReadInt32LittleEndian(),
            SecondaryCptRefIndex: r.ReadInt32LittleEndian(),
            ChannelSpeed: r.ReadFloatLittleEndian(),
            YawNormalizationAngle: r.ReadFloatLittleEndian(),
            Flags: r.ReadUInt32LittleEndian()
        );
    }

    private static CtlItemRecordRaw ReadCtlItemRecord(Stream r, long fileStart, long offset)
    {
        int itemType = r.ReadInt32LittleEndian();
        int localPieceIndex = r.ReadInt32LittleEndian();
        uint flags = r.ReadUInt32LittleEndian();
        int unknown0C = r.ReadInt32LittleEndian();
        int unknown10 = r.ReadInt32LittleEndian();
        int unknown14 = r.ReadInt32LittleEndian();
        int initialRuntimeState = r.ReadInt32LittleEndian();
        float unknown1C = r.ReadFloatLittleEndian();
        float unknown20 = r.ReadFloatLittleEndian();
        float unknown24 = r.ReadFloatLittleEndian();
        float signalAWeight = r.ReadFloatLittleEndian();
        float signalBWeight = r.ReadFloatLittleEndian();

        var commonParams = new float[16];
        for (int i = 0; i < commonParams.Length; i++)
            commonParams[i] = r.ReadFloatLittleEndian();

        string resourceArchiveName = r.ReadFixedString(32);
        string resourceResourceName = r.ReadFixedString(32);
        uint bindingCount = r.ReadUInt32LittleEndian();

        var bindingIndices = new int[checked((int)bindingCount)];
        for (int i = 0; i < bindingIndices.Length; i++)
            bindingIndices[i] = r.ReadInt32LittleEndian();

        int itemNameLength = r.ReadInt32LittleEndian();
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

        long endOffset = r.Position - fileStart;

        return new CtlItemRecordRaw(
            Offset: offset,
            EndOffset: endOffset,
            ItemType: itemType,
            LocalPieceIndex: localPieceIndex,
            Flags: flags,
            Unknown0C: unknown0C,
            Unknown10: unknown10,
            Unknown14: unknown14,
            InitialRuntimeState: initialRuntimeState,
            Param1C: unknown1C,
            Param20: unknown20,
            Param24: unknown24,
            SignalAWeight: signalAWeight,
            SignalBWeight: signalBWeight,
            CommonParams: commonParams,
            Resource: new ResourceNamedRef(resourceArchiveName, resourceResourceName),
            PieceBindingIndices: bindingIndices,
            ItemNameLength: itemNameLength,
            ItemName: itemName
        );
    }

    private static ControlCommandRecordRaw ReadCommandRecord(Stream r, long offset)
    {
        var condition = new ControlCommandConditionRaw(
            ConditionFlags: r.ReadUInt32LittleEndian(),
            ConditionMask: r.ReadUInt32LittleEndian(),
            ConditionInvertMask: r.ReadUInt32LittleEndian()
        );

        return new ControlCommandRecordRaw(
            Offset: offset,
            Condition: condition,
            Opcode: (EControlCommandOpcode)r.ReadUInt32LittleEndian(),
            Arg0: r.ReadUInt32LittleEndian(),
            Arg1: r.ReadUInt32LittleEndian(),
            Arg2: r.ReadUInt32LittleEndian(),
            Arg3: r.ReadUInt32LittleEndian(),
            Arg4: r.ReadUInt32LittleEndian(),
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
    Vector3 Position,
    Vector3 Direction);

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
    Vector3 MaxTangentialAccel,
    uint Unknown0C,
    uint Unknown10,
    uint Unknown14,
    Vector3 MaxTangentialSpeed,
    Vector3 MaxAngularSpeed,
    Vector3 ExternalTorqueResponseScale,
    Vector3 VisualEulerAngleLimit,
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
    float ChannelSpeed, // +0x24: скорость канала анимации, ед./с (время перехода = |Δ| * 1000 / ChannelSpeed мс; CItemBase::ExtendTransitionEndTimeForBinding); у орудий 4.0
    float YawNormalizationAngle, // +0x28: в данных 1.0 или π/2 (1.5708); применение не проверено
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
    int InitialRuntimeState, // +0x18: initial runtime_state of the item, -1 = keep default (CItemBase::ParseCtlItemRecordAndInit)
    float Param1C, // +0x1C: float, 0 or multiples of 12.5 (engines 450..1350, radars 175..1150, shields 200..290, batteries 175..700); CtlRawItem.mobility_contribution: item's contribution to mobility mass and to signature 0x45 (CControlBase::InitializeNdpMobilityAccounting)
    float Param20, // +0x20: raw item value (selector 0x30): base power draw per second; for batteries (0x13) the max output
    float Param24,
    float SignalAWeight, // +0x28: weight of control signal A (flags bits 8..15) in CItemBase::method9
    float SignalBWeight, // +0x2C: weight of control signal B (flags bits 16..23); wheels use 0.5 or 1.0
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
    CONTROL_CMD_INHERIT_CLAN_FROM_PARENT = 0x1D,
    CONTROL_CMD_EXECUTE_ITEM_ACTION_TRUE = 0x1E,
    CONTROL_CMD_SET_OBJECT_TRANSFORMATION = 0x1F,
    CONTROL_CMD_EXECUTE_ITEM_COMMAND_MODE0 = 0x20,
    CONTROL_CMD_EXECUTE_ITEM_COMMAND_MODE1 = 0x21,
    CONTROL_CMD_SET_ITEM_FLAG_0x200 = 0x22,
    CONTROL_CMD_DISTRIBUTE_DAMAGE = 0x23,
    CONTROL_CMD_SET_ITEM_TARGET_POINT = 0x25,
    CONTROL_CMD_SET_ITEM_FLAGS = 0x26,
    CONTROL_CMD_CYCLE_OBJECT_TRANSFORMATION = 0x27,
    CONTROL_CMD_QUEUE_PATH_TO_CONTROL_STATE = 0x28,
    CONTROL_CMD_QUEUE_PATH_TO_RANDOM_CONTROL_STATE = 0x29,
    CONTROL_CMD_SET_NET_SYNC_FLAG_0x10000000 = 0x2A,
    CONTROL_CMD_CLEAR_NET_SYNC_FLAG_0x10000000 = 0x2B,
    CONTROL_CMD_UNKNOWN_0x2c = 0x2C,
    CONTROL_CMD_FILL_BATTERIES = 0x2D,
    CONTROL_CMD_NOOP_0x2e = 0x2E,
    CONTROL_CMD_SET_NET_SYNCHRO_MODE = 0x2F,
}

public readonly record struct ControlCommandConditionRaw(
    uint ConditionFlags,
    uint ConditionMask,
    uint ConditionInvertMask);
