using System;

public class Game
{
    private int frameCount = 0; // used for animating blue tint

    // Helper to create ARGB color
    private int CreateColor(int r, int g, int b)
    {
        // Full alpha (opaque)
        return unchecked((int)((255 << 24) | (r << 16) | (g << 8) | b));
    }

    public void Tick(Surface screen)
    {
        frameCount++; // increment frame counter

        // Clear background
        screen.Clear(unchecked((int)0xFFFFFFFF));

        int width = screen.width;
        int height = screen.height;

        // Compute blue tint that fades with sine
        int blueTint = (int)((Math.Sin(frameCount * 0.05) + 1) / 2 * 255); // 0–255

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int red = (x * 255) / width;    // x → red
                int green = (y * 255) / height; // y → green
                int color = CreateColor(red, green, blueTint);
                screen.pixels[x + y * width] = color;
            }
        }
    }
}
