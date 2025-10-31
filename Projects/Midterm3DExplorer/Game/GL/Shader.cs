using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Game.GLAbstractions
{
    public class Shader : IDisposable
    {
        public int Handle { get; private set; }

        public Shader(string vertexPath, string fragmentPath)
        {
            string vertexSource = File.ReadAllText(vertexPath);
            string fragmentSource = File.ReadAllText(fragmentPath);

            int v = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(v, vertexSource);
            GL.CompileShader(v);
            GL.GetShader(v, ShaderParameter.CompileStatus, out int vs);
            if (vs == 0) throw new Exception("Vertex: " + GL.GetShaderInfoLog(v));

            int f = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(f, fragmentSource);
            GL.CompileShader(f);
            GL.GetShader(f, ShaderParameter.CompileStatus, out int fs);
            if (fs == 0) throw new Exception("Fragment: " + GL.GetShaderInfoLog(f));

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, v);
            GL.AttachShader(Handle, f);
            GL.LinkProgram(Handle);
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int link);
            if (link == 0) throw new Exception("Link: " + GL.GetProgramInfoLog(Handle));

            GL.DetachShader(Handle, v);
            GL.DetachShader(Handle, f);
            GL.DeleteShader(v);
            GL.DeleteShader(f);
        }

        public void Use() => GL.UseProgram(Handle);
        public void Dispose() => GL.DeleteProgram(Handle);

        public void SetInt(string name, int v) => GL.Uniform1(GetLoc(name), v);
        public void SetFloat(string name, float v) => GL.Uniform1(GetLoc(name), v);
        public void SetVector3(string name, Vector3 v) => GL.Uniform3(GetLoc(name), v);
        public void SetMatrix4(string name, Matrix4 m) => GL.UniformMatrix4(GetLoc(name), false, ref m);

        private int GetLoc(string name) => GL.GetUniformLocation(Handle, name);
    }
}
