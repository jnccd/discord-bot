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

        public override async void OnNonCommandMessageRecieved(SocketMessage message)
        {
            if (message.Content.Contains("https://discordapp.com/channels/") &&
                message.Channel is SocketGuildChannel &&
                Config.Config.Data.MessagePreviewServers.Contains(Program.GetGuildFromChannel(message.Channel).Id))
            {
                string[] split = message.Content.Split(new char[] { ' ', '\n' });
                foreach (string s in split)
                    if (s.StartsWith("https://discordapp.com/channels/"))
                    {
                        try
                        {
                            IMessage m = await message.Channel.GetMessageAsync(Convert.ToUInt64(s.Split('/').Last()));
                            EmbedBuilder Embed = m.ToEmbed();
                            Embed.AddField("Preview for: ", s);
                            await Program.SendEmbed(Embed, message.Channel);
                        } catch { }
                    }
            }
        }

        public override async Task Execute(SocketMessage message)
        {
            if (message.Channel is SocketGuildChannel)
            {
                SocketGuild guild = Program.GetGuildFromChannel(message.Channel);
                if (message.Author.Id == guild.OwnerId || message.Author.Id == Program.Master.Id)
                {
                    if (Config.Config.Data.MessagePreviewServers.Contains(guild.Id))
                    {
                        Config.Config.Data.MessagePreviewServers.Remove(guild.Id);
                        await Program.SendText("This server wont get linked message previews anymore!", message.Channel);
                    }
                    else
                    {
                        Config.Config.Data.MessagePreviewServers.Add(guild.Id);
                        await Program.SendText("This Server will now get linked message previews!", message.Channel);
                    }
                }
                else
                {
                    await Program.SendText("Only the server/bot owner is authorized to use this command!", message.Channel);
                }
            }
        }
    }
}
