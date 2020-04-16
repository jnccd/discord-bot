using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System;
using System.Threading;

namespace MEE7.Commands
{
    public class Meme : Command
    {
        public static string[] Subreddits = new string[] { "https://www.reddit.com/r/anime_irl/", "https://www.reddit.com/r/Animemes/",
            "https://www.reddit.com/r/memes/", "https://www.reddit.com/r/PewdiepieSubmissions/",
            "https://www.reddit.com/r/marvelmemes/", "https://www.reddit.com/r/me_irl/", "https://www.reddit.com/r/OTMemes/",
            "https://www.reddit.com/r/MemeEconomy/", "https://www.reddit.com/r/fakehistoryporn/",
            "https://www.reddit.com/r/Overwatch_Memes/", "https://www.reddit.com/r/PrequelMemes/",
            "https://www.reddit.com/r/SequelMemes/",
            "https://www.reddit.com/r/starterpacks/", "https://www.reddit.com/r/memeframe/" };

        public Meme() : base("meme", "Posts a random meme", false)
        {
            HelpMenu = new EmbedBuilder();
            HelpMenu.WithColor(0, 128, 255);
            HelpMenu.WithDescription("This command will retrieve a meme from a number of subreddits, you can also specify " +
                "a subreddit yourself by adding it as an argument, eg. `$meme me_irl`");
        }

        public override void Execute(SocketMessage commandmessage)
        {
            // Getting a subreddit
            bool worked = false;
            string url = "";

            if (commandmessage.Content.Split(new char[] { ' ', '\n' }).Length > 1)
            {
                url = "https://www.reddit.com/r/" + commandmessage.Content.Split(new char[] { ' ', '\n' })[1] + "/";
                if (!RedditHelper.IsReachable(url))
                {
                    DiscordNETWrapper.SendText("Thats not a valid subreddit!", commandmessage.Channel).Wait();
                    return;
                }
            }
            else
                url = Subreddits[Program.RDM.Next(Subreddits.Length)];

            Thread.CurrentThread.Name = "kek";
            while (!worked)
            {
                try
                {
                    string postJson = RedditHelper.GetPostJsonFromSubreddit(url);
                    RedditHelper.SendPostJsonToDiscordChannel(postJson, url, commandmessage.Channel, commandmessage.Author).Wait();
                    worked = true;
                }
                catch (Exception e)
                {
                    Console.CursorLeft = 0;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("$");

                    if (e.ToString().Contains("(404) Nicht gefunden"))
                        break;
                }
            }
        }
    }
}
