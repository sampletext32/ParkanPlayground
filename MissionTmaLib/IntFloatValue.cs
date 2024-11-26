using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MissionTmaLib;

[DebuggerDisplay("AsInt = {AsInt}, AsFloat = {AsFloat}")]
public class IntFloatValue(Span<byte> span)
{
    public int AsInt { get; set; } = MemoryMarshal.Read<int>(span);
    public float AsFloat { get; set; } = MemoryMarshal.Read<float>(span);
}