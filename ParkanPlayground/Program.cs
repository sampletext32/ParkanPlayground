using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ParkanPlayground.Effects;
using static ParkanPlayground.Effects.FxidReader;

Console.OutputEncoding = Encoding.UTF8;

if (args.Length == 0)
{
    Console.WriteLine("Usage: ParkanPlayground <effects-directory-or-fxid-file>");
    return;
}

var path = args[0];
bool anyError = false;
var sizeByType = new Dictionary<byte, int>
{
    [1] = 0xE0, // 1: Billboard
    [2] = 0x94, // 2: Sound
    [3] = 0xC8, // 3: AnimParticle
    [4] = 0xCC, // 4: AnimBillboard
    [5] = 0x70, // 5: Trail
    [6] = 0x04, // 6: Point
    [7] = 0xD0, // 7: Plane
    [8] = 0xF8, // 8: Model
    [9] = 0xD0, // 9: AnimModel
    [10] = 0xD0, // 10: Cube
};

// Check for --dump-headers flag
bool dumpHeaders = args.Length > 1 && args[1] == "--dump-headers";

if (Directory.Exists(path))
{
    var files = Directory.EnumerateFiles(path, "*.bin").ToList();
    
    if (dumpHeaders)
    {
        // Collect all headers for analysis
        var headers = new List<(string name, EffectHeader h)>();
        foreach (var file in files)
        {
            try
            {
                using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var br = new BinaryReader(fs, Encoding.ASCII, leaveOpen: false);
                if (fs.Length >= 60)
                {
                    headers.Add((Path.GetFileName(file), ReadEffectHeader(br)));
                }
            }
            catch { }
        }
        
        // Analyze unique values
        Console.WriteLine("=== UNIQUE VALUES ANALYSIS ===\n");
        
        var uniqueUnk1 = headers.Select(x => x.h.Unknown1).Distinct().OrderBy(x => x).ToList();
        Console.WriteLine($"Unknown1 unique values ({uniqueUnk1.Count}): {string.Join(", ", uniqueUnk1)}");
        
        var uniqueUnk2 = headers.Select(x => x.h.Unknown2).Distinct().OrderBy(x => x).ToList();
        Console.WriteLine($"Unknown2 unique values ({uniqueUnk2.Count}): {string.Join(", ", uniqueUnk2.Select(x => x.ToString("F2")))}");
        
        var uniqueFlags = headers.Select(x => x.h.Flags).Distinct().OrderBy(x => x).ToList();
        Console.WriteLine($"Flags unique values ({uniqueFlags.Count}): {string.Join(", ", uniqueFlags.Select(x => $"0x{x:X4}"))}");
        
        var uniqueUnk3 = headers.Select(x => x.h.Unknown3).Distinct().OrderBy(x => x).ToList();
        Console.WriteLine($"Unknown3 unique values ({uniqueUnk3.Count}): {string.Join(", ", uniqueUnk3)}");
        Console.WriteLine($"Unknown3 as hex: {string.Join(", ", uniqueUnk3.Select(x => $"0x{x:X3}"))}");
        Console.WriteLine($"Unknown3 decoded (hi.lo): {string.Join(", ", uniqueUnk3.Select(x => $"{x >> 8}.{x & 0xFF}"))}");
        
        // Check reserved bytes
        var nonZeroReserved = headers.Where(x => x.h.Reserved.Any(b => b != 0)).ToList();
        Console.WriteLine($"\nFiles with non-zero Reserved bytes: {nonZeroReserved.Count} / {headers.Count}");
        
        // Check scales
        var uniqueScales = headers.Select(x => (x.h.ScaleX, x.h.ScaleY, x.h.ScaleZ)).Distinct().ToList();
        Console.WriteLine($"Unique scale combinations: {string.Join(", ", uniqueScales.Select(s => $"({s.ScaleX:F2},{s.ScaleY:F2},{s.ScaleZ:F2})"))}");
        
        Console.WriteLine("\n=== SAMPLE HEADERS (first 30) ===");
        Console.WriteLine($"{"File",-40} | {"Cnt",3} | {"U1",2} | {"Duration",8} | {"U2",6} | {"Flags",6} | {"U3",4} | Scale");
        Console.WriteLine(new string('-', 100));
        
        foreach (var (name, h) in headers.Take(30))
        {
            Console.WriteLine($"{name,-40} | {h.ComponentCount,3} | {h.Unknown1,2} | {h.Duration,8:F2} | {h.Unknown2,6:F2} | 0x{h.Flags:X4} | {h.Unknown3,4} | ({h.ScaleX:F1},{h.ScaleY:F1},{h.ScaleZ:F1})");
        }
    }
    else
    {
        foreach (var file in files)
        {
            if (!ValidateFxidFile(file))
            {
                anyError = true;
            }
        }

        Console.WriteLine(anyError
            ? "Validation finished with errors."
            : "All FXID files parsed successfully.");
    }
}
else if (File.Exists(path))
{
    anyError = !ValidateFxidFile(path);
    Console.WriteLine(anyError ? "Validation failed." : "Validation OK.");
}
else
{
    Console.WriteLine($"Path not found: {path}");
}

void DumpEffectHeader(string path)
{
    try
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs, Encoding.ASCII, leaveOpen: false);

        if (fs.Length < 60)
        {
            Console.WriteLine($"{Path.GetFileName(path)}: file too small");
            return;
        }

        var h = ReadEffectHeader(br);
        
        // Format reserved bytes as hex (show first 8 bytes for brevity)
        var reservedHex = BitConverter.ToString(h.Reserved, 0, Math.Min(8, h.Reserved.Length)).Replace("-", " ");
        if (h.Reserved.Length > 8) reservedHex += "...";
        
        // Check if reserved has any non-zero bytes
        bool reservedAllZero = h.Reserved.All(b => b == 0);
        
        Console.WriteLine($"{Path.GetFileName(path),-40} | {h.ComponentCount,7} | {h.Unknown1,4} | {h.Duration,8:F2} | {h.Unknown2,8:F2} | 0x{h.Flags:X4} | {h.Unknown3,4} | {(reservedAllZero ? "(all zero)" : reservedHex),-20} | ({h.ScaleX:F2}, {h.ScaleY:F2}, {h.ScaleZ:F2})");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{Path.GetFileName(path)}: {ex.Message}");
    }
}

bool ValidateFxidFile(string path)
{
    try
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var br = new BinaryReader(fs, Encoding.ASCII, leaveOpen: false);

        const int headerSize = 60; // sizeof(EffectHeader) on disk
        if (fs.Length < headerSize)
        {
            Console.WriteLine($"{path}: file too small ({fs.Length} bytes).");
            return false;
        }

        var header = ReadEffectHeader(br);

        var typeCounts = new Dictionary<byte, int>();

        for (int i = 0; i < header.ComponentCount; i++)
        {
            long blockStart = fs.Position;
            if (fs.Position + 4 > fs.Length)
            {
                Console.WriteLine($"{path}: component {i}: unexpected EOF before type (offset 0x{fs.Position:X}, size 0x{fs.Length:X}).");
                return false;
            }

            uint typeAndFlags = br.ReadUInt32();
            byte type = (byte)(typeAndFlags & 0xFF);

            if (!typeCounts.TryGetValue(type, out var count))
            {
                count = 0;
            }
            typeCounts[type] = count + 1;

            if (!sizeByType.TryGetValue(type, out int blockSize))
            {
                Console.WriteLine($"{path}: component {i}: unknown type {type} (typeAndFlags=0x{typeAndFlags:X8}).");
                return false;
            }

            int remaining = blockSize - 4;
            if (fs.Position + remaining > fs.Length)
            {
                Console.WriteLine($"{path}: component {i}: block size 0x{blockSize:X} runs past EOF (blockStart=0x{blockStart:X}, fileSize=0x{fs.Length:X}).");
                return false;
            }

            if (type == 1)
            {
                var def = ReadBillboardComponent(br, typeAndFlags);

                if (def.Reserved.Length != 0x50)
                {
                    Console.WriteLine($"{path}: component {i}: type 1 reserved length {def.Reserved.Length}, expected 0x50.");
                    return false;
                }
            }
            else if (type == 2)
            {
                var def = ReadSoundComponent(br, typeAndFlags);

                if (def.SoundNameAndReserved.Length != 0x40)
                {
                    Console.WriteLine($"{path}: component {i}: type 2 reserved length {def.SoundNameAndReserved.Length}, expected 0x40.");
                    return false;
                }
            }
            else if (type == 3)
            {
                var def = ReadAnimParticleComponent(br, typeAndFlags);

                if (def.Reserved.Length != 0x38)
                {
                    Console.WriteLine($"{path}: component {i}: type 3 reserved length {def.Reserved.Length}, expected 0x38.");
                    return false;
                }
            }
            else if (type == 4)
            {
                var def = ReadAnimBillboardComponent(br, typeAndFlags);

                if (def.Reserved.Length != 0x3C)
                {
                    Console.WriteLine($"{path}: component {i}: type 4 reserved length {def.Reserved.Length}, expected 0x3C.");
                    return false;
                }
            }
            else if (type == 5)
            {
                var def = ReadTrailComponent(br, typeAndFlags);

                if (def.Unknown04To10.Length != 0x10)
                {
                    Console.WriteLine($"{path}: component {i}: type 5 prefix length {def.Unknown04To10.Length}, expected 0x10.");
                    return false;
                }

                if (def.TextureNameAndReserved.Length != 0x40)
                {
                    Console.WriteLine($"{path}: component {i}: type 5 tail length {def.TextureNameAndReserved.Length}, expected 0x40.");
                    return false;
                }
            }
            else if (type == 6)
            {
                // Point components have no extra bytes beyond the 4-byte typeAndFlags header.
                var def = ReadPointComponent(typeAndFlags);
            }
            else if (type == 7)
            {
                var def = ReadPlaneComponent(br, typeAndFlags);

                if (def.Base.Reserved.Length != 0x38)
                {
                    Console.WriteLine($"{path}: component {i}: type 7 base reserved length {def.Base.Reserved.Length}, expected 0x38.");
                    return false;
                }
            }
            else if (type == 8)
            {
                var def = ReadModelComponent(br, typeAndFlags);

                if (def.TextureNameAndFlags.Length != 0x40)
                {
                    Console.WriteLine($"{path}: component {i}: type 8 tail length {def.TextureNameAndFlags.Length}, expected 0x40.");
                    return false;
                }
            }
            else if (type == 9)
            {
                var def = ReadAnimModelComponent(br, typeAndFlags);

                if (def.TextureNameAndFlags.Length != 0x48)
                {
                    Console.WriteLine($"{path}: component {i}: type 9 tail length {def.TextureNameAndFlags.Length}, expected 0x48.");
                    return false;
                }
            }
            else if (type == 10)
            {
                var def = ReadCubeComponent(br, typeAndFlags);

                if (def.Base.Reserved.Length != 0x3C)
                {
                    Console.WriteLine($"{path}: component {i}: type 10 base reserved length {def.Base.Reserved.Length}, expected 0x3C.");
                    return false;
                }
            }
            else
            {
                // Skip the remaining bytes for other component types.
                fs.Position += remaining;
            }
        }

        // Dump a compact per-file summary of component types and counts.
        var sb = new StringBuilder();
        bool first = true;
        foreach (var kv in typeCounts)
        {
            if (!first)
            {
                sb.Append(", ");
            }
            sb.Append(kv.Key);
            sb.Append('x');
            sb.Append(kv.Value);
            first = false;
        }
        Console.WriteLine($"{path}: components={header.ComponentCount}, types=[{sb}]");

        if (fs.Position != fs.Length)
        {
            Console.WriteLine($"{path}: parsed to 0x{fs.Position:X}, but file size is 0x{fs.Length:X} (leftover {fs.Length - fs.Position} bytes).");
            return false;
        }

        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"{path}: exception while parsing: {ex.Message}");
        return false;
    }
}
