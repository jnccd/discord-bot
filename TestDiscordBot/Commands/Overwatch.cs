using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class Overwatch : Command
    {
        string url = "";

        public Overwatch() : base("overwatch", "gets a post from /r/overwatch", true)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            try
            {
                url = "https://www.reddit.com/r/Overwatch/";
                string postJson = Reddit.GetPostJsonFromSubreddit(url);
                await Reddit.SendPostJsonToDiscordChannel(postJson, url, commandmessage.Channel, commandmessage.Author);

                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Send overwatch post in " + commandmessage.Channel.Name + " for " + commandmessage.Author.Username);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("$");
            }
            catch (Exception e)
            {
                await Global.SendText("Uwu We made a fucky wucky!! A wittle fucko boingo! The code monkeys at our headquarters are working VEWY HAWD to fix this!", commandmessage.Channel);

                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e + "\n\nOn: " + url);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("$");
            }
        }
    }
}
