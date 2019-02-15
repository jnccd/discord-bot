using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestDiscordBot.Config;

namespace TestDiscordBot.Commands
{
    public class ShowMessageLinks : Command
    {
        public ShowMessageLinks() : base("toggleMessageLinkPreviews", "Preview linked messages", false)
        {

        }

        public override async void onNonCommandMessageRecieved(SocketMessage message)
        {
            if (message.Content.Contains("https://discordapp.com/channels/") &&
                message.Channel is SocketGuildChannel && 
                config.Data.MessagePreviewServers.Contains(Global.P.getGuildFromChannel(message.Channel).Id))
            {
                string[] split = message.Content.Split(new char[] { ' ', '\n' });
                foreach (string s in split)
                    if (s.StartsWith("https://discordapp.com/channels/"))
                    {
                        try
                        {
                            IMessage m = await message.Channel.GetMessageAsync(Convert.ToUInt64(s.Split('/').Last()));
                            EmbedBuilder Embed = m.toEmbed();
                            Embed.AddField("Preview for: ", s);
                            await Global.SendEmbed(Embed, message.Channel);
                        } catch (Exception e) { }
                    }
            }
        }

        public override async Task execute(SocketMessage message)
        {
            if (message.Channel is SocketGuildChannel)
            {
                SocketGuild guild = Global.P.getGuildFromChannel(message.Channel);
                if (message.Author.Id == guild.OwnerId || message.Author.Id == Global.Master.Id)
                {
                    if (config.Data.MessagePreviewServers.Contains(guild.Id))
                    {
                        config.Data.MessagePreviewServers.Remove(guild.Id);
                        await Global.SendText("This server wont get linked message previews anymore!", message.Channel);
                    }
                    else
                    {
                        config.Data.MessagePreviewServers.Add(guild.Id);
                        await Global.SendText("This Server will now get linked message previews!", message.Channel);
                    }
                }
                else
                {
                    await Global.SendText("Only the server/bot owner is authorized to use this command!", message.Channel);
                }
            }
        }
    }
}
