using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Common;

/// <summary>DEBUG ONLY. Одни и те же 4 байта, интерпретированные как int32 и float32.</summary>
/// <param name="AsInt">Значение как int32.</param>
/// <param name="AsFloat">Значение как float32.</param>
[DebuggerDisplay("AsInt = {AsInt}, AsFloat = {AsFloat}")]
public record class IntFloatValue(int AsInt, float AsFloat)
{
    public IntFloatValue(Span<byte> span)
        : this(MemoryMarshal.Read<int>(span), MemoryMarshal.Read<float>(span))
    {
    }
}
