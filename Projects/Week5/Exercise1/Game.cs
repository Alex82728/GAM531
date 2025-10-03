using System;

public class Game
{
    public void Tick(Surface screen)
    {
        // Clear background to white
        screen.Clear(unchecked((int)0xFFFFFFFF)); // ARGB: white

        int squareSize = 300;
        int startX = (screen.width - squareSize) / 2;
        int startY = (screen.height - squareSize) / 2;

        for (int x = 0; x < squareSize; x++)
        {
            // Blue gradient 0 → 255
            int blue = (x * 255) / squareSize;

            // ARGB color (cast to int to avoid overflow issues)
            int color = unchecked((int)((255 << 24) | blue));

            for (int y = 0; y < squareSize; y++)
            {
                int px = startX + x;
                int py = startY + y;
                if (px < screen.width && py < screen.height)
                    screen.pixels[px + py * screen.width] = color;
            }
        }
    }
}
