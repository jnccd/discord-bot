using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MEE7.Commands.EditLast;

namespace MEE7.Commands
{
    public class EditThis : Command
    {
        public EditThis() : base("editThis", "Edit your message", false)
        {
            HelpMenu = new EmbedBuilder();
            HelpMenu.WithColor(0, 128, 255);
            HelpMenu.WithDescription("EditThis Commands:");
            foreach (EditLastCommand ecommand in EditLast.Commands)
                HelpMenu.AddField(Prefix + CommandLine + " " + ecommand.command, ecommand.desc.Replace("editLast", "editThis"));
        }

        public override async Task Execute(SocketMessage message)
        {
            List<string> split = message.Content.Split(new char[] { ' ', '\n' }).ToList();
            if (split.Count == 1)
                await Program.SendEmbed(HelpMenu, message.Channel);
            else
            {
                foreach (EditLastCommand command in EditLast.Commands)
                {
                    if (split[1].ToLower() == command.command.ToLower())
                    {
                        // Remove Arguments from LastText Input
                        for (int i = 0; i < split.Count; i++)
                            if (split[i].StartsWith("-"))
                                try { split.RemoveRange(i, 2); i--; } catch { }
                        string inText = split.Skip(2).Foldl("", (x, y) => x + " " + y);
                        string inPic = "";
                        if (message.Attachments.Count > 0 && message.Attachments.ElementAt(0).Size > 0)
                        {
                            if (message.Attachments.ElementAt(0).Filename.EndsWith(".png"))
                                inPic = message.Attachments.ElementAt(0).Url;
                            else if (message.Attachments.ElementAt(0).Filename.EndsWith(".jpg"))
                                inPic = message.Attachments.ElementAt(0).Url;
                        }
                        string picLink = message.Content.GetPictureLink();
                        if (string.IsNullOrWhiteSpace(inPic) && picLink != null)
                            inPic = picLink;

                        if (command.textBased && string.IsNullOrWhiteSpace(inText))
                        {
                            await Program.SendText("I couldn't find text to edit here :thinking:", message.Channel);
                            return;
                        }
                        if (!command.textBased && string.IsNullOrWhiteSpace(inPic))
                        {
                            await Program.SendText("I couldn't find a picture to edit here :thinking:", message.Channel);
                            return;
                        }

                        await command.execute(message, new SelfmadeMessage(message).EditContent(inText.Trim(' ')), inPic);

                        return;
                    }
                }
                await Program.SendText("That subcommand doesn't exist :thinking:", message.Channel);
            }
        }
    }
}
