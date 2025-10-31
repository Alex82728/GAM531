
using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Game.GLAbstractions
{
    public class Mesh : IDisposable
    {
        private readonly float[] _vertices;
        private readonly uint[] _indices;

        private int _vao, _vbo, _ebo;

        public Vector3 ModelOffset;

        public Mesh(float[] vertices, uint[] indices, Vector3? modelOffset = null)
        {
            _vertices = vertices;
            _indices = indices;
            ModelOffset = modelOffset ?? Vector3.Zero;

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            int stride = 8 * sizeof(float);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            GL.BindVertexArray(0);
        }

        public void Draw()
        {
            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            GL.DeleteVertexArray(_vao);
        }

        public static Mesh CreateQuad(float size = 1f, float y = 0f)
        {
            float s = size * 0.5f;
            var verts = new float[]
            {
                -s, y, -s,   0,1,0,  0,0,
                 s, y, -s,   0,1,0,  1,0,
                 s, y,  s,   0,1,0,  1,1,
                -s, y,  s,   0,1,0,  0,1,
            };
            var idx = new uint[] { 0,1,2, 2,3,0 };
            return new Mesh(verts, idx);
        }

        public static Mesh CreateCube(float size = 1f, Vector3? center = null)
        {
            float s = size * 0.5f;
            var c = center ?? Vector3.Zero;

            float[] v = new float[] {
                // Front
                -s, -s,  s,  0,0,1,  0,0,
                 s, -s,  s,  0,0,1,  1,0,
                 s,  s,  s,  0,0,1,  1,1,
                -s,  s,  s,  0,0,1,  0,1,
                // Back
                 s, -s, -s,  0,0,-1, 0,0,
                -s, -s, -s,  0,0,-1, 1,0,
                -s,  s, -s,  0,0,-1, 1,1,
                 s,  s, -s,  0,0,-1, 0,1,
                // Left
                -s, -s, -s, -1,0,0,  0,0,
                -s, -s,  s, -1,0,0,  1,0,
                -s,  s,  s, -1,0,0,  1,1,
                -s,  s, -s, -1,0,0,  0,1,
                // Right
                 s, -s,  s,  1,0,0,  0,0,
                 s, -s, -s,  1,0,0,  1,0,
                 s,  s, -s,  1,0,0,  1,1,
                 s,  s,  s,  1,0,0,  0,1,
                // Top
                -s,  s,  s,  0,1,0,  0,0,
                 s,  s,  s,  0,1,0,  1,0,
                 s,  s, -s,  0,1,0,  1,1,
                -s,  s, -s,  0,1,0,  0,1,
                // Bottom
                -s, -s, -s,  0,-1,0, 0,0,
                 s, -s, -s,  0,-1,0, 1,0,
                 s, -s,  s,  0,-1,0, 1,1,
                -s, -s,  s,  0,-1,0, 0,1,
            };

            uint[] i = new uint[] {
                0,1,2, 2,3,0,
                4,5,6, 6,7,4,
                8,9,10, 10,11,8,
                12,13,14, 14,15,12,
                16,17,18, 18,19,16,
                20,21,22, 22,23,20
            };

            var m = new Mesh(v, i, c);
            return m;
        }

        public static Mesh CreatePyramid(float size = 1f, Vector3? baseCenter = null)
        {
            float s = size * 0.5f;
            var c = baseCenter ?? Vector3.Zero;

            Vector3 p0 = new(-s, 0, -s);
            Vector3 p1 = new( s, 0, -s);
            Vector3 p2 = new( s, 0,  s);
            Vector3 p3 = new(-s, 0,  s);
            Vector3 apex = new(0, size, 0);

            var verts = new System.Collections.Generic.List<float>();
            var indices = new System.Collections.Generic.List<uint>();
            uint baseIndex = 0;

            void Tri(Vector3 a, Vector3 b, Vector3 d, Vector2 t0, Vector2 t1, Vector2 t2)
            {
                var n = Vector3.Normalize(Vector3.Cross(b - a, d - a));
                verts.AddRange(new float[] { a.X, a.Y, a.Z, n.X, n.Y, n.Z, t0.X, t0.Y });
                verts.AddRange(new float[] { b.X, b.Y, b.Z, n.X, n.Y, n.Z, t1.X, t1.Y });
                verts.AddRange(new float[] { d.X, d.Y, d.Z, n.X, n.Y, n.Z, t2.X, t2.Y });
                indices.Add(baseIndex++);
                indices.Add(baseIndex++);
                indices.Add(baseIndex++);
            }

            // 4 sides
            Tri(p0, p1, apex, new(0,0), new(1,0), new(0.5f,1));
            Tri(p1, p2, apex, new(0,0), new(1,0), new(0.5f,1));
            Tri(p2, p3, apex, new(0,0), new(1,0), new(0.5f,1));
            Tri(p3, p0, apex, new(0,0), new(1,0), new(0.5f,1));

            // base
            Vector3 nUp = new(0,1,0);
            void Push(Vector3 p, Vector2 t) {
                verts.AddRange(new float[]{ p.X, p.Y, p.Z, nUp.X, nUp.Y, nUp.Z, t.X, t.Y });
                indices.Add(baseIndex++);
            }
            Push(p0, new(0,0)); Push(p1, new(1,0)); Push(p2, new(1,1));
            Push(p2, new(1,1)); Push(p3, new(0,1)); Push(p0, new(0,0));

            return new Mesh(verts.ToArray(), indices.ToArray(), c);
        }
    }
}
