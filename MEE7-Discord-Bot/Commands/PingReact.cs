using Discord;
using Discord.WebSocket;
using MEE7;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEE7.Commands
{
    public class PingReact : Command
    {
        readonly Emoji ANGRY_FACE = new Emoji("😠");
        readonly Emoji PING_PONG = new Emoji("🏓");

        public PingReact() : base("", "", false, true)
        {

        }

        public override void OnNonCommandMessageRecieved(SocketMessage message)
        {
            if (message.MentionedUsers.Count == 0 && message.MentionedRoles.Count == 0 || !(message is IUserMessage))
                return;

            SocketRole[] r = Program.GetGuildFromChannel(message.Channel).GetUser(Program.GetSelf().Id).Roles.ToArray();
            if (message.MentionedUsers.FirstOrDefault(x => x.Id == Program.GetSelf().Id) != null)
                PingReaction(message as IUserMessage);
            if (message.MentionedRoles.ContainsAny(r))
                PingReaction(message as IUserMessage);
        }
        void PingReaction(IUserMessage message)
        {
            message.AddReactionsAsync(new IEmote[] { PING_PONG, ANGRY_FACE }).Wait();  
        }

        public override Task Execute(SocketMessage message)
        {
            return Task.FromResult(default(object));
        }
    }
}
