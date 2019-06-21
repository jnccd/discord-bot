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

        public override void Execute(SocketMessage message)
        {
            SocketGuild g = Program.GetGuildFromChannel(message.Channel);

            EmbedBuilder info = new EmbedBuilder();
            
            info.WithDescription("Server-Information");
            
            info.AddFieldDirectly("Channels", g.Channels.Count, true);
            info.AddFieldDirectly("Category Channels", g.CategoryChannels.Count, true);
            info.AddFieldDirectly("Text Channels", g.TextChannels.Count, true);
            info.AddFieldDirectly("Voice Channels", g.VoiceChannels.Count, true);

            info.AddFieldDirectly("Owner", g.Owner.GetDisplayName(), true);
            info.AddFieldDirectly("Created At", g.CreatedAt, true);

            info.AddFieldDirectly("Member Count", g.MemberCount, true);
            info.AddFieldDirectly("Human Count", g.Users.Where(x => !x.IsBot).Count(), true);
            info.AddFieldDirectly("Bot Count", g.Users.Where(x => x.IsBot).Count(), true);
            
            info.AddFieldDirectly("Emotes", g.Emotes.Count, true);
            info.AddFieldDirectly("Features", g.Features.Count, true);
            info.AddFieldDirectly("Mfa Level", g.MfaLevel, true);
            info.AddFieldDirectly("Icon Url", g.IconUrl, true);
            if (g.SplashUrl != null)
                info.AddFieldDirectly("Splash Url", g.SplashUrl, true);
            
            info.AddFieldDirectly("Roles:", g.Roles.OrderByDescending(x => x.Position).
                Select(x => $"[{x.Members.Count()}]{x.Name}").Aggregate((x, y) => x + "\n" + y), true);

            info.AddFieldDirectly("Verification Level", g.VerificationLevel, true);
            info.AddFieldDirectly("Voice Region Id", g.VoiceRegionId, true);
            
            info.AddFieldDirectly("User with the most roles:", $"{g.Users.MaxElement(x => x.Roles.Count, out double max)} with {max} Roles", true);
            info.AddFieldDirectly("User with the longest name:", g.Users.MaxElement(x => x.GetDisplayName().Length), true);
            
            Program.SendEmbed(info, message.Channel).Wait();

            return;
        }
    }
}
