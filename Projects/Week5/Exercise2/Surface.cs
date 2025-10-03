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
