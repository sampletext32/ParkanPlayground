using System.Globalization;

namespace ControlLib;

public static class ControlResourceDump
{
    public static void DumpFiles(string ctlPath, string? cptPath = null, string? ndpPath = null, TextWriter? writer = null)
    {
        var ctl = ControlResourceReader.ReadCtlFile(ctlPath);
        var cpt = cptPath is null ? null : ControlResourceReader.ReadCptFile(cptPath);
        var ndp = ndpPath is null ? null : ControlResourceReader.ReadNdpFile(ndpPath);

        DumpAll(ctl, cpt, ndp, writer);
    }

    public static void DumpAll(CtlFile ctl, CptFile? cpt = null, NdpFile? ndp = null, TextWriter? writer = null)
    {
        var w = writer ?? Console.Out;

        w.WriteLine("================================================================================");
        w.WriteLine($"CTL: {ctl.SourceName}");
        w.WriteLine("================================================================================");
        DumpCtl(ctl, cpt, w);

        if (cpt is not null)
        {
            w.WriteLine();
            w.WriteLine("================================================================================");
            w.WriteLine($"CPT: {cpt.SourceName}");
            w.WriteLine("================================================================================");
            DumpCpt(cpt, w);
        }

        if (ndp is not null)
        {
            w.WriteLine();
            w.WriteLine("================================================================================");
            w.WriteLine($"NDP: {ndp.SourceName}");
            w.WriteLine("================================================================================");
            DumpNdp(ndp, w);
        }
    }

    private static void DumpCtl(CtlFile ctl, CptFile? cpt, TextWriter w)
    {
        var h = ctl.Header;

        w.WriteLine("Header");
        w.WriteLine($"  states       : {h.ControlStateCount}");
        w.WriteLine($"  link slots   : {h.ControlStateLinkSlotCount}");
        w.WriteLine($"  bindings     : {h.PieceBindingCount}");
        w.WriteLine($"  items        : {h.ItemRecordCount}");
        w.WriteLine($"  command lists: {h.CommandListCount}");

        DumpRootParams(ctl.RootParams, w);
        DumpStates(ctl, w);
        DumpPieceBindings(ctl, cpt, w);
        DumpItems(ctl, cpt, w);
        DumpCommandTable(ctl, w);
        DumpCommandLists(ctl, cpt, w);
    }

    private static void DumpRootParams(object root, TextWriter w)
    {
        w.WriteLine();
        w.WriteLine($"Root params @ {Off(GetLong(root, "Offset"))}");
        w.WriteLine($"  max tang accel        : {Vec(Get(root, "MaxTangentialAccel"))}");
        w.WriteLine($"  max tang speed        : {Vec(Get(root, "MaxTangentialSpeed"))}");
        w.WriteLine($"  max angular speed     : {Vec(Get(root, "MaxAngularSpeed"))}");
        w.WriteLine($"  torque response scale : {Vec(Get(root, "ExternalTorqueResponseScale"))}");
        w.WriteLine($"  visual euler limit    : {Vec(Get(root, "VisualEulerAngleLimit"))}");
        w.WriteLine($"  movement physics mode : {GetInt(root, "MovementPhysicsMode")}");
        w.WriteLine($"  slope limit angle     : {F(GetFloat(root, "SlopeLimitAngle"))}");
        w.WriteLine($"  mobility integrity    : {F(GetFloat(root, "MobilityIntegrityAmount"))}");
        w.WriteLine($"  detach cleanup ms     : {GetInt(root, "DetachCleanupDelayMs", "DetachCleanupDelay")}");
        w.WriteLine($"  unknown 0x4C / 0x50   : {F(GetFloat(root, "Unknown4C"))}, {F(GetFloat(root, "Unknown50"))}");
        w.WriteLine($"  unknown 0x58 / 0x60/64: {F(GetFloat(root, "Unknown58"))}, {GetInt(root, "Unknown60")}, {F(GetFloat(root, "Unknown64"))}");
    }

    private static void DumpStates(CtlFile ctl, TextWriter w)
    {
        w.WriteLine();
        w.WriteLine("Control states");

        if (ctl.StateRecords.Count == 0)
        {
            w.WriteLine("  <none>");
            return;
        }

        for (int i = 0; i < ctl.StateRecords.Count; i++)
        {
            var state = ctl.StateRecords[i];
            w.WriteLine($"  [{i}] @ {Off(state.Offset)} links={state.Links.Count}");

            for (int j = 0; j < state.Links.Count; j++)
            {
                var link = state.Links[j];
                w.WriteLine(
                    $"       link[{j}] " +
                    $"w0={Hex(link.Word0)} ({UncheckedInt(link.Word0)}) " +
                    $"w1={Hex(link.Word1)} ({UncheckedInt(link.Word1)}) " +
                    $"w2={Hex(link.Word2)} ({UncheckedInt(link.Word2)}) " +
                    $"w3={Hex(link.Word3)} ({UncheckedInt(link.Word3)})");
            }
        }

        if (ctl.EdgeWeightMatrix.Count != 0)
            w.WriteLine($"  edge weights: {string.Join(", ", ctl.EdgeWeightMatrix.Select(F))}");
    }

    private static void DumpPieceBindings(CtlFile ctl, CptFile? cpt, TextWriter w)
    {
        w.WriteLine();
        w.WriteLine("Piece bindings");

        for (int i = 0; i < ctl.PieceBindings.Count; i++)
        {
            var b = ctl.PieceBindings[i];

            int primary = GetInt(b, "PrimaryCptRefIndex");
            int secondary = GetInt(b, "SecondaryCptRefIndex");
            uint flags = GetUInt(b, "Flags");

            w.WriteLine(
                $"  [{i}] @ {Off(GetLong(b, "Offset"))} " +
                $"piece={GetInt(b, "LocalPieceIndex"),3} " +
                $"cptA={CptRef(cpt, primary),-24} " +
                $"cptB={CptRef(cpt, secondary),-24} " +
                $"default={F(GetFloat(b, "DefaultValue")),8} " +
                $"flags={Hex(flags)} {PieceBindingFlags(flags)}");

            w.WriteLine(
                $"       anim1=({F(GetFloat(b, "Anim1Start"))}, {F(GetFloat(b, "Anim1EndOrDuration"))}) " +
                $"anim2=({F(GetFloat(b, "Anim2Start"))}, {F(GetFloat(b, "Anim2EndOrDuration"))}) " +
                $"weight={F(GetFloat(b, "AnimBlendWeight"))} " +
                $"p24={F(GetFloat(b, "BindingParam24", "Unknown24"))} " +
                $"p28={F(GetFloat(b, "BindingParam28", "Unknown28"))}");
        }
    }
    private static string ItemFlags(uint flags)
    {
        var names = new List<string>();

        if ((flags & 0x01000000) != 0) names.Add("left_drive_group");
        if ((flags & 0x02000000) != 0) names.Add("right_drive_group");
        if ((flags & 0x04000000) != 0) names.Add("invert_motion_or_orientation");

        return names.Count == 0 ? "" : $"[{string.Join("|", names)}]";
    }
    private static void DumpItems(CtlFile ctl, CptFile? cpt, TextWriter w)
    {
        w.WriteLine();
        w.WriteLine("Items");

        for (int i = 0; i < ctl.ItemRecords.Count; i++)
        {
            var item = ctl.ItemRecords[i];

            int type = GetInt(item, "ItemType");
            uint flags = GetUInt(item, "Flags");
            var bindingIndices = ToIntList(Get(item, "PieceBindingIndices"));

            string name = GetString(item, "ItemName");
            string namePart = string.IsNullOrEmpty(name) ? "" : $" name='{name}'";

            w.WriteLine(
                $"  [{i}] @ {Off(GetLong(item, "Offset"))}-{Off(GetLong(item, "EndOffset"))} " +
                $"type=0x{type:X2} {ItemTypeName(type)} " +
                $"piece={GetInt(item, "LocalPieceIndex"),3} " +
                $"flags={Hex(flags)} {ItemFlags(flags)}{namePart}");

            w.WriteLine(
                $"       idx0C/10/14={GetInt(item, "OptionalIndex0C", "Unknown0C")}, " +
                $"{GetInt(item, "OptionalIndex10", "Unknown10")}, " +
                $"{GetInt(item, "OptionalIndex14", "Unknown14")} " +
                $"link={GetInt(item, "OptionalLinkIndex")} " +
                $"mobility={GetInt(item, "MobilityContribution")}");

            w.WriteLine(
                $"       p20/24/28/2C={F(GetFloat(item, "Param20", "Unknown20"))}, " +
                $"{F(GetFloat(item, "Param24", "Unknown24"))}, " +
                $"{F(GetFloat(item, "Param28", "Unknown28"))}, " +
                $"{F(GetFloat(item, "Param2C", "Unknown2C"))}");

            var resource = Get(item, "Resource");
            if (resource is not null && !ResourceIsEmpty(resource))
                w.WriteLine($"       resource={ResourceToString(resource)}");

            var commonParams = ToFloatList(Get(item, "CommonParams"));
            var nonZeroParams = commonParams
                .Select((v, idx) => (idx, v))
                .Where(x => Math.Abs(x.v) > 0.00001f)
                .Select(x => $"{x.idx}:{F(x.v)}")
                .ToArray();

            if (nonZeroParams.Length != 0)
                w.WriteLine($"       common params: {string.Join(", ", nonZeroParams)}");

            if (bindingIndices.Count != 0)
            {
                w.WriteLine($"       binding refs: {string.Join(", ", bindingIndices.Select(x => BindingRef(ctl, cpt, x)))}");
            }
        }
    }

    private static void DumpCommandTable(CtlFile ctl, TextWriter w)
    {
        w.WriteLine();
        w.WriteLine($"Command hook table @ {Off(ctl.CommandTable.Offset)}");
        w.WriteLine($"  on_load                : {ctl.CommandTable.OnLoad}");
        w.WriteLine($"  on_enter_critical_decay: {ctl.CommandTable.OnEnterCriticalDecay}");
        w.WriteLine($"  on_exit_critical_decay : {ctl.CommandTable.OnExitCriticalDecay}");

        for (int i = 0; i < ctl.CommandTable.Indices.Count; i++)
        {
            int index = ctl.CommandTable.Indices[i];
            if (index != -1)
                w.WriteLine($"  slot[{i,2}] -> list {index}");
        }
    }

    private static void DumpCommandLists(CtlFile ctl, CptFile? cpt, TextWriter w)
    {
        w.WriteLine();
        w.WriteLine("Command lists");

        for (int listIndex = 0; listIndex < ctl.CommandLists.Count; listIndex++)
        {
            var list = ctl.CommandLists[listIndex];
            w.WriteLine($"  list[{listIndex}] @ {Off(list.Offset)} count={list.Count}");

            for (int commandIndex = 0; commandIndex < list.Records.Count; commandIndex++)
            {
                var cmd = list.Records[commandIndex];
                uint opcode = GetUInt(cmd, "Opcode");
                var args = GetCommandArgs(cmd);

                w.WriteLine(
                    $"    [{commandIndex}] @ {Off(GetLong(cmd, "Offset"))} " +
                    $"op=0x{opcode:X2} {OpcodeName(opcode)}");

                var condition = Get(cmd, "Condition");
                if (condition is not null)
                {
                    uint cf = GetUInt(condition, "ConditionFlags");
                    uint cm = GetUInt(condition, "ConditionMask");
                    uint ci = GetUInt(condition, "ConditionInvertMask");
                    if (cf != 0 || cm != 0 || ci != 0)
                        w.WriteLine($"         cond flags={Hex(cf)} mask={Hex(cm)} invert={Hex(ci)}");
                }

                w.WriteLine(
                    $"         args int  : {string.Join(", ", args.Select(x => UncheckedInt(x).ToString(CultureInfo.InvariantCulture)))}");
                w.WriteLine(
                    $"         args float: {string.Join(", ", args.Select(x => F(U32AsFloat(x))))}");

                var resource = Get(cmd, "Resource");
                if (resource is not null && !ResourceIsEmpty(resource))
                    w.WriteLine($"         resource  : {ResourceToString(resource)}");

                PrintCommandHints(cpt, opcode, args, w);
            }
        }
    }

    private static void DumpCpt(CptFile cpt, TextWriter w)
    {
        for (int i = 0; i < cpt.Records.Count; i++)
        {
            var r = cpt.Records[i];
            string name = i < cpt.Names.Count ? cpt.Names[i] : "";
            int piece = GetInt(r, "OwnerPieceIndex");

            w.WriteLine(
                $"  [{i,2}] @ {Off(GetLong(r, "Offset"))} " +
                $"name='{name,-16}' OwnerPieceIndex={piece,3} " +
                $"TargetPieceIndex={GetInt(r, "TargetPieceIndex"),3} " +
                $"pos={Vec(Get(r, "Position"))} dir={Vec(Get(r, "Direction"))}");
        }
    }

    private static void DumpNdp(NdpFile ndp, TextWriter w)
    {
        float totalMax = 0;

        for (int i = 0; i < ndp.Records.Count; i++)
        {
            var r = ndp.Records[i];
            float max = GetFloat(r, "MaxVolume");
            float mobility = GetFloat(r, "MobilityContributionPerVolume");
            totalMax += max;

            w.WriteLine(
                $"  [{i,2}] @ {Off(GetLong(r, "Offset"))} " +
                $"max={F(max),8} mobility/vol={F(mobility),10} " +
                $"unknown0={Hex(GetUInt(r, "Unknown0"))} " +
                $"resource={ResourceToString(Get(r, "Resource")!)}");
        }

        w.WriteLine($"  total max volume: {F(totalMax)}");
    }

    private static void PrintCommandHints(CptFile? cpt, uint opcode, IReadOnlyList<uint> args, TextWriter w)
    {
        if (cpt is null || args.Count < 5)
            return;

        switch (opcode)
        {
            case 0x03:
            case 0x04:
            case 0x05:
            case 0x16:
                w.WriteLine(
                    $"         cpt hint  : " +
                    $"a0={CptRef(cpt, UncheckedInt(args[0]))}, " +
                    $"a1={CptRef(cpt, UncheckedInt(args[1]))}, " +
                    $"a2={CptRef(cpt, UncheckedInt(args[2]))}, " +
                    $"id/local={UncheckedInt(args[3])}, " +
                    $"scale/range={F(U32AsFloat(args[4]))}");
                break;
        }
    }

    private static string BindingRef(CtlFile ctl, CptFile? cpt, int bindingIndex)
    {
        if (bindingIndex < 0 || bindingIndex >= ctl.PieceBindings.Count)
            return $"{bindingIndex}:<bad>";

        var b = ctl.PieceBindings[bindingIndex];
        return $"{bindingIndex}:piece {GetInt(b, "LocalPieceIndex")} cptA={CptRef(cpt, GetInt(b, "PrimaryCptRefIndex"))}";
    }

    private static string CptRef(CptFile? cpt, int index)
    {
        if (index < 0)
            return "-";

        if (cpt is null)
            return index.ToString(CultureInfo.InvariantCulture);

        if (index >= cpt.Records.Count)
            return $"{index}:<bad>";

        string name = index < cpt.Names.Count ? cpt.Names[index] : "";
        var r = cpt.Records[index];

        string label = string.IsNullOrEmpty(name) ? $"cpt{index}" : name;
        return $"{index}:{label}/owner {r.OwnerPieceIndex}/target {r.TargetPieceIndex}";
    }

    private static string PieceBindingFlags(uint flags)
    {
        var names = new List<string>();

        if ((flags & 0x01) != 0) names.Add("wrap");
        if ((flags & 0x02) != 0) names.Add("invert");
        if ((flags & 0x04) != 0) names.Add("skip_anim_init");
        if ((flags & 0x08) != 0) names.Add("auto_item_ref");
        if ((flags & 0x100) != 0) names.Add("conditional_link_target");

        return names.Count == 0 ? "" : $"[{string.Join("|", names)}]";
    }

    private static string ItemTypeName(int type)
    {
        return type switch
        {
            0x01 => "turret drive",
            0x02 => "projectile weapon",
            0x03 => "mobility support",
            0x04 => "targeting system",
            0x05 => "engine",
            0x08 => "sensor-radar",
            0x09 => "force shield",
            0x0A => "detection shield",
            0x0F => "repair system",
            0x11 => "BULL guidance",
            0x13 => "battery",
            0x14 => "wheel mobility unit",
            0x15 => "deflector",
            0x16 => "weapon/actuator? type16",
            0x1A => "engine alt",
            0x1B => "armour",
            0x1E => "projectile weapon alt",
            _ => $"unknown item type 0x{type:X2}",
        };
    }

    private static string OpcodeName(uint op)
    {
        return Enum.IsDefined(typeof(EControlCommandOpcode), op) ? ((EControlCommandOpcode)op).ToString() : $"UNKNOWN_0x{op:X2}";
    }

    private static IReadOnlyList<uint> GetCommandArgs(ControlCommandRecordRaw cmd)
    {
        return
        [
            GetUInt(cmd, "Arg0"),
            GetUInt(cmd, "Arg1"),
            GetUInt(cmd, "Arg2"),
            GetUInt(cmd, "Arg3"),
            GetUInt(cmd, "Arg4"),
        ];
    }

    private static object? Get(object? obj, params string[] names)
    {
        if (obj is null)
            return null;

        var t = obj.GetType();

        foreach (string name in names)
        {
            var p = t.GetProperty(name);
            if (p is not null)
                return p.GetValue(obj);
        }

        return null;
    }

    private static int GetInt(object? obj, params string[] names)
    {
        var v = Get(obj, names);
        return v switch
        {
            Enum e => Convert.ToInt32(e, CultureInfo.InvariantCulture),
            int x => x,
            uint x => unchecked((int)x),
            long x => checked((int)x),
            ulong x => checked((int)x),
            short x => x,
            ushort x => x,
            byte x => x,
            sbyte x => x,
            float x => checked((int)x),
            double x => checked((int)x),
            _ => 0
        };
    }

    private static uint GetUInt(object? obj, params string[] names)
    {
        var v = Get(obj, names);
        return v switch
        {
            Enum e => Convert.ToUInt32(e, CultureInfo.InvariantCulture),
            uint x => x,
            int x => unchecked((uint)x),
            long x => checked((uint)x),
            ulong x => checked((uint)x),
            short x => unchecked((uint)x),
            ushort x => x,
            byte x => x,
            sbyte x => unchecked((uint)x),
            _ => 0
        };
    }

    private static long GetLong(object? obj, params string[] names)
    {
        var v = Get(obj, names);
        return v switch
        {
            long x => x,
            int x => x,
            uint x => x,
            ulong x => checked((long)x),
            _ => 0
        };
    }

    private static float GetFloat(object? obj, params string[] names)
    {
        var v = Get(obj, names);

        return v switch
        {
            float x => x,
            double x => (float)x,
            int x when Math.Abs(x) > 1_000_000 => BitConverter.Int32BitsToSingle(x),
            uint x when x > 1_000_000 => BitConverter.UInt32BitsToSingle(x),
            int x => x,
            uint x => x,
            long x => x,
            ulong x => x,
            _ => 0
        };
    }

    private static string GetString(object? obj, params string[] names)
    {
        return Get(obj, names) as string ?? "";
    }

    private static List<int> ToIntList(object? value)
    {
        if (value is IEnumerable<int> ints)
            return ints.ToList();

        if (value is IEnumerable<uint> uints)
            return uints.Select(uncheckedInt => unchecked((int)uncheckedInt)).ToList();

        return [];
    }

    private static List<float> ToFloatList(object? value)
    {
        if (value is IEnumerable<float> floats)
            return floats.ToList();

        if (value is IEnumerable<double> doubles)
            return doubles.Select(x => (float)x).ToList();

        return [];
    }

    private static string Vec(object? v)
    {
        if (v is null)
            return "(0, 0, 0)";

        return $"({F(GetFloat(v, "X"))}, {F(GetFloat(v, "Y"))}, {F(GetFloat(v, "Z"))})";
    }

    private static string ResourceToString(object resource)
    {
        var archive = GetString(resource, "ArchiveName");
        var name = GetString(resource, "ResourceName");
        return string.IsNullOrEmpty(archive) || string.IsNullOrEmpty(name)
            ? "<empty>"
            : $"{archive}/{name}";
    }

    private static bool ResourceIsEmpty(object resource)
    {
        var archive = GetString(resource, "ArchiveName");
        var name = GetString(resource, "ResourceName");
        return string.IsNullOrEmpty(archive) || string.IsNullOrEmpty(name);
    }

    private static string Off(long offset) => $"0x{offset:X4}";
    private static string Hex(uint value) => $"0x{value:X8}";
    private static int UncheckedInt(uint value) => unchecked((int)value);
    private static float U32AsFloat(uint value) => BitConverter.UInt32BitsToSingle(value);

    private static string F(float value)
    {
        if (float.IsNaN(value))
            return "NaN";
        if (float.IsPositiveInfinity(value))
            return "+Inf";
        if (float.IsNegativeInfinity(value))
            return "-Inf";

        return value.ToString("0.######", CultureInfo.InvariantCulture);
    }
}