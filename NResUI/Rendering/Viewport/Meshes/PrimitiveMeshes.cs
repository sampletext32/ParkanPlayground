using System.Numerics;
using Silk.NET.OpenGL;
using NResUI.Rendering.Viewport;

namespace NResUI.Rendering.Viewport.Meshes;

public static unsafe class PrimitiveMeshes
{
    public const int FloatsPerVertex = 11;

    public static GpuMesh CreateCube(GL gl)
    {
        var vertices = new List<float>();
        var indices = new List<uint>();

        AddCubeFace(vertices, indices, new(-1, -1,  1), new( 1, -1,  1), new( 1,  1,  1), new(-1,  1,  1), new(0, 0, 1),  new(1, 0, 0));
        AddCubeFace(vertices, indices, new( 1, -1, -1), new(-1, -1, -1), new(-1,  1, -1), new( 1,  1, -1), new(0, 0, -1), new(0, 1, 0));
        AddCubeFace(vertices, indices, new(-1, -1, -1), new(-1, -1,  1), new(-1,  1,  1), new(-1,  1, -1), new(-1, 0, 0), new(0, 0, 1));
        AddCubeFace(vertices, indices, new( 1, -1,  1), new( 1, -1, -1), new( 1,  1, -1), new( 1,  1,  1), new(1, 0, 0),  new(1, 1, 0));
        AddCubeFace(vertices, indices, new(-1,  1,  1), new( 1,  1,  1), new( 1,  1, -1), new(-1,  1, -1), new(0, 1, 0),  new(1, 0, 1));
        AddCubeFace(vertices, indices, new(-1, -1, -1), new( 1, -1, -1), new( 1, -1,  1), new(-1, -1,  1), new(0, -1, 0), new(0, 1, 1));

        return CreateColoredIndexedMesh(gl, vertices, indices, PrimitiveType.Triangles);
    }

    public static GpuMesh CreateWorldGrid(GL gl, int halfExtent = 20, int minorStep = 1, int majorStep = 5)
    {
        if (halfExtent <= 0) throw new ArgumentOutOfRangeException(nameof(halfExtent));
        if (minorStep <= 0) throw new ArgumentOutOfRangeException(nameof(minorStep));
        if (majorStep <= 0) throw new ArgumentOutOfRangeException(nameof(majorStep));

        var vertices = new List<float>();
        var indices = new List<uint>();

        void AddLine(float x0, float y0, float z0, float x1, float y1, float z1, float r, float g, float b)
        {
            var startIndex = (uint)(vertices.Count / FloatsPerVertex);
            AddVertex(vertices, x0, y0, z0, r, g, b);
            AddVertex(vertices, x1, y1, z1, r, g, b);
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

            AddLine(-halfExtent, y, i, halfExtent, y, i, isAxis ? 0.25f : r, isAxis ? 0.45f : g, isAxis ? 1.0f : b);
            AddLine(i, y, -halfExtent, i, y, halfExtent, isAxis ? 1.0f : r, isAxis ? 0.25f : g, isAxis ? 0.25f : b);
        }

        return CreateIndexedMesh(gl, vertices.ToArray(), indices.ToArray(), PrimitiveType.Lines);
    }

    public static GpuMesh CreateUnitWireBox(GL gl)
    {
        var vertices = new List<float>();
        var color = new Vector3(1.0f, 0.82f, 0.15f);

        Vector3[] corners =
        {
            new(-1, -1, -1), new( 1, -1, -1), new( 1,  1, -1), new(-1,  1, -1),
            new(-1, -1,  1), new( 1, -1,  1), new( 1,  1,  1), new(-1,  1,  1),
        };

        foreach (var corner in corners)
            AddVertex(vertices, corner.X, corner.Y, corner.Z, color.X, color.Y, color.Z);

        uint[] indices =
        {
            0, 1, 1, 2, 2, 3, 3, 0,
            4, 5, 5, 6, 6, 7, 7, 4,
            0, 4, 1, 5, 2, 6, 3, 7,
        };

        return CreateIndexedMesh(gl, vertices.ToArray(), indices, PrimitiveType.Lines);
    }

    public static GpuMesh CreateAxes(GL gl, float length = 1.0f)
    {
        if (length <= 0.0f) throw new ArgumentOutOfRangeException(nameof(length));

        var vertices = new List<float>();
        var indices = new List<uint>();

        void AddLine(Vector3 a, Vector3 b, Vector3 color)
        {
            var start = (uint)(vertices.Count / FloatsPerVertex);
            AddVertex(vertices, a.X, a.Y, a.Z, color.X, color.Y, color.Z);
            AddVertex(vertices, b.X, b.Y, b.Z, color.X, color.Y, color.Z);
            indices.Add(start);
            indices.Add(start + 1);
        }

        AddLine(Vector3.Zero, new Vector3(length, 0, 0), new Vector3(1.0f, 0.25f, 0.25f));
        AddLine(Vector3.Zero, new Vector3(0, length, 0), new Vector3(0.25f, 1.0f, 0.25f));
        AddLine(Vector3.Zero, new Vector3(0, 0, length), new Vector3(0.25f, 0.45f, 1.0f));

        return CreateIndexedMesh(gl, vertices.ToArray(), indices.ToArray(), PrimitiveType.Lines);
    }

    public static GpuMesh CreateColoredIndexedMesh(GL gl, IReadOnlyList<float> vertices, IReadOnlyList<uint> indices, PrimitiveType primitiveType = PrimitiveType.Triangles, ViewportMaterial? material = null)
    {
        return CreateIndexedMesh(gl, vertices.ToArray(), indices.ToArray(), primitiveType, material);
    }

    private static void AddCubeFace(List<float> vertices, List<uint> indices, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, Vector3 color)
    {
        var start = (uint)(vertices.Count / FloatsPerVertex);
        AddVertex(vertices, a.X, a.Y, a.Z, color.X, color.Y, color.Z, normal.X, normal.Y, normal.Z);
        AddVertex(vertices, b.X, b.Y, b.Z, color.X, color.Y, color.Z, normal.X, normal.Y, normal.Z);
        AddVertex(vertices, c.X, c.Y, c.Z, color.X, color.Y, color.Z, normal.X, normal.Y, normal.Z);
        AddVertex(vertices, d.X, d.Y, d.Z, color.X, color.Y, color.Z, normal.X, normal.Y, normal.Z);
        indices.Add(start + 0); indices.Add(start + 1); indices.Add(start + 2);
        indices.Add(start + 2); indices.Add(start + 3); indices.Add(start + 0);
    }

    private static void AddVertex(List<float> vertices, float x, float y, float z, float r, float g, float b, float nx = 0.0f, float ny = 1.0f, float nz = 0.0f, float u = 0.0f, float v = 0.0f)
    {
        vertices.Add(x); vertices.Add(y); vertices.Add(z);
        vertices.Add(r); vertices.Add(g); vertices.Add(b);
        vertices.Add(nx); vertices.Add(ny); vertices.Add(nz);
        vertices.Add(u); vertices.Add(v);
    }

    private static GpuMesh CreateIndexedMesh(GL gl, float[] vertices, uint[] indices, PrimitiveType primitiveType, ViewportMaterial? material = null)
    {
        var vao = gl.GenVertexArray();
        var vbo = gl.GenBuffer();
        var ebo = gl.GenBuffer();

        gl.BindVertexArray(vao);

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        fixed (float* vertexPtr = vertices)
        {
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), vertexPtr, BufferUsageARB.StaticDraw);
        }

        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        fixed (uint* indexPtr = indices)
        {
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), indexPtr, BufferUsageARB.StaticDraw);
        }

        const uint stride = FloatsPerVertex * sizeof(float);

        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, null);

        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));

        gl.EnableVertexAttribArray(2);
        gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, (void*)(6 * sizeof(float)));

        gl.EnableVertexAttribArray(3);
        gl.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, stride, (void*)(9 * sizeof(float)));

        gl.BindVertexArray(0);

        return new GpuMesh(gl, vao, vbo, ebo, (uint)indices.Length, primitiveType, material);
    }
}
