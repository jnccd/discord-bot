using Discord.WebSocket;
using MEE7.Backend;
using System;
using System.Collections.Generic;
using System.Text;

namespace MEE7.Commands.CAUServerSpecific
{
    public class AddStudentRole : Command
    {
        public AddStudentRole() : base("", "", false, true)
        {
            Program.OnConnected += Program_OnConnected;
        }

        private void Program_OnConnected()
        {
            SocketGuild uniServer = Program.GetGuildFromID(479950092938248193);
            SocketRole studentRole = uniServer.GetRole(479953406299996180);
            Program.OnUserJoined += (SocketGuildUser user) => user.AddRoleAsync(studentRole).Wait();
        }

        public override void Execute(SocketMessage message)
        {

        }
    }
}
