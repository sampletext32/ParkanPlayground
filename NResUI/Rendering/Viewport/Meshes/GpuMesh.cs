using Silk.NET.OpenGL;
using NResUI.Rendering.Viewport;

namespace NResUI.Rendering.Viewport.Meshes;

public sealed class GpuMesh : IDisposable
{
    private readonly GL _gl;
    private bool _disposed;

    public uint VertexArrayObject { get; }
    public uint VertexBufferObject { get; }
    public uint ElementBufferObject { get; }
    public uint IndexCount { get; }
    public PrimitiveType PrimitiveType { get; }
    public ViewportMaterial Material { get; }

    public GpuMesh(
        GL gl,
        uint vertexArrayObject,
        uint vertexBufferObject,
        uint elementBufferObject,
        uint indexCount,
        PrimitiveType primitiveType = PrimitiveType.Triangles,
        ViewportMaterial? material = null)
    {
        _gl = gl;
        VertexArrayObject = vertexArrayObject;
        VertexBufferObject = vertexBufferObject;
        ElementBufferObject = elementBufferObject;
        IndexCount = indexCount;
        PrimitiveType = primitiveType;
        Material = material ?? ViewportMaterial.Untextured;
    }

    public unsafe void Draw()
    {
        if (_disposed)
            return;

        _gl.BindVertexArray(VertexArrayObject);
        _gl.DrawElements(PrimitiveType, IndexCount, DrawElementsType.UnsignedInt, null);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _gl.DeleteBuffer(ElementBufferObject);
        _gl.DeleteBuffer(VertexBufferObject);
        _gl.DeleteVertexArray(VertexArrayObject);
        _disposed = true;
    }
}
