using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class aaaTemplate : Command
    {
        public aaaTemplate() : base("", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            try
            {
                await Global.SendText("no u", commandmessage.Channel);

                Console.CursorLeft = 0;
                Console.WriteLine("Literally nothing in " + commandmessage.Channel.Name + " for " + commandmessage.Author.Username);
                Console.Write("$");
            }
            catch (Exception e)
            {
                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("$");
            }
        }
    }
}
