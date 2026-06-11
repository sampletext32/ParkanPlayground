using System.Numerics;

namespace MshLib;

public static class MshAnimationSampler
{
    public static MshSampledPieceTransform SamplePieceAnimationAtTime(
        Msh0x01.Node node,
        IReadOnlyList<MshTransformKeyframe> keyframes,
        IReadOnlyList<MshAnimationMapEntry> animationMap,
        float sampleTime)
    {
        var defaultKeyframeIndex = node.DefaultKeyframeIndex0x08;
        if (defaultKeyframeIndex == ushort.MaxValue || defaultKeyframeIndex >= keyframes.Count)
            return MshSampledPieceTransform.Identity;

        var frameIndex = (int)MathF.Round(sampleTime - 0.5f, MidpointRounding.AwayFromZero);
        if (frameIndex < 0 ||
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
