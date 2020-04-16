using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;

namespace Starter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Config.Data.BotToken == "<INSERT BOT TOKEN HERE>")
            {
                ShowWindow(GetConsoleWindow(), 4);
                SystemSounds.Exclamation.Play();
                Console.Write("Give me a Bot Token: ");
                Config.Data.BotToken = Console.ReadLine();
                Config.Save();
            }

            for (int i = 0; i < 3; i++)
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
            Directory.SetCurrentDirectory(Directory.GetCurrentDirectory() + "\\MEE7-Discord-Bot");

#if DEBUG
            string runConfig = "Debug";
#else
            string runConfig = "Release";
#endif

            var startInfo = new ProcessStartInfo()
            {
                FileName = "dotnet",
                Arguments = "run -c " + runConfig,
                UseShellExecute = false,
                RedirectStandardInput = false,
            };
            startInfo.EnvironmentVariables.Add("BotToken", Config.Data.BotToken);
            Process.Start(startInfo);
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
