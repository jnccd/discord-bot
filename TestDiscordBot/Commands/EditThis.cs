using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestDiscordBot.Backend;
using static TestDiscordBot.Commands.EditLast;

namespace TestDiscordBot.Commands
{
    public class EditThis : Command
    {
        public EditThis() : base("editThis", "Edit your message", false)
        {

        }

        public override async Task Execute(SocketMessage message)
        {
            string[] split = message.Content.Split(new char[] { ' ', '\n' });
            if (split.Length == 1)
            {
                EmbedBuilder Embed = new EmbedBuilder();
                Embed.WithColor(0, 128, 255);
                foreach (EditLastCommand ecommand in EditLast.Commands)
                {
                    Embed.AddField(Prefix + CommandLine + " " + ecommand.command, ecommand.desc);
                }
                Embed.WithDescription("EditLast Commands:");
                await Global.SendEmbed(Embed, message.Channel);
            }
            else
            {
                foreach (EditLastCommand command in EditLast.Commands)
                {
                    if (split[1].ToLower() == command.command.ToLower())
                    {
                        string inText = split.Skip(2).Foldr("", (x, y) => y + " " + x);
                        string inPic = "";
                        if (message.Attachments.Count > 0 && message.Attachments.ElementAt(0).Size > 0)
                        {
                            if (message.Attachments.ElementAt(0).Filename.EndsWith(".png"))
                                inPic = message.Attachments.ElementAt(0).Url;
                            else if (message.Attachments.ElementAt(0).Filename.EndsWith(".jpg"))
                                inPic = message.Attachments.ElementAt(0).Url;
                        }
                        string picLink = message.Content.ContainsPictureLink();
                        if (inPic == null && picLink != null)
                            inPic = picLink;

                        if (command.textBased && string.IsNullOrWhiteSpace(inText))
                        {
                            await Global.SendText("I couldn't find text to edit here :thinking:", message.Channel);
                            return;
                        }
                        if (!command.textBased && inPic == null)
                        {
                            await Global.SendText("I couldn't find a picture to edit here :thinking:", message.Channel);
                            return;
                        }

                        await command.execute(message, new SelfmadeMessage(message).EditContent(inText), inPic);

                        break;
                    }
                }
            }
        }
    }
}
