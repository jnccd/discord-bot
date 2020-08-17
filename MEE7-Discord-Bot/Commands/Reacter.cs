using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using System;

namespace MEE7.Commands
{
    class Reacter : Command
    {
        Emote kenobi, padoru, hentai;

        public Reacter()
        {
            Program.OnConnected += Program_OnConnected;
            Program.OnNonCommandMessageRecieved += OnNonCommandMessageRecieved;
        }

        private void Program_OnConnected()
        {
            kenobi = Emote.Parse("<a:general_kenobi:729800823239999498>");
            padoru = Emote.Parse("<a:padoru:744966861648560277>");
            hentai = Emote.Parse("<a:FeelsHentaiMan:744966841402916925>");
        }

        private void OnNonCommandMessageRecieved(IMessage messageIn)
        {
            if (!(messageIn is SocketMessage))
                return;
            var message = messageIn as SocketMessage;

            if (message.Content.Contains("Hello there", StringComparison.OrdinalIgnoreCase))
                message.AddReactionAsync(kenobi).Wait();
            if (message.Content.Contains("Padoru", StringComparison.OrdinalIgnoreCase))
                message.AddReactionAsync(padoru).Wait();
            if (message.Content.Contains("Hentai", StringComparison.OrdinalIgnoreCase))
                message.AddReactionAsync(hentai).Wait();
        }

        public override void Execute(IMessage message)
        {
            return;
        }
    }
}
