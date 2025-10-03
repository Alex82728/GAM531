using System;

public class Game
{
    private float angle = 0f;

    // World coordinates: -2 to 2
    private float worldMinX = -2f, worldMaxX = 2f;
    private float worldMinY = -2f, worldMaxY = 2f;

    private float TX(float x, int screenWidth)
    {
        // Map world x (-2 → 2) to screen pixels (0 → screenWidth)
        return (x - worldMinX) / (worldMaxX - worldMinX) * screenWidth;
    }

    private float TY(float y, int screenHeight)
    {
        // Map world y (-2 → 2) to screen pixels (0 → screenHeight), invert y
        return screenHeight - ((y - worldMinY) / (worldMaxY - worldMinY) * screenHeight);
    }

    public void Tick(Surface screen)
    {
        screen.Clear(unchecked((int)0xFF000000)); // black background

        float halfSize = 1f; // square in world units
        float pulse = (float)(0.5 + 0.5 * Math.Sin(angle)); // pulsing size
        float size = halfSize * pulse;

        // Square corners in world coordinates
        float[] xs = { -size, size, size, -size };
        float[] ys = { -size, -size, size, size };

        // Draw rotated lines between corners
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

            // Draw white line
            screen.Line(sx0, sy0, sx1, sy1, unchecked((int)0xFFFFFFFF));
        }

        angle += 0.1f; // update rotation
    }
}
