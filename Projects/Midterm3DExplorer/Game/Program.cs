using System;
using Game;

namespace GameEntry
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using var game = new ExplorerGame(1280, 720, "Mini 3D Explorer — Pro");
            game.Run();
        }
    }
}
