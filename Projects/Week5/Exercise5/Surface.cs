using System;
using System.Drawing;
using System.Drawing.Imaging;

public class Surface
{
    public int width, height;
    public int[] pixels;
    private Bitmap bitmap;

    public Surface(int w, int h)
    {
        width = w;
        height = h;
        pixels = new int[width * height];
        bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
    }

    public void Clear(int color)
    {
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
    }

    public void SetPixel(int x, int y, int color)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        pixels[x + y * width] = color;
    }

    public void Line(int x0, int y0, int x1, int y1, int color)
    {
        int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2;

        while (true)
        {
            SetPixel(x0, y0, color);
            if (x0 == x1 && y0 == y1) break;
            e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    // Simple text rendering placeholder
    public void Print(int x, int y, string text, int color)
    {
        for (int i = 0; i < text.Length; i++)
        {
            SetPixel(x + i * 6, y, color); // crude horizontal line per char
            SetPixel(x + i * 6, y + 1, color);
        }
    }

    public Bitmap ToBitmap()
    {
        BitmapData data = bitmap.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.WriteOnly,
            PixelFormat.Format32bppArgb
        );

        System.Runtime.InteropServices.Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
        bitmap.UnlockBits(data);
        return bitmap;
    }
}
