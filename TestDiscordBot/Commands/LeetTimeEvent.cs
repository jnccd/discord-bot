using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class LeetTimeEvent : Command
    {
        public LeetTimeEvent() : base("toggleLeetEvents", "Dooms this channel for all eternity. (can only be used by server owners)", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            
        }
    }
}
