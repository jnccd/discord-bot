using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MEE7.Commands
{
    public class Google : Command
    {
        public Google() : base("google", "Search stuff", false)
        {

        }

        public override async Task Execute(SocketMessage message)
        {
            string[] split = message.Content.Split(new char[] { ' ', '\n' });
            EmbedBuilder embed = new EmbedBuilder();

            if (split.Length <= 1)
            {
                await Program.SendText("I need something to search!", message.Channel);
                return;
            }
            
            string url = string.Format("http://www.google.com/search?q=" + WebUtility.UrlEncode(string.Join(" ", split.Skip(1).ToArray())) + "&btnI");
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.KeepAlive = false;
            req.AllowAutoRedirect = true;
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0";
            WebResponse W = req.GetResponse();
            using (StreamReader sr = new StreamReader(W.GetResponseStream()))
            {
                string html = sr.ReadToEnd();
                Tuple<string, string>[] hits = html.GetEverythingBetweenAll("<p>&bull;&nbsp;<a href=\"", "/p>").
                    Select(x => new Tuple<string, string>("https://" + x.GetEverythingBetween("https://", "\" target=\""),
                                                 WebUtility.HtmlDecode(x.GetEverythingBetween("</a> ", "<")))).ToArray();
                foreach (Tuple<string, string> hit in hits)
                    embed.AddField(hit.Item1, hit.Item2);
            }
            
            await Program.SendEmbed(embed, message.Channel);
        }
    }
}
