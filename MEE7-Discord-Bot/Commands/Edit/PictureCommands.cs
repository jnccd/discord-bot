using BumpKit;
using Discord;
using Emgu.CV;
using Emgu.CV.Structure;
using MEE7.Backend.HelperFunctions;
using MEE7.Commands.Edit.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using Tesseract;
using static MEE7.Commands.Edit.Edit;
using Color = System.Drawing.Color;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace MEE7.Commands.Edit
{
    public class PictureCommands : EditCommandProvider
    {
        public string colorChannelSwapDesc = "Swap the rgb color channels for each pixel";
        public Bitmap ColorChannelSwap(Bitmap bmp, IMessage m)
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
        public Bitmap Reddify(Bitmap bmp, IMessage m)
        {
            using (UnsafeBitmapContext con = new UnsafeBitmapContext(bmp))
                for (int x = 0; x < bmp.Width; x++)
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Color c = con.GetPixel(x, y);
                        c = Color.FromArgb(255, c.R, 0, 0);
                        con.SetPixel(x, y, c);
                    }

            return bmp;
        }

        public string invertDesc = "Invert the color of each pixel";
        public Bitmap Invert(Bitmap bmp, IMessage m)
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
        public Bitmap Rekt(Bitmap bmp, IMessage m, string color)
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

        static readonly object memifyLock = new object();
        public string memifyDesc = "Turn a picture into a meme, get a list of available templates with the argument -list";
        public Bitmap Memify(Bitmap bmp, IMessage m, string Meme)
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
                    return InsertIntoRect(bmp, m, (Bitmap)Bitmap.FromFile(memeTemplateDesign), (Bitmap)Bitmap.FromFile(memeTemplateOverlay));
                else if (File.Exists(memeTemplate))
                    return InsertIntoRect(bmp, m, (Bitmap)Bitmap.FromFile(memeTemplateDesign));
                else
                    throw new Exception("Something went wrong :thinking:");
            }
        }

        public string textMemifyDesc = "Put text into a meme template, input -list as Meme and get a list templates\n" +
                "The default Font is Arial and the fontsize refers to the number of rows of text that are supposed to fit into the textbox";
        public Bitmap TextMemify(string memeName, IMessage m, string Meme, string Font = "Arial", float FontSize = 1)
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
        public Bitmap Expand(Bitmap bmp, IMessage m, Vector2 position = new Vector2(), float strength = 1)
        {
            Vector2 center = new Vector2(position.X * bmp.Width, position.Y * bmp.Height);

            return ApplyTransformation(bmp,
                (x, y) =>
                {
                    Vector2 point = new Vector2(x, y);
                    Vector2 diff = point - center;
                    float div = bmp.Width + bmp.Height;
                    float maxDistance = (center / div).LengthSquared();

                    float transformedLength = (diff / div).LengthSquared() * (1 / strength * 100);
                    return point - diff * (1 / (1 + transformedLength)) * (1 / (1 + transformedLength));
                });
        }

        public string stirDesc = "Stir the pixels";
        public Bitmap Stir(Bitmap bmp, IMessage m, Vector2 position = new Vector2(), float strength = 1)
        {
            Vector2 center = new Vector2(position.X * bmp.Width, position.Y * bmp.Height);

            return ApplyTransformation(bmp,
                (x, y) =>
                {
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
        public Bitmap Fall(Bitmap bmp, IMessage m, Vector2 position = new Vector2(), float strength = 1)
        {
            Vector2 center = new Vector2(position.X * bmp.Width, position.Y * bmp.Height);

            return ApplyTransformation(bmp,
                    (x, y) =>
                    {
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
        public Bitmap Wubble(Bitmap bmp, IMessage m, Vector2 position = new Vector2(), float strength = 1)
        {
            Vector2 center = new Vector2(position.X * bmp.Width, position.Y * bmp.Height);

            return ApplyTransformation(bmp,
                   (x, y) =>
                   {
                       Vector2 point = new Vector2(x, y);
                       Vector2 diff = point - center;
                       float div = bmp.Width + bmp.Height;

                       float transformedLength = (diff / div * 7).LengthSquared();
                       return point - diff * (1 / (1 + transformedLength * -strength));
                   });
        }

        public string cyaDesc = "Cya the pixels";
        public Bitmap Cya(Bitmap bmp, IMessage m, Vector2 position = new Vector2(), float strength = 1)
        {
            Vector2 center = new Vector2(position.X * bmp.Width, position.Y * bmp.Height);

            return ApplyTransformation(bmp,
                  (x, y) =>
                  {
                      Vector2 point = new Vector2(x, y);
                      Vector2 diff = point - center;
                      float div = bmp.Width + bmp.Height;

                      float transformedLength = (diff / div * 7).Length();
                      return point - diff * (float)(-Math.Pow(2, -transformedLength * strength) + 1);
                  });
        }

        public string inpandDesc = "Inpand the pixels";
        public Bitmap Inpand(Bitmap bmp, IMessage m, Vector2 position = new Vector2(), float strength = 1)
        {
            Vector2 center = new Vector2(position.X * bmp.Width, position.Y * bmp.Height);

            return ApplyTransformation(bmp,
                 (x, y) =>
                 {
                     Vector2 point = new Vector2(x, y);
                     Vector2 diff = point - center;
                     float div = bmp.Width + bmp.Height;

                     float transformedLength = (float)(diff / div * 7).LengthSquared();
                     return point - diff * (1 / (1 + transformedLength) * strength / 4);
                 });
        }

        public string blockifyDesc = "Blockify the pixels";
        public Bitmap Blockify(Bitmap bmp, IMessage m, float frequenzy = 1, float strength = 1, int offsetX = 1, int offsetY = 1)
        {
            return ApplyTransformation(bmp,
                    (x, y) => new Vector2(x + (float)Math.Cos(x / frequenzy + offsetX) * strength,
                                            y + (float)Math.Sin(y / frequenzy + offsetY) * strength));
        }

        public string SquiggleDesc = "Squiggle the pixels";
        public Bitmap Squiggle(Bitmap bmp, IMessage m, float percent = 1, float scale = 1, int offsetX = 1, int offsetY = 1)
        {
            return ApplyTransformation(bmp,
                    (x, y) => new Vector2(x + percent * (float)Math.Sin((y + offsetY) / scale),
                                            y + percent * (float)Math.Cos((x + offsetX) / scale)));
        }

        public string TransformPictureDesc = "Perform a liqidify transformation on the image using a custom function";
        public Bitmap TransformPicture(Bitmap bmp, IMessage m, Pipe transformationFunction)
        {
            if (transformationFunction.OutputType() != typeof(Vector2))
                throw new Exception("Boi I need sum vectors!");

            return ApplyTransformation(bmp,
                    (x, y) => (Vector2)transformationFunction.
                    Apply(m, bmp, new Dictionary<string, object>() { { "x", x }, { "y", y } }));
        }

        public string sobelEdgesDesc = "Highlights horizontal edges";
        public Bitmap SobelEdges(Bitmap bmp, IMessage m)
        {
            return ApplyKernel(bmp, new int[3, 3] { {  1,  2,  1 },
                                                    {  0,  0,  0 },
                                                    { -1, -2, -1 } }, 1, true);
        }

        public string sobelEdgesColorDesc = "Highlights horizontal edges";
        public Bitmap SobelEdgesColor(Bitmap bmp, IMessage m)
        {
            return ApplyKernel(bmp, new int[3, 3] { {  1,  2,  1 },
                                                                   {  0,  0,  0 },
                                                                   { -1, -2, -1 } }, 1, true);
        }

        public string laplaceEdgesDesc = "https://de.wikipedia.org/wiki/Laplace-Filter";
        public Bitmap LaplaceEdges(Bitmap bmp, IMessage m)
        {
            return ApplyKernel(bmp, new int[3, 3] { {  0,  1,  0 },
                                                    {  1, -4,  1 },
                                                    {  0,  1,  0 } }, 1, true);
        }

        public string laplace45EdgesDesc = "https://de.wikipedia.org/wiki/Laplace-Filter";
        public Bitmap Laplace45Edges(Bitmap bmp, IMessage m)
        {
            return ApplyKernel(bmp, new int[3, 3] { {  1,  1,  1 },
                                                    {  1, -8,  1 },
                                                    {  1,  1,  1 } }, 1, true);
        }

        public string sharpenDesc = "well guess what it does";
        public Bitmap Sharpen(Bitmap bmp, IMessage m)
        {
            return ApplyKernel(bmp, new int[3, 3] { {  0, -1,  0 },
                                                    { -1,  5, -1 },
                                                    {  0, -1,  0 } });
        }

        public string boxBlurDesc = "blur owo";
        public Bitmap BoxBlur(Bitmap bmp, IMessage m)
        {
            return ApplyKernel(bmp, new int[3, 3] { {  1,  1,  1 },
                                                    {  1,  1,  1 },
                                                    {  1,  1,  1 } }, 1 / 9f);
        }

        public string gaussianBlurDesc = "more blur owo";
        public Bitmap GaussianBlur(Bitmap bmp, IMessage m, int size = 5)
        {
            var weights = Enumerable.Range(0, size).Select(x => Gauss(x, size * 2f / 3f, size / 2)).ToArray();
            var scalar = 1 / weights.Sum();

            float[,] one = new float[size, 1], two = new float[1, size];
            for (int i = 0; i < size; i++)
            {
                one[i, 0] = weights[i];
                two[0, i] = weights[i];
            }

            var tmp = ApplyKernel(bmp, one, scalar);
            return ApplyKernel(tmp, two, scalar);
        }

        public string UnsharpMaskingDesc = "Sharpening but cooler";
        public Bitmap UnsharpMasking(Bitmap bmp, IMessage m, int size = 5)
        {
            var weights = Enumerable.Range(0, size).Select(x => Gauss(x, size * 2f / 3f, size / 2)).ToArray();

            float[,] kernel = new float[size, size]; float sum = 0;
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    var n = weights[x] * weights[y];
                    kernel[x, y] = -n;
                    sum += n;
                }

            kernel[size / 2, size / 2] += sum * 2;

            return ApplyKernel(bmp, kernel, 1 / sum);
        }

        public string gayPrideDesc = "Gay rights";
        public Bitmap GayPride(Bitmap bmp, IMessage m)
        {
            return FlagColor(new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Purple }, bmp);
        }

        public string transRightsDesc = "The input image says trans rights";
        public Bitmap TransRights(Bitmap bmp, IMessage m)
        {
            return FlagColor(new Color[] { Color.LightBlue, Color.Pink, Color.White, Color.Pink, Color.LightBlue }, bmp);
        }

        public string merkelDesc = "Add a german flag to the background of your image";
        public Bitmap Merkel(Bitmap bmp, IMessage m)
        {
            return FlagColor(new Color[] { Color.Black, Color.Red, Color.Yellow }, bmp);
        }

        public string chromaticAbberationDesc = "Shifts the color spaces";
        public Bitmap ChromaticAbberation(Bitmap bmp, IMessage m, int intensity = 4)
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
        public Bitmap Rotate(Bitmap b, IMessage m, float AngleInDegrees = 0)
        {
            Vector2 middle = new Vector2(b.Width / 2, b.Height / 2);
            return b.RotateImage(AngleInDegrees, middle);
        }

        public string rotateColorsDesc = "Rotate the color spaces of each pixel";
        public Bitmap RotateColors(Bitmap b, IMessage m, float AngleInDegrees = 0)
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

        public string compressDesc = "JPEG Compress the image, lower compression level = more compression";
        public Bitmap Compress(Bitmap b, IMessage m, long compressionLevel)
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

        public string transgroundDesc = "Make the background transparent";
        public Bitmap Transground(Bitmap b, IMessage m, Vector2 BackgroundCoords = new Vector2(), int threshold = 10)
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

        public string transgroundColorDesc = "Make the background color transparent";
        public Bitmap TransgroundColor(Bitmap b, IMessage m, Color backColor, int threshold = 50, float alphaMult = 1)
        {
            int dist = 0;
            Color c;
            using (UnsafeBitmapContext con = new UnsafeBitmapContext(b))
                for (int x = 0; x < b.Width; x++)
                    for (int y = 0; y < b.Height; y++)
                        if ((dist = (c = con.GetPixel(x, y)).GetColorDist(backColor)) < threshold)
                        {
                            dist = (int)(dist * alphaMult);
                            Color o = c.Lerp(backColor, -1);
                            o = Color.FromArgb((dist > 255 ? 255 : dist).ReLU(), o);
                            con.SetPixel(x, y, o);
                        }

            return b;
        }

        public string transgroundHSVDesc = "Thereshold HSV values to transparency, note that the SV values are in [0, 100]";
        public Bitmap TransgroundHSV(Bitmap b, IMessage m, Pipe thresholder)
        {
            using (UnsafeBitmapContext con = new UnsafeBitmapContext(b))
                for (int x = 0; x < b.Width; x++)
                    for (int y = 0; y < b.Height; y++)
                    {
                        Color c = con.GetPixel(x, y);
                        c.ColorToHSV(out double h, out double s, out double v);
                        if ((bool)thresholder.Apply(m, null, new Dictionary<string, object>() { { "h", h }, { "s", s * 100 }, { "v", v * 100 } }))
                            con.SetPixel(x, y, Color.FromArgb(0, c));
                    }

            return b;
        }

        public string hueScaleDesc = "Grayscaled Hue channel of the image";
        public Bitmap HueScale(Bitmap b, IMessage m)
        {
            using (UnsafeBitmapContext con = new UnsafeBitmapContext(b))
                for (int x = 0; x < b.Width; x++)
                    for (int y = 0; y < b.Height; y++)
                    {
                        Color c = con.GetPixel(x, y);
                        c.ColorToHSV(out double h, out double s, out double v);
                        int rangeH = (int)(h * 255 / 360);
                        con.SetPixel(x, y, Color.FromArgb(255, rangeH, rangeH, rangeH));
                    }

            return b;
        }

        public string satScaleDesc = "Grayscaled Saturation channel of the image";
        public Bitmap SatScale(Bitmap b, IMessage m)
        {
            using (UnsafeBitmapContext con = new UnsafeBitmapContext(b))
                for (int x = 0; x < b.Width; x++)
                    for (int y = 0; y < b.Height; y++)
                    {
                        Color c = con.GetPixel(x, y);
                        c.ColorToHSV(out double h, out double s, out double v);
                        int rangeS = (int)(s * 255);
                        con.SetPixel(x, y, Color.FromArgb(255, rangeS, rangeS, rangeS));
                    }

            return b;
        }

        public string valScaleDesc = "Grayscaled Value channel of the image";
        public Bitmap ValScale(Bitmap b, IMessage m)
        {
            using (UnsafeBitmapContext con = new UnsafeBitmapContext(b))
                for (int x = 0; x < b.Width; x++)
                    for (int y = 0; y < b.Height; y++)
                    {
                        Color c = con.GetPixel(x, y);
                        c.ColorToHSV(out double h, out double s, out double v);
                        int rangeV = (int)(v * 255);
                        con.SetPixel(x, y, Color.FromArgb(255, rangeV, rangeV, rangeV));
                    }

            return b;
        }

        public string transcropDesc = "Crop the transparency";
        public Bitmap Transcrop(Bitmap b, IMessage m, int threshold = 10)
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
        public Bitmap Crop(Bitmap b, IMessage m, int x, int y, int w, int h)
        {
            return b.CropImage(new Rectangle(x,y,w,h));
        }

        public string splitDesc = "Split the picture into x * y pieces";
        public void Split(Bitmap b, IMessage m, int x = 2, int y = 2)
        {
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                    DiscordNETWrapper.SendBitmap(b.CropImage(new Rectangle((int)(b.Width * (i / (float)x)), (int)(b.Height * (j / (float)y)),
                        (int)(b.Width / (float)x), (int)(b.Height / (float)y)), false), m.Channel, (i + 1) + " " + (j + 1)).Wait();

            b.Dispose();
        }

        public string rotateWholeDesc = "Rotate the image including the bounds";
        public Bitmap RotateWhole(Bitmap b, IMessage m, bool left = true)
        {
            if (left)
                b.RotateFlip(RotateFlipType.Rotate270FlipNone);
            else
                b.RotateFlip(RotateFlipType.Rotate90FlipNone);

            return b;
        }

        public string flipDesc = "Flip the image";
        public Bitmap Flip(Bitmap b, IMessage m, bool x = true)
        {
            if (x)
                b.RotateFlip(RotateFlipType.RotateNoneFlipX);
            else
                b.RotateFlip(RotateFlipType.RotateNoneFlipY);

            return b;
        }

        public string ZoomDesc = "Zoom to a certain point, zoomlevel should be between 1 and 0";
        public Bitmap Zoom(Bitmap b, IMessage m, Vector2 point, float zoomLevel)
        {
            Vector2 bSize = new Vector2(b.Width, b.Height);
            point = point * bSize;

            Vector2 upperLeft = point * zoomLevel;
            Vector2 lowerRight = bSize + (point - bSize) * zoomLevel;

            return (Bitmap)b.CropImage(new Rectangle(
                (int)upperLeft.X, (int)upperLeft.Y, (int)lowerRight.X - (int)upperLeft.X, (int)lowerRight.Y - (int)upperLeft.Y)).
                Stretch(new Size((int)bSize.X, (int)bSize.Y));
        }

        public string StretchDesc = "Stretch the Image";
        public Bitmap Stretch(Bitmap b, IMessage m, int w, int h)
        {
            return (Bitmap)b.Stretch(new Size(w, h));
        }

        public string StretchMDesc = "Stretch the Image by some multipliers";
        public static Bitmap StretchM(Bitmap b, IMessage m, float x, float y)
        {
            return (Bitmap)b.Stretch(new Size((int)(b.Width * x), (int)(b.Height * y)));
        }

        public string getSizeDesc = "Get the size in byte of an image";
        public long GetSize(Bitmap b, IMessage m)
        {
            using (var ms = new MemoryStream())
            {
                b.Save(ms, ImageFormat.Png);
                return ms.Length;
            }
        }

        public string GetDimensionsDesc = "Get the width and height";
        public string GetDimensions(Bitmap b, IMessage m)
        {
            return $"{b.Width}w {b.Height}h";
        }

        public string ResizeDesc = "Resize an image by some multiplier";
        public Bitmap Resize(Bitmap b, IMessage m, float multiplier)
        {
            return Stretch(b, m, (int)(b.Width * multiplier), (int)(b.Height * multiplier));
        }

        public string EmoteResizeDesc = "Resize an image to 48x48px which is the highest resolution discord currently displays an emote at";
        public Bitmap EmoteResize(Bitmap b, IMessage m)
        {
            if (b.Width > b.Height)
            {
                float mult = 48f / b.Width;
                int newH = (int)(b.Height * mult);
                return Stretch(b, m, 48, newH);
            }
            else
            {
                float mult = 48f / b.Height;
                int newW = (int)(b.Width * mult);
                return Stretch(b, m, newW, 48);
            }
        }

        public string SeamCarveDesc = "Carve seams";
        public Bitmap SeamCarve(Bitmap b, IMessage m, int newWidth, int newHeight)
        {
            if (newWidth > b.Width || newHeight > b.Height)
                throw new Exception("I can only make images smaller!");
            if (newWidth < 15 || newHeight < 15)
                throw new Exception("Thats a little too small");

            return new ImageScaler(b, newWidth, newHeight).commitScale();
        }

        public string SeamCarveMDesc = "Carve seams by multiplier";
        public Bitmap SeamCarveM(Bitmap b, IMessage m, float newWidthMult, float newHeightMult)
        {
            if (newWidthMult > 1 || newHeightMult > 1)
                throw new Exception("I can only make images smaller!");

            return SeamCarve(b, m, (int)(b.Width * newWidthMult), (int)(b.Height * newHeightMult));
        }

        public string ForSeamCarveMDesc = "Carve seams multiple times by multiplier";
        public Bitmap[] ForSeamCarveM(Bitmap b, IMessage m, float endValue, int numFrames, bool stretch = true)
        {
            if (endValue > 1)
                throw new Exception("I cant make larger >:("); 
            
            float getVal(int i) => 1 + (endValue - 1) * (i / (float)numFrames);

            Bitmap[] frames = new Bitmap[numFrames];
            frames[0] = b;
            for (int i = 1; i < numFrames; i++)
                frames[i] = new ImageScaler(frames[i - 1], (int)(b.Width * getVal(i)), (int)(b.Height * getVal(i))).commitScale();

            if (stretch)
                frames = frames.Select(x => (Bitmap)x.Stretch(new Size(b.Width, b.Height), false)).ToArray();

            return frames;
        }

        CascadeClassifier cascadeClassifier = null;
        public string FaceDetecDesc = "Detect faces";
        public Bitmap FaceDetec(Bitmap b, IMessage m, string classifier = "", double scaleFactor = 1.1)
        {
            if (cascadeClassifier == null)
                cascadeClassifier = new CascadeClassifier($"Commands{s}Edit{s}Resources{s}opencv-cascades{s}haarcascade_frontalface_alt_tree.xml");
            if (!string.IsNullOrWhiteSpace(classifier) && classifier.All(x => char.IsLetterOrDigit(x) || x == '_'))
                cascadeClassifier = new CascadeClassifier($"Commands{s}Edit{s}Resources{s}opencv-cascades{s}{classifier}.xml");
            else
                throw new Exception("no");

            BitmapData bdata = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, b.PixelFormat);
            Image<Bgr, byte> grayImage = new Image<Bgr, byte>(b.Width, b.Height, bdata.Stride, bdata.Scan0);
            b.UnlockBits(bdata);
            Rectangle[] faces = cascadeClassifier.DetectMultiScale(grayImage, scaleFactor, 0);

            using Graphics g = Graphics.FromImage(b);
            using Pen p = new Pen(Color.Red);

            foreach (var face in faces)
                g.DrawRectangle(p, face);

            return b;
        }

        public string ReadDesc = "Reads text";
        public string Read(Bitmap b, IMessage m, bool getLargestBlock = false)
        {
            var imgPath = $"Commands{s}Edit{s}Workspace{s}uwu.png";
            if (File.Exists(imgPath))
                File.Delete(imgPath);
            b.Save(imgPath);

            using var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            using var img = Pix.LoadFromFile(imgPath);
            using var page = engine.Process(img);
            var text = page.GetText();

            if (getLargestBlock)
                return text.Split("\n\n").MaxElement(x => x.Length);

            return $"Confidence: {page.GetMeanConfidence()}\n\n" +
                $"{text}";
        }

        public string InsertIntoRectDesc = "Inserts picture into the red rectangle of another picture";
        public static Bitmap InsertIntoRect(Bitmap insertion, IMessage m, Bitmap design, Bitmap overlay = null, bool drawDesign = true)
        {
            Rectangle redRekt = FindRectangle(design, Color.FromArgb(255, 0, 0), 30);
            if (redRekt.Width == 0)
                redRekt = FindRectangle(design, Color.FromArgb(254, 34, 34), 20);
            if (!drawDesign)
                using (UnsafeBitmapContext con = new UnsafeBitmapContext(design))
                    for (int x = 0; x < design.Width; x++)
                        for (int y = 0; y < design.Height; y++)
                            con.SetPixel(x, y, Color.Transparent);
            using (Graphics graphics = Graphics.FromImage(design))
            {
                graphics.DrawImage(insertion, IncreaseSize(redRekt, 1, 1));
                if (overlay != null)
                    graphics.DrawImage(overlay, new Point(0, 0));
            }
            if (overlay != null)
                overlay.Dispose();
            return design;
        }

        public string InsertDesc = "Inserts picture into the rectangle of another picture";
        public static Bitmap Insert(Bitmap backGround, IMessage m, Bitmap insertion, Rectangle r)
        {
            using (Graphics graphics = Graphics.FromImage(backGround))
                graphics.DrawImage(insertion, r);
            insertion.Dispose();
            return backGround;
        }

        public string TranslateDesc = "Translate picture";
        public static Bitmap Translate(Bitmap b, IMessage m, int deltaX, int deltaY)
        {
            Bitmap n = new Bitmap(b.Width, b.Height);
            using (Graphics graphics = Graphics.FromImage(n))
                graphics.DrawImage(b, deltaX, deltaY);
            b.Dispose();
            return n;
        }

        public string DuplicateDesc = "Duplicate picture into gif";
        public static Gif Duplicate(Bitmap b, IMessage m, int amount)
        {
            if (amount > 100)
                throw new Exception("no");

            Bitmap[] pics = Enumerable.Range(0, amount).Select(x => (Bitmap)b.Clone()).ToArray();
            int[] timings = Enumerable.Repeat(33, amount).ToArray();

            return new Gif(pics, timings);
        }

        public string CircleTransDesc = "Add circular transparency to the picture";
        public static Bitmap CircleTrans(Bitmap b, IMessage m)
        {
            using UnsafeBitmapContext c = new UnsafeBitmapContext(b);
            for (int x = 0; x < b.Width; x++)
                for (int y = 0; y < b.Height; y++)
                {
                    float sx = x / (float)b.Width - 0.5f, 
                          sy = y / (float)b.Height - 0.5f;
                    if (sx * sx + sy * sy > 0.25)
                        c.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                }

            return b;
        }


        static readonly char s = Path.DirectorySeparatorChar;
        static readonly float gcache = (float)Math.Sqrt(2 * Math.PI);
        static readonly float ecache = (float)Math.E;
        static float Gauss(float x, float sigma, float mu) => (float)Math.Pow(1 / (sigma * gcache) * ecache, 0.5 * (x - mu) * (x - mu) / sigma);
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

            for (int x = 0; x < Pic.Width; x++)
                for (int y = 0; y < Pic.Height; y++)
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
            float[,] fKernel = new float[kernel.GetLength(0), kernel.GetLength(1)];
            for (int x = 0; x < kernel.GetLength(0); x++)
                for (int y = 0; y < kernel.GetLength(1); y++)
                    fKernel[x, y] = kernel[x, y];
            return ApplyKernel(bmp, fKernel, factor, grayscale);
        }
        static Bitmap ApplyKernel(Bitmap bmp, float[,] kernel, float factor = 1, bool grayscale = false)
        {
            int kernelW = kernel.GetLength(0);
            int kernelH = kernel.GetLength(1);
            Bitmap output = new Bitmap(bmp.Width, bmp.Height);

            static int inBounds(int x, int bound)
            {
                if (x > bound - 1)
                    x = bound - 1;
                if (x < 0)
                    x = 0;
                return x;
            }

            if (grayscale)
            {
                using UnsafeBitmapContext c = new UnsafeBitmapContext(output);
                using UnsafeBitmapContext cb = new UnsafeBitmapContext(bmp);
                for (int x = 0; x < output.Width; x++)
                    for (int y = 0; y < output.Height; y++)
                    {
                        float activation = 0;
                        for (int xk = 0; xk < kernelW; xk++)
                            for (int yk = 0; yk < kernelH; yk++)
                                activation += kernel[xk, yk] * cb.GetPixel(inBounds(x + xk - kernelW / 2, bmp.Width), inBounds(y + yk - kernelH / 2, bmp.Height)).GetGrayScale();
                        activation = (int)Math.Abs(activation * factor);
                        if (activation > 255)
                            activation = 255;
                        if (activation < 0)
                            activation = 0;
                        c.SetPixel(x, y, Color.FromArgb((int)activation, (int)activation, (int)activation));
                    }
            }
            else
            {
                using UnsafeBitmapContext c = new UnsafeBitmapContext(output);
                using UnsafeBitmapContext cb = new UnsafeBitmapContext(bmp);
                for (int x = 0; x < output.Width; x++)
                    for (int y = 0; y < output.Height; y++)
                    {
                        float[] activation = new float[3] { 0, 0, 0 };
                        for (int i = 0; i < activation.Length; i++)
                        {
                            for (int xk = 0; xk < kernelW; xk++)
                                for (int yk = 0; yk < kernelH; yk++)
                                    activation[i] += kernel[xk, yk] * (i == 0 ?
                                        cb.GetPixel(inBounds(x + xk - kernelW / 2, bmp.Width), inBounds(y + yk - kernelH / 2, bmp.Height)).R : (i == 1 ?
                                        cb.GetPixel(inBounds(x + xk - kernelW / 2, bmp.Width), inBounds(y + yk - kernelH / 2, bmp.Height)).G : 
                                        cb.GetPixel(inBounds(x + xk - kernelW / 2, bmp.Width), inBounds(y + yk - kernelH / 2, bmp.Height)).B));
                            activation[i] = (int)Math.Abs(activation[i] * factor);
                            if (activation[i] > 255)
                                activation[i] = 255;
                            if (activation[i] < 0)
                                activation[i] = 0;
                        }
                        c.SetPixel(x, y, Color.FromArgb((int)activation[0], (int)activation[1], (int)activation[2]));
                    }
            }

            bmp.Dispose();
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
                        if (C.A > 5 && edges.All(a => a.GetColorDist(C) < 30))
                            if (Horz)
                                c.SetPixel(x, y, Cs[x * Cs.Length / P.Width]);
                            else
                                c.SetPixel(x, y, Cs[y * Cs.Length / P.Height]);
                    }
            }
            return P;
        }
        static Rectangle IncreaseSize(Rectangle r, int x, int y)
        {
            r.Width += x;
            r.Height += y;
            return r;
        }
    }
}
