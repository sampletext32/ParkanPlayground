using System.Numerics;
using MshLib;

namespace NResUI.Rendering.Inspection;

/// <summary>
/// CPU-документ меша: общая модель данных для viewport и inspector до загрузки в OpenGL.
/// Здесь хранятся ссылки на исходные MSH-компоненты и уже удобные для UI metadata.
/// </summary>
public sealed class MeshDocument
{
    /// <summary>Путь к MSH/NRes ресурсу, из которого построен документ.</summary>
    public required string SourcePath { get; init; }

    /// <summary>Тип MSH по набору компонентов. Сейчас inspector поддерживает только обычный Model.</summary>
    public required MshType MeshType { get; init; }

    /// <summary>Список pieces из 0x01. Каждый piece сам выбирает slot для state/LOD.</summary>
    public IReadOnlyList<MeshPieceInfo> Pieces { get; init; } = [];

    /// <summary>Материалы, пришедшие из WEA. Текстуры резолвятся отдельно, потому что это уже UI/OpenGL слой.</summary>
    public IReadOnlyList<MeshMaterialInfo> Materials { get; init; } = [];

    /// <summary>Нефатальные проблемы загрузки: отсутствующие optional-компоненты, WEA/material/texture misses.</summary>
    public IReadOnlyList<string> Warnings { get; init; } = [];

    /// <summary>Количество известных model states в 0x01 таблице.</summary>
    public int ModelStateCount { get; init; } = Msh0x01.StateCount;

    /// <summary>Количество LOD-уровней на каждый state в 0x01 таблице.</summary>
    public int LodCount { get; init; } = Msh0x01.MaxLodCount;

    /// <summary>Сырые MSH-потоки, нужные для перестройки GPU-мешей при смене state/LOD.</summary>
    public MshModelGeometry? MshModelGeometry { get; init; }
}

/// <summary>
/// Информация об одном piece/node из 0x01.
/// Важно: state/LOD не глобальны, поэтому этот объект хранит собственную slot-таблицу.
/// </summary>
public sealed class MeshPieceInfo
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required int ParentId { get; init; }
    public required NodeFlags Flags { get; init; }
    public required IReadOnlyList<ushort> GeometrySlotsByStateAndLod { get; init; }
    public required IReadOnlyList<MeshBatchInfo> Batches { get; init; }
    public required Matrix4x4 LocalTransform { get; init; }
    public required Matrix4x4 MeshSpaceTransform { get; init; }
    public required Vector3 BoundsMin { get; init; }
    public required Vector3 BoundsMax { get; init; }
    public int FallbackKeyframeIndex { get; init; } = -1;
    public bool HasRestPose { get; init; }

    /// <summary>
    /// Возвращает geometry slot 0x02 для пары state/LOD именно этого piece.
    /// 0xFFFF означает, что piece в такой комбинации не должен рендериться.
    /// </summary>
    public ushort ResolveSlotIndex(int state, int lod)
    {
        var index = state * Msh0x01.MaxLodCount + lod;
        return index >= 0 && index < GeometrySlotsByStateAndLod.Count
            ? GeometrySlotsByStateAndLod[index]
            : ushort.MaxValue;
    }
}

/// <summary>
/// Inspector metadata для batch 0x0D. Геометрия остается в MshModelGeometry, здесь только диапазоны и флаги.
/// </summary>
public sealed class MeshBatchInfo
{
    public required int BatchIndex { get; init; }
    public required int MaterialId { get; init; }
    public string? MaterialName { get; init; }
    public required BatchFlags Flags { get; init; }
    public required uint IndexStart { get; init; }
    public required int IndexCount { get; init; }
    public required uint BaseVertex { get; init; }
    public required int VertexCount { get; init; }
    public required int TriangleCount { get; init; }
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

/// <summary>Материал из WEA по id. Это еще не ViewportMaterial и не OpenGL texture.</summary>
public sealed class MeshMaterialInfo
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public string? TextureName { get; init; }
    public bool HasTexture => TextureName != null;
}
