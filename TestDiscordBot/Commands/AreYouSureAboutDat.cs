using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class AreYouSureAboutDat : Command
    {
        public AreYouSureAboutDat() : base("usure", "Send if some people are too sure about themselves.", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            await Global.SendText("https://cdn.discordapp.com/attachments/322690053870714883/501812717901053962/sure2smol.gif", commandmessage.Channel);
        }
    }
}