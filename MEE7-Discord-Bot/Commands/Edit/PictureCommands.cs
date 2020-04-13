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
using static MEE7.Commands.Edit;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace MEE7.Commands
{
    public class PictureCommands : EditCommandProvider
    {
        public string colorChannelSwapDesc = "Swap the rgb color channels for each pixel";
        public Bitmap ColorChannelSwap(Bitmap bmp, SocketMessage m)
        {
            using (UnsafeBitmapContext con = new UnsafeBitmapContext(bmp))
                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Color c = con.GetPixel(x, y);
                        c = Color.FromArgb(c.B, c.R, c.G);
                        con.SetPixel(x, y, c);
                    }

            return bmp;
        }

        public string reddifyDesc = "Make it red af";
        public Bitmap Reddify(Bitmap bmp, SocketMessage m)
        {
            using (UnsafeBitmapContext con = new UnsafeBitmapContext(bmp))
                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Color c = con.GetPixel(x, y);
                        c = Color.FromArgb(c.R, 0, 0, 255);
                        con.SetPixel(x, y, c);
                    }

            return bmp;
        }

        public string invertDesc = "Invert the color of each pixel";
        public Bitmap Invert(Bitmap bmp, SocketMessage m)
        {
            using (UnsafeBitmapContext con = new UnsafeBitmapContext(bmp))
                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Color c = con.GetPixel(x, y);
                        c = Color.FromArgb(255 - c.R, 255 - c.G, 255 - c.B);
                        con.SetPixel(x, y, c);
                    }

            return bmp;
        }

        public string RektDesc = "Finds colored rectangles in pictures";
        public Bitmap Rekt(Bitmap bmp, SocketMessage m, string color)
        {
            Bitmap output = new Bitmap(bmp.Width, bmp.Height);

            Color c;
            if (string.IsNullOrWhiteSpace(color))
                c = Color.FromArgb(254, 34, 34);
            else
                c = Color.FromName(color);
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
        }

        public string memifyDesc = "Turn a picture into a meme, get a list of available templates with the argument -list";
        public Bitmap Memify(Bitmap bmp, SocketMessage m, string Meme)
        {
            lock (memifyLock)
            {
                string[] files = Directory.GetFiles($"Commands{Path.DirectorySeparatorChar}MemeTemplates");

                string memeTemplateDesign = "";
                if (Meme == "-list")
                    throw new Exception("Templates: \n\n" + files.Select(x => Path.GetFileNameWithoutExtension(x)).
                                                                    Where(x => x.EndsWith("design")).
                                                                    Select(x => x.RemoveLastGroup('-').RemoveLastGroup('-')).
                                                                    Aggregate((x, y) => x + "\n" + y));
                else if (!string.IsNullOrWhiteSpace(Meme))
                    memeTemplateDesign = files.Where(x => Path.GetFileNameWithoutExtension(x).StartsWith(Meme) &&
                                                          Path.GetFileNameWithoutExtension(x).EndsWith("design")).ToArray().FirstOrDefault();
                else
                    memeTemplateDesign = files.Where(x => Path.GetFileNameWithoutExtension(x).EndsWith("design")).ToArray().GetRandomValue();

                if (string.IsNullOrWhiteSpace(memeTemplateDesign))
                    throw new Exception("I don't have that meme in my registry!");

                string memeName = memeTemplateDesign.RemoveLastGroup('-');
                string memeTemplate = files.FirstOrDefault(x => x.StartsWith(memeName) && !x.Contains("-design."));
                string memeTemplateOverlay = files.FirstOrDefault(x => x.StartsWith(memeName) && Path.GetFileNameWithoutExtension(x).EndsWith("overlay"));

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
        }

        public string textMemifyDesc = "Put text into a meme template, input -list as Meme and get a list templates\n" +
                "The default Font is Arial and the fontsize refers to the number of rows of text that are supposed to fit into the textbox";
        public Bitmap TextMemify(string memeName, SocketMessage m, string Meme, string Font = "Arial", float FontSize = 1)
        {
            string[] files = Directory.GetFiles($"Commands{Path.DirectorySeparatorChar}MemeTextTemplates");

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
                float fontSize = redRekt.Height / 5f / FontSize;
                using (Graphics graphics = Graphics.FromImage(template))
                    graphics.DrawString(memeName, new Font(Font, fontSize), Brushes.Black, redRekt);

                return template;
            }
            else
                throw new Exception("uwu");
        }

        public string expandDesc = "Expand the pixels";
        public Bitmap Expand(Bitmap bmp, SocketMessage m, Vector2 position = new Vector2(), float strength = 1)
        {
            Vector2 center = new Vector2(position.X * bmp.Width, position.Y * bmp.Height);

            return ApplyTransformation(bmp,
                (x, y) => {
                    Vector2 point = new Vector2(x, y);
                    Vector2 diff = point - center;
                    float div = bmp.Width + bmp.Height;
                    float maxDistance = (center / div).LengthSquared();

                    float transformedLength = (diff / div).LengthSquared() * (1 / strength * 100);
                    return point - diff * (1 / (1 + transformedLength)) * (1 / (1 + transformedLength));
                });
        }

        public string stirDesc = "Stir the pixels";
        public Bitmap Stir(Bitmap bmp, SocketMessage m, Vector2 position = new Vector2(), float strength = 1)
        {
            Vector2 center = new Vector2(position.X * bmp.Width, position.Y * bmp.Height);

            return ApplyTransformation(bmp,
                (x, y) => {
                    Vector2 point = new Vector2(x, y);
                    Vector2 diff = point - center;
                    float div = bmp.Width + bmp.Height;
                    float maxDistance = (center / div).LengthSquared();

                    float transformedLength = (diff / div * 7).LengthSquared();
                    float rotationAngle = (float)Math.Pow(2, -transformedLength * transformedLength) * strength;
                    double cos = Math.Cos(rotationAngle);
                    double sin = Math.Sin(rotationAngle);
                    return new Vector2((float)(cos * (point.X - center.X) - sin * (point.Y - center.Y) + center.X),
                                             (float)(sin * (point.X - center.X) + cos * (point.Y - center.Y) + center.Y));
                });
        }

        public string fallDesc = "Fall the pixels";
        public Bitmap Fall(Bitmap bmp, SocketMessage m, Vector2 position = new Vector2(), float strength = 1)
        {
            Vector2 center = new Vector2(position.X * bmp.Width, position.Y * bmp.Height);

            return ApplyTransformation(bmp,
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
        }

        public string wubbleDesc = "Wubble the pixels";
        public Bitmap Wubble(Bitmap bmp, SocketMessage m, Vector2 position = new Vector2(), float strength = 1)
        {
            Vector2 center = new Vector2(position.X * bmp.Width, position.Y * bmp.Height);

            return ApplyTransformation(bmp,
                   (x, y) => {
                       Vector2 point = new Vector2(x, y);
                       Vector2 diff = point - center;
                       float div = bmp.Width + bmp.Height;

                       float transformedLength = (diff / div * 7).LengthSquared();
                       return point - diff * (1 / (1 + transformedLength * -strength));
                   });
        }

        public string cyaDesc = "Cya the pixels";
        public Bitmap Cya(Bitmap bmp, SocketMessage m, Vector2 position = new Vector2(), float strength = 1)
        {
            Vector2 center = new Vector2(position.X * bmp.Width, position.Y * bmp.Height);

            return ApplyTransformation(bmp,
                  (x, y) => {
                      Vector2 point = new Vector2(x, y);
                      Vector2 diff = point - center;
                      float div = bmp.Width + bmp.Height;

                      float transformedLength = (diff / div * 7).Length();
                      return point - diff * (float)(-Math.Pow(2, -transformedLength * strength) + 1);
                  });
        }

        public string inpandDesc = "Inpand the pixels";
        public Bitmap Inpand(Bitmap bmp, SocketMessage m, Vector2 position = new Vector2(), float strength = 1)
        {
            Vector2 center = new Vector2(position.X * bmp.Width, position.Y * bmp.Height);

            return ApplyTransformation(bmp,
                 (x, y) => {
                     Vector2 point = new Vector2(x, y);
                     Vector2 diff = point - center;
                     float div = bmp.Width + bmp.Height;

                     float transformedLength = (float)(diff / div * 7).LengthSquared();
                     return point - diff * (1 / (1 + transformedLength) * strength / 4);
                 });
        }

        public string blockifyDesc = "Blockify the pixels";
        public Bitmap Blockify(Bitmap bmp, SocketMessage m, float frequenzy = 1, float strength = 1, int offsetX = 1, int offsetY = 1)
        {
            return ApplyTransformation(bmp,
                    (x, y) => new Vector2(x + (float)Math.Cos(x / frequenzy + offsetX) * strength,
                                            y + (float)Math.Sin(y / frequenzy + offsetY) * strength));
        }

        public string SquiggleDesc = "Squiggle the pixels";
        public Bitmap Squiggle(Bitmap bmp, SocketMessage m, float percent = 1, float scale = 1, int offsetX = 1, int offsetY = 1)
        {
            return ApplyTransformation(bmp,
                    (x, y) => new Vector2(x + percent * (float)Math.Sin((y + offsetY) / scale),
                                            y + percent * (float)Math.Cos((x + offsetX) / scale)));
        }

        public string sobelEdgesDesc = "Highlights horizontal edges";
        public Bitmap sobelEdges(Bitmap bmp, SocketMessage m)
        {
            return ApplyKernel(bmp, new int[3, 3] { {  1,  2,  1 },
                                                                   {  0,  0,  0 },
                                                                   { -1, -2, -1 } }, 1, true);
        }

        public string sobelEdgesColorDesc = "Highlights horizontal edges";
        public Bitmap SobelEdgesColor(Bitmap bmp, SocketMessage m)
        {
            return ApplyKernel(bmp, new int[3, 3] { {  1,  2,  1 },
                                                                   {  0,  0,  0 },
                                                                   { -1, -2, -1 } }, 1, true);
        }

        public string sharpenDesc = "well guess what it does";
        public Bitmap Sharpen(Bitmap bmp, SocketMessage m)
        {
            return ApplyKernel(bmp, new int[3, 3] { {  0, -1,  0 },
                                                               { -1,  5, -1 },
                                                               {  0, -1,  0 } }, 1 / 5f);
        }

        public string boxBlurDesc = "blur owo";
        public Bitmap BoxBlur(Bitmap bmp, SocketMessage m)
        {
            return ApplyKernel(bmp, new int[3, 3] { {  1,  1,  1 },
                                                               {  1,  1,  1 },
                                                               {  1,  1,  1 } }, 1 / 9f);
        }

        public string gaussianBlurDesc = "more blur owo";
        public Bitmap GaussianBlur(Bitmap bmp, SocketMessage m)
        {
            return ApplyKernel(bmp, new int[3, 3] { {  1,  2,  1 },
                                                                   {  2,  4,  2 },
                                                                   {  1,  2,  1 } }, 1 / 16f);
        }

        public string jkrowlingDesc = "Gay rights";
        public Bitmap Jkrowling(Bitmap bmp, SocketMessage m)
        {
            return FlagColor(new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Purple }, bmp);
        }

        public string merkelDesc = "German rights";
        public Bitmap Merkel(Bitmap bmp, SocketMessage m)
        {
            return FlagColor(new Color[] { Color.Black, Color.Red, Color.Yellow }, bmp);
        }

        public string transRightsDesc = "The input image says trans rights";
        public Bitmap TransRights(Bitmap bmp, SocketMessage m)
        {
            return FlagColor(new Color[] { Color.LightBlue, Color.Pink, Color.White, Color.Pink, Color.LightBlue }, bmp);
        }

        public string rainbowDesc = "I'll try spinning colors that's a good trick";
        public Gif Rainbow(Bitmap b, SocketMessage m)
        {
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

            return new Gif(re, Enumerable.Repeat(33, re.Length).ToArray());
        }

        public string spinToWinDesc = "I'll try spinning that's a good trick";
        public Gif SpinToWin(Bitmap b, SocketMessage m)
        {
            Vector2 middle = new Vector2(b.Width / 2, b.Height / 2);

            int steps = 20;
            int stepWidth = 360 / steps;
            Bitmap[] re = new Bitmap[steps];
            for (int i = 0; i < steps; i++)
                re[i] = b.RotateImage(-stepWidth * i, middle);

            return new Gif(re, Enumerable.Repeat(33, re.Length).ToArray());
        }

        public string chromaticAbberationDesc = "Shifts the color spaces";
        public Bitmap ChromaticAbberation(Bitmap bmp, SocketMessage m, int intensity = 4)
        {
            using (UnsafeBitmapContext con = new UnsafeBitmapContext(bmp))
                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Color r = con.GetPixel(x + intensity > bmp.Width - 1 ? bmp.Width - 1 : x + intensity, y);
                        Color g = con.GetPixel(x, y);
                        Color b = con.GetPixel(x - intensity < 0 ? 0 : x - intensity, y);
                        con.SetPixel(x, y, Color.FromArgb(g.A, r.R, g.G, b.B));
                    }

            return bmp;
        }

        public string rotateDesc = "Rotate the image";
        public Bitmap Rotate(Bitmap b, SocketMessage m, float AngleInDegrees = 0)
        {
            Vector2 middle = new Vector2(b.Width / 2, b.Height / 2);
            return b.RotateImage(AngleInDegrees, middle);
        }

        public string rotateColorsDesc = "Rotate the color spaces of each pixel";
        public Bitmap RotateColors(Bitmap b, SocketMessage m, float AngleInDegrees = 0)
        {
            using (UnsafeBitmapContext c = ImageExtensions.CreateUnsafeContext(b))
                for (int x = 0; x < b.Width; x++)
                    for (int y = 0; y < b.Height; y++)
                    {
                        Color col = c.GetPixel(x, y);
                        c.SetPixel(x, y, Color.FromArgb(col.A,
                            new Vector3(col.GetHue() + AngleInDegrees, col.GetSaturation(), col.GetValue()).HsvToRgb()));
                    }

            return b;
        }

        public string compressDesc = "JPEG Compress the image";
        public Bitmap Compress(Bitmap b, SocketMessage m, long compressionLevel)
        {
            ImageCodecInfo jpgEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);
            Encoder myEncoder = Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);

            MemoryStream tmp = new MemoryStream();
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, compressionLevel);
            myEncoderParameters.Param[0] = myEncoderParameter;
            b.Save(tmp, jpgEncoder, myEncoderParameters);

            Bitmap output = new Bitmap(System.Drawing.Image.FromStream(tmp));

            b.Dispose();
            tmp.Dispose();

            return output;
        }

        public string backAndForthDesc = "Make the gif go backward after it went forward and " +
                "then it goes forward again because it loops and its all very fancy n stuff";
        public Gif BackAndForth(Gif gif, SocketMessage m)
        {
            return new Gif(gif.Item1.Concat(gif.Item1.Skip(1).Reverse().Select(x => (Bitmap)x.Clone())).ToArray(), 
                           gif.Item2.Concat(gif.Item2.Skip(1).Reverse()).ToArray());
        }

        public string getDesc = "Get single picture from a gif";
        public Bitmap get(Gif gif, SocketMessage m, int index = 0)
        {
            for (int i = 0; i < gif.Item1.Length; i++)
                if (i != index)
                    gif.Item1[i].Dispose();
            return gif.Item1[index];
        }

        public string transgroundDesc = "Make the background transparent";
        public Bitmap Transground(Bitmap b, SocketMessage m, Vector2 BackgroundCoords = new Vector2(), int threshold = 10)
        {
            Bitmap reB = new Bitmap(b);
            Vector2 coords = BackgroundCoords;
            List<Point> OpenList = new List<Point>(new Point[] { new Point((int)(coords.X * (b.Width - 1)), (int)(coords.Y * (b.Height - 1))) });

            if (threshold > byte.MaxValue - 1)
                threshold = byte.MaxValue - 1;

            List<Point> getNeighbors(Point p)
            {
                List<Point> re = new List<Point>();
                if (p.X > 0) re.Add(new Point(p.X - 1, p.Y));
                if (p.X < reB.Width - 1) re.Add(new Point(p.X + 1, p.Y));
                if (p.Y > 0) re.Add(new Point(p.X, p.Y - 1));
                if (p.Y < reB.Height - 1) re.Add(new Point(p.X, p.Y + 1));
                return re;
            }

            using (UnsafeBitmapContext c = new UnsafeBitmapContext(reB))
            {
                Color backColor = c.GetPixel(OpenList[0].X, OpenList[0].Y);

                while (OpenList.Count > 0)
                {
                    Point cur = OpenList[0];
                    OpenList.RemoveAt(0);

                    int dist; Color C;
                    foreach (Point p in getNeighbors(cur))
                    {
                        if (c.GetPixel(p.X, p.Y).A == byte.MaxValue &&
                           (dist = (C = c.GetPixel(p.X, p.Y)).GetColorDist(backColor).ReLU() / 3) < threshold)
                        {
                            c.SetPixel(p.X, p.Y, Color.FromArgb(dist > 255 ? 255 : dist, C.R, C.G, C.B));
                            OpenList.Add(p);
                        }
                    }
                }
            }

            b.Dispose();
            return reB;
        }

        public string transcropDesc = "Crop the transparency";
        public Bitmap Transcrop(Bitmap b, SocketMessage m, int threshold = 10)
        {
            if (threshold > byte.MaxValue)
                threshold = byte.MaxValue;

            int X = 0, Y = 0, W = b.Width - 1, H = b.Height - 1;

            using (UnsafeBitmapContext c = new UnsafeBitmapContext(b))
            {
                while (X < b.Width - 1 && Enumerable.Range(0, b.Height - 1).Select(y => c.GetPixel(X, y)).Where(a => a.A > threshold).Count() < 4)
                    X++;
                var de = Enumerable.Range(0, b.Height - 1).Select(y => c.GetPixel(X, y)).Where(a => a.A < threshold);
                var bug = de.Count();

                while (Y < b.Height - 1 && Enumerable.Range(0, b.Width - 1).Select(x => c.GetPixel(x, Y)).Where(a => a.A > threshold).Count() < 4)
                    Y++;

                while (W > 0 && Enumerable.Range(0, b.Height - 1).Select(y => c.GetPixel(W, y)).Where(a => a.A > threshold).Count() < 4)
                    W--;

                while (H > 0 && Enumerable.Range(0, b.Width - 1).Select(x => c.GetPixel(x, H)).Where(a => a.A > threshold).Count() < 4)
                    H--;
            }

            Bitmap re = b.CropImage(new Rectangle(X, Y, W - X, H - Y));
            return re;
        }

        public string cropDesc = "Crop the picture";
        public Bitmap Crop(Bitmap b, SocketMessage m, float x, float y, float w, float h)
        {
            return b.CropImage(new Rectangle((int)(x * b.Width), (int)(y * b.Height), (int)(w * b.Width), (int)(h * b.Height)));
        }

        public string splitDesc = "Split the picture into x * y pieces";
        public void Split(Bitmap b, SocketMessage m, int x = 2, int y = 2)
        {
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                    DiscordNETWrapper.SendBitmap(b.CropImage(new Rectangle((int)(b.Width * (i / (float)x)), (int)(b.Height * (j / (float)y)), 
                        (int)(b.Width / (float)x), (int)(b.Height / (float)y)), false), m.Channel, (i+1) + " " + (j+1)).Wait();

            b.Dispose();
        }

        public string rotateWholeDesc = "Rotate the image including the bounds";
        public Bitmap RotateWhole(Bitmap b, SocketMessage m, bool left = true)
        {
            if (left)
                b.RotateFlip(RotateFlipType.Rotate270FlipNone);
            else
                b.RotateFlip(RotateFlipType.Rotate90FlipNone);

            return b;
        }

        public string flipDesc = "Flip the image";
        public Bitmap Flip(Bitmap b, SocketMessage m, bool x = true)
        {
            if (x)
                b.RotateFlip(RotateFlipType.RotateNoneFlipX);
            else
                b.RotateFlip(RotateFlipType.RotateNoneFlipY);

            return b;
        }

        public string zoomDesc = "Zoom to a certain point, zoomlevel should be between 1 and 0";
        public Bitmap Zoom(Bitmap b, SocketMessage m, Vector2 point, float zoomLevel)
        {
            Vector2 bSize = new Vector2(b.Width, b.Height);
            point = point * bSize;

            Vector2 upperLeft = point * zoomLevel;
            Vector2 lowerRight = bSize + (point - bSize) * zoomLevel;

            return (Bitmap)b.CropImage(new Rectangle(
                (int)upperLeft.X, (int)upperLeft.Y, (int)lowerRight.X - (int)upperLeft.X, (int)lowerRight.Y - (int)upperLeft.Y)).
                Stretch(new Size((int)bSize.X, (int)bSize.Y));
        }

        public string stretchDesc = "Stretch the Image";
        public Bitmap Stretch(Bitmap b, SocketMessage m, int w, int h)
        {
            return (Bitmap)b.Stretch(new Size(w, h));
        }

        public string toGifDesc = "Converts a bitmap array to a gif";
        public Gif toGif(Bitmap[] input, SocketMessage m)
        {
            return new Gif(input, Enumerable.Repeat(33, input.Length).ToArray());
        }

        public string toBitmapArrayDesc = "Converts a gif to a bitmap array";
        public Bitmap[] toBitmapArray(Gif input, SocketMessage m)
        {
            return input.Item1;
        }

        public string ChangeSpeedDesc = "Change the gifs playback speed";
        public Gif ChangeSpeed(Gif gif, SocketMessage m, float multiplier)
        {
            return new Gif(gif.Item1, gif.Item2.Select(x => (int)(x * multiplier)).ToArray());
        }

        public string SetDelayDesc = "Change the gifs playback speed";
        public Gif SetDelay(Gif gif, SocketMessage m, int delay)
        {
            return new Gif(gif.Item1, gif.Item2.Select(x => delay).ToArray());
        }

        public string MultiplyFramesDesc = "Copy each frame x times";
        public Gif MultiplyFrames(Gif gif, SocketMessage m, int x)
        {
            List<Bitmap> re = new List<Bitmap>();
            foreach (Bitmap b in gif.Item1)
                for (int i = 0; i < x; i++)
                    re.Add(b);

            return new Gif(re.ToArray(), gif.Item2.Select(y => y / x > 0 ? y / x : 1).ToArray());
        }

        public string renderHTMLDesc = "Render HTML";
        public Bitmap RenderHTML(string html, SocketMessage m)
        {
            return html.ConvertHtmlToImage();
        }

        public string getHTMLFromWebsiteDesc = "Get the websites html";
        public string GetHTMLFromWebsite(string url, SocketMessage m)
        {
            return url.GetHTMLfromURL();
        }

        public string getSizeDesc = "Get the size in byte of an image";
        public long GetSize(Bitmap b, SocketMessage m)
        {
            using (var ms = new MemoryStream())
            {
                b.Save(ms, ImageFormat.Png);
                return ms.Length;
            }
        }

        public string GetDimensionsDesc = "Get the width and height";
        public string GetDimensions(Bitmap b, SocketMessage m)
        {
            return $"{b.Width}w {b.Height}h";
        }


        static readonly object memifyLock = new object();

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
                        if (C.A > 5 && edges.All(a => a.GetColorDist(C) > 70))
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
