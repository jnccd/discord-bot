using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using System;

namespace MEE7.Commands
{
    class GeneralKenobi : Command
    {
        Emote kenobi;

        public GeneralKenobi()
        {
            Program.OnConnected += Program_OnConnected;
            Program.OnNonCommandMessageRecieved += OnNonCommandMessageRecieved;
        }

        private void Program_OnConnected()
        {
            kenobi = Emote.Parse("<a:general_kenobi:729800823239999498>");
        }

        private void OnNonCommandMessageRecieved(IMessage messageIn)
        {
            if (!(messageIn is SocketMessage))
                return;
            var message = messageIn as SocketMessage;

            if (message.Content.Contains("Hello there", StringComparison.OrdinalIgnoreCase))
                message.AddReactionAsync(kenobi).Wait();
        }

        public override void Execute(IMessage message)
        {
            return;
        }
    }
}
