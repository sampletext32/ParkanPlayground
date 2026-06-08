using System.Collections;
using System.Numerics;
using Common;
using MshLib;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace NResUI.Rendering.Viewport.Msh;

public static class MshRestPoseBuilder
{
    /// <summary>
    /// Собирает bind/rest pose для pieces из fallback keyframes 0x08.
    /// Циклы и битые parent-ссылки не должны ломать viewport, поэтому такие узлы остаются с identity transform.
    /// </summary>
    public static IReadOnlyList<MshPieceRestPose> BuildRestPose(Msh0x01.Msh0x01Component nodesComponent, List<Msh0x08.AnimationDescriptor> animationDescriptors)
    {
        var nodeList = nodesComponent.Nodes;
        var animationList = animationDescriptors;

        var poses = new MshPieceRestPose[nodeList.Count];
        var state = new byte[nodeList.Count];

        for (var nodeIndex = 0; nodeIndex < nodeList.Count; nodeIndex++)
            BuildNodePose(nodeIndex, nodeList, animationList, poses, state);

        return poses;
    }

    private static Matrix4x4 BuildNodePose(
        int nodeIndex,
        List<Msh0x01.Node> nodes,
        List<Msh0x08.AnimationDescriptor> animationDescriptors,
        MshPieceRestPose[] poses,
        byte[] state)
    {
        if (state[nodeIndex] == 2)
            return poses[nodeIndex].MeshSpaceTransform;
        
        if (state[nodeIndex] == 1)
        {
            // Broken/cyclic parent chain. Keep this node local so the viewer remains usable.
            return Matrix4x4.Identity;
        }
        state[nodeIndex] = 1;

        var node = nodes[nodeIndex]!;
        var parentIndex = GetParentIndex(node);
        var fallbackKeyframeIndex = GetFallbackKeyframeIndex(node);
        var hasFallbackPose = fallbackKeyframeIndex >= 0 && fallbackKeyframeIndex < animationDescriptors.Count;

        // var localTransform = Matrix4x4.Identity;
        Matrix4x4 localTransform;
        if (hasFallbackPose)
        {
            localTransform = BuildTransformFromAnimationDescriptor(animationDescriptors[fallbackKeyframeIndex]!);
        }
        else
        {
            localTransform = Matrix4x4.Identity;
        }

        if (parentIndex == -1)
        {
            // Root nodes describe object placement in game space; the viewer keeps the model centered.
            localTransform.Translation = Vector3.Zero;
        }
        
        var meshSpaceTransform = localTransform;
        if (parentIndex >= 0 && parentIndex < nodes.Count && parentIndex != nodeIndex)
        {
            var parentTransform = BuildNodePose(parentIndex, nodes, animationDescriptors, poses, state);
            meshSpaceTransform = localTransform * parentTransform;
        }

        poses[nodeIndex] = new MshPieceRestPose(
            NodeIndex: nodeIndex,
            ParentIndex: parentIndex,
            FallbackKeyframeIndex: fallbackKeyframeIndex,
            HasFallbackPose: hasFallbackPose,
            LocalTransform: localTransform,
            MeshSpaceTransform: meshSpaceTransform);

        state[nodeIndex] = 2;
        return meshSpaceTransform;
    }

    private static int GetParentIndex(Msh0x01.Node node)
    {
        return node.ParentIndexOrLink == ushort.MaxValue ? -1 : node.ParentIndexOrLink;
    }

    private static int GetFallbackKeyframeIndex(Msh0x01.Node node)
    {
        try
        {
            var value = Convert.ToUInt16(node.FallbackKey0x08);
            return value == ushort.MaxValue ? -1 : value;
        }
        catch
        {
            return -1;
        }
    }

    private static Matrix4x4 BuildTransformFromAnimationDescriptor(Msh0x08.AnimationDescriptor descriptor)
    {
        var position = ToSystemVector3(descriptor.Position);
        var rotation = ToSystemQuaternion(descriptor.Rotation);

        var matrix = Matrix4x4.CreateFromQuaternion(rotation);
        matrix.Translation = position;
        return matrix;
    }

    private static Vector3 ToSystemVector3(dynamic vector)
    {
        return new Vector3(
            Convert.ToSingle(vector.X),
            Convert.ToSingle(vector.Y),
            Convert.ToSingle(vector.Z));
    }

    private static Quaternion ToSystemQuaternion(UShortQuaternion packedQuaternion)
    {
        // MSH 0x08 stores quaternion as W, X, Y, Z.
        // System.Numerics.Quaternion constructor expects X, Y, Z, W.

        var q = new Quaternion(
            packedQuaternion.X / 32767f,
            packedQuaternion.Y / 32767f,
            packedQuaternion.Z / 32767f,
            packedQuaternion.W / 32767f
        );

        if (q.LengthSquared() < 1e-8f)
            return Quaternion.Identity;

        // MSH/game convention is opposite to our System.Numerics/OpenGL transform convention.
        // Conjugating converts the stored piece rotation into the viewport convention.
        // The MSH keyframe rotation is stored as mesh-to-parent instead of parent-to-mesh, so the viewer needs the inverse.
        return Quaternion.Conjugate(q);
    }
}

public readonly record struct MshPieceRestPose(
    int NodeIndex,
    int ParentIndex,
    int FallbackKeyframeIndex,
    bool HasFallbackPose,
    Matrix4x4 LocalTransform,
    Matrix4x4 MeshSpaceTransform);
