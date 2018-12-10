using Discord;
using Discord.WebSocket;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class EditLast : Command
    {
        EditLastCommand[] Commands;

        public EditLast() : base("editLast", "Edit the last message", false)
        {
            Commands = new EditLastCommand[] {
            
            new EditLastCommand("swedish", "Convert text to swedish", true, async (SocketMessage message, string lastText, string lastPic) => {
                await Global.SendText(string.Join("", lastText.Select((x) => x + "f")).TrimEnd('f'), message.Channel);
            }),
            new EditLastCommand("stupid", "Convert text to stupid", true, async (SocketMessage message, string lastText, string lastPic) => {
                await Global.SendText(string.Join("", lastText.Select((x) => { return (Global.RDM.Next(2) == 1 ? char.ToUpper(x) : char.ToLower(x)); })), message.Channel);
            }),
            new EditLastCommand("CAPS", "Convert text to CAPS", true, async (SocketMessage message, string lastText, string lastPic) => {
                await Global.SendText(string.Join("", lastText.Select((x) => { return char.ToUpper(x); })), message.Channel);
            }),
            new EditLastCommand("colorChannelSwap", "Swap the rgb color channels for each pixel", false, async (SocketMessage message, string lastText, string lastPic) => {
                Bitmap bmp = Global.GetBitmapFromURL(lastPic);

                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        System.Drawing.Color c = bmp.GetPixel(x, y);
                        c = System.Drawing.Color.FromArgb(c.B, c.R, c.G);
                        bmp.SetPixel(x, y, c);
                    }

                await Global.SendBitmap(bmp, message.Channel);
            }),
            new EditLastCommand("invert", "Invert the color of each pixel", false, async (SocketMessage message, string lastText, string lastPic) => {
                Bitmap bmp = Global.GetBitmapFromURL(lastPic);

                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        System.Drawing.Color c = bmp.GetPixel(x, y);
                        c = System.Drawing.Color.FromArgb(255 - c.R, 255 - c.G, 255 - c.B);
                        bmp.SetPixel(x, y, c);
                    }

                await Global.SendBitmap(bmp, message.Channel);
            }),
            new EditLastCommand("expand", "Expand the picture", false, async (SocketMessage message, string lastText, string lastPic) => {
                Bitmap bmp = Global.GetBitmapFromURL(lastPic);
                Bitmap output = new Bitmap(bmp.Width, bmp.Height);

                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Vector2 target = new Vector2(x, y).RepelFrom(new Vector2(bmp.Width / 2, bmp.Height / 2), bmp, true);
                        output.SetPixel(x, y, bmp.GetPixel((int)target.X, (int)target.Y));
                    }

                await Global.SendBitmap(output, message.Channel);
            }),
            new EditLastCommand("collapse", "Collapse the picture", false, async (SocketMessage message, string lastText, string lastPic) => {
                Bitmap bmp = Global.GetBitmapFromURL(lastPic);
                Bitmap output = new Bitmap(bmp.Width, bmp.Height);

                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Vector2 target = new Vector2(x, y).RepelFrom(new Vector2(bmp.Width / 2, bmp.Height / 2), bmp, false);
                        output.SetPixel(x, y, bmp.GetPixel((int)target.X, (int)target.Y));
                    }

                await Global.SendBitmap(output, message.Channel);
            })

            };
        }
        
        public override async Task execute(SocketMessage message)
        {
            IEnumerable<IMessage> messages = await message.Channel.GetMessagesAsync().Flatten();
            string lastText = null, lastPic = null;
            foreach (IMessage m in messages)
            {
                if (lastText == null && m.Id != message.Id && !string.IsNullOrWhiteSpace(m.Content))
                    lastText = m.Content;
                if (lastPic == null && m.Id != message.Id && m.Attachments.Count > 0 && m.Attachments.ElementAt(0).Size > 0 && m.Attachments.ElementAt(0).Filename.EndsWith(".png") ||
                    lastPic == null && m.Id != message.Id && m.Attachments.Count > 0 && m.Attachments.ElementAt(0).Size > 0 && m.Attachments.ElementAt(0).Filename.EndsWith(".jpg"))
                    lastPic = m.Attachments.ElementAt(0).Url;
                if (lastPic == null && m.Id != message.Id && m.Content.ContainsPictureLink() != null)
                    lastPic = m.Content.ContainsPictureLink();
                if (lastText != null && lastPic != null)
                    break;
            }

            string[] split = message.Content.Split(' ');
            if (split.Length == 1)
            {
                EmbedBuilder Embed = new EmbedBuilder();
                Embed.WithColor(0, 128, 255);
                foreach (EditLastCommand ecommand in Commands)
                {
                    Embed.AddField(prefix + command + " " + ecommand.command, ecommand.desc);
                }
                Embed.WithDescription("EditLast Commands:");
                await Global.SendEmbed(Embed, message.Channel);
            }
            else
            {
                foreach (EditLastCommand command in Commands)
                {
                    if (split[1] == command.command)
                    {
                        if (command.textBased && lastText == null)
                        {
                            await Global.SendText("I couldn't find text to edit here :thinking:", message.Channel);
                            return;
                        }
                        if (!command.textBased && lastPic == null)
                        {
                            await Global.SendText("I couldn't find a picture to edit here :thinking:", message.Channel);
                            return;
                        }

                        command.execute(message, lastText, lastPic);

                        break;
                    }
                }
            }
        }

        class EditLastCommand
        {
            public delegate void Execution(SocketMessage message, string lastText, string lastPic);
            public bool textBased;
            public string command, desc;
            public Execution execute;

            public EditLastCommand(string command, string desc, bool textBased, Execution execute)
            {
                this.textBased = textBased;
                this.command = command;
                this.desc = desc;
                this.execute = execute;
            }
        }
    }
}
