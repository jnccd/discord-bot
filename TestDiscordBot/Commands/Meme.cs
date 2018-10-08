using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class Meme : Command
    {
        string url = "";
        public static string[] Subreddits = new string[] { "https://www.reddit.com/r/anime_irl/", "https://www.reddit.com/r/Animemes/",
            "https://www.reddit.com/r/hmmm/", "https://www.reddit.com/r/memes/", "https://www.reddit.com/r/PewdiepieSubmissions/",
            "https://www.reddit.com/r/marvelmemes/", "https://www.reddit.com/r/me_irl/", "https://www.reddit.com/r/OTMemes/",
            "https://www.reddit.com/r/MemeEconomy/", "https://www.reddit.com/r/fakehistoryporn/",
            "https://www.reddit.com/r/Overwatch_Memes/", "https://www.reddit.com/r/PrequelMemes/", "https://www.reddit.com/r/greentext/",
            "https://www.reddit.com/r/pyrocynical/", "https://www.reddit.com/r/SequelMemes/",
            "https://www.reddit.com/r/starterpacks/", "https://www.reddit.com/r/memeframe/" };

        public Meme() : base("meme", "posts a random meme from a random subreddit, crashes sometimes cuz its succing its memes from many different sources, might send some weird ass shit so be advised", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            try
            {
                // Getting a subreddit
                url = Subreddits[Global.RDM.Next(Subreddits.Length)];
                bool worked = false;

                while (!worked)
                {
                    try
                    {
                        string postJson = Reddit.GetPostJsonFromSubreddit(url);
                        await Reddit.SendPostJsonToDiscordChannel(postJson, url, commandmessage.Channel, commandmessage.Author);
                        worked = true;
                    } catch (Exception e) {
                        Console.CursorLeft = 0;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
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
