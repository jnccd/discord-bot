using Discord;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System;

namespace MEE7.Commands
{
    class Ping : Command
    {
        public override void Execute(IMessage message)
        {
            DiscordNETWrapper.SendText($"Pong in {(int)(DateTime.Now - message.CreatedAt).TotalMilliseconds}ms!", message.Channel).Wait();
        }
    }
}
