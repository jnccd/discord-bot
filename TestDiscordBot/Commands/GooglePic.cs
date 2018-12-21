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
    public class GooglePic : Command
    {
        public GooglePic() : base("picture", "Get a picture from a string", false, true)
        {

        }

        public override async Task execute(SocketMessage message)
        {
            string[] split = message.Content.Split(new char[] { ' ', '\n' });

            if (split.Length <= 1)
            {
                await Global.SendText("I need something to search!", message.Channel);
                return;
            }

            string url = string.Format("https://www.google.de/search?hs=238&source=lnms&tbm=isch&sa=X&q=" + WebUtility.UrlEncode(string.Join(" ", split.Skip(1).ToArray())));
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.KeepAlive = false;
            WebResponse W = req.GetResponse();
            string picURL = "oof";
            using (StreamReader sr = new StreamReader(W.GetResponseStream()))
            {
                string html = sr.ReadToEnd();

                picURL = html.GetEverythingBetween("src=\"", "\" ");
            }

            await Global.SendText(picURL, message.Channel);
        }
    }
}
