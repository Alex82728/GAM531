using OpenTK.Graphics.OpenGL;

public class Game
{
    public float[,] h = new float[128, 128];
    public int size = 128;
    public float rotation = 0f;

    public void Init(string path)
    {
        var map = new System.Drawing.Bitmap(path);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var color = map.GetPixel(x, y);
                // Normalize to 0-1
                h[x, y] = color.R / 255f;
            }
        }
    }

    public void Render()
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Modelview);
        GL.LoadIdentity();

        // Move camera back
        GL.Translate(0f, -0.5f, -3f);
        // Rotate landscape
        GL.Rotate(rotation, 0f, 1f, 0f);

        float scale = 0.05f; // spacing between points

        // Draw triangles for each quad
        for (int y = 0; y < size - 1; y++)
        {
            for (int x = 0; x < size - 1; x++)
            {
                float h00 = h[x, y];
                float h10 = h[x + 1, y];
                float h01 = h[x, y + 1];
                float h11 = h[x + 1, y + 1];

                GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles);

                // Triangle 1
                GL.Color3(h00 * 5, h00 * 5, h00 * 5); // scale color for visibility
                GL.Vertex3((x - size / 2) * scale, h00, (y - size / 2) * scale);

                GL.Color3(h10 * 5, h10 * 5, h10 * 5);
                GL.Vertex3((x + 1 - size / 2) * scale, h10, (y - size / 2) * scale);

                GL.Color3(h01 * 5, h01 * 5, h01 * 5);
                GL.Vertex3((x - size / 2) * scale, h01, (y + 1 - size / 2) * scale);

                // Triangle 2
                GL.Color3(h10 * 5, h10 * 5, h10 * 5);
                GL.Vertex3((x + 1 - size / 2) * scale, h10, (y - size / 2) * scale);

                GL.Color3(h11 * 5, h11 * 5, h11 * 5);
                GL.Vertex3((x + 1 - size / 2) * scale, h11, (y + 1 - size / 2) * scale);

                GL.Color3(h01 * 5, h01 * 5, h01 * 5);
                GL.Vertex3((x - size / 2) * scale, h01, (y + 1 - size / 2) * scale);

                GL.End();
            }
        }

        rotation += 0.5f; // rotate each frame
    }
}
