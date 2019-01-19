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
    public class Google : Command
    {
        public Google() : base("google", "Search stuff", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            string[] split = commandmessage.Content.Split(new char[] { ' ', '\n' });

            if (split.Length <= 1)
            {
                await Global.SendText("I need something to search!", commandmessage.Channel);
                return;
            }

            string hitUrl;
            string url = string.Format("http://www.google.com/search?q=" + WebUtility.UrlEncode(string.Join(" ", split.Skip(1).ToArray())) + "&btnI");
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.KeepAlive = false;
            req.AllowAutoRedirect = true;
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0";
            WebResponse W = req.GetResponse();
            using (StreamReader sr = new StreamReader(W.GetResponseStream()))
            {
                string html = sr.ReadToEnd();
                hitUrl = html.GetEverythingBetween("cite class", "/cite>").GetEverythingBetween(">", "<");
            }

            await Global.SendText(hitUrl, commandmessage.Channel);
        }
    }
}
