using System;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenTKCubeDemo
{
    public class Game : GameWindow
    {
        private int _vao;
        private int _vbo;
        private int _ebo;
        private int _shaderProgram;
        private int _mvpLocation;

        private float _rotation = 0f;

        private readonly float[] _vertices =
        {
            // Positions
            -0.5f, -0.5f, -0.5f, // 0
             0.5f, -0.5f, -0.5f, // 1
             0.5f,  0.5f, -0.5f, // 2
            -0.5f,  0.5f, -0.5f, // 3
            -0.5f, -0.5f,  0.5f, // 4
             0.5f, -0.5f,  0.5f, // 5
             0.5f,  0.5f,  0.5f, // 6
            -0.5f,  0.5f,  0.5f  // 7
        };

        private readonly uint[] _indices =
        {
            // Back face
            0, 1, 2, 2, 3, 0,
            // Front face
            4, 5, 6, 6, 7, 4,
            // Left face
            0, 3, 7, 7, 4, 0,
            // Right face
            1, 5, 6, 6, 2, 1,
            // Bottom face
            0, 1, 5, 5, 4, 0,
            // Top face
            3, 2, 6, 6, 7, 3
        };

        public Game(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) { }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(Color4.CornflowerBlue);

            // VAO
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            // VBO
            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            // EBO
            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            // Shaders
            string vertexShaderSource = @"
                #version 330 core
                layout(location = 0) in vec3 aPosition;
                uniform mat4 mvp;
                void main()
                {
                    gl_Position = mvp * vec4(aPosition, 1.0);
                }
            ";

            string fragmentShaderSource = @"
                #version 330 core
                out vec4 FragColor;
                void main()
                {
                    FragColor = vec4(0.2, 0.8, 1.0, 1.0);
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

            _mvpLocation = GL.GetUniformLocation(_shaderProgram, "mvp");
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_shaderProgram);
            _rotation += (float)args.Time;

            // Matrices
            var model = Matrix4.CreateRotationY(_rotation);
            var view = Matrix4.LookAt(new Vector3(2, 2, 2), Vector3.Zero, Vector3.UnitY);
            var projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45f),
                Size.X / (float)Size.Y,
                0.1f,
                100f
            );

            var mvp = model * view * projection;
            GL.UniformMatrix4(_mvpLocation, false, ref mvp);

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
        private static void Main()
        {
            var gws = GameWindowSettings.Default;
            var nws = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "OpenTK 3D Cube"
            };

            using (var window = new Game(gws, nws))
            {
                window.Run();
            }
        }
    }
}
