using Silk.NET.OpenGL;

namespace NResUI.Rendering.Viewport.Meshes;

public sealed class GpuMesh
{
    private readonly GL _gl;

    public uint VertexArrayObject { get; }
    public uint VertexBufferObject { get; }
    public uint ElementBufferObject { get; }
    public uint IndexCount { get; }
    public PrimitiveType PrimitiveType { get; }

    public GpuMesh(
        GL gl,
        uint vertexArrayObject,
        uint vertexBufferObject,
        uint elementBufferObject,
        uint indexCount,
        PrimitiveType primitiveType = PrimitiveType.Triangles)
    {
        _gl = gl;
        VertexArrayObject = vertexArrayObject;
        VertexBufferObject = vertexBufferObject;
        ElementBufferObject = elementBufferObject;
        IndexCount = indexCount;
        PrimitiveType = primitiveType;
    }

    public void Draw()
    {
        _gl.BindVertexArray(VertexArrayObject);
        _gl.DrawElements(PrimitiveType, IndexCount, DrawElementsType.UnsignedInt, null);
    }
}
