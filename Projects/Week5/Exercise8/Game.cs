using System;
using System.Drawing; // Bitmap, Color
using OpenTK.Graphics.OpenGL;

public class Game
{
    public float[,] h = new float[128, 128];
    public int size = 128;
    public float[] vertexData = Array.Empty<float>();
    public int vertexCount;
    public int VBO;
    public float rotation = 0f;

    public void Init(string path)
    {
        // Load heightmap
        Bitmap map = new Bitmap(path);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Color color = map.GetPixel(x, y);
                h[x, y] = color.R / 255f; // grayscale height
            }
        }

        // Prepare vertex array
        vertexData = new float[(size - 1) * (size - 1) * 2 * 3 * 3]; // 2 triangles per quad, 3 vertices each, x/y/z
        int index = 0;
        float scale = 0.05f;

        for (int y = 0; y < size - 1; y++)
        {
            for (int x = 0; x < size - 1; x++)
            {
                float h00 = h[x, y];
                float h10 = h[x + 1, y];
                float h01 = h[x, y + 1];
                float h11 = h[x + 1, y + 1];

                // Triangle 1
                vertexData[index++] = (x - size / 2) * scale;
                vertexData[index++] = h00;
                vertexData[index++] = (y - size / 2) * scale;

                vertexData[index++] = (x + 1 - size / 2) * scale;
                vertexData[index++] = h10;
                vertexData[index++] = (y - size / 2) * scale;

                vertexData[index++] = (x - size / 2) * scale;
                vertexData[index++] = h01;
                vertexData[index++] = (y + 1 - size / 2) * scale;

                // Triangle 2
                vertexData[index++] = (x + 1 - size / 2) * scale;
                vertexData[index++] = h10;
                vertexData[index++] = (y - size / 2) * scale;

                vertexData[index++] = (x + 1 - size / 2) * scale;
                vertexData[index++] = h11;
                vertexData[index++] = (y + 1 - size / 2) * scale;

                vertexData[index++] = (x - size / 2) * scale;
                vertexData[index++] = h01;
                vertexData[index++] = (y + 1 - size / 2) * scale;
            }
        }

        vertexCount = vertexData.Length / 3;

        // Generate VBO
        VBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        Console.WriteLine("Vertex count: " + vertexCount);
    }

    public void Render()
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Modelview);
        GL.LoadIdentity();
        GL.Translate(0f, -0.5f, -3f);
        GL.Rotate(rotation, 0f, 1f, 0f);

        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
        GL.EnableClientState(OpenTK.Graphics.OpenGL.ArrayCap.VertexArray);
        GL.VertexPointer(3, OpenTK.Graphics.OpenGL.VertexPointerType.Float, 0, 0);

        // Draw all triangles in one call
        GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);

        GL.DisableClientState(OpenTK.Graphics.OpenGL.ArrayCap.VertexArray);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        rotation += 0.5f; // rotate each frame
    }
}
