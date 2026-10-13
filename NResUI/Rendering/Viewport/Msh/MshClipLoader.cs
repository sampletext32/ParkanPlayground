using System.Text.RegularExpressions;
using ControlLib;

namespace NResUI.Rendering.Viewport.Msh;

/// <summary>
/// Строит список клипов меша из состояний CTL и ищет CTL рядом с файлом меша.
/// </summary>
public static partial class MshClipLoader
{
    /// <summary>Клипы и цепочки по состояниям CTL.</summary>
    public static MshClipSet FromCtl(CtlFile ctl)
    {
        return new MshClipSet(
            BuildClips(ctl),
            BuildChains(ctl, joinThroughEndpoints: false),
            BuildChains(ctl, joinThroughEndpoints: true),
            BuildChannels(ctl));
    }

    // Привязки с диапазоном: значение канала 0..1 -> время start + value * (end - start), как у деталей орудий и колёс.
    private static List<MshChannel> BuildChannels(CtlFile ctl)
    {
        var channels = new List<MshChannel>();
        for (var i = 0; i < ctl.PieceBindings.Count; i++)
        {
            var b = ctl.PieceBindings[i];
            var useRange2 = b.Anim2Start >= 0.0f && b.Anim2EndOrDuration >= b.Anim2Start;
            var useRange1 = !useRange2 && b.Anim1Start >= 0.0f && b.Anim1EndOrDuration >= b.Anim1Start;
            if (!useRange2 && !useRange1)
                continue;

            var start = useRange2 ? b.Anim2Start : b.Anim1Start;
            var end = useRange2 ? b.Anim2EndOrDuration : b.Anim1EndOrDuration;
            var defaultValue = (b.Flags & 0x2) != 0 ? 1.0f - b.DefaultValue : b.DefaultValue;

            channels.Add(new MshChannel(
                i,
                b.LocalPieceIndex,
                start,
                end,
                Math.Clamp(defaultValue, 0.0f, 1.0f),
                b.ChannelSpeed,
                (b.Flags & 0x1) != 0,
                useRange2));
        }

        return channels;
    }

    // Состояния с одинаковыми диапазонами и весом схлопываются в первое.
    private static List<MshClipSequence> BuildClips(CtlFile ctl)
    {
        var clips = new List<MshClipSequence>();
        var seen = new HashSet<(float, float, float, float, float, float)>();

        for (var i = 0; i < ctl.StateRecords.Count; i++)
        {
            var a = ctl.StateRecords[i].Animation;
            if (a.Anim1Start < 0.0f && a.Anim2Start < 0.0f)
                continue;

            var key = (a.Anim1Start, a.Anim1End, a.Anim2Start, a.Anim2End, a.Anim2WeightAtMinSpeed, a.DurationMs);
            if (seen.Add(key))
                clips.Add(new MshClipSequence([ToClip(i, a)]));
        }

        return clips;
    }

    private static List<MshClipSequence> BuildChains(CtlFile ctl, bool joinThroughEndpoints)
    {
        return CtlStateChains.Build(ctl, joinThroughEndpoints)
            .Select(chain => new MshClipSequence(
                chain.StateIndices.Select(i => ToClip(i, ctl.StateRecords[i].Animation)).ToList()))
            .ToList();
    }

    private static MshClip ToClip(int stateIndex, CtlStateAnimation a)
    {
        return new MshClip(
            stateIndex,
            a.Anim1Start,
            a.Anim1End,
            a.Anim2Start,
            a.Anim2End,
            a.Anim2WeightAtMinSpeed,
            a.DurationMs,
            MaxComponent(a.MinTangentialSpeed),
            MaxComponent(a.MaxTangentialSpeed),
            a.Flags);
    }

    private static float MaxComponent((float X, float Y, float Z) v)
    {
        return MathF.Max(MathF.Abs(v.X), MathF.Max(MathF.Abs(v.Y), MathF.Abs(v.Z)));
    }

    /// <summary>
    /// Ищет CTL с тем же именем, что у меша (<c>R_H_02.msh</c> или <c>25_MESH_R_H_02.msh</c> -> <c>*_CTLD_r_h_02.ctl</c>):
    /// в папке меша, её родителях (до трёх уровней) и во вложенных папках первого уровня.
    /// </summary>
    public static string? FindCtlForMsh(string mshPath)
    {
        var name = ExtractModelName(mshPath);
        if (name.Length == 0)
            return null;

        var dir = Path.GetDirectoryName(Path.GetFullPath(mshPath));
        for (var level = 0; level < 4 && dir != null; level++, dir = Path.GetDirectoryName(dir))
        {
            var found = SearchDirectory(dir, name);
            if (found != null)
                return found;

            IEnumerable<string> subDirs;
            try
            {
                subDirs = Directory.EnumerateDirectories(dir);
            }
            catch (Exception)
            {
                continue;
            }

            foreach (var sub in subDirs)
            {
                found = SearchDirectory(sub, name);
                if (found != null)
                    return found;
            }
        }

        return null;
    }

    private static string? SearchDirectory(string dir, string name)
    {
        try
        {
            foreach (var file in Directory.EnumerateFiles(dir, "*.ctl"))
            {
                var stem = Path.GetFileNameWithoutExtension(file);
                var ctlName = CtlPrefixRegex().Replace(stem, "");
                if (string.Equals(ctlName, name, StringComparison.OrdinalIgnoreCase))
                    return file;
            }
        }
        catch (Exception)
        {
            // Папка недоступна - просто идём дальше.
        }

        return null;
    }

    private static string ExtractModelName(string mshPath)
    {
        var stem = Path.GetFileNameWithoutExtension(mshPath);
        return MeshPrefixRegex().Replace(stem, "");
    }

    [GeneratedRegex(@"^\d+_MESH_", RegexOptions.IgnoreCase)]
    private static partial Regex MeshPrefixRegex();

    [GeneratedRegex(@"^(\d+_)?CTLD_", RegexOptions.IgnoreCase)]
    private static partial Regex CtlPrefixRegex();
}
