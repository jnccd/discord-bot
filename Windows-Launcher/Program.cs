using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace Starter
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    if (Config.Data.BotToken == "<INSERT BOT TOKEN HERE>")
                    {
                        ShowWindow(GetConsoleWindow(), 4);
                        //SystemSounds.Exclamation.Play(); TODO: Support for SystemSounds is planned for .Net Core 3.0
                        ConsoleWrapper.ConsoleWrite("Give me a Bot Token: ");
                        Config.Data.BotToken = Console.ReadLine();
                        Config.Save();
                    }

                    client.LoginAsync(TokenType.Bot, Config.Data.BotToken).Wait();
                    client.StartAsync().Wait();

                    gotWorkingToken = true;
                }
                catch { Config.Data.BotToken = "<INSERT BOT TOKEN HERE>"; }
            }


            for (int i = 0; i < 3; i++)
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
            Directory.SetCurrentDirectory(Directory.GetCurrentDirectory() + "\\MEE7-Discord-Bot");

#if DEBUG
            string runConfig = "Debug";
#else
            string runConfig = "Release";
#endif

            var envVars = new StringDictionary();
            envVars.Add("BotToken", Config.Data.BotToken);
            Process.Start(new ProcessStartInfo()
            {
                FileName = "dotnet",
                Arguments = "run -c " + runConfig,
                UseShellExecute = false,
                RedirectStandardInput = false,
                EnvironmentVariables = envVars,
            });
        }
    }
}
