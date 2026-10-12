using System.Text;
using Common;

namespace ControlLib;

/// <summary>
/// Чтение ресурсов управления первой части игры: контрольных точек (CPT), повреждений (NDP) и файлов управления (CTL).
/// Раскладки подтверждены разбором <c>CControlBase</c> (Control.dll) и 449 файлами CTL, 225 CPT, 417 NDP; файлы второй части (<c>is2_bases</c>) не поддерживаются.
/// </summary>
public static class ControlResourceReader
{
    /// <summary>Проверяет, что разбор дошёл до конца потока (если поток позволяет определить длину).</summary>
    private static void ExpectEofOrThrow(Stream stream, string sourceName)
    {
        if (stream.CanSeek && stream.Position != stream.Length)
            throw new ControlReadException($"{sourceName}: parser stopped at 0x{stream.Position:X}, file length is 0x{stream.Length:X}");
    }

    /// <summary>Читает файл контрольных точек CPT с диска.</summary>
    /// <param name="path">Путь к файлу.</param>
    /// <exception cref="ControlReadException">После разбора остались необработанные байты.</exception>
    public static CptFile ReadCptFile(string path)
    {
        using var fs = File.OpenRead(path);
        return ReadCptFile(fs, path);
    }

    /// <summary>Читает CPT из потока, установленного на начало файла.</summary>
    /// <param name="stream">Поток с содержимым CPT.</param>
    /// <param name="sourceName">Имя источника для сообщений об ошибках.</param>
    /// <exception cref="ControlReadException">После разбора остались необработанные байты.</exception>
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

    /// <summary>Читает файл повреждений NDP с диска.</summary>
    /// <param name="path">Путь к файлу.</param>
    /// <exception cref="ControlReadException">После разбора остались необработанные байты.</exception>
    public static NdpFile ReadNdpFile(string path)
    {
        using var fs = File.OpenRead(path);
        return ReadNdpFile(fs, path);
    }

    /// <summary>Читает NDP из потока, установленного на начало файла.</summary>
    /// <param name="stream">Поток с содержимым NDP.</param>
    /// <param name="sourceName">Имя источника для сообщений об ошибках.</param>
    /// <exception cref="ControlReadException">После разбора остались необработанные байты.</exception>
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

    /// <summary>Читает файл управления CTL первой части игры с диска.</summary>
    /// <param name="path">Путь к файлу.</param>
    /// <exception cref="ControlReadException">Отрицательная длина имени предмета или остаток данных после разбора.</exception>
    public static CtlFile ReadCtlFile(string path)
    {
        using var fs = File.OpenRead(path);
        return ReadCtlFile(fs, path);
    }

    /// <summary>Читает CTL из потока, установленного на начало файла.</summary>
    /// <param name="stream">Поток с содержимым CTL.</param>
    /// <param name="sourceName">Имя источника для сообщений об ошибках.</param>
    /// <exception cref="ControlReadException">Отрицательная длина имени предмета или остаток данных после разбора.</exception>
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

    /// <summary>Читает 0x6C байт корневых параметров CTL.</summary>
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

    /// <summary>Читает привязку детали (0x30 байт).</summary>
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

    /// <summary>Читает запись предмета: фиксированная часть 0x70 байт, число привязок, индексы привязок, длина и имя предмета.</summary>
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
            // Движок выделяет длина + 1 байт и копирует длина + 1 байт из файла.
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

    /// <summary>Читает команду списка (0x64 байта).</summary>
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
