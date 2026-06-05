using System.Numerics;

namespace NResUI.Rendering.Viewport;

public static class ViewportSelection
{
    public static int PickPiece(
        ViewportScene scene,
        ViewportCamera camera,
        Vector2 localMouse,
        Vector2 viewportSize)
    {
        if (viewportSize.X <= 1.0f || viewportSize.Y <= 1.0f)
            return -1;

        var aspect = viewportSize.X / viewportSize.Y;
        var fovRadians = ViewportCamera.ToRadians(60.0f);
        var tanHalfFov = MathF.Tan(fovRadians * 0.5f);

        var ndcX = (2.0f * localMouse.X / viewportSize.X) - 1.0f;
        var ndcY = 1.0f - (2.0f * localMouse.Y / viewportSize.Y);

        var rayOriginWorld = new Vector3(0.0f, 0.0f, camera.Distance);
        var rayDirectionWorld = Vector3.Normalize(new Vector3(
            ndcX * aspect * tanHalfFov,
            ndcY * tanHalfFov,
            -1.0f));

        var sceneRotation = camera.GetSceneRotationMatrix();

        var bestPieceId = -1;
        var bestDistance = float.PositiveInfinity;

        foreach (var piece in scene.Pieces)
        {
            var model = piece.LocalTransform * sceneRotation;

            if (!Matrix4x4.Invert(model, out var inverseModel))
                continue;

            var rayOriginLocal = Vector3.Transform(rayOriginWorld, inverseModel);
            var rayDirectionLocal = Vector3.Normalize(
                Vector3.TransformNormal(rayDirectionWorld, inverseModel));

            if (IntersectRayAabb(
                    rayOriginLocal,
                    rayDirectionLocal,
                    piece.BoundsMin,
                    piece.BoundsMax,
                    out var hitDistance) &&
                hitDistance < bestDistance)
            {
                bestDistance = hitDistance;
                bestPieceId = piece.Id;
            }
        }

        return bestPieceId;
    }

    private static bool IntersectRayAabb(
        Vector3 rayOrigin,
        Vector3 rayDirection,
        Vector3 boundsMin,
        Vector3 boundsMax,
        out float hitDistance)
    {
        hitDistance = 0.0f;

        var tMin = 0.0f;
        var tMax = float.PositiveInfinity;

        if (!IntersectSlab(rayOrigin.X, rayDirection.X, boundsMin.X, boundsMax.X, ref tMin, ref tMax) ||
            !IntersectSlab(rayOrigin.Y, rayDirection.Y, boundsMin.Y, boundsMax.Y, ref tMin, ref tMax) ||
            !IntersectSlab(rayOrigin.Z, rayDirection.Z, boundsMin.Z, boundsMax.Z, ref tMin, ref tMax))
        {
            return false;
        }

        hitDistance = tMin;
        return true;
    }

    private static bool IntersectSlab(
        float origin,
        float direction,
        float min,
        float max,
        ref float tMin,
        ref float tMax)
    {
        const float epsilon = 1e-6f;

        if (MathF.Abs(direction) < epsilon)
            return origin >= min && origin <= max;

        var invDirection = 1.0f / direction;
        var t1 = (min - origin) * invDirection;
        var t2 = (max - origin) * invDirection;

        if (t1 > t2)
            (t1, t2) = (t2, t1);

        tMin = MathF.Max(tMin, t1);
        tMax = MathF.Min(tMax, t2);

        return tMin <= tMax;
    }
}
