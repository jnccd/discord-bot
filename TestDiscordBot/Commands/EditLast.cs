using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XnaGeometry;

namespace TestDiscordBot.Commands
{
    public static partial class Extensions
    {

    }

    public class EditLast : Command
    {
        public EditLast() : base("editLast", "Edit the last message", false)
        {

        }
        public override async Task Execute(SocketMessage message)
        {
            IEnumerable<IMessage> messages = await message.Channel.GetMessagesAsync().Flatten();
            IMessage lastText = null;
            string lastPic = null;
            ulong ownID = Program.OwnID;
            foreach (IMessage m in messages)
            {
                if (m.Id != message.Id/* && m.Author.Id != ownID*/ && !m.Content.StartsWith(Program.prefix))
                {
                    if (lastText == null && !string.IsNullOrWhiteSpace(m.Content) && !m.Content.StartsWith(Program.prefix))
                        lastText = m;
                    if (lastPic == null && m.Attachments.Count > 0 && m.Attachments.ElementAt(0).Size > 0)
                    {
                        if (m.Attachments.ElementAt(0).Filename.EndsWith(".png"))
                            lastPic = m.Attachments.ElementAt(0).Url;
                        else if (m.Attachments.ElementAt(0).Filename.EndsWith(".jpg"))
                            lastPic = m.Attachments.ElementAt(0).Url;
                    }
                    string picLink = m.Content.ContainsPictureLink();
                    if (string.IsNullOrWhiteSpace(lastPic) && picLink != null)
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
                    Embed.AddField(Prefix + CommandLine + " " + ecommand.command, ecommand.desc);
                }
                Embed.WithDescription("EditLast Commands:");
                await Program.SendEmbed(Embed, message.Channel);
            }
            else
            {
                foreach (EditLastCommand command in Commands)
                {
                    if (split[1].ToLower() == command.command.ToLower())
                    {
                        if (command.textBased && lastText == null)
                        {
                            await Program.SendText("I couldn't find text to edit here :thinking:", message.Channel);
                            return;
                        }
                        if (!command.textBased && lastPic == null)
                        {
                            await Program.SendText("I couldn't find a picture to edit here :thinking:", message.Channel);
                            return;
                        }

                        await command.execute(message, lastText, lastPic);

                        return;
                    }
                }
                await Program.SendText("That subcommand doesn't exist :thinking:", message.Channel);
            }
        }

        public static EditLastCommand[] Commands = new EditLastCommand[] {
            new EditLastCommand("swedish", "Convert text to swedish", true, async (SocketMessage message, IMessage lastText, string lastPic) => {
                await Program.SendText(string.Join("", lastText.Content.Select((x) => x + "f")).TrimEnd('f'), message.Channel);
            }),
            new EditLastCommand("mock", "Mock the text above", true, async (SocketMessage message, IMessage lastText, string lastPic) => {
                await Program.SendText(string.Join("", lastText.Content.Select((x) => { return (Program.RDM.Next(2) == 1 ? char.ToUpper(x) : char.ToLower(x)); })) +
                    "\nhttps://images.complex.com/complex/images/c_limit,w_680/fl_lossy,pg_1,q_auto/bujewhyvyyg08gjksyqh/spongebob", message.Channel);
            }),
            new EditLastCommand("crab", "Crab the text above", true, async (SocketMessage message, IMessage lastText, string lastPic) => {
                await Program.SendText(":crab: " + lastText.Content + " :crab:" +
                    "\nhttps://www.youtube.com/watch?v=LDU_Txk06tM&t=75s", message.Channel);
            }),
            new EditLastCommand("CAPS", "Convert text to CAPS",true, async (SocketMessage message, IMessage lastText, string lastPic) => {
                await Program.SendText(string.Join("", lastText.Content.Select((x) => { return char.ToUpper(x); })), message.Channel);
            }),
            new EditLastCommand("SUPERCAPS", "Convert text to SUPER CAPS", true, async (SocketMessage message, IMessage lastText, string lastPic) => {
                await Program.SendText(string.Join("", lastText.Content.Select((x) => { return char.ToUpper(x) + " "; })), message.Channel);
            }),
            new EditLastCommand("Spoilerify", "Convert text to a spoiler", true, async (SocketMessage message, IMessage lastText, string lastPic) => {
                await Program.SendText(string.Join("", lastText.Content.Select((x) => { return "||" + x + "||"; })), message.Channel);
            }),
            new EditLastCommand("Unspoilerify", "Convert spoiler text to readable text", true, async (SocketMessage message, IMessage lastText, string lastPic) => {
                await Program.SendText(lastText.Content.Replace("|", ""), message.Channel);
            }),
            new EditLastCommand("Crosspost", $"Crossposts the message into another channel\neg. {Program.prefix}editLast Crossost #general",
                true, async (SocketMessage message, IMessage lastText, string lastPic) => {
                try
                {
                    SocketChannel targetChannel = Program.GetChannelFromID(Convert.ToUInt64(message.Content.Split(' ')[2].Trim(new char[] { '<', '>', '#' })));
                    EmbedBuilder Embed = lastText.ToEmbed();
                    Embed.AddField("Crosspost from: ", $"<#{message.Channel.Id}>");
                    await Program.SendEmbed(Embed, targetChannel as ISocketMessageChannel);
                } catch { }
            }),
            new EditLastCommand("colorChannelSwap", "Swap the rgb color channels for each pixel", false, async (SocketMessage message, IMessage lastText, string lastPic) => {
                Bitmap bmp = lastPic.GetBitmapFromURL();

                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        System.Drawing.Color c = bmp.GetPixel(x, y);
                        c = System.Drawing.Color.FromArgb(c.B, c.R, c.G);
                        bmp.SetPixel(x, y, c);
                    }

                await Program.SendBitmap(bmp, message.Channel);
            }),
            new EditLastCommand("invert", "Invert the color of each pixel", false, async (SocketMessage message, IMessage lastText, string lastPic) => {
                Bitmap bmp = lastPic.GetBitmapFromURL();

                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        System.Drawing.Color c = bmp.GetPixel(x, y);
                        c = System.Drawing.Color.FromArgb(255 - c.R, 255 - c.G, 255 - c.B);
                        bmp.SetPixel(x, y, c);
                    }

                await Program.SendBitmap(bmp, message.Channel);
            }),
            new EditLastCommand("Rekt", "Finds colored rectangles in pictures", false, async (SocketMessage message, IMessage lastText, string lastPic) => {
                Bitmap bmp = lastPic.GetBitmapFromURL();
                Bitmap output = new Bitmap(bmp.Width, bmp.Height);

                if (message.Content.Split(' ').Length < 2)
                {
                    await Program.SendText("This command requires a color as an additional argument!", message.Channel);
                    return;
                }

                System.Drawing.Color c = System.Drawing.Color.FromName(message.Content.Split(' ')[2]);
                Rectangle redRekt = FindRectangle(bmp, System.Drawing.Color.FromArgb(254, 34, 34), 20);

                if (redRekt.Width == 0)
                    await Program.SendText("No rekt!", message.Channel);
                else
                {
                    using (Graphics graphics = Graphics.FromImage(output))
                    {
                        graphics.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));
                        graphics.DrawRectangle(Pens.Red, redRekt);
                    }

                    await Program.SendBitmap(output, message.Channel);
                }
            }),
            new EditLastCommand("memify", "Turn the last Picture into a meme, get a list of available templates with the argument -list",
                false, async (SocketMessage message, IMessage lastText, string lastPic) => {
                await memifyLock.WaitAsync();
                Exception e = null;
                try
                {
                    Bitmap bmp = lastPic.GetBitmapFromURL();
                    string[] files = Directory.GetFiles("Commands\\MemeTemplates");
                    string[] split = message.Content.Split(' ');

                    string memeTemplateDesign = "";
                    if (split.Length == 3 && split[2] == "-list")
                    {
                        await Program.SendText("Templates: \n\n" + files.Select(x => Path.GetFileNameWithoutExtension(x)).
                                                                        Where(x => x.EndsWith("design")).
                                                                        Select(x => new string(x.TakeWhile(y => !char.IsDigit(y)).ToArray()).Trim('-')).
                                                                        Aggregate((x, y) => x + "\n" + y), message.Channel);
                        return;
                    }
                    else if (split.Length > 2)
                        memeTemplateDesign = files.Where(x => Path.GetFileNameWithoutExtension(x).StartsWith(split[2]) &&
                                                              Path.GetFileNameWithoutExtension(x).EndsWith("design")).ToArray().FirstOrDefault();
                    else
                        memeTemplateDesign = files.Where(x => Path.GetFileNameWithoutExtension(x).EndsWith("design")).ToArray().GetRandomValue();

                    if (memeTemplateDesign == "")
                    {
                        await Program.SendText("I don't have that meme in my registry!", message.Channel);
                        return;
                    }

                    string memeName = memeTemplateDesign.RemoveLastGroup('-').TrimEnd('-');
                    string memeTemplate = files.FirstOrDefault(x => x.StartsWith(memeName) && !x.Contains("-design."));
                    string memeTemplateOverlay = files.FirstOrDefault(x => x.StartsWith(memeName) && Path.GetFileNameWithoutExtension(x).EndsWith("overlay"));

                    if (File.Exists(memeTemplateOverlay))
                    {
                        Rectangle redRekt = FindRectangle((Bitmap)Bitmap.FromFile(memeTemplateDesign), System.Drawing.Color.FromArgb(254, 34, 34), 20);
                        Bitmap overlay;
                        using (FileStream stream = new FileStream(memeTemplateOverlay, FileMode.Open))
                            overlay = (Bitmap)Bitmap.FromStream(stream);
                        Bitmap output = new Bitmap(overlay.Width, overlay.Height);
                        using (Graphics graphics = Graphics.FromImage(output))
                        {
                           graphics.DrawImage(bmp, redRekt);
                           graphics.DrawImage(overlay, new Point(0, 0));
                        }

                        await Program.SendBitmap(output, message.Channel);
                    }
                    else if (File.Exists(memeTemplate))
                    {
                        Rectangle redRekt = FindRectangle((Bitmap)Bitmap.FromFile(memeTemplateDesign), System.Drawing.Color.FromArgb(254, 34, 34), 20);
                        if (redRekt.Width == 0)
                            redRekt = FindRectangle((Bitmap)Bitmap.FromFile(memeTemplateDesign), System.Drawing.Color.FromArgb(255, 0, 0), 20);
                        Bitmap template;
                        using (FileStream stream = new FileStream(memeTemplate, FileMode.Open))
                            template = (Bitmap)Bitmap.FromStream(stream);
                        using (Graphics graphics = Graphics.FromImage(template))
                            graphics.DrawImage(bmp, redRekt);

                        await Program.SendBitmap(template, message.Channel);
                    }
                    else
                        throw new Exception("uwu");
                }
                catch (Exception ex) { e = ex; }
                finally
                {
                    memifyLock.Release();
                    if (e != null)
                        throw e;
                }
            }),
            new EditLastCommand("textMemify", "Turn the last Picture into a meme, get a list of available templates with the argument -list, " +
                "additional arguments are -f for the font, -s for the font size and of course -m for the meme", true,
                    (SocketMessage message, IMessage lastText, string lastPic) => {
                        string[] files = Directory.GetFiles("Commands\\MemeTextTemplates");
                        List<string> split = message.Content.Split(' ').ToList();
                        split.RemoveRange(0, 2);

                        if (split.Contains("-list"))
                        {
                            Program.SendText("Templates: \n\n" + files.Select(x => Path.GetFileNameWithoutExtension(x)).
                                                                       Where(x => x.EndsWith("-design")).
                                                                       Select(x => x.Remove(x.IndexOf("-design"), "-design".Length)).
                                                                       Aggregate((x, y) => x + "\n" + y), message.Channel).Wait();
                            return Task.FromResult(default(object));
                        }

                        string font = "Arial";
                        int fontSize = 16;
                        int index = split.FindIndex(x => x == "-f");
                        if (index != -1)
                        {
                            font = split[index + 1];
                            split.RemoveRange(index, 2);
                        }
                        index = split.FindIndex(x => x == "-s");
                        if (index != -1)
                        {
                            fontSize = Convert.ToInt32(split[index + 1]);
                            split.RemoveRange(index, 2);
                        }
                        index = split.FindIndex(x => x == "-m");
                        string memeTemplate = "";
                        string memeDesign = "";
                        if (index != -1)
                        {
                            memeDesign = files.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).StartsWith(split[index + 1]) && 
                                Path.GetFileNameWithoutExtension(x).EndsWith("-design"));
                            memeTemplate = files.FirstOrDefault(x => memeDesign.Contains(Path.GetFileNameWithoutExtension(x)) && !x.Contains("design"));
                            split.RemoveRange(index, 2);
                        }
                        else
                        {
                            memeDesign = files.Where(x => x.Contains("-design")).GetRandomValue();
                            memeTemplate = files.FirstOrDefault(x => memeDesign.Contains(Path.GetFileNameWithoutExtension(x)) && !x.Contains("design"));
                        }

                        if (string.IsNullOrWhiteSpace(memeTemplate))
                        {
                            Program.SendText("I don't have that meme in my registry!", message.Channel).Wait();
                            return Task.FromResult(default(object));
                        }

                        if (File.Exists(memeTemplate))
                        {
                            Bitmap template, design;
                            using (FileStream stream = new FileStream(memeTemplate, FileMode.Open))
                                template = (Bitmap)Bitmap.FromStream(stream);
                            using (FileStream stream = new FileStream(memeDesign, FileMode.Open))
                                design = (Bitmap)Bitmap.FromStream(stream);
                            Rectangle redRekt = FindRectangle(design, System.Drawing.Color.FromArgb(255, 0, 0), 20);
                            using (Graphics graphics = Graphics.FromImage(template))
                                graphics.DrawString(lastText.Content, new Font(font, fontSize), Brushes.Black, redRekt);

                            Program.SendBitmap(template, message.Channel).Wait();
                        }
                        else
                            throw new Exception("uwu");

                        return Task.FromResult(default(object));
                    }),
            new EditLastCommand("liq", "Liquidify the picture with either expand, collapse, stir or fall.\nWithout any arguments it will automatically call \"liq expand 0.5,0.5 1\"" +
                "\nThe syntax is: liq [mode] [position, eg. 0.5,1 to center the transformation at the middle of the bottom of the pciture] [strength, eg. 0.7, for 70% transformation " +
                "strength]",
                false, async (SocketMessage message, IMessage lastText, string lastPic) => {
                Bitmap bmp = lastPic.GetBitmapFromURL();
                Bitmap output = new Bitmap(bmp.Width, bmp.Height);
                Vector2 center = new Vector2(bmp.Width / 2, bmp.Height / 2);
                float Strength = 1;
                string[] split = message.Content.Split(new char[] { ' ', '\n' });

                try
                {
                    string cen = split[3];
                    string[] cent = cen.Split(',');
                    center.X = (float)cent[0].ConvertToDouble() * bmp.Width;
                    center.Y = (float)cent[1].ConvertToDouble() * bmp.Height;
                } catch { }

                try
                {
                    Strength = (float)split[4].ConvertToDouble();
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

                await Program.SendBitmap(output, message.Channel);
            })
        };
        static SemaphoreSlim memifyLock = new SemaphoreSlim(1, 1);
        static SemaphoreSlim memifyTextLock = new SemaphoreSlim(1, 1);
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
            float maxDistance = (float)(center / div).LengthSquared();
            switch (mode)
            {
                case TransformMode.Expand:
                    transformedLength = (float)(diff / div).LengthSquared() * strength;
                    target = point - diff * (1 / (1 + transformedLength)) * (1 / (1 + transformedLength));
                    break;

                case TransformMode.Collapse:
                    transformedLength = (float)(diff / div).LengthSquared() * strength;
                    target = point + diff * (1 / (1 + transformedLength)) * (1 / (1 + transformedLength));
                    break;

                case TransformMode.Stir:
                    transformedLength = (float)(diff / div).LengthSquared() * strength;
                    rotationAngle = (float)Math.Pow((maxDistance - transformedLength), 5) / 3000;
                    cos = Math.Cos(rotationAngle);
                    sin = Math.Sin(rotationAngle);
                    target = new Vector2((float)(cos * (point.X - center.X) - sin * (point.Y - center.Y) + center.X),
                                         (float)(sin * (point.X - center.X) + cos * (point.Y - center.Y) + center.Y));
                    break;

                case TransformMode.Fall:
                    transformedLength = (float)(diff / div).LengthSquared() * strength;
                    rotationAngle = transformedLength / 3;
                    cos = Math.Cos(rotationAngle);
                    sin = Math.Sin(rotationAngle);
                    target = new Vector2((float)(cos * (point.X - center.X) - sin * (point.Y - center.Y) + center.X),
                                         (float)(sin * (point.X - center.X) + cos * (point.Y - center.Y) + center.Y));
                    break;
            }

            if (float.IsNaN((float)target.X) || float.IsInfinity((float)target.X))
                target.X = point.X;
            if (float.IsNaN((float)target.Y) || float.IsInfinity((float)target.Y))
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
        private static Rectangle FindRectangle(Bitmap Pic, System.Drawing.Color C, int MinSize)
        {
            for (int x = 1; x < Pic.Width; x++)
                for (int y = 1; y < Pic.Height; y++)
                    if (IsSameColor(Pic.GetPixel(x, y), C))
                    {
                        int a = x;
                        while (a < Pic.Width && IsSameColor(Pic.GetPixel(a, y), C))
                            a++;

                        int b = y;
                        while (b < Pic.Height && IsSameColor(Pic.GetPixel(x, b), C))
                            b++;

                        if (a - x > MinSize && b - y > MinSize)
                            return new Rectangle(x, y, a - x - 1, b - y - 1);
                    }

            return new Rectangle();
        }
        private static bool IsSameColor(System.Drawing.Color C1, System.Drawing.Color C2)
        {
            return Math.Abs(C1.R - C2.R) < 5 && Math.Abs(C1.G - C2.G) < 5 && Math.Abs(C1.B - C2.B) < 5;
        }

        public class EditLastCommand
        {
            public bool textBased;
            public string command, desc;
            public Func<SocketMessage, IMessage, string, Task> execute;

            public EditLastCommand(string command, string desc, bool textBased, Func<SocketMessage, IMessage, string, Task> execute)
            {
                this.textBased = textBased;
                this.command = command;
                this.desc = desc;
                this.execute = execute;
            }
        }
    }
}
