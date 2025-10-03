using System;
using System.Windows.Forms;
using System.Drawing;

public class Template : Form
{
    private PictureBox pb;
    private System.Windows.Forms.Timer timer; // explicitly WinForms Timer
    private Surface screen;
    private Game game;

    public Template(int w, int h)
    {
        Width = w;
        Height = h;
        Text = "Exercise 1 - Colorful Square";

        pb = new PictureBox();
        pb.Dock = DockStyle.Fill;
        Controls.Add(pb);

        screen = new Surface(w, h);
        game = new Game();

        timer = new System.Windows.Forms.Timer();
        timer.Interval = 16; // ~60 FPS
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e) // nullability fix
    {
        game.Tick(screen);
        pb.Image = screen.ToBitmap();
    }

    [STAThread]
    public static void RunApp()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Template(640, 480));
    }
}
