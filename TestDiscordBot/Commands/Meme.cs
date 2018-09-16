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

        public Meme() : base("meme", "this works sometimes, which is better than not at all", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            try
            {
                // Resutls
                string ResultURL = "", ResultPicURL = "", ResultTitle = "", ResultTimestamp = "0", ResultPoints = "";
                string postJson;

                // Getting a subreddit
                switch (Global.RDM.Next(3))
                {
                    case 0:
                        url = "https://www.reddit.com/r/me_irl/";
                        break;

                    case 1:
                        url = "https://www.reddit.com/r/Animemes/";
                        break;

                    case 2:
                        url = "https://www.reddit.com/r/PrequelMemes/";
                        break;
                }

                // Getting a post
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url + ".json?limit=100");
                req.KeepAlive = false;
                WebResponse W = req.GetResponse();
                using (StreamReader sr = new StreamReader(W.GetResponseStream()))
                {
                    string html = sr.ReadToEnd();

                    List<int> Posts = html.AllIndexesOf("{\"kind\": \"t3\",");
                    string cuthtml = html.Remove(0, Posts[Posts.Count - Global.RDM.Next(100)]);
                    List<int> index = cuthtml.AllIndexesOf("}},");
                    postJson = cuthtml.Remove(index[1]);
                }

                // Looking into the json
                ResultURL = "https://www.reddit.com" + postJson.GetEverythingBetween("\"permalink\": \"", "\", ");
                ResultTitle = postJson.GetEverythingBetween("\"title\": \"", "\", ");
                ResultTimestamp = postJson.GetEverythingBetween("\"created\": ", ", ");
                ResultPoints = postJson.GetEverythingBetween("\"score\": ", ", ");
                ResultPicURL = postJson.GetEverythingBetween("\"images\": [{\"source\": {\"url\": \"", "\", ");

                EmbedBuilder Embed = new EmbedBuilder();

                Embed.WithUrl(ResultURL);
                Embed.WithTitle(ResultTitle);
                Embed.WithImageUrl(ResultPicURL);
                Embed.WithColor(0, 128, 255);
                if (ResultTimestamp != "0" && ResultTimestamp != "")
                {
                    DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dtDateTime = dtDateTime.AddSeconds(Convert.ToDouble(ResultTimestamp.Replace('.', ','))).AddHours(-10);
                    Embed.WithTimestamp(new DateTimeOffset(dtDateTime));
                }
                Embed.WithFooter(ResultPoints + (ResultPoints == "1" ? " fake internet point" : " fake internet points"));

                await Global.SendEmbed(Embed, commandmessage.Channel);

                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Send meme in " + commandmessage.Channel.Name + " for " + commandmessage.Author.Username);
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
