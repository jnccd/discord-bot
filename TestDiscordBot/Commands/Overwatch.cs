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
        public Overwatch() : base("overwatch", true)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            try
            {
                // Resutls
                string ResultURL = "", ResultPicURL = "", ResultTitle = "", ResultHoursAgo = "0", ResultPoints = "";
                string postLink;

                // Getting a subreddit
                string url = "https://www.reddit.com/r/Overwatch/";

                // Getting a post
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                //req.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
                req.Headers.Add(HttpRequestHeader.AcceptLanguage, "de-DE,de;q=0.9,en-US;q=0.8,en;q=0.7");
                req.Referer = "https://www.google.de/";
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36 OPR/55.0.2994.44";
                req.KeepAlive = true;
                WebResponse W = req.GetResponse();
                using (StreamReader sr = new StreamReader(W.GetResponseStream()))
                {
                    string html = sr.ReadToEnd();

                    html = html.Remove(0, html.AllIndexesOf("</div><!-- SC_ON --></div></div>").Last());
                    html = html.Remove(html.IndexOf("Community Details"));

                    List<int> Posts = html.AllIndexesOf("href=\"" + url + "comments");
                    string cuthtml = html.Remove(0, Posts[Global.RDM.Next(Posts.Count)]);
                    List<int> index = cuthtml.AllIndexesOf("\"");
                    postLink = cuthtml.Remove(index[1]);
                }

                // Looking into a post
                url = postLink.Remove(0, 5).Trim('"');
                req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.KeepAlive = false;
                W = req.GetResponse();
                using (StreamReader sr = new StreamReader(W.GetResponseStream()))
                {
                    string html = sr.ReadToEnd();

                    ResultPicURL = html.Remove(0, html.IndexOf("href=\"https://i.redd.it") + "href=\"".Length);
                    ResultPicURL = ResultPicURL.Remove(ResultPicURL.IndexOf("\"")).Trim('"');
                    Uri output = null;
                    if (Uri.TryCreate("", UriKind.Absolute, out output))
                    {
                        ResultPicURL = output.AbsoluteUri;
                    }
                    else
                    {
                        Console.CursorLeft = 0;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Stupid discord cant handle normal image links: " + ResultPicURL);
                        Console.ForegroundColor = ConsoleColor.White;
                        await execute(commandmessage);
                        return;
                    }

                    ResultPoints = html.Remove(0, html.IndexOf("style=\"color:#1A1A1B\">") + "style=\"color:#1A1A1B\">".Length);
                    ResultPoints = ResultPoints.Remove(ResultPoints.IndexOf("<"));

                    ResultTitle = html.Remove(0, html.IndexOf(":{\"title\":\"") + ":{\"title\":\"".Length);
                    ResultTitle = ResultTitle.Remove(ResultTitle.IndexOf(":"));

                    if (html.Contains("hours ago</a>"))
                    {
                        ResultHoursAgo = html.Remove(0, html.IndexOf("target=\"_blank\" rel=\"nofollow noopener\">") + "target=\"_blank\" rel=\"nofollow noopener\">".Length);
                        ResultHoursAgo = ResultHoursAgo.Remove(ResultHoursAgo.IndexOf(" hours ago</a>"));
                    }

                    ResultURL = url;
                }

                EmbedBuilder Embed = new EmbedBuilder();

                Embed.WithUrl(ResultURL);
                Embed.WithTitle(ResultTitle);
                Embed.WithImageUrl(ResultPicURL);
                Embed.WithColor(0, 128, 255);
                if (ResultHoursAgo != "0")
                {
                    DateTime postDate = DateTime.Now.AddHours(-Convert.ToInt32(ResultHoursAgo)).AddMinutes(-DateTime.Now.Minute);
                    Embed.WithTimestamp(postDate);
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
                await Global.SendText("Uwu We made a fucky wucky!! A wittle fucko boingo! The code monkey at our headquarters is working VEWY HAWD to fix this!", commandmessage.Channel);

                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("$");
            }
        }
    }
}
