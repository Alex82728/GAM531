using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Exercise9
{
    public class Game : GameWindow
    {
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _shaderProgram;

        private readonly float[] _vertices =
        {
            // positions       // colors
             0.0f,  0.5f, 0f,  1f, 0f, 0f, // top
            -0.5f, -0.5f, 0f,  0f, 1f, 0f, // left
             0.5f, -0.5f, 0f,  0f, 0f, 1f  // right
        };

        public Game(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            // Setup VAO
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            // Setup VBO
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            // Load shaders
            _shaderProgram = ShaderLoader.CreateProgram("vs.glsl", "fs.glsl");
            GL.UseProgram(_shaderProgram);

            // vertex positions
            int posLocation = GL.GetAttribLocation(_shaderProgram, "aPosition");
            GL.VertexAttribPointer(posLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(posLocation);

            // vertex colors
            int colLocation = GL.GetAttribLocation(_shaderProgram, "aColor");
            GL.VertexAttribPointer(colLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(colLocation);

            GL.ClearColor(0.1f, 0.2f, 0.3f, 1.0f);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(_vertexBufferObject);
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(_vertexArrayObject);
            GL.DeleteProgram(_shaderProgram);
        }
    }
}
