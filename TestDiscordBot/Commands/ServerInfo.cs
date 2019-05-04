using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEE7.Commands
{
    public class ServerInfo : Command
    {
        public ServerInfo() : base("serverInfo", "Posts server information", false)
        {

        }

        public override Task Execute(SocketMessage message)
        {
            SocketGuild g = Program.GetGuildFromChannel(message.Channel);

            EmbedBuilder info = new EmbedBuilder();
            
            info.WithDescription("Server-Information");
            
            info.AddField("Channels", g.Channels.Count, true);
            info.AddField("Category Channels", g.CategoryChannels.Count, true);
            info.AddField("Text Channels", g.TextChannels.Count, true);
            info.AddField("Voice Channels", g.VoiceChannels.Count, true);

            info.AddField("Owner", g.Owner.Username, true);
            info.AddField("Created At", g.CreatedAt, true);

            info.AddField("Member Count", g.MemberCount, true);
            info.AddField("Human Count", g.Users.Where(x => !x.IsBot).Count(), true);
            info.AddField("Bot Count", g.Users.Where(x => x.IsBot).Count(), true);
            
            info.AddField("Emotes", g.Emotes.Count, true);
            info.AddField("Features", g.Features.Count, true);
            info.AddField("Mfa Level", g.MfaLevel, true);
            info.AddField("Icon Url", g.IconUrl, true);
            if (g.SplashUrl != null)
                info.AddField("Splash Url", g.SplashUrl, true);
            info.AddField("Verification Level", g.VerificationLevel, true);
            info.AddField("Voice Region Id", g.VoiceRegionId, true);
            
            info.AddField("Roles:", g.Roles.OrderByDescending(x => x.Position).ToArray().Select(x => $"[{x.Members.Count()}]{x.Name}").ToArray().Aggregate((x, y) => x + "\n" + y));

            int maxRoles = 0; SocketGuildUser maxRolesUser = null;
            int maxNameLength = 0; SocketGuildUser maxNameLengthUser = null;
            foreach (SocketGuildUser u in g.Users)
            {
                if (u.Roles.Count > maxRoles)
                {
                    maxRolesUser = u;
                    maxRoles = u.Roles.Count;
                }
                if (u.Username.Length > maxNameLength)
                {
                    maxNameLengthUser = u;
                    maxNameLength = u.Username.Length;
                }
            }
            info.AddField("User with the most roles:", $"{maxRolesUser.Username} with {maxRoles} Roles");
            info.AddField("User with the longest name:", maxNameLengthUser.Username);
            
            Program.SendEmbed(info, message.Channel).Wait();

            return Task.FromResult(default(object));
        }
    }
}
