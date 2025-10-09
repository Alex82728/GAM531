using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Assignment5
{
    public class Game : GameWindow
    {
        private readonly float[] _vertices =
        {
            // Positions         // Normals
            -0.5f, -0.5f, -0.5f,  0f,  0f, -1f,
             0.5f, -0.5f, -0.5f,  0f,  0f, -1f,
             0.5f,  0.5f, -0.5f,  0f,  0f, -1f,
             0.5f,  0.5f, -0.5f,  0f,  0f, -1f,
            -0.5f,  0.5f, -0.5f,  0f,  0f, -1f,
            -0.5f, -0.5f, -0.5f,  0f,  0f, -1f,

            -0.5f, -0.5f,  0.5f,  0f,  0f, 1f,
             0.5f, -0.5f,  0.5f,  0f,  0f, 1f,
             0.5f,  0.5f,  0.5f,  0f,  0f, 1f,
             0.5f,  0.5f,  0.5f,  0f,  0f, 1f,
            -0.5f,  0.5f,  0.5f,  0f,  0f, 1f,
            -0.5f, -0.5f,  0.5f,  0f,  0f, 1f,

            -0.5f,  0.5f,  0.5f, -1f,  0f,  0f,
            -0.5f,  0.5f, -0.5f, -1f,  0f,  0f,
            -0.5f, -0.5f, -0.5f, -1f,  0f,  0f,
            -0.5f, -0.5f, -0.5f, -1f,  0f,  0f,
            -0.5f, -0.5f,  0.5f, -1f,  0f,  0f,
            -0.5f,  0.5f,  0.5f, -1f,  0f,  0f,

             0.5f,  0.5f,  0.5f,  1f,  0f,  0f,
             0.5f,  0.5f, -0.5f,  1f,  0f,  0f,
             0.5f, -0.5f, -0.5f,  1f,  0f,  0f,
             0.5f, -0.5f, -0.5f,  1f,  0f,  0f,
             0.5f, -0.5f,  0.5f,  1f,  0f,  0f,
             0.5f,  0.5f,  0.5f,  1f,  0f,  0f,

            -0.5f, -0.5f, -0.5f,  0f, -1f,  0f,
             0.5f, -0.5f, -0.5f,  0f, -1f,  0f,
             0.5f, -0.5f,  0.5f,  0f, -1f,  0f,
             0.5f, -0.5f,  0.5f,  0f, -1f,  0f,
            -0.5f, -0.5f,  0.5f,  0f, -1f,  0f,
            -0.5f, -0.5f, -0.5f,  0f, -1f,  0f,

            -0.5f,  0.5f, -0.5f,  0f, 1f,  0f,
             0.5f,  0.5f, -0.5f,  0f, 1f,  0f,
             0.5f,  0.5f,  0.5f,  0f, 1f,  0f,
             0.5f,  0.5f,  0.5f,  0f, 1f,  0f,
            -0.5f,  0.5f,  0.5f,  0f, 1f,  0f,
            -0.5f,  0.5f, -0.5f,  0f, 1f,  0f
        };

        private int _vbo, _vao;
        private int _shaderProgram;
        private float _angle = 0f;

        private Vector3 _lightPos = new(2.0f, 2.0f, 2.0f);

        public Game(GameWindowSettings settings, NativeWindowSettings native)
            : base(settings, native) { }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);
            GL.Enable(EnableCap.DepthTest);

            _shaderProgram = ShaderLoader.CreateShader("shader.vert", "shader.frag");

            _vbo = GL.GenBuffer();
            _vao = GL.GenVertexArray();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            int stride = 6 * sizeof(float);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_shaderProgram);

            _angle += 50f * (float)args.Time;
            var model = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_angle));
            var view = Matrix4.LookAt(new Vector3(2, 2, 2), Vector3.Zero, Vector3.UnitY);
            var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size.X / (float)Size.Y, 0.1f, 100f);

            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "model"), false, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "view"), false, ref view);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "projection"), false, ref projection);

            GL.Uniform3(GL.GetUniformLocation(_shaderProgram, "lightPos"), _lightPos);
            GL.Uniform3(GL.GetUniformLocation(_shaderProgram, "viewPos"), new Vector3(2, 2, 2));

            GL.Uniform3(GL.GetUniformLocation(_shaderProgram, "lightColor"), new Vector3(1.0f, 1.0f, 1.0f));
            GL.Uniform3(GL.GetUniformLocation(_shaderProgram, "objectColor"), new Vector3(0.3f, 0.7f, 1.0f));

            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
            GL.DeleteProgram(_shaderProgram);
            base.OnUnload();
        }
    }

    public static class ShaderLoader
    {
        public static int CreateShader(string vertPath, string fragPath)
        {
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, System.IO.File.ReadAllText(vertPath));
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, System.IO.File.ReadAllText(fragPath));
            GL.CompileShader(fragmentShader);

            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return program;
        }
    }
}
