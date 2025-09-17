using System;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTKRectangleDemo.MathLib; 

namespace OpenTKRectangleDemo
{
    public class Game : GameWindow
    {
        private int _vao;
        private int _vbo;
        private int _ebo;
        private int _shaderProgram;

        private readonly float[] _vertices =
        {
            -0.5f, -0.5f, 0.0f,
             0.5f, -0.5f, 0.0f,
             0.5f,  0.5f, 0.0f,
            -0.5f,  0.5f, 0.0f
        };

        private readonly uint[] _indices = { 0, 1, 2, 2, 3, 0 };

        public Game(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) { }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(Color4.CornflowerBlue);

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            string vertexShaderSource = @"
                #version 330 core
                layout(location = 0) in vec3 aPosition;
                void main()
                {
                    gl_Position = vec4(aPosition, 1.0);
                }
            ";

            string fragmentShaderSource = @"
                #version 330 core
                out vec4 FragColor;
                void main()
                {
                    FragColor = vec4(1.0, 0.2, 0.2, 1.0);
                }
            ";

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);

            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, vertexShader);
            GL.AttachShader(_shaderProgram, fragmentShader);
            GL.LinkProgram(_shaderProgram);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            GL.DeleteVertexArray(_vao);
            GL.DeleteProgram(_shaderProgram);
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            // --- DEMONSTRATE VECTOR OPERATIONS ---
            Console.WriteLine("=== Vector Operations ===");
            var v1 = new Vector3D(1, 2, 3);
            var v2 = new Vector3D(4, 5, 6);

            Console.WriteLine($"v1 = {v1}");
            Console.WriteLine($"v2 = {v2}");
            Console.WriteLine($"v1 + v2 = {v1 + v2}");
            Console.WriteLine($"v1 - v2 = {v1 - v2}");
            Console.WriteLine($"Dot(v1, v2) = {Vector3D.Dot(v1, v2)}");
            Console.WriteLine($"Cross(v1, v2) = {Vector3D.Cross(v1, v2)}");

            // --- DEMONSTRATE MATRIX OPERATIONS ---
            Console.WriteLine("\n=== Matrix Operations ===");
            var scaleMatrix = Matrix4x4D.CreateScale(2.0, 2.0, 2.0);
            var rotationMatrix = Matrix4x4D.CreateRotationZ(Math.PI / 4); // 45° rotation around Z-axis
            var transformMatrix = scaleMatrix * rotationMatrix;

            var transformedVector = transformMatrix * v1;
            Console.WriteLine($"Original Vector: {v1}");
            Console.WriteLine($"After Scaling + Rotation: {transformedVector}");

            // --- RUN THE WINDOW ---
            var gws = GameWindowSettings.Default;
            var nws = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "OpenTK Rectangle Demo"
            };

            using (var window = new Game(gws, nws))
            {
                window.Run();
            }
        }
    }
}
