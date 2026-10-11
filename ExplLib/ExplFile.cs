using System.Text;
using MaterialLib;

namespace ExplLib;

/// <summary>
/// Reference to an item of an NRes archive: NRes archive name (32 bytes) + item name (32 bytes).
/// In EXPL files the archive is always <c>effects.rlb</c> and the item is an FXID effect.
/// </summary>
public readonly record struct ExplResourceRef(string ArchiveName, string ItemName)
{
    public bool IsEmpty => string.IsNullOrEmpty(ArchiveName) && string.IsNullOrEmpty(ItemName);
}

/// <summary>
/// How the explosion effect is oriented (EXPL +0x14). From <c>CControlBase::CommitNdpDamageStateTransition</c> (Control.dll):
/// the direction is a column of a placement matrix, or the normal of the surface that was hit.
/// In the data only 0 (56 files) and 7 (48 files) occur.
/// </summary>
public enum ExplDirectionMode : uint
{
    /// <summary>Default (also 0): column 1 of the piece placement matrix.</summary>
    PieceColumn1 = 0,
    /// <summary>Column 0 of the piece placement matrix.</summary>
    PieceColumn0 = 1,
    /// <summary>Column 0 of the piece placement matrix (same as 1).</summary>
    PieceColumn0_Alt = 2,
    /// <summary>Column 2 of the piece placement matrix.</summary>
    PieceColumn2 = 3,
    /// <summary>Column 0 of the object placement matrix.</summary>
    ObjectColumn0 = 4,
    /// <summary>Column 1 of the object placement matrix.</summary>
    ObjectColumn1 = 5,
    /// <summary>Column 2 of the object placement matrix.</summary>
    ObjectColumn2 = 6,
    /// <summary>Normal of the surface that was hit (collision face normal).</summary>
    SurfaceNormal = 7,
}

/// <summary>
/// EXPL item (explosion description, 4CC "EXPL", extension .exp), 792 bytes in all 104 files of the first game:
/// 0x18 byte header + 12 resource slots of 0x40 bytes. VERIFIED from <c>CControlBase::CommitNdpDamageStateTransition</c>
/// and <c>ComputeNdpDamageEffectBoundsRadius</c> (Control.dll), which read this item when an NDP piece reaches a worse
/// damage state (the item is referenced from NDP records, BULL and CTL item records). The engine does not check the 4CC.
/// </summary>
/// <remarks>
/// Slot selection: the surface hit by the object gives a material id <c>m</c>. If 0 &lt;= m &lt;= 10, <c>Slots[m + 1]</c> is tried
/// (offset 0x58 + 0x40 * m), and if its effect cannot be loaded (empty name) or there is no hit, <c>Slots[0]</c> (offset 0x18) is used.
/// A hit without a triangle gives m = 10. 76 of 104 files have only slot 0. Effect name suffixes (m, st, w, wb/wl/wm/wt, al, an, s, *_metal)
/// follow the material ids (see <see cref="MaterialLib.SurfaceMaterial"/>; the id is byte 4 of the MAT0 item of the hit surface).
/// The unused part of the name buffers can contain uninitialized memory (garbage after the terminating zero).
/// </remarks>
public sealed record ExplFile(
    // Damage kind (inferred from the branching in CControlBase::method_173e0): 2 = damage to the hit object only,
    // 3 and 4 = area damage in a sphere. In the data: 2 (48 files), 3 (43), 1 (13); 4 does not occur.
    uint Field00,
    float LifeAmountOffset,
    // Effect scale. VERIFIED (CControlBase::CommitNdpDamageStateTransition, Control.dll 0x10015B50): for a bullet (LOGIC_BULLET_AGENT_BULL)
    // it is used as is (absolute number: 1.0, 3.0, 7.0, 60.0 ...), for other objects it is multiplied by the radius of the piece's bounding
    // sphere. All component sizes of the FXID are multiplied by it (SetEffectScale); it is also the radius of area damage.
    float ScaleMultiplier,
    float Field0C,
    float Field10,
    ExplDirectionMode DirectionMode,
    IReadOnlyList<ExplResourceRef?> Slots)
{
    public const int FileSize = 0x318;
    public const int HeaderSize = 0x18;
    public const int SlotSize = 0x40;
    public const int SlotCount = 12;

    /// <summary>Effect for a material id (0..10); falls back to slot 0 when the slot is empty.</summary>
    public ExplResourceRef? GetEffectForMaterial(int materialId)
    {
        if (materialId is >= 0 and <= 10 && Slots[materialId + 1] is { } r) return r;
        return Slots[0];
    }

    public static ExplFile Read(ReadOnlySpan<byte> data)
    {
        if (data.Length != FileSize)
            throw new InvalidDataException($"EXPL: expected {FileSize} bytes, got {data.Length}");

        var slots = new ExplResourceRef?[SlotCount];
        for (int i = 0; i < SlotCount; i++)
        {
            var slot = data.Slice(HeaderSize + i * SlotSize, SlotSize);
            var r = new ExplResourceRef(ReadZString(slot[..32]), ReadZString(slot.Slice(32, 32)));
            slots[i] = r.IsEmpty ? null : r;
        }

        return new ExplFile(
            BitConverter.ToUInt32(data[0x00..]),
            BitConverter.ToSingle(data[0x04..]),
            BitConverter.ToSingle(data[0x08..]),
            BitConverter.ToSingle(data[0x0C..]),
            BitConverter.ToSingle(data[0x10..]),
            (ExplDirectionMode)BitConverter.ToUInt32(data[0x14..]),
            slots);
    }

    private static string ReadZString(ReadOnlySpan<byte> bytes)
    {
        int n = bytes.IndexOf((byte)0);
        if (n < 0) n = bytes.Length;
        return Encoding.ASCII.GetString(bytes[..n]);
    }
}
