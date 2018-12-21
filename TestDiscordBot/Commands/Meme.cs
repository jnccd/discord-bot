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
    public class Meme : Command
    {
        string url = "";
        public static string[] Subreddits = new string[] { "https://www.reddit.com/r/anime_irl/", "https://www.reddit.com/r/Animemes/",
            "https://www.reddit.com/r/memes/", "https://www.reddit.com/r/PewdiepieSubmissions/",
            "https://www.reddit.com/r/marvelmemes/", "https://www.reddit.com/r/me_irl/", "https://www.reddit.com/r/OTMemes/",
            "https://www.reddit.com/r/MemeEconomy/", "https://www.reddit.com/r/fakehistoryporn/",
            "https://www.reddit.com/r/Overwatch_Memes/", "https://www.reddit.com/r/PrequelMemes/", "https://www.reddit.com/r/greentext/",
            "https://www.reddit.com/r/pyrocynical/", "https://www.reddit.com/r/SequelMemes/",
            "https://www.reddit.com/r/starterpacks/", "https://www.reddit.com/r/memeframe/" };

        public Meme() : base("meme", "Posts a random meme", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            // Getting a subreddit
            bool worked = false;

            if (commandmessage.Content.Split(new char[] { ' ', '\n' }).Length > 1)
            {
                url = "https://www.reddit.com/r/" + commandmessage.Content.Split(new char[] { ' ', '\n' })[1] + "/";
                if (!RedditHelper.IsReachable(url))
                {
                    await Global.SendText("Thats not a valid subreddit!", commandmessage.Channel);
                    return;
                }
            }
            else
                url = Subreddits[Global.RDM.Next(Subreddits.Length)];

            while (!worked)
            {
                try
                {
                    string postJson = RedditHelper.GetPostJsonFromSubreddit(url);
                    await RedditHelper.SendPostJsonToDiscordChannel(postJson, url, commandmessage.Channel, commandmessage.Author);
                    worked = true;
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
}
