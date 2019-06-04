using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using XnaGeometry;
using MEE7;
using System.IO;

namespace MEE7.Commands
{
    public class Edit : Command
    {
        public Edit() : base("edit", "Edit stuff using various functions")
        {
            Commands = InputCommands.Union(TextCommands.Union(PictureCommands));

            HelpMenu = new EmbedBuilder();
            HelpMenu.WithDescription("Operators:\n" +
                "> Concatinates functions\n" +
                "() Let you add additional arguments for the command (optional)\n" +
               $"\neg. {PrefixAndCommand} thisT(omegaLUL) > swedish > Aestheticify\n" +
                "\nEdit Commands:");
            AddToHelpmenu("Input Commands", InputCommands);
            AddToHelpmenu("Text Commands", TextCommands);
            AddToHelpmenu("Picture Commands", PictureCommands);
        }
        void AddToHelpmenu(string Name, EditCommand[] editCommands)
        {
            string CommandToCommandTypeString(EditCommand c) => $"**{c.Command}**: " +
                //  $"`{(c.ExpectedInputType == null ? "_" : c.ExpectedInputType.ToReadableString())}` -> " +
                //  $"`{c.Function(default, "", c.ExpectedInputType.GetDefault()).GetType().ToReadableString()}`" +
                $"";
            int maxlength = editCommands.
                Select(CommandToCommandTypeString).
                Select(x => x.Length).
                Max();
            HelpMenu.AddFieldDirectly(Name, "" + editCommands.
                Select(c => CommandToCommandTypeString(c) +
                $"{new string(Enumerable.Repeat(' ', maxlength - c.Command.Length - 1).ToArray())}{c.Desc}\n").
                Combine() + "");
        }

        public override void Execute(SocketMessage message)
        {
            if (message.Content.Length <= PrefixAndCommand.Length + 1)
                Program.SendEmbed(HelpMenu, message.Channel).Wait();
            else
                PrintResult(RunCommands(message), message);
        }
        object RunCommands(SocketMessage message)
        {
            string input = message.Content.Remove(0, PrefixAndCommand.Length + 1);
            IEnumerable<string> commands = input.Split('|').First().Split('>').Select(x => x.Trim(' '));
            object currentData = null;

            if (commands.Count() > 50)
            {
                Program.SendText($"That's too many commands for one message.", message.Channel).Wait();
                return null;
            }

            foreach (string c in commands)
            {
                string cwoargs = new string(c.TakeWhile(x => x != '(').ToArray());
                string args = c.GetEverythingBetween("(", ")");

                EditCommand command = Commands.FirstOrDefault(x => x.Command.ToLower() == cwoargs.ToLower());
                if (command == null)
                {
                    Program.SendText($"I don't know a command called {cwoargs}", message.Channel).Wait();
                    return null;
                }

                if (command.InputType != null && (currentData == null || currentData.GetType() != command.InputType))
                {
                    Program.SendText($"Wrong Data Type Error in {c}\nExpected: {command.InputType}\nGot: {currentData.GetType()}", message.Channel).Wait();
                    return null;
                }

                try
                {
                    currentData = command.Function(message, args, currentData);
                }
                catch (Exception e)
                {
                    Program.SendText($"[{c}] {e.Message}",
                        message.Channel).Wait();
                    return null;
                }
            }

            return currentData;
        }
        void PrintResult(object currentData, SocketMessage message)
        {
            if (currentData is EmbedBuilder)
                Program.SendEmbed(currentData as EmbedBuilder, message.Channel).Wait();
            else if (currentData is Tuple<string, EmbedBuilder>)
            {
                var t = currentData as Tuple<string, EmbedBuilder>;
                Program.SendEmbed(t.Item2, message.Channel).Wait();
                Program.SendText(t.Item1, message.Channel).Wait();
            }
            else if (currentData is Bitmap)
                Program.SendBitmap(currentData as Bitmap, message.Channel).Wait();
            else if (currentData == null)
#pragma warning disable CS0642 // Its supposed to be like this
                ;
#pragma warning restore CS0642
            else
                Program.SendText(currentData.ToString(), message.Channel).Wait();
        }

        class EditCommand
        {
            public string Command, Desc;
            public Type InputType, OutputType;
            public Func<SocketMessage, string, object, object> Function;

            public EditCommand(string Command, string Desc, Func<SocketMessage, string, object, object> Function, Type InputType = null, Type OutputType = null)
            {
                if (Command.ContainsOneOf(new string[] { "|", ">", "<", "." }))
                    throw new IllegalCommandException("Illegal Symbol!");

                this.Command = Command;
                this.Desc = Desc;
                this.Function = Function;
                this.InputType = InputType;
                this.OutputType = OutputType;
            }
        }
        readonly IEnumerable<EditCommand> Commands;

        readonly EditCommand[] InputCommands = new EditCommand[] {
            new EditCommand("lastT", "Gets the last messages text", (SocketMessage m, string a, object o) => {
                return m.Channel.GetMessagesAsync(2).FlattenAsync().Result.Last().Content;
            }),
            new EditCommand("lastP", "Gets the last messages picture", (SocketMessage m, string a, object o) => {
                IMessage lm = m.Channel.GetMessagesAsync(2).FlattenAsync().Result.Last();
                string pic = null;
                if (lm.Attachments.Count > 0 && lm.Attachments.ElementAt(0).Size > 0)
                {
                    if (lm.Attachments.ElementAt(0).Filename.EndsWith(".png"))
                        pic = lm.Attachments.ElementAt(0).Url;
                    else if (lm.Attachments.ElementAt(0).Filename.EndsWith(".jpg"))
                        pic = lm.Attachments.ElementAt(0).Url;
                }
                string picLink = lm.Content.GetPictureLink();
                if (string.IsNullOrWhiteSpace(pic) && picLink != null)
                    pic = picLink;
                return pic.GetBitmapFromURL();
            }),
            new EditCommand("thisT", "Outputs the given arguments", (SocketMessage m, string a, object o) => {
                return a;
            }),
            new EditCommand("thisP", "Gets this messages picture / picture link in the arguments", (SocketMessage m, string a, object o) => {
                string pic = null;
                if (m.Attachments.Count > 0 && m.Attachments.ElementAt(0).Size > 0)
                {
                    if (m.Attachments.ElementAt(0).Filename.EndsWith(".png"))
                        pic = m.Attachments.ElementAt(0).Url;
                    else if (m.Attachments.ElementAt(0).Filename.EndsWith(".jpg"))
                        pic = m.Attachments.ElementAt(0).Url;
                }
                string picLink = a.GetPictureLink();
                if (string.IsNullOrWhiteSpace(pic) && picLink != null)
                    pic = picLink;
                return pic.GetBitmapFromURL();
            }),
            new EditCommand("profilePicture", "Gets a profile picture", (SocketMessage m, string a, object o) => {
                return Program.GetUserFromId(Convert.ToUInt64((a as string).Trim(new char[] { '<', '>', '@' }))).GetAvatarUrl(ImageFormat.Png, 512).GetBitmapFromURL();
            }),
        };
        readonly EditCommand[] TextCommands = new EditCommand[]
        {
            new EditCommand("swedish", "Convert the text to swedish", (SocketMessage m, string a, object o) => {
                return string.Join("", (o as string).Select(x => x + "f"));
            }, typeof(string)),
            new EditCommand("mock", "Mock the text", (SocketMessage m, string a, object o) => {
                return Program.CreateEmbedBuilder("",
                    string.Join("", (o as string).Select((x) => { return (Program.RDM.Next(2) == 1 ? char.ToUpper(x) : char.ToLower(x)); })),
                    "https://images.complex.com/complex/images/c_limit,w_680/fl_lossy,pg_1,q_auto/bujewhyvyyg08gjksyqh/spongebob", m.Author);
            }, typeof(string)),
            new EditCommand("crab", "Crab the text", (SocketMessage m, string a, object o) => {
                return ":crab: " + (o as string) + " :crab:\n https://www.youtube.com/watch?v=LDU_Txk06tM&t=75s";
            }, typeof(string)),
            new EditCommand("CAPS", "Convert text to CAPS", (SocketMessage m, string a, object o) => {
                return string.Join("", (o as string).Select((x) => { return char.ToUpper(x); }));
            }),
            new EditCommand("SUPERCAPS", "Convert text to SUPER CAPS", (SocketMessage m, string a, object o) => {
                return string.Join("", (o as string).Select((x) => { return char.ToUpper(x) + " "; }));
            }),
            new EditCommand("CopySpoilerify", "Convert text to a spoiler", (SocketMessage m, string a, object o) => {
                return "`" + string.Join("", (o as string).Select((x) => { return "||" + x + "||"; })) + "`";
            }),
            new EditCommand("Spoilerify", "Convert text to a spoiler", (SocketMessage m, string a, object o) => {
                return string.Join("", (o as string).Select((x) => { return "||" + x + "||"; }));
            }),
            new EditCommand("Unspoilerify", "Convert spoiler text to readable text", (SocketMessage m, string a, object o) => {
                return (o as string).Replace("|", "");
            }),
            new EditCommand("Aestheticify", "Convert text to Ａｅｓｔｈｅｔｉｃ text", (SocketMessage m, string a, object o) => {
                return (o as string).Select(x => x == ' ' || x == '\n' ? x : (char)(x - '!' + '！')).Foldl("", (x, y) => x + y);
            }),
        };
        readonly EditCommand[] PictureCommands = new EditCommand[] {
            new EditCommand("colorChannelSwap", "Swap the rgb color channels for each pixel", (SocketMessage m, string a, object o) => {
                Bitmap bmp = (o as Bitmap);

                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        System.Drawing.Color c = bmp.GetPixel(x, y);
                        c = System.Drawing.Color.FromArgb(c.B, c.R, c.G);
                        bmp.SetPixel(x, y, c);
                    }

                return bmp;
            }, typeof(Bitmap)),
            new EditCommand("invert", "Invert the color of each pixel", (SocketMessage m, string a, object o) => {
                Bitmap bmp = (o as Bitmap);

                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        System.Drawing.Color c = bmp.GetPixel(x, y);
                        c = System.Drawing.Color.FromArgb(255 - c.R, 255 - c.G, 255 - c.B);
                        bmp.SetPixel(x, y, c);
                    }

                return bmp;
            }, typeof(Bitmap)),
            new EditCommand("Rekt", "Finds colored rectangles in pictures", (SocketMessage m, string a, object o) => {
                Bitmap bmp = (o as Bitmap);
                Bitmap output = new Bitmap(bmp.Width, bmp.Height);

                System.Drawing.Color c;
                if (string.IsNullOrWhiteSpace(a))
                    c = System.Drawing.Color.FromArgb(254, 34, 34);
                else
                    c = System.Drawing.Color.FromName(a);
                Rectangle redRekt = FindRectangle(bmp, c, 20);

                if (redRekt.Width == 0)
                    throw new Exception("No rekt!");
                else
                {
                    using (Graphics graphics = Graphics.FromImage(output))
                    {
                        graphics.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));
                        graphics.DrawRectangle(Pens.Red, redRekt);
                    }

                    return output;
                }
            }),
            new EditCommand("memify", "Turn a picture into a meme, get a list of available templates with the argument -list",
                (SocketMessage m, string a, object o) => {
                lock (memifyLock)
                {
                    string[] files = Directory.GetFiles("Commands\\MemeTemplates");
                    string[] split = a.Split(' ');

                    string memeTemplateDesign = "";
                    if (a == "-list")
                        throw new Exception("Templates: \n\n" + files.Select(x => Path.GetFileNameWithoutExtension(x)).
                                                                        Where(x => x.EndsWith("design")).
                                                                        Select(x => x.RemoveLastGroup('-').RemoveLastGroup('-')).
                                                                        Aggregate((x, y) => x + "\n" + y));
                    else if (!string.IsNullOrWhiteSpace(a))
                        memeTemplateDesign = files.Where(x => Path.GetFileNameWithoutExtension(x).StartsWith(a) &&
                                                              Path.GetFileNameWithoutExtension(x).EndsWith("design")).ToArray().FirstOrDefault();
                    else
                        memeTemplateDesign = files.Where(x => Path.GetFileNameWithoutExtension(x).EndsWith("design")).ToArray().GetRandomValue();

                    if (string.IsNullOrWhiteSpace(memeTemplateDesign))
                        throw new Exception("I don't have that meme in my registry!");

                    string memeName = memeTemplateDesign.RemoveLastGroup('-');
                    string memeTemplate = files.FirstOrDefault(x => x.StartsWith(memeName) && !x.Contains("-design."));
                    string memeTemplateOverlay = files.FirstOrDefault(x => x.StartsWith(memeName) && Path.GetFileNameWithoutExtension(x).EndsWith("overlay"));

                    Bitmap bmp = o as Bitmap;
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

                        return output;
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

                        return template;
                    }
                    else
                        throw new Exception("Something went wrong :thinking:");
                }
            }),
            new EditCommand("textMemify", "Turn the last Picture into a meme, get a list of available templates with the argument -list, " +
                "additional arguments are -f for the font, -r for the number of text lines and of course -m for the meme", (SocketMessage m, string a, object o) => {
                string[] files = Directory.GetFiles("Commands\\MemeTextTemplates");
                List<string> split = a.Split(' ').ToList();

                if (split.Contains("-list"))
                    throw new Exception("Templates: \n\n" + files.Select(x => Path.GetFileNameWithoutExtension(x)).
                                                                Where(x => x.EndsWith("-design")).
                                                                Select(x => x.Remove(x.IndexOf("-design"), "-design".Length)).
                                                                Aggregate((x, y) => x + "\n" + y));

                string font = "Arial";
                float fontSize = 1;
                int index = split.FindIndex(x => x == "-f");
                if (index != -1)
                {
                    font = split[index + 1];
                    split.RemoveRange(index, 2);
                }
                index = split.FindIndex(x => x == "-r");
                if (index != -1)
                {
                    fontSize = (float)split[index + 1].ConvertToDouble();
                    split.RemoveRange(index, 2);
                }
                index = split.FindIndex(x => x == "-m");
                string memeTemplate = "";
                string memeDesign = "";
                if (index != -1)
                {
                    memeDesign = files.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).StartsWith(split[index + 1]) &&
                        Path.GetFileNameWithoutExtension(x).EndsWith("-design"));
                    if (memeDesign == null)
                        throw new Exception("Error, couldn't find that meme design!");
                    memeTemplate = files.FirstOrDefault(x => memeDesign.Contains(Path.GetFileNameWithoutExtension(x)) && !x.Contains("design"));
                    split.RemoveRange(index, 2);
                }
                else
                {
                    memeDesign = files.Where(x => x.Contains("-design")).GetRandomValue();
                    memeTemplate = files.FirstOrDefault(x => memeDesign.Contains(Path.GetFileNameWithoutExtension(x)) && !x.Contains("design"));
                }

                if (string.IsNullOrWhiteSpace(memeTemplate))
                    throw new Exception("I don't have that meme in my registry!");

                if (File.Exists(memeTemplate))
                {
                    Bitmap template, design;
                    using (FileStream stream = new FileStream(memeTemplate, FileMode.Open))
                        template = (Bitmap)Bitmap.FromStream(stream);
                    using (FileStream stream = new FileStream(memeDesign, FileMode.Open))
                        design = (Bitmap)Bitmap.FromStream(stream);
                    Rectangle redRekt = FindRectangle(design, System.Drawing.Color.FromArgb(255, 0, 0), 20);
                    if (redRekt.Width == 0)
                        throw new Exception("Error, couldn't find a rectangle to write in!");
                    fontSize = redRekt.Height / 5f / fontSize;
                    using (Graphics graphics = Graphics.FromImage(template))
                        graphics.DrawString(o as string, new Font(font, fontSize), Brushes.Black, redRekt);

                    return template;
                }
                else
                    throw new Exception("uwu");
            }),
            new EditCommand("liq", "Liquidify the picture with either expand, collapse, stir or fall.\n" +
                "Without any arguments it will automatically call \"expand 0.5,0.5 1\"" +
                "\nThe argument syntax is: [mode] [position, eg. 0.5,1 to center the transformation at the middle of the bottom of the picture] " +
                "[strength, eg. 0.7, for 70% transformation strength]",
                (SocketMessage m, string a, object o) => {
                Bitmap bmp = o as Bitmap;
                Bitmap output = new Bitmap(bmp.Width, bmp.Height);
                Vector2 center = new Vector2(bmp.Width / 2, bmp.Height / 2);
                float Strength = 1;
                string[] split = a.Split(new char[] { ' ', '\n' });

                TransformMode mode = TransformMode.Expand;
                try
                {
                    Enum.TryParse(split[0].ToCapital(), out mode);
                } catch { }

                try
                {
                    string cen = split[1];
                    string[] cent = cen.Split(',');
                    center.X = (float)cent[0].ConvertToDouble() * bmp.Width;
                    center.Y = (float)cent[1].ConvertToDouble() * bmp.Height;
                } catch { }

                try
                {
                    Strength = (float)split[2].ConvertToDouble();
                } catch { }

                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Vector2 target = Transform(new Vector2(x, y), center, bmp, Strength, mode);
                        output.SetPixel(x, y, bmp.GetPixel((int)target.X, (int)target.Y));
                    }

                return output;
            }),
            new EditCommand("sobelEdges", "´Highlights horizontal edges", (SocketMessage m, string a, object o) =>
                ApplyKernel(o as Bitmap, new int[3,3] { {  1,  2,  1 },
                                                        {  0,  0,  0 },
                                                        { -1, -2, -1 } }, 1, true)),
            new EditCommand("sobelEdgesColor", "´Highlights horizontal edges", (SocketMessage m, string a, object o) =>
                ApplyKernel(o as Bitmap, new int[3,3] { {  1,  2,  1 },
                                                        {  0,  0,  0 },
                                                        { -1, -2, -1 } })),
            new EditCommand("sharpen", "well guess what it does", (SocketMessage m, string a, object o) =>
                ApplyKernel(o as Bitmap, new int[3,3] { {  0, -1,  0 },
                                                        { -1,  5, -1 },
                                                        {  0, -1,  0 } })),
            new EditCommand("boxBlur", "well guess what it does", (SocketMessage m, string a, object o) =>
                ApplyKernel(o as Bitmap, new int[3,3] { {  1,  1,  1 },
                                                        {  1,  1,  1 },
                                                        {  1,  1,  1 } })),
            new EditCommand("gaussianBlur", "well guess what it does", (SocketMessage m, string a, object o) =>
                ApplyKernel(o as Bitmap, new int[3,3] { {  1,  2,  1 },
                                                        {  2,  4,  2 },
                                                        {  1,  2,  1 } }, 1/16f)),
        };
        static readonly object memifyLock = new object();
        static Vector2 Transform(Vector2 point, Vector2 center, Bitmap within, float strength, TransformMode mode)
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
        enum TransformMode { Expand, Collapse, Stir, Fall }
        static Rectangle FindRectangle(Bitmap Pic, System.Drawing.Color C, int MinSize)
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
        static bool IsSameColor(System.Drawing.Color C1, System.Drawing.Color C2)
        {
            return Math.Abs(C1.R - C2.R) < 10 && Math.Abs(C1.G - C2.G) < 10 && Math.Abs(C1.B - C2.B) < 10;
        }
        static Bitmap ApplyKernel(Bitmap bmp, int[,] kernel, float factor = 1, bool grayscale = false)
        {
            int kernelW = kernel.GetLength(0);
            int kernelH = kernel.GetLength(1);
            Bitmap output = new Bitmap(bmp.Width - kernel.GetLength(0) + 1, bmp.Height - kernel.GetLength(1) + 1);

            if (grayscale)
            {
                for (int x = 0; x < output.Width; x++)
                    for (int y = 0; y < output.Height; y++)
                    {
                        int activation = 0;
                        for (int xk = x; xk < x + kernelW; xk++)
                            for (int yk = y; yk < y + kernelH; yk++)
                                activation += kernel[xk - x, yk - y] * bmp.GetPixel(xk, yk).GetGrayScale();
                        activation = (int)(activation * factor);
                        activation += 255 / 2;
                        if (activation > 255)
                            activation = 255;
                        if (activation < 0)
                            activation = 0;
                        output.SetPixel(x, y, System.Drawing.Color.FromArgb(activation, activation, activation));
                    }
            }
            else
            {
                for (int x = 0; x < output.Width; x++)
                    for (int y = 0; y < output.Height; y++)
                    {
                        int[] activation = new int[3] { 0, 0, 0 };
                        for (int i = 0; i < activation.Length; i++)
                        {
                            for (int xk = x; xk < x + kernelW; xk++)
                                for (int yk = y; yk < y + kernelH; yk++)
                                    activation[i] += kernel[xk - x, yk - y] * (i == 0 ?
                                        bmp.GetPixel(xk, yk).R : (i == 1 ?
                                        bmp.GetPixel(xk, yk).G : bmp.GetPixel(xk, yk).B));
                            activation[i] = (int)(activation[i] * factor);
                            activation[i] += 255 / 2;
                            if (activation[i] > 255)
                                activation[i] = 255;
                            if (activation[i] < 0)
                                activation[i] = 0;
                        }
                        output.SetPixel(x, y, System.Drawing.Color.FromArgb(activation[0], activation[1], activation[2]));
                    }
            }

            return output;
        }
    }
}
