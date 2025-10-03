using OpenTK.Windowing.Desktop;

class Program
{
    static void Main()
    {
        var gws = GameWindowSettings.Default;
        var nws = new NativeWindowSettings()
        {
            Size = new OpenTK.Mathematics.Vector2i(800, 600),
            Title = "Exercise 8 - VBO Landscape"
        };

        using (var window = new TemplateGL(gws, nws))
        {
            window.Run();
        }
    }
}
