using System;
using Lab2;
using OpenTK.Windowing.Desktop;

namespace Lab2
{
    internal static class Program
    {
        public static void Main()
        {
            var gameWindowSettings = GameWindowSettings.Default;
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new OpenTK.Mathematics.Vector2i(1280, 720),
                Title = "Billiards Game",
            };

            using (var game = new Game(gameWindowSettings, nativeWindowSettings))
            {
                game.Run();
            }
        }
    }
}
