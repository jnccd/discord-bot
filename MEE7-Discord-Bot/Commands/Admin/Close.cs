using Discord;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System;
using System.Threading.Tasks;

namespace MEE7.Commands
{
    public class Close : Command
    {
        public Close() : base("close", "Closes the bot", false, true)
        {
            Task.Run(() =>
            {
                DateTime lastConnected = DateTime.Now;
                while (true)
                {
                    Task.Delay(1000).Wait();
                    if (Program.Client.ConnectionState == ConnectionState.Connected)
                    {
                        lastConnected = DateTime.Now;
                    }
                    if (DateTime.Now - lastConnected > TimeSpan.FromMinutes(5))
                    {
                        ConsoleWrapper.WriteLineAndDiscordLog("Closing due to long disconnect...");
                        Program.Exit(0);
                    }
                }
            });
        }

        public override void Execute(IMessage message)
        {
            if (message.Author.Id == Program.Master.Id)
            {
                DiscordNETWrapper.SendText("Closing...", message.Channel).Wait();
                Program.Exit(0);
            }
        }
    }
}
