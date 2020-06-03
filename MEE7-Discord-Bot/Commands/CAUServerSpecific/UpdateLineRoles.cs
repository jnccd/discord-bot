using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using System.Linq;

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
                if (arg2.Guild.Id == uniServer.Id)
                {
                    var roles = arg2.Roles;
                    if (roles.Any(x => x.Position > topLine.Position) && !roles.Any(x => x.Id == topLine.Id))
                        try { arg2.AddRoleAsync(topLine).Wait(); } catch { }
                    if (!roles.Any(x => x.Position > topLine.Position) && roles.Any(x => x.Id == topLine.Id))
                        try { arg2.RemoveRoleAsync(topLine).Wait(); } catch { }

                    if (roles.Any(x => x.Position < bottomLine.Position && !x.IsEveryone) && !roles.Any(x => x.Id == bottomLine.Id))
                        try { arg2.AddRoleAsync(bottomLine).Wait(); } catch { }
                    if (!roles.Any(x => x.Position < bottomLine.Position && !x.IsEveryone) && roles.Any(x => x.Id == bottomLine.Id))
                        try { arg2.RemoveRoleAsync(bottomLine).Wait(); } catch { }
                }
            };
        }

        public override void Execute(IMessage message) { }
    }
}
