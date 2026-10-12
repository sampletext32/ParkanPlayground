using Common;
using System.Text;

namespace SkeLib;

/// <summary>
/// Time of day record (32 bytes, read by <c>FUN_10052550</c> in Terrain.dll). The first three dwords are zero in all files, then hour,
/// minute and a dword that is zero everywhere; the last 8 bytes are a flag/in-memory pointer left over from the original dump
/// (1 + garbage pointer in the zone times, zeros in the entries) and carry no information.
/// </summary>
public readonly record struct SkeTime(int Hour, int Minute)
{
    public const int Size = 32;
    /// <summary>Seconds since 00:00; the engine sorts entries by <c>(hour * 60 + minute) * 60</c>.</summary>
    public int Seconds => (Hour * 60 + Minute) * 60;
}

/// <summary>
/// Entry type (<c>CAtmData::GetEvents</c>, Terrain.dll): types 0..9 map to create/delete events of atmosphere objects handled by
/// <c>CAtmosphere::HandleEvents</c> (<c>CreateAtmosphereObject</c> types: 0 Sun, 1 Sky, 2 Rain, 3 Snow, 4 Lightning). Type 7 raises no event
/// (plain sky color key, 529 of 1466 entries in the files), type 2 does not occur.
/// </summary>
public enum SkeEntryType
{
    /// <summary>Creates the Sun object; <see cref="SkeEntry.Name"/> = "sun" or "moon" selects the body (sun gets different size/limits than moon).</summary>
    SunStart = 0,
    /// <summary>Deletes the Sun object.</summary>
    SunEnd = 1,
    /// <summary>Plain color key, no event (never produced by the engine as an event).</summary>
    SkyKey = 7,
    /// <summary>Creates Rain; <see cref="SkeEntry.Names"/>[0] is the background sound ("atm_rain1.wav"), mandatory ("Rain background sound not specified").</summary>
    RainStart = 3,
    RainEnd = 4,
    /// <summary>Creates Snow.</summary>
    SnowStart = 5,
    SnowEnd = 6,
    /// <summary>Creates Lightning; <see cref="SkeEntry.Names"/>[0] is the FXID name in effects.rlb ("env_lightning"), mandatory ("Lightning effect not specified").</summary>
    LightningStart = 8,
    LightningEnd = 9,
}

/// <summary>
/// One key frame of the sky (entry of a zone, 0xF0 bytes in memory, <c>FUN_1003d080</c>). Colors are 0xAARRGGBB dwords. Between two neighbouring
/// entries every value is interpolated linearly (per channel) by <c>CAtmData::method6</c> and handed to the object selected by the entry type
/// (the field list says which object reads what). Field names follow the in-memory offsets.
/// Objects as <c>method6</c> fills them: Sun gets 7 values, Sky 17, Rain 2, Snow 2, Lightning 1.
/// </summary>
public sealed record SkeEntry(
    SkeTime Time,
    SkeEntryType Type,        // +0x28: see SkeEntryType (verified in CAtmData::GetEvents); the colors are present in every entry
    int Reserved2C,           // +0x2C: looks like a leftover pointer/garbage, varies between files
    int[] Colors30,           // +0x30..0x3C: Sky: horizon color for each of the 4 compass sectors (Sky slots 0..3 in the order [1],[2],[0],[3]); the renderer blends two neighbours by the camera heading (90 degrees per sector) and uses the result for the lowest row of the dome
    float FadeInner,            // +0x40: Sky slot 13 -> ShaderConfig.fadeInnerFactor; the engine multiplies it by 700 (0.0 in all files)
    float FadeOuter,            // +0x44: Sky slot 14 -> ShaderConfig.fadeOuterFactor, also times 700 (0.6..1.0): distance fade of everything drawn by the shader
    int[] Fields48,           // +0x48..0x80, file order 48,4C,50,54,58,5C,60,64,68,70,6C,74,78,7C,80 (array index = file order). Colors:
                              // [0..3] -> Sky slots 4..7 (middle ring, order [0],[3],[1],[2]); [4..7] -> slots 8..11 (upper ring, same order);
                              // [8] (+0x68): Sky slot 12, colour of the apex and the first ring of the dome;
                              // [9] (+0x70): not read by method6 (always 0xFF000000);
                              // [10] (+0x6C): ARGB, Sun slot 5: flare color; its alpha is the glare strength (the sun light is boosted up to 5x when the camera looks at the sun); [11] (+0x74): Sky slot 15, RGB color (diffuse) of the cloud layer, the third pass of the dome, drawn only when the AtmCloudsOn setting is on;
                              // [12] (+0x78): color of the main Sun light (RGB, multiplied by ParamsCC[2]), of Rain (min 0x50 per channel, alpha 0x96) and of Snow;
                              // [13] (+0x7C): Sky slot 16 -> ShaderConfig.minDiffuseClamp (RGB, the minimum diffuse light color of the shader = ambient floor); [14] (+0x80): Sun slot 6: RGB of the second directional light (opposite to the sun, no specular), alpha unused.
    string Name,              // +0x84: "sun" or another name (moon); first of six strings, the other five are empty. "sun": disc material 3, orbit 90/30 degrees; otherwise material 4, orbit 0/50 degrees (CAtmData::GetEvents)
    IReadOnlyList<string> OtherStrings, // +0x90, 0x9C, 0xA8, 0xB4, 0xC0
    float[] ParamsCC,         // +0xCC, 0xD0, 0xD4, 0xD8: four floats. Sun: [0] and [1] are the width and height scale of the sun disc billboard, [2] is the light intensity (multiplies Fields48[12]);
                              // [3] is the single float of Rain, Snow and Lightning (0 = nothing .. 1; in the Lightning entries it is 1.0)
    IReadOnlyList<string> Names); // +0xE8 array: 0 or 4 strings; for Type 8 Names[0] = FXID name ("env_lightning"), for Type 3 Names[0] = wav ("atm_rain1.wav")

/// <summary>
/// Zone: a cycle of the atmosphere (element of 0x60 bytes in memory). <paramref name="ClockSpan"/> is the span of the game clock the entries live in
/// (23:59 in all files), <paramref name="RealDuration"/> is how long the cycle lasts in real time (hh:mm, 0:09 .. 24:00 in the files; read by the
/// container as the zone length <c>(hour * 60 + minute) * 60</c> seconds). The engine places an entry at
/// <c>RealDuration.Seconds * entry.Time.Seconds / 86400</c> seconds from the beginning of the zone (see <c>CAtmData::unk4</c>).
/// </summary>
public sealed record SkeZone(SkeTime ClockSpan, SkeTime RealDuration, IReadOnlyList<SkeEntry> Entries);

/// <summary>
/// sky.ske: atmosphere description of a mission (sun, sky, rain, snow, lightning), read by <c>CAtmData::ctor</c> / <c>FUN_1003dba0</c> (Terrain.dll),
/// message "SunDll panic: Old version ske file". All 29 files of the first game have version 5 and parse to the last byte.
/// Layout: int -1, int version (5), int zoneCount, per zone: int tag (1 = present), int entryCount, time start, time end, entries;
/// then <c>StartTime</c> (probably the game clock time at mission start, e.g. 9:45; not verified), <c>Reserved20</c> (always 0) and <c>Flag24</c> (0/1; the container reports it as a bool and CAtmosphere passes it to the Sky object when it creates it). Versions 2..4 (older entries, no trailer in v2) are not supported.
/// </summary>
public sealed record SkeFile(IReadOnlyList<SkeZone> Zones, SkeTime StartTime, int Reserved20, int Flag24)
{
    public static SkeFile Read(ReadOnlySpan<byte> data)
    {
        using var stream = new MemoryStream(data.ToArray(), writable: false);
        var r = new Reader(stream);
        if (r.Int() != -1) throw new InvalidDataException("sky.ske: expected -1 at the beginning");
        int version = r.Int();
        if (version != 5) throw new InvalidDataException($"sky.ske: version {version} is not supported");

        int zoneCount = r.Int();
        var zones = new List<SkeZone>(zoneCount);
        for (int z = 0; z < zoneCount; z++)
        {
            int tag = r.Int();
            if (tag != 1) { zones.Add(new SkeZone(default, default, [])); continue; }
            int entryCount = r.Int();
            var start = r.Time();
            var end = r.Time();
            var entries = new List<SkeEntry>(entryCount);
            for (int i = 0; i < entryCount; i++) entries.Add(ReadEntry(r));
            zones.Add(new SkeZone(start, end, entries));
        }

        var tail = r.Time();
        int i20 = r.Int();
        int i24 = r.Int();
        if (!r.AtEnd) throw new InvalidDataException($"sky.ske: {r.Remaining} bytes left after the last field");
        return new SkeFile(zones, tail, i20, i24);
    }

    private static SkeEntry ReadEntry(Reader r)
    {
        int entryVersion = r.Int();
        if (entryVersion != 3) throw new InvalidDataException($"sky.ske: entry version {entryVersion} is not supported");
        var time = r.Time();
        var type = (SkeEntryType)r.Int();
        int reserved = r.Int();
        var colors30 = new[] { r.Int(), r.Int(), r.Int(), r.Int() };
        float field40 = r.Float();
        float value44 = r.Float();
        var fields48 = new int[15];
        for (int i = 0; i < fields48.Length; i++) fields48[i] = r.Int();
        string name = r.String();
        var other = new List<string>(5);
        for (int i = 0; i < 5; i++) other.Add(r.String());
        var paramsCc = new[] { r.Float(), r.Float(), r.Float(), r.Float() };
        int nameCount = r.Int();
        var names = new List<string>(nameCount);
        for (int i = 0; i < nameCount; i++) names.Add(r.String());
        return new SkeEntry(time, type, reserved, colors30, field40, value44, fields48, name, other, paramsCc, names);
    }

    /// <summary>Чтение полей sky.ske из потока через <c>Common</c>; ошибки конца файла оформляются как <see cref="InvalidDataException"/>.</summary>
    private sealed class Reader(Stream stream)
    {
        public bool AtEnd => Remaining == 0;
        public int Remaining => (int)(stream.Length - stream.Position);

        public int Int()
        {
            if (Remaining < 4) throw new InvalidDataException("sky.ske: unexpected end of file");
            return stream.ReadInt32LittleEndian();
        }

        public float Float()
        {
            if (Remaining < 4) throw new InvalidDataException("sky.ske: unexpected end of file");
            return stream.ReadFloatLittleEndian();
        }

        /// <summary>Time record: 3 zero dwords, hour, minute, zero dword, 8 bytes of leftovers.</summary>
        public SkeTime Time()
        {
            if (Remaining < SkeTime.Size) throw new InvalidDataException("sky.ske: unexpected end of file");
            stream.Seek(12, SeekOrigin.Current);
            int hour = stream.ReadInt32LittleEndian();
            int minute = stream.ReadInt32LittleEndian();
            stream.Seek(SkeTime.Size - 20, SeekOrigin.Current);
            return new SkeTime(hour, minute);
        }

        /// <summary>String with a dword length prefix (no terminator).</summary>
        public string String()
        {
            int n = Int();
            if (n <= 0) return string.Empty;
            if (Remaining < n) throw new InvalidDataException("sky.ske: string exceeds file size");
            return Encoding.ASCII.GetString(stream.ReadBytesExact(n));
        }
    }
}
