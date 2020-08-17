using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEE7.Commands.CAUServerSpecific
{
    class Shitpost : Command
    {
        IMessageChannel arena;

        public Shitpost() : base("shitpost", "", false, true)
        {
            Program.OnConnected += Program_OnConnected;
        }

        private void Program_OnConnected()
        {
            arena = (IMessageChannel)Program.GetChannelFromID(552976757217820693);
        }

        public override void Execute(IMessage message)
        {
            string Content = message.Content.Split(" ").Skip(1).Combine(" ");

            try
            {
                if (message.Attachments.Count == 0)
                    arena.SendMessageAsync(Content);
                else
                    arena.SendFileAsync(message.Attachments.First().Url.GetStreamFromUrl(), message.Attachments.First().Filename, Content);
            }
            catch { }
        }
    }
}
