using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class SourceCode : Command
    {
        public SourceCode() : base("sourceCode", "Sends a link to my source code.", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            await Global.SendText("https://github.com/niklasCarstensen/Discord-Bot", commandmessage.Channel);
        }
    }
}
