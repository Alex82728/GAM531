using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL;

public class TemplateGL : GameWindow
{
    private Game game;

    public TemplateGL(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws)
    {
        game = new Game();
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1f);
        GL.Enable(EnableCap.DepthTest);
        game.Init("heightmap.png");
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Projection);
        GL.LoadIdentity();
        GL.Ortho(-2, 2, -1, 1, 0.1, 10);
        GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Modelview);

        game.Render();

        SwapBuffers();
    }
}
