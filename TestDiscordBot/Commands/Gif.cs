using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class Gif : Command
    {
        public Gif() : base("gif", "Creates gifs", true, true)
        {

        }

        public override Task Execute(SocketMessage message)
        {
            return Task.FromResult(default(object));
        }
    }
}
