using Discord.WebSocket;
using MEE7.Backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEE7.Commands.CAUServerSpecific
{
    class UpdateLineRoles : Command
    {
        public UpdateLineRoles() : base("", "", false, true)
        {
            Program.OnConnected += Program_OnConnected;
        }

        private void Program_OnConnected()
        {
            var uniServer = Program.GetGuildFromID(479950092938248193);
            var topLine = uniServer.GetRole(647144287485820928);
            var bottomLine = uniServer.GetRole(665555692983156746);
            Program.OnGuildMemberUpdated += (SocketGuildUser arg1, SocketGuildUser arg2) =>
            {
                if (arg2.Roles.FirstOrDefault(x => x.Position > topLine.Position) != null)
                    try { arg2.AddRoleAsync(topLine).Wait(); } catch { }
                else
                    try { arg2.RemoveRoleAsync(topLine).Wait(); } catch { }

                if (arg2.Roles.FirstOrDefault(x => !x.IsEveryone && x.Position < bottomLine.Position) != null)
                    try { arg2.AddRoleAsync(bottomLine).Wait(); } catch { }
                else
                    try { arg2.RemoveRoleAsync(bottomLine).Wait(); } catch { }
            };
        }

        public override void Execute(SocketMessage message) { }
    }
}
