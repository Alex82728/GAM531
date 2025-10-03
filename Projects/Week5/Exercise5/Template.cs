using System;
using System.Windows.Forms;
using System.Drawing;

public class Template : Form
{
    private PictureBox pb;
    private System.Windows.Forms.Timer timer;
    private Surface screen;
    private Game game;

    public Template(int w, int h)
    {
        Width = w;
        Height = h;
        Text = "Exercise 5 - Interactive Function Plotter";

        pb = new PictureBox();
        pb.Dock = DockStyle.Fill;
        Controls.Add(pb);

        screen = new Surface(w, h);
        game = new Game();

        this.KeyDown += Template_KeyDown;

        timer = new System.Windows.Forms.Timer();
        timer.Interval = 16; // ~60 FPS
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        game.Tick(screen);
        pb.Image = screen.ToBitmap();
    }

    private void Template_KeyDown(object? sender, KeyEventArgs e)
    {
        float panAmountX = (game.worldMaxX - game.worldMinX) / 20f;
        float panAmountY = (game.worldMaxY - game.worldMinY) / 20f;

        if (e.KeyCode == Keys.Left) game.Pan(-panAmountX, 0);
        if (e.KeyCode == Keys.Right) game.Pan(panAmountX, 0);
        if (e.KeyCode == Keys.Up) game.Pan(0, panAmountY);
        if (e.KeyCode == Keys.Down) game.Pan(0, -panAmountY);
        if (e.KeyCode == Keys.Z) game.Zoom(1.1f);  // zoom in
        if (e.KeyCode == Keys.X) game.Zoom(0.9f);  // zoom out
    }

    [STAThread]
    public static void RunApp()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Template(800, 600));
    }
}
