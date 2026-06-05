using System.Numerics;
using Silk.NET.OpenGL;

namespace NResUI.Rendering.Viewport.OpenGL;

public sealed unsafe class ShaderProgram
{
    private readonly GL _gl;

    public uint Handle { get; }

    public ShaderProgram(GL gl, string vertexShaderSource, string fragmentShaderSource, string debugName)
    {
        _gl = gl;

        var vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource, debugName);
        var fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource, debugName);

        Handle = _gl.CreateProgram();
        _gl.AttachShader(Handle, vertexShader);
        _gl.AttachShader(Handle, fragmentShader);
        _gl.LinkProgram(Handle);

        _gl.GetProgram(Handle, ProgramPropertyARB.LinkStatus, out var linked);
        if (linked == 0)
        {
            var log = _gl.GetProgramInfoLog(Handle);
            throw new InvalidOperationException($"{debugName} shader link failed: {log}");
        }

        _gl.DetachShader(Handle, vertexShader);
        _gl.DetachShader(Handle, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);
    }

    public void Use()
    {
        _gl.UseProgram(Handle);
    }

    public int GetUniformLocation(string name)
    {
        return _gl.GetUniformLocation(Handle, name);
    }

    public void SetMatrix4(int location, Matrix4x4 matrix)
    {
        _gl.UniformMatrix4(location, 1, false, (float*)&matrix);
    }

    public void SetVector4(int location, Vector4 value)
    {
        _gl.Uniform4(location, value.X, value.Y, value.Z, value.W);
    }

    private uint CompileShader(ShaderType type, string source, string debugName)
    {
        var shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);

        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out var compiled);
        if (compiled == 0)
        {
            var log = _gl.GetShaderInfoLog(shader);
            throw new InvalidOperationException($"{debugName} {type} compile failed: {log}");
        }

        return shader;
    }
}
