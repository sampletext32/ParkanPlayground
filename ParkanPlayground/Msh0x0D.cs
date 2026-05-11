using System.Buffers.Binary;
using NResLib;

namespace ParkanPlayground;

/// <summary>
/// MSH-компонент 0x0D: таблица render/intersection batches.
/// Используется через MSH_02_geometry_slot.batch_start_0x0d / batch_count_0x0d.
/// </summary>
public static class Msh0x0D
{
    public const int ElementSize = 20;

    public static List<Batch> ReadComponent(FileStream mshFs, NResArchive archive)
    {
        var entry = archive.Files.FirstOrDefault(x => x.FileType == "0D 00 00 00");

        if (entry is null)
        {
            throw new Exception("Archive doesn't contain file (0D)");
        }

        if (entry.ElementSize != ElementSize)
        {
            throw new Exception("Batch table component (0x0D) element size is not 20");
        }

        if (entry.FileLength % entry.ElementSize != 0)
        {
            throw new Exception("Batch table component (0x0D) payload size is not divisible by element size");
        }

        var data = new byte[entry.FileLength];

        mshFs.Seek(entry.OffsetInFile, SeekOrigin.Begin);
        mshFs.ReadExactly(data, 0, data.Length);

        return data
            .Chunk(ElementSize)
            .Select(x => new Batch(
                (BatchFlags)BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0x00)),
                BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0x02)),
                BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0x04)),
                BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0x06)),
                BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0x08)),
                BinaryPrimitives.ReadUInt32LittleEndian(x.AsSpan(0x0A)),
                BinaryPrimitives.ReadUInt16LittleEndian(x.AsSpan(0x0E)),
                BinaryPrimitives.ReadUInt32LittleEndian(x.AsSpan(0x10))))
            .ToList();
    }

    /// <summary>MSH batch 0x0D, sizeof 0x14.</summary>
    /// <param name="Flags">[0x00..0x02] Флаги batch.</param>
    /// <param name="MaterialIndex">[0x02..0x04] Индекс material slot. Может быть overridden CAniMesh::forced_material_index.</param>
    /// <param name="Opaque04">[0x04..0x06] Opaque. В GetBatchRenderData попадает в packed field вместе с material/lightmap данными.</param>
    /// <param name="LocalBatchIndex">[0x06..0x08] Локальный batch index. В GetBatchRenderData складывается с MSH_piece.local_index_base / batch base.</param>
    /// <param name="IndexCount">[0x08..0x0A] Количество индексов из component 0x06.</param>
    /// <param name="IndexStart">[0x0A..0x0E] Первый индекс в component 0x06.</param>
    /// <param name="VertexCount">[0x0E..0x10] Количество вершин для render primitive.</param>
    /// <param name="BaseVertex">[0x10..0x14] Base vertex в vertex streams, включая position stream 0x03.</param>
    public readonly record struct Batch(
        BatchFlags Flags,
        ushort MaterialIndex,
        ushort Opaque04,
        ushort LocalBatchIndex,
        ushort IndexCount,
        uint IndexStart,
        ushort VertexCount,
        uint BaseVertex);
}

[Flags]
public enum BatchFlags : ushort
{
    None = 0,

    /// <summary>
    /// Special/immediate submit path in CShade::SubmitMeshPieceBatches.
    /// </summary>
    SpecialSubmit = 0x0001,

    /// <summary>
    /// Intersection path tries both facing directions / disables facing test.
    /// Also likely related to two-sided handling.
    /// </summary>
    DisableFacingTest = 0x0002,

    /// <summary>
    /// Use material phase path instead of normal material animation frame.
    /// </summary>
    UseMaterialPhase = 0x0004,

    /// <summary>
    /// Special pass / conditional batch logic.
    /// In GetBatchRenderData this participates in conditional suppression logic.
    /// </summary>
    SpecialPassOrConditional = 0x0008,

    /// <summary>
    /// Batch is suppressed when paired with SpecialPassOrConditional.
    /// </summary>
    SuppressBatch = 0x0020,

    /// <summary>
    /// Forces second/translucent-ish render pass. Exact render-state meaning still provisional.
    /// </summary>
    ForcePass2 = 0x0100,

    /// <summary>
    /// Enables point-light emulation path.
    /// </summary>
    EmulatePointLights = 0x0800,

    /// <summary>
    /// Batch has lightmap / secondary texcoord related data.
    /// </summary>
    HasLightmapOrTexcoord1 = 0x2000,

    /// <summary>
    /// Facing/culling mode bit. Exact render meaning is still provisional.
    /// </summary>
    FacingTestEarlyOut = 0x4000,
}