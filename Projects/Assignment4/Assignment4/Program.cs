// Program.cs
using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Assignment4
{
    public class Game : GameWindow
    {
        private int _vao;
        private int _vbo;
        private int _ebo;
        private int _shaderProgram;
        private int _texture;
        private int _mvpLocation;
        private float _rotation = 0f;

        
        private readonly float[] _vertices =
        {
            
            -0.5f, -0.5f, -0.5f,   0.0f, 0.0f,
             0.5f, -0.5f, -0.5f,   1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,   1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,   0.0f, 1.0f,

            
            -0.5f, -0.5f,  0.5f,   0.0f, 0.0f,
             0.5f, -0.5f,  0.5f,   1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,   1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,   0.0f, 1.0f,

          
            -0.5f, -0.5f, -0.5f,   0.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,   1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,   1.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,   0.0f, 1.0f,

          
             0.5f, -0.5f, -0.5f,   0.0f, 0.0f,
             0.5f,  0.5f, -0.5f,   1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,   1.0f, 1.0f,
             0.5f, -0.5f,  0.5f,   0.0f, 1.0f,

         
            -0.5f, -0.5f, -0.5f,   0.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,   1.0f, 0.0f,
             0.5f, -0.5f,  0.5f,   1.0f, 1.0f,
             0.5f, -0.5f, -0.5f,   0.0f, 1.0f,

           
            -0.5f,  0.5f, -0.5f,   0.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,   1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,   1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,   0.0f, 1.0f,
        };

      
        private readonly uint[] _indices =
        {
             0,1,2, 2,3,0,
             4,5,6, 6,7,4,
             8,9,10, 10,11,8,
            12,13,14, 14,15,12,
            16,17,18, 18,19,16,
            20,21,22, 22,23,20
        };

        public Game(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) { }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(Color4.CornflowerBlue);

            // Create and bind VAO/VBO/EBO
            _vao = GL.GenVertexArray(); GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer(); GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _ebo = GL.GenBuffer(); GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            // vertex attributes: pos (location=0), texcoord (location=1)
            var stride = 5 * sizeof(float);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // shaders
            var vertexSrc = @"
                #version 330 core
                layout(location = 0) in vec3 aPos;
                layout(location = 1) in vec2 aTex;
                out vec2 vTex;
                uniform mat4 mvp;
                void main()
                {
                    vTex = aTex;
                    gl_Position = mvp * vec4(aPos, 1.0);
                }
            ";

            var fragmentSrc = @"
                #version 330 core
                in vec2 vTex;
                out vec4 FragColor;
                uniform sampler2D tex;
                void main()
                {
                    FragColor = texture(tex, vTex);
                }
            ";

            int vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, vertexSrc);
            GL.CompileShader(vs);
            PrintShaderLog(vs, "VERTEX");

            int fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, fragmentSrc);
            GL.CompileShader(fs);
            PrintShaderLog(fs, "FRAGMENT");

            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, vs);
            GL.AttachShader(_shaderProgram, fs);
            GL.LinkProgram(_shaderProgram);
            PrintProgramLog(_shaderProgram);

            GL.DeleteShader(vs); GL.DeleteShader(fs);

            // uniforms
            _mvpLocation = GL.GetUniformLocation(_shaderProgram, "mvp");

            // load texture (expects "texture.png" in project folder)
            _texture = LoadTexture("texture.png");
            if (_texture == 0) Console.WriteLine("texture.png not found. Put a texture file named texture.png in the project folder.");

            // bind sampler to texture unit 0
            GL.UseProgram(_shaderProgram);
            GL.Uniform1(GL.GetUniformLocation(_shaderProgram, "tex"), 0);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _rotation += (float)args.Time;

            var model = Matrix4.CreateRotationY(_rotation) * Matrix4.CreateRotationX(_rotation * 0.4f);
            var view = Matrix4.LookAt(new Vector3(2.0f, 2.0f, 3.0f), Vector3.Zero, Vector3.UnitY);
            var proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size.X / (float)Size.Y, 0.1f, 100f);

            var mvp = model * view * proj;

            GL.UseProgram(_shaderProgram);
            GL.UniformMatrix4(_mvpLocation, false, ref mvp);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            GL.DeleteTexture(_texture);
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            GL.DeleteVertexArray(_vao);
            GL.DeleteProgram(_shaderProgram);
            base.OnUnload();
        }

        private static void PrintShaderLog(int shader, string name)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int ok);
            if (ok == (int)All.False)
            {
                var log = GL.GetShaderInfoLog(shader);
                Console.WriteLine($"[{name}] compile error:\n{log}");
            }
        }

        private static void PrintProgramLog(int prog)
        {
            GL.GetProgram(prog, GetProgramParameterName.LinkStatus, out int ok);
            if (ok == (int)All.False)
            {
                var log = GL.GetProgramInfoLog(prog);
                Console.WriteLine($"[PROGRAM] link error:\n{log}");
            }
        }

        private static int LoadTexture(string path)
        {
            if (!System.IO.File.Exists(path)) return 0;

            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            // parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // load with System.Drawing
            using (var bmp = new Bitmap(path))
            {
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY); // flip because OpenGL expects bottom-left origin

                var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                var data = bmp.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    data.Width,
                    data.Height,
                    0,
                    OpenTK.Graphics.OpenGL4.PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0);

                bmp.UnlockBits(data);
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            return id;
        }
    }

    internal static class Program
    {
        private static void Main()
        {
            var gws = GameWindowSettings.Default;
            var nws = new NativeWindowSettings
            {
                ClientSize = new Vector2i(900, 700),
                Title = "Assignment 4 - Texture-mapped Cube"
            };

            using var game = new Game(gws, nws);
            game.Run();
        }
    }
}
