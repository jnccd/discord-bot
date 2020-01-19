using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using BumpKit;
using System.Numerics;
using MEE7.Backend;
using Color = System.Drawing.Color;
using MEE7.Backend.HelperFunctions.Extensions;
using MEE7.Backend.HelperFunctions;
using System.Drawing.Imaging;
using System.Web;
using System.Net;

namespace MEE7.Commands
{
    public partial class Edit : Command
    {
        enum TransformMode { Expand, Stir, Fall, Wubble, Cya, Inpand }
        static readonly object memifyLock = new object();

        readonly EditCommand[] PictureCommands = new EditCommand[] {
            new EditCommand("colorChannelSwap", "Swap the rgb color channels for each pixel", typeof(Bitmap), typeof(Bitmap), new Argument[0],
                (SocketMessage m, object[] a, object o) => {

                Bitmap bmp = (o as Bitmap);

                using (UnsafeBitmapContext con = new UnsafeBitmapContext(bmp))
                    for (int x = 0; x < bmp.Width; x++)
                        for (int y = 0; y < bmp.Height; y++)
                        {
                            Color c = con.GetPixel(x, y);
                            c = Color.FromArgb(c.B, c.R, c.G);
                            con.SetPixel(x, y, c);
                        }

                return bmp;
            }),
            new EditCommand("reddify", "Make it red af", typeof(Bitmap), typeof(Bitmap), new Argument[0],
                (SocketMessage m, object[] a, object o) => {

                Bitmap bmp = (o as Bitmap);

                using (UnsafeBitmapContext con = new UnsafeBitmapContext(bmp))
                    for (int x = 0; x < bmp.Width; x++)
                        for (int y = 0; y < bmp.Height; y++)
                        {
                            Color c = con.GetPixel(x, y);
                            c = Color.FromArgb(c.R, 0, 0, 255);
                            con.SetPixel(x, y, c);
                        }

                return bmp;
            }),
            new EditCommand("invert", "Invert the color of each pixel", typeof(Bitmap), typeof(Bitmap), new Argument[0],
                (SocketMessage m, object[] a, object o) => {

                Bitmap bmp = (o as Bitmap);

                using (UnsafeBitmapContext con = new UnsafeBitmapContext(bmp))
                    for (int x = 0; x < bmp.Width; x++)
                        for (int y = 0; y < bmp.Height; y++)
                        {
                            Color c = con.GetPixel(x, y);
                            c = Color.FromArgb(255 - c.R, 255 - c.G, 255 - c.B);
                            con.SetPixel(x, y, c);
                        }

                return bmp;
            }),
            new EditCommand("Rekt", "Finds colored rectangles in pictures", typeof(Bitmap), typeof(Bitmap),
                new Argument[] { new Argument("Color", typeof(string), "") },
                (SocketMessage m, object[] a, object o) => {

                Bitmap bmp = (o as Bitmap);
                Bitmap output = new Bitmap(bmp.Width, bmp.Height);

                Color c;
                if (string.IsNullOrWhiteSpace(a[0] as string))
                    c = Color.FromArgb(254, 34, 34);
                else
                    c = Color.FromName(a[0] as string);
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
            new EditCommand("memify", "Turn a picture into a meme, get a list of available templates with the argument -list", typeof(Bitmap), typeof(Bitmap),
                new Argument[] { new Argument("Meme", typeof(string), "") },
                (SocketMessage m, object[] a, object o) => {

                lock (memifyLock)
                {
                    string[] files = Directory.GetFiles($"Commands{Path.DirectorySeparatorChar}MemeTemplates");

                    string memeTemplateDesign = "";
                    if (a[0] as string == "-list")
                        throw new Exception("Templates: \n\n" + files.Select(x => Path.GetFileNameWithoutExtension(x)).
                                                                        Where(x => x.EndsWith("design")).
                                                                        Select(x => x.RemoveLastGroup('-').RemoveLastGroup('-')).
                                                                        Aggregate((x, y) => x + "\n" + y));
                    else if (!string.IsNullOrWhiteSpace(a[0] as string))
                        memeTemplateDesign = files.Where(x => Path.GetFileNameWithoutExtension(x).StartsWith(a[0] as string) &&
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
            }),
            new EditCommand("textMemify", "Put text into a meme template, input -list as Meme and get a list templates\n" +
                "The default Font is Arial and the fontsize refers to the number of rows of text that are supposed to fit into the textbox", typeof(string), typeof(Bitmap),
                new Argument[] {
                    new Argument("Meme", typeof(string), ""),
                    new Argument("Font", typeof(string), "Arial"),
                    new Argument("FontSize", typeof(float), 1)
                },
                (SocketMessage m, object[] a, object o) => {

                string[] files = Directory.GetFiles($"Commands{Path.DirectorySeparatorChar}MemeTextTemplates");
                string memeName = a[0] as string;

                if (memeName.Contains("-list"))
                    throw new Exception("Templates: \n\n" + files.Select(x => Path.GetFileNameWithoutExtension(x)).
                                                                Where(x => x.EndsWith("-design")).
                                                                Select(x => x.Remove(x.IndexOf("-design"), "-design".Length)).
                                                                Aggregate((x, y) => x + "\n" + y));

                string memeTemplate = "";
                string memeDesign = "";
                if (memeName != "")
                {
                    memeDesign = files.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).StartsWith(memeName) &&
                        Path.GetFileNameWithoutExtension(x).EndsWith("-design"));
                    if (memeDesign == null)
                        throw new Exception("Error, couldn't find that meme design!");
                    memeTemplate = files.FirstOrDefault(x => memeDesign.Contains(Path.GetFileNameWithoutExtension(x)) && !x.Contains("design"));
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
                    float fontSize = redRekt.Height / 5f / (float)a[2];
                    using (Graphics graphics = Graphics.FromImage(template))
                        graphics.DrawString(o as string, new Font(a[1] as string, fontSize), Brushes.Black, redRekt);

                    return template;
                }
                else
                    throw new Exception("uwu");
            }),
            new EditCommand("expand", "Expand the pixels", typeof(Bitmap), typeof(Bitmap), new Argument[] {
                    new Argument("Position", typeof(Vector2), new Vector2(0.5f, 0.5f)),
                    new Argument("Strength", typeof(float), 1f),
                },
                (SocketMessage m, object[] a, object o) => {

                Bitmap bmp = o as Bitmap;

                Vector2 position = (Vector2)a[0];
                Vector2 center = new Vector2(position.X * bmp.Width, position.Y * bmp.Height);
                float strength = (float)a[1];

                return ApplyTransformation(o as Bitmap,
                    (x, y) => {
                        Vector2 point = new Vector2(x, y);
                        Vector2 diff = point - center;
                        float div = bmp.Width + bmp.Height;
                        float maxDistance = (center / div).LengthSquared();

                        float transformedLength = (diff / div).LengthSquared() * (1 / strength / 10);
                        return point - diff * (1 / (1 + transformedLength)) * (1 / (1 + transformedLength));
                    });
            }),
            new EditCommand("stir", "Stir the pixels", typeof(Bitmap), typeof(Bitmap), new Argument[] {
                    new Argument("Position", typeof(Vector2), new Vector2(0.5f, 0.5f)),
                    new Argument("Strength", typeof(float), 1f),
                },
                (SocketMessage m, object[] a, object o) => {

                Bitmap bmp = o as Bitmap;

                Vector2 position = (Vector2)a[0];
                Vector2 center = new Vector2(position.X * bmp.Width, position.Y * bmp.Height);
                float strength = (float)a[1];

                return ApplyTransformation(o as Bitmap,
                    (x, y) => {
                            Vector2 point = new Vector2(x, y);
                            Vector2 diff = point - center;
                            float div = bmp.Width + bmp.Height;
                            float maxDistance = (center / div).LengthSquared();

                            float transformedLength = (diff / div * 7).LengthSquared();
                            float rotationAngle = (float)Math.Pow(2, - transformedLength * transformedLength) * strength;
                            double cos = Math.Cos(rotationAngle);
                            double sin = Math.Sin(rotationAngle);
                            return new Vector2((float)(cos * (point.X - center.X) - sin * (point.Y - center.Y) + center.X),
                                                 (float)(sin * (point.X - center.X) + cos * (point.Y - center.Y) + center.Y));
                    });
            }),
            new EditCommand("fall", "Fall the pixels", typeof(Bitmap), typeof(Bitmap), new Argument[] {
                    new Argument("Position", typeof(Vector2), new Vector2(0.5f, 0.5f)),
                    new Argument("Strength", typeof(float), 1f),
                },
                (SocketMessage m, object[] a, object o) => {

                Bitmap bmp = o as Bitmap;

                Vector2 position = (Vector2)a[0];
                Vector2 center = new Vector2(position.X * bmp.Width, position.Y * bmp.Height);
                float strength = (float)a[1];

                return ApplyTransformation(o as Bitmap,
                    (x, y) => {
                            Vector2 point = new Vector2(x, y);
                            Vector2 diff = point - center;
                            float div = bmp.Width + bmp.Height;
                            float maxDistance = (center / div).LengthSquared();

                            float transformedLength = (diff / div * 7).LengthSquared();
                            float rotationAngle = -transformedLength / 3 * strength;
                            double cos = Math.Cos(rotationAngle);
                            double sin = Math.Sin(rotationAngle);
                            return new Vector2((float)(cos * (point.X - center.X) - sin * (point.Y - center.Y) + center.X),
                                                 (float)(sin * (point.X - center.X) + cos * (point.Y - center.Y) + center.Y));
                    });
            }),
            new EditCommand("wubble", "Wubble the pixels", typeof(Bitmap), typeof(Bitmap), new Argument[] {
                    new Argument("Position", typeof(Vector2), new Vector2(0.5f, 0.5f)),
                    new Argument("Strength", typeof(float), 1f),
                },
                (SocketMessage m, object[] a, object o) => {

                Bitmap bmp = o as Bitmap;

                Vector2 position = (Vector2)a[0];
                Vector2 center = new Vector2(position.X * bmp.Width, position.Y * bmp.Height);
                float strength = (float)a[1];

                return ApplyTransformation(o as Bitmap,
                    (x, y) => {
                            Vector2 point = new Vector2(x, y);
                            Vector2 diff = point - center;
                            float div = bmp.Width + bmp.Height;

                            float transformedLength = (diff / div * 7).LengthSquared();
                            return point - diff * (1 / (1 + transformedLength * -strength));
                    });
            }),
            new EditCommand("cya", "Cya the pixels", typeof(Bitmap), typeof(Bitmap), new Argument[] {
                    new Argument("Position", typeof(Vector2), new Vector2(0.5f, 0.5f)),
                    new Argument("Strength", typeof(float), 1f),
                },
                (SocketMessage m, object[] a, object o) => {

                Bitmap bmp = o as Bitmap;

                Vector2 position = (Vector2)a[0];
                Vector2 center = new Vector2(position.X * bmp.Width, position.Y * bmp.Height);
                float strength = (float)a[1];

                return ApplyTransformation(o as Bitmap,
                    (x, y) => {
                            Vector2 point = new Vector2(x, y);
                            Vector2 diff = point - center;
                            float div = bmp.Width + bmp.Height;

                            float transformedLength = (diff / div * 7).Length();
                            return point - diff * (float)(-Math.Pow(2, -transformedLength * strength) + 1);
                    });
            }),
            new EditCommand("inpand", "Inpand the pixels", typeof(Bitmap), typeof(Bitmap), new Argument[] {
                    new Argument("Position", typeof(Vector2), new Vector2(0.5f, 0.5f)),
                    new Argument("Strength", typeof(float), 1f),
                },
                (SocketMessage m, object[] a, object o) => {

                Bitmap bmp = o as Bitmap;

                Vector2 position = (Vector2)a[0];
                Vector2 center = new Vector2(position.X * bmp.Width, position.Y * bmp.Height);
                float strength = (float)a[1];

                return ApplyTransformation(o as Bitmap,
                    (x, y) => {
                            Vector2 point = new Vector2(x, y);
                            Vector2 diff = point - center;
                            float div = bmp.Width + bmp.Height;

                            float transformedLength = (float)(diff / div * 7).LengthSquared();
                            return point - diff * (1 / (1 + transformedLength) * strength / 4);
                    });
            }),
            new EditCommand("blockify", "Blockify the pixels", typeof(Bitmap), typeof(Bitmap), new Argument[] {
                    new Argument("Frequenzy", typeof(float), 1f),
                    new Argument("Strength", typeof(float), 1f),
                    new Argument("OffsetX", typeof(int), 1),
                    new Argument("OffsetY", typeof(int), 1),
                },
                (SocketMessage m, object[] a, object o) => {

                float frequenzy = (float)a[0];
                float strength = (float)a[1];
                int offsetX = (int)a[2];
                int offsetY = (int)a[3];

                return ApplyTransformation(o as Bitmap,
                    (x, y) => new Vector2(  x + (float)Math.Cos(x / frequenzy + offsetX) * strength,
                                            y + (float)Math.Sin(y / frequenzy + offsetY) * strength));
            }),
            new EditCommand("squiggle", "Squiggle the pixels", typeof(Bitmap), typeof(Bitmap), new Argument[] {
                    new Argument("Percent", typeof(float), 1f),
                    new Argument("Scale", typeof(float), 1f),
                    new Argument("OffsetX", typeof(int), 1),
                    new Argument("OffsetY", typeof(int), 1),
                },
                (SocketMessage m, object[] a, object o) => {

                float percent = (float)a[0];
                float scale = (float)a[1];
                int offsetX = (int)a[2];
                int offsetY = (int)a[3];

                return ApplyTransformation(o as Bitmap,
                    (x, y) => new Vector2(  x + percent * (float)Math.Sin((y + offsetY) / scale),
                                            y + percent * (float)Math.Cos((x + offsetX) / scale)));
            }),
            new EditCommand("sobelEdges", "Highlights horizontal edges", typeof(Bitmap), typeof(Bitmap), new Argument[0], (SocketMessage m, object[] a, object o) => {
                    return ApplyKernel(o as Bitmap, new int[3,3] { {  1,  2,  1 },
                                                                   {  0,  0,  0 },
                                                                   { -1, -2, -1 } }, 1, true);
            }),
            new EditCommand("sobelEdgesColor", "Highlights horizontal edges", typeof(Bitmap), typeof(Bitmap), new Argument[0], (SocketMessage m, object[] a, object o) => {
                return ApplyKernel(o as Bitmap, new int[3,3] { {  1,  2,  1 },
                                                               {  0,  0,  0 },
                                                               { -1, -2, -1 } });
            }),
            new EditCommand("sharpen", "well guess what it does", typeof(Bitmap), typeof(Bitmap), new Argument[0], (SocketMessage m, object[] a, object o) => {
                return ApplyKernel(o as Bitmap, new int[3,3] { {  0, -1,  0 },
                                                               { -1,  5, -1 },
                                                               {  0, -1,  0 } }, 1/5f);
            }),
            new EditCommand("boxBlur", "blur owo", typeof(Bitmap), typeof(Bitmap), new Argument[0], (SocketMessage m, object[] a, object o) => {
                return ApplyKernel(o as Bitmap, new int[3,3] { {  1,  1,  1 },
                                                               {  1,  1,  1 },
                                                               {  1,  1,  1 } }, 1/9f);
            }),
            new EditCommand("gaussianBlur", "more blur owo", typeof(Bitmap), typeof(Bitmap), new Argument[0], (SocketMessage m, object[] a, object o) => {
                    return ApplyKernel(o as Bitmap, new int[3,3] { {  1,  2,  1 },
                                                                   {  2,  4,  2 },
                                                                   {  1,  2,  1 } }, 1/16f);
            }),
            new EditCommand("jkrowling", "Gay rights", typeof(Bitmap), typeof(Bitmap), new Argument[0], (SocketMessage m, object[] a, object o) => {
                return FlagColor(new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Purple }, o as Bitmap);
            }),
            new EditCommand("merkel", "German rights", typeof(Bitmap), typeof(Bitmap), new Argument[0], (SocketMessage m, object[] a, object o) => {
                return FlagColor(new Color[] { Color.Black, Color.Red, Color.Yellow }, o as Bitmap);
            }),
            new EditCommand("transRights", "The input image says trans rights", typeof(Bitmap), typeof(Bitmap), new Argument[0], (SocketMessage m, object[] a, object o) => {
                return FlagColor(new Color[] { Color.LightBlue, Color.Pink, Color.White, Color.Pink, Color.LightBlue }, o as Bitmap);
            }),
            new EditCommand("rainbow", "I'll try spinning colors that's a good trick!", typeof(Bitmap), typeof(Bitmap[]), new Argument[0], (SocketMessage m, object[] a, object o) => {
                Bitmap b = o as Bitmap;
                Vector3[,] HSVimage = new Vector3[b.Width, b.Height];
                int[,] Alphas = new int[b.Width, b.Height];

                using (UnsafeBitmapContext c = ImageExtensions.CreateUnsafeContext(b))
                    for (int x = 0; x < b.Width; x++)
                        for (int y = 0; y < b.Height; y++)
                        {
                            Color col = c.GetPixel(x, y);
                            Alphas[x, y] = col.A;
                            HSVimage[x, y] = new Vector3(col.GetHue(), col.GetSaturation(), col.GetValue());
                        }

                int steps = 20;
                int stepWidth = 360 / steps;
                Bitmap[] re = new Bitmap[steps];
                for (int i = 0; i < steps; i++)
                {
                    re[i] = new Bitmap(b.Width, b.Height);
                    using (UnsafeBitmapContext c = ImageExtensions.CreateUnsafeContext(re[i]))
                        for (int x = 0; x < b.Width; x++)
                            for (int y = 0; y < b.Height; y++)
                            {
                                c.SetPixel(x, y, Color.FromArgb(Alphas[x, y], HSVimage[x, y].HsvToRgb()));
                                HSVimage[x, y].X += stepWidth;
                                while (HSVimage[x, y].X > 360)
                                    HSVimage[x, y].X -= 360;
                            }
                }

                return re;
            }),
            new EditCommand("spinToWin", "I'll try spinning that's a good trick!", typeof(Bitmap), typeof(Bitmap[]), new Argument[0], (SocketMessage m, object[] a, object o) => {
                Bitmap b = o as Bitmap;
                Vector2 middle = new Vector2(b.Width / 2, b.Height / 2);

                int steps = 20;
                int stepWidth = 360 / steps;
                Bitmap[] re = new Bitmap[steps];
                for (int i = 0; i < steps; i++)
                    re[i] = b.RotateImage(-stepWidth * i, middle);

                return re;
            }),
            new EditCommand("chromaticAbberation", "Shifts the color spaces", typeof(Bitmap), typeof(Bitmap), new Argument[] { new Argument("Intensity", typeof(int), 4) },
                (SocketMessage m, object[] a, object o) => {

                    Bitmap bmp = (o as Bitmap);
                    int intesity = (int)a[0];

                    using (UnsafeBitmapContext con = new UnsafeBitmapContext(bmp))
                        for (int x = 0; x < bmp.Width; x++)
                            for (int y = 0; y < bmp.Height; y++)
                            {
                                Color r = con.GetPixel(x + intesity > bmp.Width - 1 ? bmp.Width - 1 : x + intesity, y);
                                Color g = con.GetPixel(x, y);
                                Color b = con.GetPixel(x - intesity < 0 ? 0 : x - intesity, y);
                                con.SetPixel(x, y, Color.FromArgb(g.A, r.R, g.G, b.B));
                            }

                    return bmp;
            }),
            new EditCommand("rotate", "Rotate the image", typeof(Bitmap), typeof(Bitmap), new Argument[] { new Argument("Angle in degrees", typeof(float), 0) },
                (SocketMessage m, object[] a, object o) => {
                    Bitmap b = o as Bitmap;
                    Vector2 middle = new Vector2(b.Width / 2, b.Height / 2);
                    return b.RotateImage((float)a[0], middle);
            }),
            new EditCommand("rotateColors", "Rotate the color spaces of each pixel", typeof(Bitmap), typeof(Bitmap), new Argument[] { new Argument("Angle in degrees", typeof(float), 0) },
                (SocketMessage m, object[] a, object o) => {
                    Bitmap b = o as Bitmap;

                    using (UnsafeBitmapContext c = ImageExtensions.CreateUnsafeContext(b))
                        for (int x = 0; x < b.Width; x++)
                            for (int y = 0; y < b.Height; y++)
                            {
                                Color col = c.GetPixel(x, y);
                                c.SetPixel(x, y, Color.FromArgb(col.A,
                                    new Vector3(col.GetHue() + (float)a[0], col.GetSaturation(), col.GetValue()).HsvToRgb()));
                            }

                    return b;
            }),
            new EditCommand("compress", "JPEG Compress the image", typeof(Bitmap), typeof(Bitmap), new Argument[] {
                new Argument("Compression level as unsigned integer number", typeof(long), null) },
                (SocketMessage m, object[] a, object o) => {
                    Bitmap b = o as Bitmap;
                    long compressLevel = (a[0] as long?).GetValueOrDefault();

                    ImageCodecInfo jpgEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);
                    Encoder myEncoder = Encoder.Quality;
                    EncoderParameters myEncoderParameters = new EncoderParameters(1);

                    MemoryStream tmp = new MemoryStream();
                    EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, compressLevel);
                    myEncoderParameters.Param[0] = myEncoderParameter;
                    b.Save(tmp, jpgEncoder, myEncoderParameters);

                    Bitmap output = new Bitmap(System.Drawing.Image.FromStream(tmp));

                    b.Dispose();
                    tmp.Dispose();

                    return output;
            }),
            new EditCommand("backAndForth", "Make the gif go backward after it went forward and " +
                "then it goes forward again because it loops and its all very fancy n stuff",
                typeof(Bitmap[]), typeof(Bitmap[]), new Argument[] { },
                (SocketMessage m, object[] a, object o) => {
                    Bitmap[] bs = o as Bitmap[];
                    return bs.Concat(bs.Skip(1).Reverse()).ToArray();
            }),
            new EditCommand("transground", "Make the background transparent",
                typeof(Bitmap), typeof(Bitmap), new Argument[] { 
                    new Argument("Thereshold", typeof(int), 75) },
                (SocketMessage m, object[] a, object o) => {
                    Bitmap b = o as Bitmap;
                    int Thereshold = (a[0] as int?).GetValueOrDefault();

                    using (UnsafeBitmapContext c = new UnsafeBitmapContext(b))
                    {
                        Color[] edges = new Color[] { c.GetPixel(0, 0),
                            c.GetPixel(0, c.Height - 1), c.GetPixel(c.Width - 1, 0),
                            c.GetPixel(c.Width - 1, c.Height - 1) };

                        for (int x = 0; x < b.Width; x++)
                            for (int y = 0; y < b.Height; y++)
                            {
                                Color C = c.GetPixel(x, y);
                                var dists = edges.Select(q => Math.Abs(q.GetColorDiff(C)));
                                if (dists.Min() < Thereshold)
                                    c.SetPixel(x,y, Color.FromArgb(dists.Min() > 255 ? 255 : dists.Min(), C.R, C.G, C.B));
                            }
                    }

                    return b;
            }),
            //new EditCommand("caption", "Attempts to find a good caption for the image",
            //    typeof(Bitmap), typeof(string), new Argument[] { },
            //    (SocketMessage m, object[] a, object o) => {
            //        Bitmap b = o as Bitmap;

            //        var dumpChannel = Program.GetChannelFromID(667787680855359510);
            //        var dumpedImage = DiscordNETWrapper.SendBitmap(b, (IMessageChannel)dumpChannel).Result;
            //        string url = dumpedImage.Attachments.First().Url;

            //        WebResponse imgSearchResponse = ("https://www.google.de/searchbyimage?image_url=" + 
            //            HttpUtility.UrlEncode(url) + "&encoded_image=&image_content=&filename=&hl=de").
            //            GetWebResponsefromURL();
            //        string location = imgSearchResponse.ResponseUri.AbsoluteUri;

            //        string resultHTML = location.GetHTMLfromURL();
            //        string trimmedResult = resultHTML.GetEverythingBetween("<a class=\"fKDtNb\"", "</div>");

            //        string href = "https://www.google.de" + trimmedResult.GetEverythingBetween("href=\"", "\"");
            //        string caption = trimmedResult.GetEverythingBetween("style=\"font-style:italic\">", "</a>");

            //        return caption;
            //}),
        };

        static Bitmap ApplyTransformation(Bitmap bmp, Func<int, int, Vector2> trans)
        {
            Bitmap output = new Bitmap(bmp.Width, bmp.Height);

            using (UnsafeBitmapContext ocon = new UnsafeBitmapContext(output))
            using (UnsafeBitmapContext bcon = new UnsafeBitmapContext(bmp))
                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Vector2 target = trans(x, y);

                        if (float.IsNaN((float)target.X) || float.IsInfinity((float)target.X))
                            target.X = x;
                        if (float.IsNaN((float)target.Y) || float.IsInfinity((float)target.Y))
                            target.Y = y;
                        if (target.X < 0)
                            target.X = 0;
                        if (target.X > bmp.Width - 1)
                            target.X = bmp.Width - 1;
                        if (target.Y < 0)
                            target.Y = 0;
                        if (target.Y > bmp.Height - 1)
                            target.Y = bmp.Height - 1;

                        ocon.SetPixel(x, y, bcon.GetPixel((int)target.X, (int)target.Y));
                    }

            return output;
        }
        static ImageCodecInfo GetEncoder(System.Drawing.Imaging.ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
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
                using (UnsafeBitmapContext c = new UnsafeBitmapContext(output))
                using (UnsafeBitmapContext cb = new UnsafeBitmapContext(bmp))
                    for (int x = 0; x < output.Width; x++)
                        for (int y = 0; y < output.Height; y++)
                        {
                            int activation = 0;
                            for (int xk = x; xk < x + kernelW; xk++)
                                for (int yk = y; yk < y + kernelH; yk++)
                                    activation += kernel[xk - x, yk - y] * cb.GetPixel(xk, yk).GetGrayScale();
                            activation = (int)(activation * factor);
                            activation += 255 / 2;
                            if (activation > 255)
                                activation = 255;
                            if (activation < 0)
                                activation = 0;
                            c.SetPixel(x, y, Color.FromArgb(activation, activation, activation));
                        }
            }
            else
            {
                using (UnsafeBitmapContext c = new UnsafeBitmapContext(output))
                using (UnsafeBitmapContext cb = new UnsafeBitmapContext(bmp))
                    for (int x = 0; x < output.Width; x++)
                        for (int y = 0; y < output.Height; y++)
                        {
                            int[] activation = new int[3] { 0, 0, 0 };
                            for (int i = 0; i < activation.Length; i++)
                            {
                                for (int xk = x; xk < x + kernelW; xk++)
                                    for (int yk = y; yk < y + kernelH; yk++)
                                        activation[i] += kernel[xk - x, yk - y] * (i == 0 ?
                                            cb.GetPixel(xk, yk).R : (i == 1 ?
                                            cb.GetPixel(xk, yk).G : cb.GetPixel(xk, yk).B));
                                activation[i] = (int)(activation[i] * factor);
                                activation[i] += 255 / 2;
                                if (activation[i] > 255)
                                    activation[i] = 255;
                                if (activation[i] < 0)
                                    activation[i] = 0;
                            }
                            c.SetPixel(x, y, Color.FromArgb(activation[0], activation[1], activation[2]));
                        }
            }

            return output;
        }
        static Bitmap FlagColor(Color[] Cs, Bitmap P, bool Horz = false)
        {
            using (UnsafeBitmapContext c = new UnsafeBitmapContext(P))
            {
                Color[] edges = new Color[] { c.GetPixel(0, 0),
                    c.GetPixel(0, c.Height - 1), c.GetPixel(c.Width - 1, 0),
                    c.GetPixel(c.Width - 1, c.Height - 1) };

                for (int x = 0; x < P.Width; x++)
                    for (int y = 0; y < P.Height; y++)
                    {
                        Color C = c.GetPixel(x, y);
                        if (C.A > 5 && edges.All(a => Math.Abs(a.GetColorDiff(C)) > 70))
                            if (Horz)
                                c.SetPixel(x, y, Cs[x * Cs.Length / P.Width]);
                            else
                                c.SetPixel(x, y, Cs[y * Cs.Length / P.Height]);
                    }
            }
            return P;
        }
    }
}
