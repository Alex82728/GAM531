using System;

public class Game
{
    public float worldMinX = -10f, worldMaxX = 10f;
    public float worldMinY = -2f, worldMaxY = 2f;
    public float centerX = 0f, centerY = 0f;

    public float zoomFactorX = 1f;
    public float zoomFactorY = 1f;

    // Convert world X to screen pixels
    private float TX(float x, int screenWidth)
    {
        float worldWidth = (worldMaxX - worldMinX) / zoomFactorX;
        float normalizedX = (x - (centerX - worldWidth / 2)) / worldWidth;
        return normalizedX * screenWidth;
    }

    // Convert world Y to screen pixels (invert y)
    private float TY(float y, int screenHeight)
    {
        float worldHeight = (worldMaxY - worldMinY) / zoomFactorY;
        float normalizedY = (y - (centerY - worldHeight / 2)) / worldHeight;
        return screenHeight - (normalizedY * screenHeight);
    }

    public void Tick(Surface screen)
    {
        screen.Clear(unchecked((int)0xFF000000));

        int xAxis = (int)TY(0f, screen.height);
        int yAxis = (int)TX(0f, screen.width);

        // Draw axes
        screen.Line(0, xAxis, screen.width - 1, xAxis, unchecked((int)0xFFFFFFFF)); // X-axis
        screen.Line(yAxis, 0, yAxis, screen.height - 1, unchecked((int)0xFFFFFFFF)); // Y-axis

        // Draw axis labels
        screen.Print(5, xAxis + 5, "0", unchecked((int)0xFFFFFFFF));
        screen.Print(screen.width - 30, xAxis + 5, "X", unchecked((int)0xFFFFFFFF));
        screen.Print(yAxis + 5, 5, "Y", unchecked((int)0xFFFFFFFF));

        // Plot y = sin(x)
        int prevX = 0, prevY = 0;
        bool first = true;
        for (int px = 0; px < screen.width; px++)
        {
            float worldX = centerX - (worldMaxX - worldMinX) / (2 * zoomFactorX) + ((float)px / screen.width) * ((worldMaxX - worldMinX) / zoomFactorX);
            float worldY = (float)Math.Sin(worldX);

            int py = (int)TY(worldY, screen.height);

            if (!first)
            {
                screen.Line(prevX, prevY, px, py, unchecked((int)0xFFFF0000));
            }

            prevX = px;
            prevY = py;
            first = false;
        }
    }

    public void Zoom(float factor)
    {
        zoomFactorX *= factor;
        zoomFactorY *= factor;
    }

    public void Pan(float dx, float dy)
    {
        centerX += dx;
        centerY += dy;
    }
}
