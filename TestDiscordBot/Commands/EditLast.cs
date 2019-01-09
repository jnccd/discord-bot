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
            new EditLastCommand("mock", "Mock the text above", true, async (SocketMessage message, string lastText, string lastPic) => {
                await Global.SendText(string.Join("", lastText.Select((x) => { return (Global.RDM.Next(2) == 1 ? char.ToUpper(x) : char.ToLower(x)); })) + 
                    "\nhttps://images.complex.com/complex/images/c_limit,w_680/fl_lossy,pg_1,q_auto/bujewhyvyyg08gjksyqh/spongebob", message.Channel);
            }),
            new EditLastCommand("CAPS", "Convert text to CAPS", true, async (SocketMessage message, string lastText, string lastPic) => {
                await Global.SendText(string.Join("", lastText.Select((x) => { return char.ToUpper(x); })), message.Channel);
            }),
            new EditLastCommand("SUPERCAPS", "Convert text to SUPER CAPS", true, async (SocketMessage message, string lastText, string lastPic) => {
                await Global.SendText(string.Join("", lastText.Select((x) => { return char.ToUpper(x) + " "; })), message.Channel);
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
            new EditLastCommand("liq", "Liquidify the picture with either expand, collapse, stir or fall.\nWithout any arguments it will automatically call \"liq expand 0.5,0.5 1\"" + 
            "\nThe syntax is: liq [mode] [eg. 0.5,1 to center the transformation at the middle of the bottom of the pciture] [eg. 0.7, for 70% transformation strength]", false, async (SocketMessage message, string lastText, string lastPic) => {
                Bitmap bmp = Global.GetBitmapFromURL(lastPic);
                Bitmap output = new Bitmap(bmp.Width, bmp.Height);
                Vector2 center = new Vector2(bmp.Width / 2, bmp.Height / 2);
                float Strength = 1;
                string[] split = message.Content.Split(new char[] { ' ', '\n' });

                try
                {
                    string cen = split[3];
                    string[] cent = cen.Split(',');
                    center.X = (float)Global.ConvertToDouble(cent[0]) * bmp.Width;
                    center.Y = (float)Global.ConvertToDouble(cent[1]) * bmp.Height;
                } catch { }

                try
                {
                    Strength = (float)Global.ConvertToDouble(split[4]);
                } catch { }

                TransformMode mode = TransformMode.Expand;
                try
                {
                    Enum.TryParse(split[2].ToCapital(), out mode);
                } catch { }

                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Vector2 target = Transform(new Vector2(x, y), center, bmp, Strength, mode);
                        output.SetPixel(x, y, bmp.GetPixel((int)target.X, (int)target.Y));
                    }

                await Global.SendBitmap(output, message.Channel);
            })

            };
        }
        private static Vector2 Transform(Vector2 point, Vector2 center, Bitmap within, float strength, TransformMode mode)
        {
            Vector2 diff = point - center;
            Vector2 move = diff;
            move.Normalize();
            
            Vector2 target = Vector2.Zero;
            float transformedLength = 0;
            float rotationAngle = 0;
            double cos = 0;
            double sin = 0;
            float div = ((within.Width + within.Height) / 7);
            float maxDistance = (center / div).LengthSquared();
            switch (mode)
            {
                case TransformMode.Expand:
                    transformedLength = (diff / div).LengthSquared() * strength;
                    target = point - diff * (1 / (1 + transformedLength)) * (1 / (1 + transformedLength));
                    break;

                case TransformMode.Collapse:
                    transformedLength = (diff / div).LengthSquared() * strength;
                    target = point + diff * (1 / (1 + transformedLength)) * (1 / (1 + transformedLength));
                    break;

                case TransformMode.Stir:
                    transformedLength = (diff / div).LengthSquared() * strength;
                    rotationAngle = (float)Math.Pow((maxDistance - transformedLength), 5) / 3000;
                    cos = Math.Cos(rotationAngle);
                    sin = Math.Sin(rotationAngle);
                    target = new Vector2((float)(cos * (point.X - center.X) - sin * (point.Y - center.Y) + center.X), 
                                         (float)(sin * (point.X - center.X) + cos * (point.Y - center.Y) + center.Y));
                    break;

                case TransformMode.Fall:
                    transformedLength = (diff / div).LengthSquared() * strength;
                    rotationAngle = transformedLength / 3;
                    cos = Math.Cos(rotationAngle);
                    sin = Math.Sin(rotationAngle);
                    target = new Vector2((float)(cos * (point.X - center.X) - sin * (point.Y - center.Y) + center.X),
                                         (float)(sin * (point.X - center.X) + cos * (point.Y - center.Y) + center.Y));
                    break;
            }

            if (float.IsNaN(target.X) || float.IsInfinity(target.X))
                target.X = point.X;
            if (float.IsNaN(target.Y) || float.IsInfinity(target.Y))
                target.Y = point.Y;
            if (target.X < 0)
                target.X = 0;
            if (target.X > within.Width - 1)
                target.X = within.Width - 1;
            if (target.Y < 0)
                target.Y = 0;
            if (target.Y > within.Height - 1)
                target.Y = within.Height - 1;

            return target;
        }
        private enum TransformMode { Expand, Collapse, Stir, Fall }

        public override async Task execute(SocketMessage message)
        {
            IEnumerable<IMessage> messages = await message.Channel.GetMessagesAsync().Flatten();
            string lastText = null, lastPic = null;
            ulong ownID = Global.OwnID;
            foreach (IMessage m in messages)
            {
                if (m.Id != message.Id && m.Author.Id != ownID)
                {
                    if (lastText == null && !string.IsNullOrWhiteSpace(m.Content))
                        lastText = m.Content;
                    if (lastPic == null && m.Attachments.Count > 0 && m.Attachments.ElementAt(0).Size > 0)
                    {
                        if (m.Attachments.ElementAt(0).Filename.EndsWith(".png"))
                            lastPic = m.Attachments.ElementAt(0).Url;
                        else if (m.Attachments.ElementAt(0).Filename.EndsWith(".jpg"))
                            lastPic = m.Attachments.ElementAt(0).Url;
                    }
                    string picLink = m.Content.ContainsPictureLink();
                    if (lastPic == null && picLink != null)
                        lastPic = picLink;

                    if (lastText != null && lastPic != null)
                        break;
                }
            }

            string[] split = message.Content.Split(new char[] { ' ', '\n' });
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
