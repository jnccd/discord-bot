using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using XnaGeometry;
using Color = System.Drawing.Color;

namespace MEE7.Commands
{
    public partial class Edit : Command
    {
        enum TransformMode { Expand, Collapse, Stir, Fall }
        static readonly object memifyLock = new object();

        readonly EditCommand[] PictureCommands = new EditCommand[] {
            new EditCommand("colorChannelSwap", "Swap the rgb color channels for each pixel", (SocketMessage m, string a, object o) => {
                Bitmap bmp = (o as Bitmap);

                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Color c = bmp.GetPixel(x, y);
                        c = Color.FromArgb(c.B, c.R, c.G);
                        bmp.SetPixel(x, y, c);
                    }

                return bmp;
            }, typeof(Bitmap)),
            new EditCommand("invert", "Invert the color of each pixel", (SocketMessage m, string a, object o) => {
                Bitmap bmp = (o as Bitmap);

                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Color c = bmp.GetPixel(x, y);
                        c = Color.FromArgb(255 - c.R, 255 - c.G, 255 - c.B);
                        bmp.SetPixel(x, y, c);
                    }

                return bmp;
            }, typeof(Bitmap)),
            new EditCommand("Rekt", "Finds colored rectangles in pictures", (SocketMessage m, string a, object o) => {
                Bitmap bmp = (o as Bitmap);
                Bitmap output = new Bitmap(bmp.Width, bmp.Height);

                Color c;
                if (string.IsNullOrWhiteSpace(a))
                    c = Color.FromArgb(254, 34, 34);
                else
                    c = Color.FromName(a);
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
            }, typeof(Bitmap)),
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
                        Rectangle redRekt = FindRectangle((Bitmap)Bitmap.FromFile(memeTemplateDesign), Color.FromArgb(254, 34, 34), 20);
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
                        Rectangle redRekt = FindRectangle((Bitmap)Bitmap.FromFile(memeTemplateDesign), Color.FromArgb(254, 34, 34), 20);
                        if (redRekt.Width == 0)
                            redRekt = FindRectangle((Bitmap)Bitmap.FromFile(memeTemplateDesign), Color.FromArgb(255, 0, 0), 20);
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
            }, typeof(Bitmap)),
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
                    Rectangle redRekt = FindRectangle(design, Color.FromArgb(255, 0, 0), 20);
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

                Vector2 Transform(Vector2 point, Vector2 centerT, Bitmap within, float strength, TransformMode modeT)
                {
                    Vector2 diff = point - centerT;
                    Vector2 move = diff;
                    move.Normalize();

                    Vector2 target = Vector2.Zero;
                    float transformedLength = 0;
                    float rotationAngle = 0;
                    double cos = 0;
                    double sin = 0;
                    float div = ((within.Width + within.Height) / 7);
                    float maxDistance = (float)(centerT / div).LengthSquared();
                    switch (modeT)
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
                            target = new Vector2((float)(cos * (point.X - centerT.X) - sin * (point.Y - centerT.Y) + centerT.X),
                                                 (float)(sin * (point.X - centerT.X) + cos * (point.Y - centerT.Y) + centerT.Y));
                            break;

                        case TransformMode.Fall:
                            transformedLength = (float)(diff / div).LengthSquared() * strength;
                            rotationAngle = transformedLength / 3;
                            cos = Math.Cos(rotationAngle);
                            sin = Math.Sin(rotationAngle);
                            target = new Vector2((float)(cos * (point.X - centerT.X) - sin * (point.Y - centerT.Y) + centerT.X),
                                                 (float)(sin * (point.X - centerT.X) + cos * (point.Y - centerT.Y) + centerT.Y));
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
            }, typeof(Bitmap)),
            new EditCommand("sobelEdges", "´Highlights horizontal edges", (SocketMessage m, string a, object o) => {
                    return ApplyKernel(o as Bitmap, new int[3,3] { {  1,  2,  1 },
                                                               {  0,  0,  0 },
                                                               { -1, -2, -1 } }, 1, true);
                }, typeof(Bitmap)),
            new EditCommand("sobelEdgesColor", "´Highlights horizontal edges", (SocketMessage m, string a, object o) => {
                return ApplyKernel(o as Bitmap, new int[3,3] { {  1,  2,  1 },
                                                               {  0,  0,  0 },
                                                               { -1, -2, -1 } });
            }),
            new EditCommand("sharpen", "well guess what it does", (SocketMessage m, string a, object o) => {
                return ApplyKernel(o as Bitmap, new int[3,3] { {  0, -1,  0 },
                                                               { -1,  5, -1 },
                                                               {  0, -1,  0 } });
            }, typeof(Bitmap)),
            new EditCommand("boxBlur", "blur owo", (SocketMessage m, string a, object o) => {
                return ApplyKernel(o as Bitmap, new int[3,3] { {  1,  1,  1 },
                                                               {  1,  1,  1 },
                                                               {  1,  1,  1 } }, 1/9f);
            }, typeof(Bitmap)),
            new EditCommand("gaussianBlur", "more blur owo", (SocketMessage m, string a, object o) => {
                    return ApplyKernel(o as Bitmap, new int[3,3] { {  1,  2,  1 },
                                                                   {  2,  4,  2 },
                                                                   {  1,  2,  1 } }, 1/16f);
                }, typeof(Bitmap)),
            new EditCommand("jkrowling", "Gay rights", (SocketMessage m, string a, object o) => {
                return FlagColor(new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Purple }, o as Bitmap);
            }, typeof(Bitmap)),
            new EditCommand("merkel", "German rights", (SocketMessage m, string a, object o) => {
                return FlagColor(new Color[] { Color.Black, Color.Red, Color.Yellow }, o as Bitmap);
            }, typeof(Bitmap)),
            new EditCommand("transRights", "The input image says trans rights", (SocketMessage m, string a, object o) => {
                return FlagColor(new Color[] { Color.LightBlue, Color.Pink, Color.White, Color.Pink, Color.LightBlue }, o as Bitmap);
            }, typeof(Bitmap)),
        };
        static Rectangle FindRectangle(Bitmap Pic, Color C, int MinSize)
        {
            bool IsSameColor(Color C1, Color C2)
            {
                return Math.Abs(C1.R - C2.R) < 10 && Math.Abs(C1.G - C2.G) < 10 && Math.Abs(C1.B - C2.B) < 10;
            }

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
                        output.SetPixel(x, y, Color.FromArgb(activation, activation, activation));
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
                        output.SetPixel(x, y, Color.FromArgb(activation[0], activation[1], activation[2]));
                    }
            }

            return output;
        }
        static Bitmap FlagColor(Color[] Cs, Bitmap P, bool Horz = false)
        {
            bool ShouldBeRecolored(Color C) =>
                new Color[] { P.GetPixel(0, 0),
                    P.GetPixel(0, P.Height - 1), P.GetPixel(P.Width - 1, 0),
                    P.GetPixel(P.Width - 1, P.Height - 1) }.
                    Select(c => Math.Abs(c.GetColorDiff(C)) > 70).
                    Aggregate((x, y) => x && y) && C.A > 5;
            return FlagColor(ShouldBeRecolored, Cs, P, Horz);
        }
        static Bitmap FlagColor(Func<Color, bool> ShouldBeRecolored, Color[] Cs, Bitmap P, bool Horz = false)
        {
            for (int x = 0; x < P.Width; x++)
                for (int y = 0; y < P.Height; y++)
                {
                    Color c = P.GetPixel(x, y);
                    if (ShouldBeRecolored(c))
                        if (Horz)
                            P.SetPixel(x, y, Cs[x * Cs.Length / P.Width]);
                        else
                            P.SetPixel(x, y, Cs[y * Cs.Length / P.Height]);
                }
            return P;
        }
    }
}
