// Configure window options

using System.Buffers.Binary;
using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

public static class Program
{
    private static string vertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec3 aPos;
            uniform mat4 uMVP;

            void main()
            {
                gl_Position = uMVP * vec4(aPos, 1.0);
                gl_PointSize = 8.0;
            }
        ";

    private static string fragmentShaderSource = @"
            #version 330 core
            out vec4 FragColor;

            void main()
            {
                FragColor = vec4(1.0, 1.0, 1.0, 1.0); // White points
            }
        ";


    private static IWindow? window;
    private static GL? gl = null;


    private static uint shaderProgram = uint.MaxValue;
    private static uint vao = uint.MaxValue;
    private static uint vbo = uint.MaxValue;
    private static Matrix4x4 mvp = new Matrix4x4();
    private static float[] points = []; 

    public static void Main(string[] args)
    {
        var path = "C:\\ParkanUnpacked\\Land.msh\\2_03 00 00 00_Land.bin";
        var bytes = File.ReadAllBytes(path);

        points = new float[bytes.Length / 4];
        for (int i = 0; i < bytes.Length / 4; i++)
        {
            points[i] = BinaryPrimitives.ReadSingleBigEndian(bytes.AsSpan()[(i * 4)..]);
        }

        var options = WindowOptions.Default;
        options.API = new GraphicsAPI(ContextAPI.OpenGL, new APIVersion(3, 3));
        options.Title = "3D Points with Silk.NET";
        window = Window.Create(options);

        window.Load += OnLoad;
        window.Render += OnRender;
        window.Run();
    }


    unsafe static void OnLoad()
    {
        gl = window.CreateOpenGL();
        // Compile shaders
        uint vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(vertexShader, vertexShaderSource);
        gl.CompileShader(vertexShader);
        CheckShaderCompile(vertexShader);

        uint fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(fragmentShader, fragmentShaderSource);
        gl.CompileShader(fragmentShader);
        CheckShaderCompile(fragmentShader);

        // Create shader program
        shaderProgram = gl.CreateProgram();
        gl.AttachShader(shaderProgram, vertexShader);
        gl.AttachShader(shaderProgram, fragmentShader);
        gl.LinkProgram(shaderProgram);
        CheckProgramLink(shaderProgram);

        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);

        // Create VAO and VBO
        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);

        vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        unsafe
        {
            fixed (float* ptr = points)
            {
                gl.BufferData(
                    BufferTargetARB.ArrayBuffer,
                    (nuint) (points.Length * sizeof(float)),
                    ptr,
                    BufferUsageARB.StaticDraw
                );
            }
        }

        gl.VertexAttribPointer(
            0,
            3,
            VertexAttribPointerType.Float,
            false,
            3 * sizeof(float),
            (void*) 0
        );
        gl.EnableVertexAttribArray(0);

        gl.BindVertexArray(0); // Unbind VAO

        gl.Enable(EnableCap.DepthTest);
    }

    unsafe static void OnRender(double dt)
    {
        gl.ClearColor(
            0.1f,
            0.1f,
            0.1f,
            1.0f
        );
        gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        // Set up MVP matrix
        Matrix4x4 view = Matrix4x4.CreateLookAt(
            new Vector3(100, 100, 40), // Camera position
            Vector3.Zero, // Look at origin
            Vector3.UnitY
        ); // Up direction
        Matrix4x4 projection = Matrix4x4.CreatePerspectiveFieldOfView(
            (float) Math.PI / 4f, // 45 degrees
            (float) window.Size.X / window.Size.Y,
            0.1f,
            100f
        );
        mvp = view * projection;
        
        gl.UseProgram(shaderProgram);

        // Set MVP matrix (transpose=true for column-major format)
        int mvpLocation = gl.GetUniformLocation(shaderProgram, "uMVP");

        fixed (Matrix4x4* ptr = &mvp)
        {
            gl.UniformMatrix4(
                mvpLocation,
                1,
                true,
                (float*) ptr
            );
        }

        gl.BindVertexArray(vao);
        gl.DrawArrays(PrimitiveType.Points, 0, (uint) (points.Length / 3));
    }

    // Error checking methods
    static void CheckShaderCompile(uint shader)
    {
        gl.GetShader(shader, ShaderParameterName.CompileStatus, out int success);
        if (success == 0)
            Console.WriteLine(gl.GetShaderInfoLog(shader));
    }

    static void CheckProgramLink(uint program)
    {
        gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int success);
        if (success == 0)
            Console.WriteLine(gl.GetProgramInfoLog(program));
    }
}