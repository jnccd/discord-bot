using Discord.WebSocket;
using MEE7.Backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MEE7.Backend.HelperFunctions;
using Discord;
using MEE7.Backend.HelperFunctions.Extensions;

namespace MEE7.Commands._1FileCommands
{
    class MessageInfo : Command
    {
        public MessageInfo() : base("messageInfo", "Posts message information, takes message ID as argument", false)
        {

        }

        public override void Execute(SocketMessage message)
        {
            string idText = message.Content.Split(' ').Last().Split('/').Last();
            IMessage m;
            try { m = message.Channel.GetMessageAsync(Convert.ToUInt64(idText)).Result; }
            catch { DiscordNETWrapper.SendText("That's not a valid message id", message.Channel).Wait(); return; }

            EmbedBuilder embed = DiscordNETWrapper.CreateEmbedBuilder("", m.Content, "", m.Author);
            embed.AddFieldDirectly("Created at:", m.CreatedAt, true);
            embed.AddFieldDirectly("Sent at:", m.Timestamp, true);
            if (m.EditedTimestamp != null)
                embed.AddFieldDirectly("Edited at:", m.EditedTimestamp, true);
            if (m.Activity != null)
                embed.AddFieldDirectly("Activity:", m.Activity, true);
            if (m.Application != null)
                embed.AddFieldDirectly("Application:", m.Application, true);
            embed.AddFieldDirectly("IsPinned:", m.IsPinned, true);
            embed.AddFieldDirectly("IsTTS:", m.IsTTS, true);
            embed.AddFieldDirectly("ID:", m.Id, true);
            embed.AddFieldDirectly("Source:", m.Source, true);
            embed.AddFieldDirectly("Tags:", m.Tags.Count, true);
            embed.AddFieldDirectly("Type:", m.Type, true);
            DiscordNETWrapper.SendEmbed(embed, message.Channel).Wait();
        }
    }
}
