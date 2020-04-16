using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System.Linq;

namespace MEE7.Commands
{
    class UserInfo : Command
    {
        public UserInfo() : base("userInfo", "Posts user information, takes message ID as argument", isExperimental: false, isHidden: false)
        {

        }

        public override void Execute(SocketMessage message)
        {
            string idText = message.Content.Split(' ').Last().Trim('<', '>', '@');
            SocketUser u;
            try { u = Program.GetUserFromId(ulong.Parse(idText)); }
            catch { DiscordNETWrapper.SendText("That's not a valid user id", message.Channel).Wait(); return; }

            EmbedBuilder embed = DiscordNETWrapper.CreateEmbedBuilder(u.Username, "", u.GetAvatarUrl(), u, u.GetDefaultAvatarUrl());
            embed.AddFieldDirectly("Activity", $"{u.Activity}, {u.Activity.Type}");
            embed.AddFieldDirectly("AvatarId", u.AvatarId);
            embed.AddFieldDirectly("CreatedAt", u.CreatedAt);
            embed.AddFieldDirectly("Discriminator", u.Discriminator);
            embed.AddFieldDirectly("DiscriminatorValue", u.DiscriminatorValue);
            embed.AddFieldDirectly("Id", u.Id);
            embed.AddFieldDirectly("IsBot", u.IsBot);
            embed.AddFieldDirectly("IsWebhook", u.IsWebhook);
            embed.AddFieldDirectly("Mention", u.Mention);
            embed.AddFieldDirectly("MutualGuilds", u.MutualGuilds.Select(x => $"({x.Name}, {x.Id}) by {x.Owner.Username}").Combine("\n"));
            embed.AddFieldDirectly("Status", u.Status);

            DiscordNETWrapper.SendEmbed(embed, message.Channel).Wait();
        }
    }
}
