using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using MEE7.Configuration;
using System;

namespace MEE7.Commands
{
    public class ShowMessageLinks : Command
    {
        public ShowMessageLinks() : base("toggleMessageLinkPreviews", "Preview linked messages", false, false)
        {
            Program.OnNonCommandMessageRecieved += OnNonCommandMessageRecieved;
        }

        public void OnNonCommandMessageRecieved(IMessage message)
        {
            if (Config.Data.MessagePreviewServers.Contains(Program.GetGuildFromChannel(message.Channel).Id))
            {
                string[] split = message.Content.Split(new char[] { ' ', '\n' });
                foreach (string s in split)
                    if (s.StartsWith("https://discordapp.com/channels/"))
                    {
                        try
                        {
                            string[] linkSplit = s.Split('/');
                            IMessage m = (Program.GetGuildFromID(Convert.ToUInt64(linkSplit[4])).
                                GetChannel(Convert.ToUInt64(linkSplit[5])) as ITextChannel).
                                GetMessageAsync(Convert.ToUInt64(linkSplit[6])).Result;
                            EmbedBuilder Embed = m.ToEmbed();
                            //Embed.AddFieldDirectly("Preview for: ", s);
                            DiscordNETWrapper.SendEmbed(Embed, message.Channel).Wait();
                        }
                        catch { }
                    }
            }
        }

        public override void Execute(IMessage message)
        {
            if (message.Channel is SocketGuildChannel)
            {
                SocketGuild guild = Program.GetGuildFromChannel(message.Channel);
                if (message.Author.Id == guild.OwnerId || message.Author.Id == Program.Master.Id)
                {
                    if (Config.Data.MessagePreviewServers.Contains(guild.Id))
                    {
                        Config.Data.MessagePreviewServers.Remove(guild.Id);
                        DiscordNETWrapper.SendText("This server wont get linked message previews anymore!", message.Channel).Wait();
                    }
                    else
                    {
                        Config.Data.MessagePreviewServers.Add(guild.Id);
                        DiscordNETWrapper.SendText("This Server will now get linked message previews!", message.Channel).Wait();
                    }
                }
                else
                {
                    DiscordNETWrapper.SendText("Only the server/bot owner is authorized to use this command!", message.Channel).Wait();
                }
            }
        }
    }
}
