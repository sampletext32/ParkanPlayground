using System.Numerics;
using MshLib;
using NResUI.Rendering.Inspection;
using NResUI.Rendering.Materials;
using NResUI.Rendering.Viewport;
using NResUI.Rendering.Viewport.Meshes;
using NResUI.Rendering.Viewport.Msh;
using Silk.NET.OpenGL;

namespace NResUI.Rendering.Import.Msh;

public static class MshViewportMeshFactory
{
    /// <summary>
    /// Строит OpenGL-меши из уже разобранного MeshDocument.
    /// State/LOD выбираются отдельно для каждого piece через 0x01, а не через общий список geometry slots.
    /// </summary>
    public static IReadOnlyList<ViewportPiece> BuildViewportPieces(
        GL gl,
        MeshDocument document,
        MeshRenderState renderState,
        ViewportMaterialSet materialSet)
    {
        if (document.MshModelGeometry == null)
            return [];

        var result = new List<ViewportPiece>();
        var modelGeometry = document.MshModelGeometry;
        var mshToViewportTransform = Matrix4x4.CreateRotationX(-MathF.PI * 0.5f);
        var poses = MshRestPoseBuilder.BuildPose(
            modelGeometry.Nodes,
            modelGeometry.TransformKeyframes,
            modelGeometry.AnimationMap,
            renderState.AnimationPoseMode,
            renderState.AnimationSampleTime);

        foreach (var pieceInfo in document.Pieces)
        {
            if (!renderState.IsPieceVisible(pieceInfo.Id))
                continue;

            var pieceTransform = poses[pieceInfo.Id].MeshSpaceTransform;
            var (pieceState, pieceLod) = ResolvePieceStateLod(document, pieceInfo, renderState);
            var slotIndex = pieceInfo.ResolveSlotIndex(pieceState, pieceLod);
            if (slotIndex == ushort.MaxValue || slotIndex >= modelGeometry.GeometrySlots.Slots.Count)
            {
                if (renderState.ShowEmptyPieces)
                    AddEmptyPieceMarker(gl, result, pieceInfo, pieceTransform, mshToViewportTransform, "No geometry slot for selected state/LOD");
                continue;
            }

            var slot = modelGeometry.GeometrySlots.Slots[slotIndex];
            if (!slot.HasBatches)
            {
                if (renderState.ShowEmptyPieces)
                    AddEmptyPieceMarker(gl, result, pieceInfo, pieceTransform, mshToViewportTransform, "Geometry slot has no batches", slotIndex);
                continue;
            }

            var meshBuildResult = BuildPieceMeshes(
                gl,
                pieceInfo.Id,
                slot,
                modelGeometry,
                renderState,
                materialSet);

            if (meshBuildResult == null)
            {
                if (renderState.ShowEmptyPieces)
                    AddEmptyPieceMarker(gl, result, pieceInfo, pieceTransform, mshToViewportTransform, "Geometry slot produced no valid triangles", slotIndex);
                continue;
            }

            result.Add(new ViewportPiece(
                id: pieceInfo.Id,
                name: pieceInfo.Name,
                meshes: meshBuildResult.Meshes,
                localTransform: pieceTransform * mshToViewportTransform,
                boundsMin: meshBuildResult.BoundsMin,
                boundsMax: meshBuildResult.BoundsMax,
                debugInfo: new ViewportPieceDebugInfo
                {
                    SourceKind = "MSH 0x01 piece",
                    SourcePieceIndex = pieceInfo.Id,
                    SourceParentIndex = pieceInfo.ParentId,
                    GeometrySlotIndex = slotIndex,
                    Msh01Flags = (uint)pieceInfo.Flags,
                    BatchCount = meshBuildResult.BatchCount,
                    TriangleCount = meshBuildResult.TriangleCount,
                    FallbackKeyframeIndex = pieceInfo.FallbackKeyframeIndex,
                    HasRestPose = pieceInfo.HasRestPose
                },
                sourceBatchIndices: meshBuildResult.BatchIndices));
        }

        return result;
    }

    private static (int State, int Lod) ResolvePieceStateLod(
        MeshDocument document,
        MeshPieceInfo pieceInfo,
        MeshRenderState renderState)
    {
        if (renderState.PieceStateOverrides.ContainsKey(pieceInfo.Id) ||
            renderState.PieceLodOverrides.ContainsKey(pieceInfo.Id))
        {
            return (renderState.GetPieceState(pieceInfo.Id), renderState.GetPieceLod(pieceInfo.Id));
        }

        // Без явного override piece использует первую реально присутствующую пару state/LOD из своей 0x01 таблицы.
        for (var state = 0; state < document.ModelStateCount; state++)
        {
            for (var lod = 0; lod < document.LodCount; lod++)
            {
                var slot = pieceInfo.ResolveSlotIndex(state, lod);
                if (slot != ushort.MaxValue &&
                    (document.MshModelGeometry == null || slot < document.MshModelGeometry.GeometrySlots.Slots.Count))
                {
                    return (state, lod);
                }
            }
        }

        return (0, 0);
    }

    private static void AddEmptyPieceMarker(
        GL gl,
        List<ViewportPiece> pieces,
        MeshPieceInfo pieceInfo,
        Matrix4x4 pieceTransform,
        Matrix4x4 mshToViewportTransform,
        string reason,
        int geometrySlotIndex = -1)
    {
        // Пустые pieces все равно важны: это могут быть сокеты, логические узлы или placeholders поврежденных состояний.
        var marker = PrimitiveMeshes.CreateAxes(gl, 0.35f);
        pieces.Add(new ViewportPiece(
            id: pieceInfo.Id,
            name: pieceInfo.Name,
            mesh: marker,
            localTransform: pieceTransform * mshToViewportTransform,
            boundsMin: new Vector3(-0.35f, -0.35f, -0.35f),
            boundsMax: new Vector3(0.35f, 0.35f, 0.35f),
            debugInfo: new ViewportPieceDebugInfo
            {
                SourceKind = reason,
                SourcePieceIndex = pieceInfo.Id,
                SourceParentIndex = pieceInfo.ParentId,
                GeometrySlotIndex = geometrySlotIndex,
                Msh01Flags = (uint)pieceInfo.Flags,
                BatchCount = 0,
                TriangleCount = 0,
                FallbackKeyframeIndex = pieceInfo.FallbackKeyframeIndex,
                HasRestPose = pieceInfo.HasRestPose
            }));
    }

    private static PieceMeshBuildResult? BuildPieceMeshes(
        GL gl,
        int pieceId,
        Msh0x02.GeometrySlot slot,
        MshModelGeometry modelGeometry,
        MeshRenderState renderState,
        ViewportMaterialSet materialSet)
    {
        var meshes = new List<GpuMesh>();
        var batchIndices = new List<int>();
        var debugColor = PickDebugColor(pieceId);
        var batchCount = 0;
        var triangleCount = 0;

        var boundsMin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        var boundsMax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        for (var batchIndex = slot.BatchStart0x0D; batchIndex < slot.BatchEndExclusive0x0D; batchIndex++)
        {
            if (batchIndex < 0 || batchIndex >= modelGeometry.Batches.Count || !renderState.IsBatchVisible(batchIndex))
                continue;

            var batch = modelGeometry.Batches[batchIndex];
            if (batch.IndexStart0x06 + batch.IndexCount0x06 > modelGeometry.Indices.Count)
                continue;

            var vertices = new List<float>();
            var outIndices = new List<uint>();
            var batchTriangleCount = 0;

            var material = materialSet.FindMaterial(batch.MaterialIndexLo) ?? ViewportMaterial.Untextured;
            var vertexColor = material.HasTexture ? Vector3.One : debugColor;

            for (var i = 0; i + 2 < batch.IndexCount0x06; i += 3)
            {
                var indexBase = (int)batch.IndexStart0x06 + i;
                var vertexIndex0 = checked((int)batch.BaseVertex0x03 + modelGeometry.Indices[indexBase + 0]);
                var vertexIndex1 = checked((int)batch.BaseVertex0x03 + modelGeometry.Indices[indexBase + 1]);
                var vertexIndex2 = checked((int)batch.BaseVertex0x03 + modelGeometry.Indices[indexBase + 2]);

                if (!IsValidTriangle(modelGeometry.Positions.Count, vertexIndex0, vertexIndex1, vertexIndex2))
                    continue;

                var p0 = ToNumericsVector3(modelGeometry.Positions[vertexIndex0]);
                var p1 = ToNumericsVector3(modelGeometry.Positions[vertexIndex1]);
                var p2 = ToNumericsVector3(modelGeometry.Positions[vertexIndex2]);

                var fallbackNormal = BuildFaceNormal(p0, p1, p2);

                AddTriangleVertex(p0, vertexColor, GetNormal(modelGeometry.Normals, vertexIndex0, fallbackNormal, renderState.UseStoredNormals), GetUv(modelGeometry.Uvs, vertexIndex0), vertices, outIndices, ref boundsMin, ref boundsMax);
                AddTriangleVertex(p1, vertexColor, GetNormal(modelGeometry.Normals, vertexIndex1, fallbackNormal, renderState.UseStoredNormals), GetUv(modelGeometry.Uvs, vertexIndex1), vertices, outIndices, ref boundsMin, ref boundsMax);
                AddTriangleVertex(p2, vertexColor, GetNormal(modelGeometry.Normals, vertexIndex2, fallbackNormal, renderState.UseStoredNormals), GetUv(modelGeometry.Uvs, vertexIndex2), vertices, outIndices, ref boundsMin, ref boundsMax);
                batchTriangleCount++;
            }

            if (batchTriangleCount == 0)
                continue;

            batchCount++;
            batchIndices.Add(batchIndex);
            triangleCount += batchTriangleCount;
            meshes.Add(PrimitiveMeshes.CreateColoredIndexedMesh(gl, vertices, outIndices, PrimitiveType.Triangles, material));
        }

        if (triangleCount == 0 || meshes.Count == 0)
            return null;

        return new PieceMeshBuildResult(meshes, boundsMin, boundsMax, batchCount, triangleCount, batchIndices);
    }

    private static Vector3 BuildFaceNormal(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        var normal = Vector3.Cross(p1 - p0, p2 - p0);
        return normal.LengthSquared() < 1e-8f ? Vector3.UnitY : Vector3.Normalize(normal);
    }

    private static Vector3 GetNormal(
        IReadOnlyList<Msh04Normal> normals,
        int vertexIndex,
        Vector3 fallbackNormal,
        bool useStoredNormals)
    {
        if (!useStoredNormals || vertexIndex < 0 || vertexIndex >= normals.Count)
            return fallbackNormal;

        var packed = normals[vertexIndex];
        var normal = new Vector3(
            Math.Clamp(packed.X / 127.0f, -1.0f, 1.0f),
            Math.Clamp(packed.Y / 127.0f, -1.0f, 1.0f),
            Math.Clamp(packed.Z / 127.0f, -1.0f, 1.0f));

        // У битых/нулевых normal entries оставляем face normal, чтобы освещение не превращалось в NaN.
        return normal.LengthSquared() < 1e-8f ? fallbackNormal : Vector3.Normalize(normal);
    }

    private static Vector2 GetUv(IReadOnlyList<Msh05Uv> uvs, int vertexIndex)
    {
        if (vertexIndex < 0 || vertexIndex >= uvs.Count)
            return Vector2.Zero;

        var uv = uvs[vertexIndex];
        return new Vector2(uv.U / 1024.0f, uv.V / 1024.0f);
    }

    private static Vector3 ToNumericsVector3(Common.Vector3 position)
    {
        return new Vector3(position.X, position.Y, position.Z);
    }

    private static void AddTriangleVertex(
        Vector3 position,
        Vector3 color,
        Vector3 normal,
        Vector2 uv,
        List<float> vertices,
        List<uint> indices,
        ref Vector3 boundsMin,
        ref Vector3 boundsMax)
    {
        var vertexIndex = (uint)(vertices.Count / PrimitiveMeshes.FloatsPerVertex);

        vertices.Add(position.X);
        vertices.Add(position.Y);
        vertices.Add(position.Z);
        vertices.Add(color.X);
        vertices.Add(color.Y);
        vertices.Add(color.Z);
        vertices.Add(normal.X);
        vertices.Add(normal.Y);
        vertices.Add(normal.Z);
        vertices.Add(uv.X);
        vertices.Add(uv.Y);

        indices.Add(vertexIndex);
        boundsMin = Vector3.Min(boundsMin, position);
        boundsMax = Vector3.Max(boundsMax, position);
    }

    private static bool IsValidTriangle(int vertexCount, int vertexIndex0, int vertexIndex1, int vertexIndex2)
    {
        return IsValidVertexIndex(vertexCount, vertexIndex0) &&
               IsValidVertexIndex(vertexCount, vertexIndex1) &&
               IsValidVertexIndex(vertexCount, vertexIndex2);
    }

    private static bool IsValidVertexIndex(int vertexCount, int vertexIndex)
    {
        return vertexIndex >= 0 && vertexIndex < vertexCount;
    }

    private static Vector3 PickDebugColor(int index)
    {
        ReadOnlySpan<Vector3> palette =
        [
            new Vector3(0.90f, 0.36f, 0.30f),
            new Vector3(0.30f, 0.68f, 0.95f),
            new Vector3(0.42f, 0.82f, 0.42f),
            new Vector3(0.95f, 0.78f, 0.32f),
            new Vector3(0.78f, 0.48f, 0.95f),
            new Vector3(0.36f, 0.86f, 0.78f),
            new Vector3(0.95f, 0.55f, 0.78f),
            new Vector3(0.72f, 0.72f, 0.72f),
        ];

        return palette[index % palette.Length];
    }

    private sealed record PieceMeshBuildResult(
        IReadOnlyList<GpuMesh> Meshes,
        Vector3 BoundsMin,
        Vector3 BoundsMax,
        int BatchCount,
        int TriangleCount,
        IReadOnlyList<int> BatchIndices);
}
