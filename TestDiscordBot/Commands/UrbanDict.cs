using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class UrbanDict : Command
    {
        public UrbanDict() : base("urbanDict", "Educate yourself.", false)
        {

        }

        public override async Task Execute(SocketMessage message)
        {
            string[] split = message.Content.Split(new char[] { ' ', '\n' });

            if (split.Length <= 1)
            {
                await Global.SendText("I need something to search!", message.Channel);
                return;
            }

            string url = string.Format("https://www.urbandictionary.com/define.php?term=" + WebUtility.UrlEncode(string.Join(" ", split.Skip(1).ToArray())));
            await Global.SendText(url, message.Channel);
        }
    }
}
