using System;

public class Game
{
    private float angle = 0f;

    // World range and center
    public float worldMinX = -10f, worldMaxX = 10f;
    public float worldMinY = -10f, worldMaxY = 10f;
    public float centerX = 0f, centerY = 0f;

    // Convert world X to screen pixel
    private float TX(float x, int screenWidth)
    {
        float worldWidth = worldMaxX - worldMinX;
        float normalizedX = (x - (centerX - worldWidth / 2)) / worldWidth;
        return normalizedX * screenWidth;
    }

    // Convert world Y to screen pixel (inverted)
    private float TY(float y, int screenHeight)
    {
        float worldHeight = worldMaxY - worldMinY;
        float normalizedY = (y - (centerY - worldHeight / 2)) / worldHeight;
        return screenHeight - (normalizedY * screenHeight);
    }

    public void Tick(Surface screen)
    {
        screen.Clear(unchecked((int)0xFF000000)); // black background

        float halfSize = 2f; // square in world units
        float pulse = (float)(0.5 + 0.5 * Math.Sin(angle)); // pulsing effect
        float size = halfSize * pulse;

        // Square corners in world coordinates
        float[] xs = { -size, size, size, -size };
        float[] ys = { -size, -size, size, size };

        // Draw rotated square
        for (int i = 0; i < 4; i++)
        {
            int next = (i + 1) % 4;

            float rx0 = xs[i] * (float)Math.Cos(angle) - ys[i] * (float)Math.Sin(angle);
            float ry0 = xs[i] * (float)Math.Sin(angle) + ys[i] * (float)Math.Cos(angle);

            float rx1 = xs[next] * (float)Math.Cos(angle) - ys[next] * (float)Math.Sin(angle);
            float ry1 = xs[next] * (float)Math.Sin(angle) + ys[next] * (float)Math.Cos(angle);

            int sx0 = (int)TX(rx0, screen.width);
            int sy0 = (int)TY(ry0, screen.height);
            int sx1 = (int)TX(rx1, screen.width);
            int sy1 = (int)TY(ry1, screen.height);

            screen.Line(sx0, sy0, sx1, sy1, unchecked((int)0xFFFFFFFF)); // white line
        }

        angle += 0.1f;
    }
}
