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
        public Google() : base("google", "Ay fam google that shit", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            string[] split = commandmessage.Content.Split(' ');

            if (split.Length <= 1)
            {
                await Global.SendText("I need something to search!", commandmessage.Channel);
                return;
            }

            string url = string.Format("http://www.google.com/search?q=" + split[1] + "&btnI");
            url = url.Replace(' ', '+');
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.KeepAlive = false;
            req.AllowAutoRedirect = true;
            req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36 OPR/56.0.3051.52";
            Uri U = req.GetResponse().ResponseUri;

            await Global.SendText(U.AbsoluteUri, commandmessage.Channel);
        }
    }
}
