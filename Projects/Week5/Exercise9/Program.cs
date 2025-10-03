using System;
using OpenTK.Windowing.Desktop;

namespace Exercise9
{
    class Program
    {
        static void Main()
        {
            var gameSettings = new GameWindowSettings
            {
                RenderFrequency = 60.0,
                UpdateFrequency = 60.0
            };

            var nativeSettings = new NativeWindowSettings
            {
                Title = "Exercise 9 - Shaders Demo",
                Size = new OpenTK.Mathematics.Vector2i(800, 600)
            };

            using (var game = new Game(gameSettings, nativeSettings))
            {
                game.Run();
            }
        }
    }
}
