using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

namespace Assignment6
{
    internal static class Program
    {
        static void Main()
        {
            var nativeSettings = new NativeWindowSettings()
            {
                Title = "Assignment 6 - FPS Camera",
                ClientSize = new Vector2i(800, 600),
            };

            using var window = new Game(GameWindowSettings.Default, nativeSettings);
            window.Run();
        }
    }
}
