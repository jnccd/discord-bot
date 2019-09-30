using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEE7.Commands
{
    public class AddLikeSpamRole : Command
    {
        public AddLikeSpamRole() : base("", "", false, true)
        {
            Program.OnConnected += Program_OnConnected;
        }

        private void Program_OnConnected()
        {
            SocketGuild uniServer = Program.GetGuildFromID(479950092938248193);
            Program.OnUserJoined += (SocketGuildUser user) => {
                try { user.AddRoleAsync(uniServer.GetRole(552459506895028225)).Wait(); } catch { }
            };
        }

        public override void Execute(SocketMessage message)
        {
            return;
        }
    }
}
