using MshLib;

namespace NResUI.Rendering.Inspection;

/// <summary>
/// Parsed MSH component data retained so state/LOD changes can rebuild GPU meshes without reparsing the file.
/// </summary>
public sealed class MshModelGeometry
{
    /// <summary>Таблица узлов 0x01: именно она связывает каждый piece с geometry slot для пары state/LOD.</summary>
    public required Msh0x01.Msh0x01Component Nodes { get; init; }

    /// <summary>Слоты геометрии 0x02. Один piece выбирает один слот через 0x01.ResolveSlotIndex.</summary>
    public required Msh0x02.Msh0x02Component GeometrySlots { get; init; }

    /// <summary>Позиции вершин из 0x03.</summary>
    public required IReadOnlyList<Common.Vector3> Positions { get; init; }

    /// <summary>Нормали из 0x04. Компонент опционален, поэтому фабрика мешей обязана иметь fallback.</summary>
    public required IReadOnlyList<Msh04Normal> Normals { get; init; }

    /// <summary>UV из 0x05. Компонент опционален для части ресурсов.</summary>
    public required IReadOnlyList<Msh05Uv> Uvs { get; init; }

    /// <summary>Индексный буфер 0x06 для обычных моделей.</summary>
    public required IReadOnlyList<ushort> Indices { get; init; }

    /// <summary>Render batches 0x0D: диапазоны индексов, material id и render flags.</summary>
    public required IReadOnlyList<Msh0x0D.Batch> Batches { get; init; }

    /// <summary>Piece transform keyframes from MSH 0x08.</summary>
    public required IReadOnlyList<MshTransformKeyframe> TransformKeyframes { get; init; }

    /// <summary>Animation frame to 0x08 keyframe map from MSH 0x13. Optional in some files.</summary>
    public required IReadOnlyList<MshAnimationMapEntry> AnimationMap { get; init; }

    /// <summary>Animation frame count/window from MSH 0x13 NRes metadata.</summary>
    public required int MaxAnimationTime { get; init; }
}
