using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class SourceCode : Command
    {
        public SourceCode() : base("sourceCode", "Sends a link to my source code.", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            try
            {
                await Global.SendText("https://github.com/niklasCarstensen/Discord-Bot", commandmessage.Channel);

                Console.CursorLeft = 0;
                Console.WriteLine("Source Code in " + commandmessage.Channel.Name + " for " + commandmessage.Author.Username);
                Console.Write("$");
            }
            catch (Exception e)
            {
                await Global.SendText("Uwu We made a fucky wucky!! A wittle fucko boingo! The code monkeys at our headquarters are working VEWY HAWD to fix this!", commandmessage.Channel);

                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("$");
            }
        }
    }
}
