using OpenTK.Graphics.OpenGL;

public class Game
{
    public float[,] h = new float[128, 128];
    public int size = 128;
    public float[] vertexData; // Vertex buffer array
    public int vertexCount;
    public int VBO;

    public void Init(string path)
    {
        // Load heightmap
        var map = new System.Drawing.Bitmap(path);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var color = map.GetPixel(x, y);
                h[x, y] = color.R / 255f;
            }
        }

        // Prepare vertex array for all quads as triangles
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

        System.Console.WriteLine("Vertex count: " + vertexCount);
    }
}
