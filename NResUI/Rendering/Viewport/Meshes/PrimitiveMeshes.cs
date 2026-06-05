using System;
using System.Collections.Generic;
using Silk.NET.OpenGL;

namespace NResUI.Rendering.Viewport.Meshes;

public static unsafe class PrimitiveMeshes
{
    public static GpuMesh CreateCube(GL gl)
    {
        // position.xyz, color.rgb
        float[] vertices =
        {
            // front
            -1, -1,  1,  1, 0, 0,
             1, -1,  1,  0, 1, 0,
             1,  1,  1,  0, 0, 1,
            -1,  1,  1,  1, 1, 0,

            // back
            -1, -1, -1,  1, 0, 1,
             1, -1, -1,  0, 1, 1,
             1,  1, -1,  1, 1, 1,
            -1,  1, -1,  0.5f, 0.5f, 0.5f,
        };

        uint[] indices =
        {
            // front
            0, 1, 2,
            2, 3, 0,

            // right
            1, 5, 6,
            6, 2, 1,

            // back
            5, 4, 7,
            7, 6, 5,

            // left
            4, 0, 3,
            3, 7, 4,

            // top
            3, 2, 6,
            6, 7, 3,

            // bottom
            4, 5, 1,
            1, 0, 4,
        };

        return CreateIndexedMesh(gl, vertices, indices, PrimitiveType.Triangles);
    }

    public static GpuMesh CreateWorldGrid(
        GL gl,
        int halfExtent = 20,
        int minorStep = 1,
        int majorStep = 5)
    {
        if (halfExtent <= 0)
            throw new ArgumentOutOfRangeException(nameof(halfExtent));

        if (minorStep <= 0)
            throw new ArgumentOutOfRangeException(nameof(minorStep));

        if (majorStep <= 0)
            throw new ArgumentOutOfRangeException(nameof(majorStep));

        var vertices = new List<float>();
        var indices = new List<uint>();

        void AddVertex(float x, float y, float z, float r, float g, float b)
        {
            vertices.Add(x);
            vertices.Add(y);
            vertices.Add(z);
            vertices.Add(r);
            vertices.Add(g);
            vertices.Add(b);
        }

        void AddLine(
            float x0, float y0, float z0,
            float x1, float y1, float z1,
            float r, float g, float b)
        {
            var startIndex = (uint)(vertices.Count / 6);

            AddVertex(x0, y0, z0, r, g, b);
            AddVertex(x1, y1, z1, r, g, b);

            indices.Add(startIndex);
            indices.Add(startIndex + 1);
        }

        const float y = 0.0f;

        for (var i = -halfExtent; i <= halfExtent; i += minorStep)
        {
            var isAxis = i == 0;
            var isMajor = i % majorStep == 0;

            var brightness = isMajor ? 0.38f : 0.24f;
            var r = brightness;
            var g = brightness;
            var b = brightness;

            // Lines parallel to X. The Z axis itself is blue.
            if (isAxis)
            {
                AddLine(-halfExtent, y, i, halfExtent, y, i, 0.25f, 0.45f, 1.0f);
            }
            else
            {
                AddLine(-halfExtent, y, i, halfExtent, y, i, r, g, b);
            }

            // Lines parallel to Z. The X axis itself is red.
            if (isAxis)
            {
                AddLine(i, y, -halfExtent, i, y, halfExtent, 1.0f, 0.25f, 0.25f);
            }
            else
            {
                AddLine(i, y, -halfExtent, i, y, halfExtent, r, g, b);
            }
        }

        return CreateIndexedMesh(gl, vertices.ToArray(), indices.ToArray(), PrimitiveType.Lines);
    }

    private static GpuMesh CreateIndexedMesh(
        GL gl,
        float[] vertices,
        uint[] indices,
        PrimitiveType primitiveType)
    {
        var vao = gl.GenVertexArray();
        var vbo = gl.GenBuffer();
        var ebo = gl.GenBuffer();

        gl.BindVertexArray(vao);

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        fixed (float* vertexPtr = vertices)
        {
            gl.BufferData(
                BufferTargetARB.ArrayBuffer,
                (nuint)(vertices.Length * sizeof(float)),
                vertexPtr,
                BufferUsageARB.StaticDraw);
        }

        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        fixed (uint* indexPtr = indices)
        {
            gl.BufferData(
                BufferTargetARB.ElementArrayBuffer,
                (nuint)(indices.Length * sizeof(uint)),
                indexPtr,
                BufferUsageARB.StaticDraw);
        }

        const uint stride = 6 * sizeof(float);

        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(
            0,
            3,
            VertexAttribPointerType.Float,
            false,
            stride,
            null);

        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(
            1,
            3,
            VertexAttribPointerType.Float,
            false,
            stride,
            (void*)(3 * sizeof(float)));

        gl.BindVertexArray(0);

        return new GpuMesh(gl, vao, vbo, ebo, (uint)indices.Length, primitiveType);
    }
}
