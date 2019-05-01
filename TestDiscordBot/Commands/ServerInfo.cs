using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
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
            EmbedBuilder roles = new EmbedBuilder();


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
            info.AddField("Verification Level", g.VerificationLevel, true);
            info.AddField("Voice Region Id", g.VoiceRegionId, true);


            roles.WithDescription("Roles: " + g.Roles.Count);

            foreach (SocketRole r in g.Roles.OrderByDescending(x => x.Position))
                roles.AddField(r.Name, $"Members: {r.Members.Count()}, Permission Int: {r.Permissions}, Created At: {r.CreatedAt.ToLocalTime()}");

            roles.WithFooter($"The user with the most roles is {g.Users.FirstOrDefault(x => x.Roles.Count == g.Users.Max(y => y.Roles.Count)).Username} " +
                $"with {g.Users.Max(x => x.Roles.Count)} Roles");


            Program.SendEmbed(info, message.Channel).Wait();
            Program.SendEmbed(roles, message.Channel).Wait();

            return Task.FromResult(default(object));
        }
    }
}
