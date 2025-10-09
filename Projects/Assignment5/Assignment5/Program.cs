using System;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

namespace Assignment5
{
    internal static class Program
    {
        static void Main()
        {
            var settings = GameWindowSettings.Default;
            var native = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "Phong Lighting - Cube",
                Flags = OpenTK.Windowing.Common.ContextFlags.ForwardCompatible
            };

            using (var game = new Game(settings, native))
            {
                game.Run();
            }
        }
    }
}
