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
        public PingReact() : base("", "", false, true)
        {

        }

        public override void OnNonCommandMessageRecieved(SocketMessage message)
        {
            if (message.MentionedUsers.Count == 0 && message.MentionedRoles.Count == 0)
                return;

            SocketRole[] r = Program.GetGuildFromChannel(message.Channel).GetUser(Program.GetSelf().Id).Roles.ToArray();
            if (message.MentionedUsers.FirstOrDefault(x => x.Id == Program.GetSelf().Id) != null)
                PingReaction(message);
            //if (message.MentionedRoles.)
        }
        void PingReaction(SocketMessage message)
        {

        }

        public override Task Execute(SocketMessage message)
        {
            return Task.FromResult(default(object));
        }
    }
}
