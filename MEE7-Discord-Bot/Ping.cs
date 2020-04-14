using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MEE7
{
    class Ping : Command
    {
        public override void Execute(SocketMessage message)
        {
            DiscordNETWrapper.SendText($"Pong in {(int)(message.Timestamp - DateTime.Now).TotalMilliseconds}ms!", message.Channel).Wait();
        }
    }
}
