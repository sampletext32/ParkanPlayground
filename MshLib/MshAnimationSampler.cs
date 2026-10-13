using System.Numerics;

namespace MshLib;

public static class MshAnimationSampler
{
    /// <summary>
    /// Поза детали в момент <paramref name="sampleTime"/> по ключам 0x08 и карте кадров 0x13 (<c>MSH_piece::LerpTransform</c>, AniMesh.dll 0x100135F0).
    /// Кадр = <c>round(time - 0.5)</c> по правилу FPU (<c>FISTP</c>, к ближайшему чётному: 2.5 -> 2, 1.5 -> 2). Ключ по умолчанию (<c>FallbackKey</c>)
    /// берётся, если кадр выходит за блок детали (<paramref name="frameCount"/>), у детали нет карты или значение карты не меньше индекса ключа по умолчанию.
    /// </summary>
    /// <param name="frameCount">Длина блока карты на деталь (<c>Magic1</c> компонента 0x13, <see cref="Msh0x13Component.MaxAnimationTime"/>); 0 - не ограничивать.</param>
    public static MshSampledPieceTransform SamplePieceAnimationAtTime(
        Msh0x01.Node node,
        IReadOnlyList<MshTransformKeyframe> keyframes,
        IReadOnlyList<MshAnimationMapEntry> animationMap,
        float sampleTime,
        int frameCount = 0)
    {
        var defaultKeyframeIndex = node.DefaultKeyframeIndex0x08;
        if (defaultKeyframeIndex == ushort.MaxValue || defaultKeyframeIndex >= keyframes.Count)
            return MshSampledPieceTransform.Identity;

        var frameIndex = (int)MathF.Round(sampleTime - 0.5f, MidpointRounding.ToEven);
        if (frameIndex < 0 ||
            (frameCount > 0 && frameIndex >= frameCount) ||
            node.AnimMapStart0x13 == ushort.MaxValue ||
            node.AnimMapStart0x13 + frameIndex >= animationMap.Count)
        {
            return FromKeyframe(keyframes[defaultKeyframeIndex]);
        }

        var keyframeIndex = animationMap[node.AnimMapStart0x13 + frameIndex].KeyframeIndex;
        if (keyframeIndex >= defaultKeyframeIndex ||
            keyframeIndex >= keyframes.Count ||
            keyframeIndex + 1 >= keyframes.Count)
        {
            return FromKeyframe(keyframes[defaultKeyframeIndex]);
        }

        var key0 = keyframes[keyframeIndex];
        var key1 = keyframes[keyframeIndex + 1];

        if (sampleTime == key0.Time)
            return FromKeyframe(key0);

        if (sampleTime == key1.Time)
            return FromKeyframe(key1);

        var duration = key1.Time - key0.Time;
        if (MathF.Abs(duration) < 1e-6f)
            return FromKeyframe(key0);

        var t = (sampleTime - key0.Time) / duration;
        return new MshSampledPieceTransform(
            Vector3.Lerp(ToNumericsVector3(key0.Position), ToNumericsVector3(key1.Position), t),
            Quaternion.Slerp(ToNumericsQuaternion(key0.Rotation), ToNumericsQuaternion(key1.Rotation), t));
    }

    /// <summary>
    /// Поза детали в момент <paramref name="time1"/> (диапазон 1) и <paramref name="time2"/> (диапазон 2), смешанная с весом диапазона 2
    /// (<c>MSH_piece::SampleBlendedAnimationPoseMatrix</c>, AniMesh.dll 0x100132D0). Диапазон с отрицательным временем не участвует:
    /// диапазон 1 - если вес &lt; 1, диапазон 2 - если вес &gt; 0. Если не участвует ни один, возвращается поза по умолчанию.
    /// </summary>
    public static MshSampledPieceTransform SamplePieceBlendedAtTime(
        Msh0x01.Node node,
        IReadOnlyList<MshTransformKeyframe> keyframes,
        IReadOnlyList<MshAnimationMapEntry> animationMap,
        float time1,
        float time2,
        float weight2,
        int frameCount = 0)
    {
        var use1 = weight2 < 1.0f && time1 >= 0.0f;
        var use2 = weight2 > 0.0f && time2 >= 0.0f;

        if (!use1 && !use2)
            return SamplePieceAnimationAtTime(node, keyframes, animationMap, -1.0f, frameCount);

        if (use1 && !use2)
            return SamplePieceAnimationAtTime(node, keyframes, animationMap, time1, frameCount);

        if (!use1)
            return SamplePieceAnimationAtTime(node, keyframes, animationMap, time2, frameCount);

        var pose1 = SamplePieceAnimationAtTime(node, keyframes, animationMap, time1, frameCount);
        var pose2 = SamplePieceAnimationAtTime(node, keyframes, animationMap, time2, frameCount);

        // Кватернионы сводятся к одной полусфере, как в движке (иначе slerp пошёл бы длинной дугой).
        var q2 = Quaternion.Dot(pose1.Rotation, pose2.Rotation) < 0.0f
            ? Quaternion.Negate(pose2.Rotation)
            : pose2.Rotation;

        return new MshSampledPieceTransform(
            Vector3.Lerp(pose1.Position, pose2.Position, weight2),
            Quaternion.Slerp(pose1.Rotation, q2, weight2));
    }

    public static Matrix4x4 ToMatrix(MshSampledPieceTransform transform)
    {
        var matrix = Matrix4x4.CreateFromQuaternion(transform.Rotation);
        matrix.Translation = transform.Position;
        return matrix;
    }

    public static MshSampledPieceTransform FromKeyframe(MshTransformKeyframe keyframe)
    {
        return new MshSampledPieceTransform(
            ToNumericsVector3(keyframe.Position),
            ToNumericsQuaternion(keyframe.Rotation));
    }

    private static Vector3 ToNumericsVector3(Common.Vector3 vector)
    {
        return new Vector3(vector.X, vector.Y, vector.Z);
    }

    private static Quaternion ToNumericsQuaternion(Common.UShortQuaternion packedQuaternion)
    {
        var q = new Quaternion(
            packedQuaternion.X / 32767f,
            packedQuaternion.Y / 32767f,
            packedQuaternion.Z / 32767f,
            packedQuaternion.W / 32767f);

        if (q.LengthSquared() < 1e-8f)
            return Quaternion.Identity;

        // MSH stores mesh-to-parent rotation; viewport composition uses the inverse.
        return Quaternion.Conjugate(Quaternion.Normalize(q));
    }
}

public readonly record struct MshSampledPieceTransform(
    Vector3 Position,
    Quaternion Rotation)
{
    public static MshSampledPieceTransform Identity { get; } = new(Vector3.Zero, Quaternion.Identity);
}
