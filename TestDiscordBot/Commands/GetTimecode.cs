using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class GetTimecode : Command
    {
        public GetTimecode() : base("getTimecode", "", true, true)
        {

        }

        public override async Task execute(SocketMessage message)
        {
            string[] split = message.Content.Split(' ');
            //if (split.Length)
        }
    }
}
