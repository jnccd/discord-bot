using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Discord;
using Emgu.CV;
using MEE7.Backend.HelperFunctions;
using MEE7.Commands.Edit.Resources;
using SkiaSharp;
using static MEE7.Backend.HelperFunctions.SkiaSharpExtensions;
using static MEE7.Commands.Edit.Edit;

namespace MEE7.Commands.Edit
{
    public class PictureCommands : EditCommandProvider
    {
        public string GetColorDesc = "Get the color of a pixel";
        public SKColor GetColor(SKBitmap b, IMessage m, int x, int y)
        {
            return b.GetPixel(x, y);
        }

        public string colorChannelSwapDesc = "Swap the rgb color channels for each pixel";
        public SKBitmap ColorChannelSwap(SKBitmap bitmap, IMessage m)
        {
            using (var pixmap = bitmap.PeekPixels())
            {
                Span<byte> pixels = pixmap.GetPixelSpan();
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        SKColor c = pixels.GetPixel(x, y, pixmap.RowBytes);
                        c = new SKColor(c.Blue, c.Red, c.Green);
                        pixels.SetPixel(x, y, pixmap.RowBytes, c);
                    }
            }

            return bitmap;
        }

        public string reddifyDesc = "Make it red af";
        public SKBitmap Reddify(SKBitmap bitmap, IMessage m)
        {
            using (var pixmap = bitmap.PeekPixels())
            {
                Span<byte> pixels = pixmap.GetPixelSpan();
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        SKColor c = pixels.GetPixel(x, y, pixmap.RowBytes);
                        c = new SKColor(255, c.Red, 0, 0);
                        pixels.SetPixel(x, y, pixmap.RowBytes, c);
                    }
            }

            return bitmap;
        }

        public string invertDesc = "Invert the color of each pixel";
        public SKBitmap Invert(SKBitmap bitmap, IMessage m)
        {
            using (var pixmap = bitmap.PeekPixels())
            {
                Span<byte> pixels = pixmap.GetPixelSpan();
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        SKColor c = pixels.GetPixel(x, y, pixmap.RowBytes);
                        c = new SKColor((byte)(255 - c.Red), (byte)(255 - c.Green), (byte)(255 - c.Blue), c.Alpha);
                        pixels.SetPixel(x, y, pixmap.RowBytes, c);
                    }
            }

            return bitmap;
        }

        public string RektDesc = "Finds colored rectangles in pictures";
        public SKBitmap Rekt(SKBitmap bmp, IMessage m, string color)
        {
            SKBitmap output = new SKBitmap(bmp.Width, bmp.Height);

            SKColor c;
            if (string.IsNullOrWhiteSpace(color))
                c = SKColor.Parse("#FE2222");
            else
                c = SKColor.Parse(color);
            SKRect redRekt = FindRectangle(bmp, c, 20);

            if (redRekt.Width == 0)
                throw new Exception("No rekt!");
            else
            {
                using (SKCanvas canvas = new(output))
                {
                    canvas.Clear(SKColors.White);
                    canvas.DrawRect(redRekt, new SKPaint { Color = SKColors.Red });
                }

                return output;
            }
        }

        static readonly object memifyLock = new object();
        public string memifyDesc = "Turn a picture into a meme, get a list of available templates with the argument -list";
        public SKBitmap Memify(SKBitmap bmp, IMessage m, string Meme)
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
                    return InsertIntoRect(bmp, m, SKBitmap.Decode(memeTemplateDesign), SKBitmap.Decode(memeTemplateOverlay));
                else if (File.Exists(memeTemplate))
                    return InsertIntoRect(bmp, m, SKBitmap.Decode(memeTemplateDesign));
                else
                    throw new Exception("Something went wrong :thinking:");
            }
        }

        public string textMemifyDesc = "Put text into a meme template, input -list as Meme and get a list templates\n" +
                "The default Font is Arial and the fontsize refers to the number of rows of text that are supposed to fit into the textbox";
        public SKBitmap TextMemify(string memeName, IMessage m, string Meme, string Font = "Arial", float FontSize = 1)
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
                SKBitmap template, design;
                using (FileStream stream = new FileStream(memeTemplate, FileMode.Open))
                    template = SKBitmap.Decode(stream);
                using (FileStream stream = new FileStream(memeDesign, FileMode.Open))
                    design = SKBitmap.Decode(stream);
                SKRect redRekt = FindRectangle(design, SKColor.Parse("#FE2222"), 20);
                if (redRekt.Width == 0)
                    throw new Exception("Error, couldn't find a rectangle to write in!");
                float fontSize = redRekt.Height / 5f / FontSize;
                using (SKCanvas canvas = new SKCanvas(template))
                {
                    canvas.DrawText(memeName, redRekt.Left, redRekt.Top, new SKPaint { Color = SKColors.Black, TextSize = fontSize });
                }

                return template;
            }
            else
                throw new Exception("uwu");
        }

        public string expandDesc = "Expand the pixels";
        public SKBitmap Expand(SKBitmap bmp, IMessage m, Vector2 position = new Vector2(), float strength = 1)
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
        public SKBitmap Stir(SKBitmap bmp, IMessage m, Vector2 position = new Vector2(), float strength = 1)
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
        public SKBitmap Fall(SKBitmap bmp, IMessage m, Vector2 position = new Vector2(), float strength = 1)
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
        public SKBitmap Wubble(SKBitmap bmp, IMessage m, Vector2 position = new Vector2(), float strength = 1)
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
        public SKBitmap Cya(SKBitmap bmp, IMessage m, Vector2 position = new Vector2(), float strength = 1)
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
        public SKBitmap Inpand(SKBitmap bmp, IMessage m, Vector2 position = new Vector2(), float strength = 1)
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
        public SKBitmap Blockify(SKBitmap bmp, IMessage m, float frequenzy = 1, float strength = 1, int offsetX = 1, int offsetY = 1)
        {
            return ApplyTransformation(bmp,
                    (x, y) => new Vector2(x + (float)Math.Cos(x / frequenzy + offsetX) * strength,
                                            y + (float)Math.Sin(y / frequenzy + offsetY) * strength));
        }

        public string SquiggleDesc = "Squiggle the pixels";
        public SKBitmap Squiggle(SKBitmap bmp, IMessage m, float percent = 1, float scale = 1, int offsetX = 1, int offsetY = 1)
        {
            return ApplyTransformation(bmp,
                    (x, y) => new Vector2(x + percent * (float)Math.Sin((y + offsetY) / scale),
                                            y + percent * (float)Math.Cos((x + offsetX) / scale)));
        }

        public string TransformPictureDesc = "Perform a liqidify transformation on the image using a custom function";
        public SKBitmap TransformPicture(SKBitmap bmp, IMessage m, Pipe transformationFunction)
        {
            if (transformationFunction.OutputType() != typeof(Vector2))
                throw new Exception("Boi I need sum vectors!");

            return ApplyTransformation(bmp,
                    (x, y) => (Vector2)transformationFunction.
                    Apply(m, bmp, new Dictionary<string, object>() { { "x", x }, { "y", y } }));
        }

        public string sobelEdgesDesc = "Highlights horizontal edges";
        public SKBitmap SobelEdges(SKBitmap bmp, IMessage m)
        {
            return ApplyKernel(bmp, new int[3, 3] { {  1,  2,  1 },
                                                    {  0,  0,  0 },
                                                    { -1, -2, -1 } }, 1, true);
        }

        public string sobelEdgesColorDesc = "Highlights horizontal edges";
        public SKBitmap SobelEdgesColor(SKBitmap bmp, IMessage m)
        {
            return ApplyKernel(bmp, new int[3, 3] { {  1,  2,  1 },
                                                                   {  0,  0,  0 },
                                                                   { -1, -2, -1 } }, 1, true);
        }

        public string laplaceEdgesDesc = "https://de.wikipedia.org/wiki/Laplace-Filter";
        public SKBitmap LaplaceEdges(SKBitmap bmp, IMessage m)
        {
            return ApplyKernel(bmp, new int[3, 3] { {  0,  1,  0 },
                                                    {  1, -4,  1 },
                                                    {  0,  1,  0 } }, 1, true);
        }

        public string laplace45EdgesDesc = "https://de.wikipedia.org/wiki/Laplace-Filter";
        public SKBitmap Laplace45Edges(SKBitmap bmp, IMessage m)
        {
            return ApplyKernel(bmp, new int[3, 3] { {  1,  1,  1 },
                                                    {  1, -8,  1 },
                                                    {  1,  1,  1 } }, 1, true);
        }

        public string sharpenDesc = "well guess what it does";
        public SKBitmap Sharpen(SKBitmap bmp, IMessage m)
        {
            return ApplyKernel(bmp, new int[3, 3] { {  0, -1,  0 },
                                                    { -1,  5, -1 },
                                                    {  0, -1,  0 } });
        }

        public string boxBlurDesc = "blur owo";
        public SKBitmap BoxBlur(SKBitmap bmp, IMessage m)
        {
            return ApplyKernel(bmp, new int[3, 3] { {  1,  1,  1 },
                                                    {  1,  1,  1 },
                                                    {  1,  1,  1 } }, 1 / 9f);
        }

        public string gaussianBlurDesc = "more blur owo";
        public SKBitmap GaussianBlur(SKBitmap bmp, IMessage m, int size = 5)
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
        public SKBitmap UnsharpMasking(SKBitmap bmp, IMessage m, int size = 5)
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
        public SKBitmap GayPride(SKBitmap bmp, IMessage m)
        {
            return FlagColor(new SKColor[] { SKColor.Parse("#EE3124"), SKColor.Parse("#F57F29"), SKColor.Parse("#FFF000"), SKColor.Parse("#58B947"), SKColor.Parse("#0054A6"), SKColor.Parse("#9F248F") }, bmp);
        }

        public string transRightsDesc = "The input image says trans rights";
        public SKBitmap TransRights(SKBitmap bmp, IMessage m)
        {
            return FlagColor(new SKColor[] { SKColor.Parse("#7CC0EA"), SKColor.Parse("#F498C0"), SKColor.Parse("#FFFFFF"), SKColor.Parse("#F498C0"), SKColor.Parse("#7CC0EA"), }, bmp);
        }

        public string merkelDesc = "Add a german flag to the background of your image";
        public SKBitmap Merkel(SKBitmap bmp, IMessage m)
        {
            return FlagColor(new SKColor[] { SKColor.Parse("#000000"), SKColor.Parse("#DE0000"), SKColor.Parse("#FFCF00"), }, bmp);
        }

        public string chromaticAbberationDesc = "Shifts the color spaces";
        public SKBitmap ChromaticAbberation(SKBitmap bitmap, IMessage m, int intensity = 4)
        {
            using (var pixmap = bitmap.PeekPixels())
            {
                Span<byte> pixels = pixmap.GetPixelSpan();
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        SKColor r = pixels.GetPixel(x + intensity > bitmap.Width - 1 ? bitmap.Width - 1 : x + intensity, y, pixmap.RowBytes);
                        SKColor g = pixels.GetPixel(x, y, pixmap.RowBytes);
                        SKColor b = pixels.GetPixel(x - intensity < 0 ? 0 : x - intensity, y, pixmap.RowBytes);
                        pixels.SetPixel(x, y, pixmap.RowBytes, new SKColor(r.Red, g.Green, b.Blue, g.Alpha));
                    }
            }

            return bitmap;
        }

        public string rotateDesc = "Rotate the image";
        public SKBitmap Rotate(SKBitmap b, IMessage m, float AngleInDegrees = 0)
        {
            Vector2 middle = new Vector2(b.Width / 2, b.Height / 2);
            return b.RotateImage(AngleInDegrees, middle);
        }

        public string rotateColorsDesc = "Rotate the color spaces of each pixel";
        public SKBitmap RotateColors(SKBitmap bitmap, IMessage m, float AngleInDegrees = 0)
        {
            using (var pixmap = bitmap.PeekPixels())
            {
                Span<byte> pixels = pixmap.GetPixelSpan();
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        SKColor col = pixels.GetPixel(x, y, pixmap.RowBytes);
                        col.ToHsv(out float h, out float s, out float v);
                        h += AngleInDegrees;
                        var newCol = SKColor.FromHsv(h, s, v);
                        pixels.SetPixel(x, y, pixmap.RowBytes, newCol);
                    }
            }

            return bitmap;
        }

        public string compressDesc = "JPEG Compress the image, lower compression level = more compression";
        public SKBitmap Compress(SKBitmap b, IMessage m, int compressionLevel)
        {
            var image = SKImage.FromBitmap(b);
            var data = image.Encode(SKEncodedImageFormat.Jpeg, quality: compressionLevel);
            var compressedBitmap = SKBitmap.Decode(data);

            image.Dispose();
            data.Dispose();

            return compressedBitmap;
        }

        public string transgroundDesc = "Make the background transparent";
        public SKBitmap Transground(SKBitmap b, IMessage m, Vector2 BackgroundCoords = new Vector2(), int threshold = 10)
        {
            SKBitmap reB = b.Copy();
            Vector2 coords = BackgroundCoords;
            List<SKPoint> OpenList = new List<SKPoint>(new SKPoint[] { new SKPoint((int)(coords.X * (b.Width - 1)), (int)(coords.Y * (b.Height - 1))) });

            if (threshold > byte.MaxValue - 1)
                threshold = byte.MaxValue - 1;

            List<SKPoint> getNeighbors(SKPoint p)
            {
                List<SKPoint> re = new List<SKPoint>();
                if (p.X > 0) re.Add(new SKPoint(p.X - 1, p.Y));
                if (p.X < reB.Width - 1) re.Add(new SKPoint(p.X + 1, p.Y));
                if (p.Y > 0) re.Add(new SKPoint(p.X, p.Y - 1));
                if (p.Y < reB.Height - 1) re.Add(new SKPoint(p.X, p.Y + 1));
                return re;
            }

            using (var pixmap = reB.PeekPixels())
            {
                Span<byte> pixels = pixmap.GetPixelSpan();

                SKColor backColor = pixels.GetPixel((int)OpenList[0].X, (int)OpenList[0].Y, pixmap.RowBytes);

                while (OpenList.Count > 0)
                {
                    SKPoint cur = OpenList[0];
                    OpenList.RemoveAt(0);

                    int dist; SKColor C;
                    foreach (SKPoint p in getNeighbors(cur))
                    {
                        if (pixels.GetPixel((int)p.X, (int)p.Y, pixmap.RowBytes).Alpha == byte.MaxValue &&
                           (dist = (C = pixels.GetPixel((int)p.X, (int)p.Y, pixmap.RowBytes)).GetColorDist(backColor).ReLU() / 3) < threshold)
                        {
                            pixels.SetPixel((int)p.X, (int)p.Y, pixmap.RowBytes, new SKColor(C.Red, C.Green, C.Blue, (byte)dist));
                            OpenList.Add(p);
                        }
                    }
                }
            }

            b.Dispose();
            return reB;
        }

        public string transgroundColorDesc = "Make the background color transparent";
        public SKBitmap TransgroundColor(SKBitmap bitmap, IMessage m, SKColor backColor, int threshold = 50, float alphaMult = 1)
        {
            int dist = 0;
            SKColor c;

            using (var pixmap = bitmap.PeekPixels())
            {
                Span<byte> pixels = pixmap.GetPixelSpan();
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        if ((dist = (c = pixels.GetPixel(x, y, pixmap.RowBytes)).GetColorDist(backColor)) < threshold)
                        {
                            dist = (int)(dist * alphaMult);
                            SKColor o = c.Lerp(backColor, -1);
                            o = new SKColor(o.Red, o.Green, o.Blue, (byte)(dist > 255 ? 255 : dist).ReLU());
                            pixels.SetPixel(x, y, pixmap.RowBytes, o);
                        }
                    }
            }

            return bitmap;
        }

        public string transgroundHSVDesc = "Thereshold HSV values to transparency, note that the SV values are in [0, 100]";
        public SKBitmap TransgroundHSV(SKBitmap bitmap, IMessage m, Pipe thresholder)
        {
            using (var pixmap = bitmap.PeekPixels())
            {
                Span<byte> pixels = pixmap.GetPixelSpan();
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        SKColor c = pixels.GetPixel(x, y, pixmap.RowBytes);
                        c.ColorToHSV(out double h, out double s, out double v);
                        if ((bool)thresholder.Apply(m, null, new Dictionary<string, object>() { { "h", h }, { "s", s * 100 }, { "v", v * 100 } }))
                            pixels.SetPixel(x, y, pixmap.RowBytes, new SKColor(c.Red, c.Green, c.Blue, 0));
                    }
            }

            return bitmap;
        }

        public string layerHSVDesc = "Split image into two layers ";
        public SKBitmap LayerHSV(SKBitmap bitmap, IMessage m, Pipe thresholder)
        {
            using (var pixmap = bitmap.PeekPixels())
            {
                Span<byte> pixels = pixmap.GetPixelSpan();
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        SKColor c = pixels.GetPixel(x, y, pixmap.RowBytes);
                        c.ColorToHSV(out double h, out double s, out double v);
                        if ((bool)thresholder.Apply(m, null, new Dictionary<string, object>() { { "h", h }, { "s", s * 100 }, { "v", v * 100 } }))
                            pixels.SetPixel(x, y, pixmap.RowBytes, new SKColor(c.Red, c.Green, c.Blue, 0));
                    }
            }

            return bitmap;
        }

        public string hueScaleDesc = "Grayscaled Hue channel of the image";
        public SKBitmap HueScale(SKBitmap bitmap, IMessage m)
        {
            using (var pixmap = bitmap.PeekPixels())
            {
                Span<byte> pixels = pixmap.GetPixelSpan();
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        SKColor c = pixels.GetPixel(x, y, pixmap.RowBytes);
                        c.ColorToHSV(out double h, out double s, out double v);
                        byte rangeH = (byte)(h * 255 / 360);
                        pixels.SetPixel(x, y, pixmap.RowBytes, new SKColor(rangeH, rangeH, rangeH, 255));
                    }
            }

            return bitmap;
        }

        public string satScaleDesc = "Grayscaled Saturation channel of the image";
        public SKBitmap SatScale(SKBitmap bitmap, IMessage m)
        {
            using (var pixmap = bitmap.PeekPixels())
            {
                Span<byte> pixels = pixmap.GetPixelSpan();
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        SKColor c = pixels.GetPixel(x, y, pixmap.RowBytes);
                        c.ColorToHSV(out double h, out double s, out double v);
                        byte rangeS = (byte)(s * 255);
                        pixels.SetPixel(x, y, pixmap.RowBytes, new SKColor(rangeS, rangeS, rangeS, 255));
                    }
            }

            return bitmap;
        }

        public string valScaleDesc = "Grayscaled Value channel of the image";
        public SKBitmap ValScale(SKBitmap bitmap, IMessage m)
        {
            using (var pixmap = bitmap.PeekPixels())
            {
                Span<byte> pixels = pixmap.GetPixelSpan();
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        SKColor c = pixels.GetPixel(x, y, pixmap.RowBytes);
                        c.ColorToHSV(out double h, out double s, out double v);
                        byte rangeV = (byte)(v * 255);
                        pixels.SetPixel(x, y, pixmap.RowBytes, new SKColor(rangeV, rangeV, rangeV, 255));
                    }
            }

            return bitmap;
        }

        public string transcropDesc = "Crop the transparency";
        public SKBitmap Transcrop(SKBitmap bitmap, IMessage m, int threshold = 10)
        {
            if (threshold > byte.MaxValue)
                threshold = byte.MaxValue;

            int X = 0, Y = 0, W = bitmap.Width - 1, H = bitmap.Height - 1;

            using (var pixmap = bitmap.PeekPixels())
            {
                Span<byte> pixels = pixmap.GetPixelSpan();
                byte[] pixelsArray = pixels.ToArray();
                while (X < bitmap.Width - 1 && Enumerable.Range(0, bitmap.Height - 1).Select(y => pixelsArray.GetPixel(X, y, pixmap.RowBytes)).Count(a => a.Alpha > threshold) < 4)
                    X++;
                var de = Enumerable.Range(0, bitmap.Height - 1).Select(y => pixelsArray.GetPixel(X, y, pixmap.RowBytes)).Where(a => a.Alpha < threshold);
                var bug = de.Count();

                while (Y < bitmap.Height - 1 && Enumerable.Range(0, bitmap.Width - 1).Select(x => pixelsArray.GetPixel(x, Y, pixmap.RowBytes)).Count(a => a.Alpha > threshold) < 4)
                    Y++;

                while (W > 0 && Enumerable.Range(0, bitmap.Height - 1).Select(y => pixelsArray.GetPixel(W, y, pixmap.RowBytes)).Count(a => a.Alpha > threshold) < 4)
                    W--;

                while (H > 0 && Enumerable.Range(0, bitmap.Width - 1).Select(x => pixelsArray.GetPixel(x, H, pixmap.RowBytes)).Count(a => a.Alpha > threshold) < 4)
                    H--;
            }

            SKBitmap re = bitmap.CropImage(new SKRect(X, Y, W - X, H - Y));
            return re;
        }

        public string cropDesc = "Crop the picture";
        public SKBitmap Crop(SKBitmap bitmap, IMessage m, int x, int y, int w, int h)
        {
            return bitmap.CropImage(new SKRect(x, y, w, h));
        }

        public string splitDesc = "Split the picture into x * y pieces";
        public void Split(SKBitmap bitmap, IMessage m, int x = 2, int y = 2)
        {
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                    DiscordNETWrapper.SendBitmap(bitmap.CropImage(new SKRect((int)(bitmap.Width * (i / (float)x)), (int)(bitmap.Height * (j / (float)y)),
                        (int)(bitmap.Width / (float)x), (int)(bitmap.Height / (float)y)), false), m.Channel, (i + 1) + " " + (j + 1)).Wait();

            bitmap.Dispose();
        }

        public string rotateWholeDesc = "Rotate the image including the bounds";
        public SKBitmap RotateWhole(SKBitmap bitmap, IMessage m, bool left = true)
        {
            if (left)
                bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
            else
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);

            return bitmap;
        }

        public string flipDesc = "Flip the image";
        public SKBitmap Flip(SKBitmap bitmap, IMessage m, bool x = true)
        {
            if (x)
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
            else
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

            return bitmap;
        }

        public string ZoomDesc = "Zoom to a certain point, zoomlevel should be between 1 and 0";
        public SKBitmap Zoom(SKBitmap b, IMessage m, Vector2 point, float zoomLevel)
        {
            Vector2 bSize = new Vector2(b.Width, b.Height);
            point = point * bSize;

            Vector2 upperLeft = point * zoomLevel;
            Vector2 lowerRight = bSize + (point - bSize) * zoomLevel;

            return b.CropImage(new SKRect(
                (int)upperLeft.X, (int)upperLeft.Y, (int)lowerRight.X - (int)upperLeft.X, (int)lowerRight.Y - (int)upperLeft.Y)).
                Stretch((int)bSize.X, (int)bSize.Y);
        }

        public string StretchDesc = "Stretch the Image";
        public SKBitmap Stretch(SKBitmap b, IMessage m, int w, int h)
        {
            return b.Stretch(w, h);
        }

        public string StretchMDesc = "Stretch the Image by some multipliers";
        public static SKBitmap StretchM(SKBitmap b, IMessage m, float x, float y)
        {
            return b.Stretch((int)(b.Width * x), (int)(b.Height * y));
        }

        public string getSizeDesc = "Get the size in byte of an image";
        public long GetSize(SKBitmap b, IMessage m)
        {
            using (var ms = new MemoryStream())
            {
                b.Encode(SKEncodedImageFormat.Png, 100).SaveTo(ms);
                return ms.Length;
            }
        }

        public string GetDimensionsDesc = "Get the width and height";
        public string GetDimensions(SKBitmap b, IMessage m)
        {
            return $"{b.Width}w {b.Height}h";
        }

        public string ResizeDesc = "Resize an image by some multiplier";
        public SKBitmap Resize(SKBitmap b, IMessage m, float multiplier)
        {
            return Stretch(b, m, (int)(b.Width * multiplier), (int)(b.Height * multiplier));
        }

        public string EmoteResizeDesc = "Resize an image to 48x48px which is the highest resolution discord currently displays an emote at";
        public SKBitmap EmoteResize(SKBitmap b, IMessage m)
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
        public SKBitmap SeamCarve(SKBitmap b, IMessage m, int newWidth, int newHeight)
        {
            if (newWidth > b.Width || newHeight > b.Height)
                throw new Exception("I can only make images smaller!");
            if (newWidth < 15 || newHeight < 15)
                throw new Exception("Thats a little too small");

            return new ImageScaler(b, newWidth, newHeight).commitScale();
        }

        public string SeamCarveMDesc = "Carve seams by multiplier";
        public SKBitmap SeamCarveM(SKBitmap b, IMessage m, float newWidthMult, float newHeightMult)
        {
            if (newWidthMult > 1 || newHeightMult > 1)
                throw new Exception("I can only make images smaller!");

            return SeamCarve(b, m, (int)(b.Width * newWidthMult), (int)(b.Height * newHeightMult));
        }

        public string ForSeamCarveMDesc = "Carve seams multiple times by multiplier";
        public SKBitmap[] ForSeamCarveM(SKBitmap b, IMessage m, float endValue, int numFrames, bool stretch = true)
        {
            if (endValue > 1)
                throw new Exception("I cant make larger >:(");

            float getVal(int i) => 1 + (endValue - 1) * (i / (float)numFrames);

            SKBitmap[] frames = new SKBitmap[numFrames];
            frames[0] = b;
            for (int i = 1; i < numFrames; i++)
                frames[i] = new ImageScaler(frames[i - 1], (int)(b.Width * getVal(i)), (int)(b.Height * getVal(i))).commitScale();

            if (stretch)
                frames = frames.Select(x => x.Stretch(b.Width, b.Height)).ToArray();

            return frames;
        }

        public string ReadDesc = "Reads text";
        public string Read(SKBitmap b, IMessage m, bool getLargestBlock = false, string language = "eng+jpn+chi_sim")
        {
            if (!Program.RunningOnLinux)
            {
                m.Channel.SendMessageAsync("Im in the wrong env :(").Wait();
            }

            var imgPath = $"Commands{s}Edit{s}Workspace{s}uwu.png";
            if (File.Exists(imgPath))
                File.Delete(imgPath);
            using (var fstrm = File.OpenWrite(imgPath))
                b.Encode(SKEncodedImageFormat.Png, 100).SaveTo(fstrm);

            if (!language.All(x => char.IsLetter(x) || x == '_' || x == '+'))
                throw new Exception("nope, thats not a language");

            string text = $"tesseract {imgPath} - quiet -l {language}".GetShellOut();

            if (getLargestBlock)
                return text.Split("\n\n").MaxElement(x => x.Length);

            return $"{text}";
        }

        public string InsertIntoRectDesc = "Inserts picture into the red rectangle of another picture";
        public static SKBitmap InsertIntoRect(SKBitmap insertion, IMessage m, SKBitmap design, SKBitmap overlay = null, bool drawDesign = true)
        {
            SKRect redRekt = FindRectangle(design, new SKColor(255, 0, 0), 30);
            if (redRekt.Width == 0)
                redRekt = FindRectangle(design, new SKColor(254, 34, 34), 20);
            if (!drawDesign)
                using (var pixmap = design.PeekPixels())
                {
                    Span<byte> pixels = pixmap.GetPixelSpan();
                    for (int y = 0; y < design.Height; y++)
                        for (int x = 0; x < design.Width; x++)
                        {
                            pixels.SetPixel(x, y, pixmap.RowBytes, SKColor.Empty);
                        }
                }
            using (var canvas = new SKCanvas(design))
            {
                canvas.DrawBitmap(insertion, IncreaseSize(redRekt, 1, 1));
                if (overlay != null)
                    canvas.DrawBitmap(overlay, 0, 0);
            }
            if (overlay != null)
                overlay.Dispose();
            return design;
        }

        public string InsertDesc = "Inserts picture into the rectangle of another picture";
        public static SKBitmap Insert(SKBitmap backGround, IMessage m, SKBitmap insertion, SKRect r)
        {
            using (var canvas = new SKCanvas(backGround))
                canvas.DrawBitmap(insertion, r);
            insertion.Dispose();
            return backGround;
        }

        public string TranslatePDesc = "Translate picture";
        public SKBitmap TranslateP(SKBitmap b, IMessage m, int deltaX, int deltaY)
        {
            SKBitmap n = new SKBitmap(b.Width, b.Height);
            using (var canvas = new SKCanvas(n))
                canvas.DrawBitmap(b, deltaX, deltaY);
            b.Dispose();
            return n;
        }

        public string DuplicateDesc = "Duplicate picture into gif";
        public Gif Duplicate(SKBitmap b, IMessage m, int amount)
        {
            if (amount > 100)
                throw new Exception("no");

            SKBitmap[] pics = Enumerable.Range(0, amount).Select(x => b.Copy()).ToArray();
            int[] timings = Enumerable.Repeat(33, amount).ToArray();

            return new Gif(pics, timings);
        }

        public string CircleTransDesc = "Add circular transparency to the picture";
        public SKBitmap CircleTrans(SKBitmap b, IMessage m)
        {
            using (var pixmap = b.PeekPixels())
            {
                Span<byte> pixels = pixmap.GetPixelSpan();
                for (int y = 0; y < b.Height; y++)
                    for (int x = 0; x < b.Width; x++)
                    {
                        float sx = x / (float)b.Width - 0.5f,
                      sy = y / (float)b.Height - 0.5f;
                        if (sx * sx + sy * sy > 0.25)
                            pixels.SetPixel(x, y, pixmap.RowBytes, SKColor.Empty);
                    }
            }

            return b;
        }


        static readonly char s = Path.DirectorySeparatorChar;
        static readonly float gcache = (float)Math.Sqrt(2 * Math.PI);
        static readonly float ecache = (float)Math.E;
        static float Gauss(float x, float sigma, float mu) => (float)Math.Pow(1 / (sigma * gcache) * ecache, 0.5 * (x - mu) * (x - mu) / sigma);
        static SKBitmap ApplyTransformation(SKBitmap input, Func<int, int, Vector2> trans)
        {
            SKBitmap output = new SKBitmap(input.Width, input.Height);

            using (var outputPixmap = input.PeekPixels())
            {
                Span<byte> outputPixels = outputPixmap.GetPixelSpan();
                using (var inputPixmap = input.PeekPixels())
                {
                    Span<byte> inputPixels = inputPixmap.GetPixelSpan();
                    for (int x = 0; x < input.Width; x++)
                        for (int y = 0; y < input.Height; y++)
                        {
                            Vector2 target = trans(x, y);

                            if (float.IsNaN((float)target.X) || float.IsInfinity((float)target.X))
                                target.X = x;
                            if (float.IsNaN((float)target.Y) || float.IsInfinity((float)target.Y))
                                target.Y = y;
                            if (target.X < 0)
                                target.X = 0;
                            if (target.X > input.Width - 1)
                                target.X = input.Width - 1;
                            if (target.Y < 0)
                                target.Y = 0;
                            if (target.Y > input.Height - 1)
                                target.Y = input.Height - 1;

                            outputPixels.SetPixel(x, y, outputPixmap.RowBytes, inputPixels.GetPixel((int)target.X, (int)target.Y, inputPixmap.RowBytes));
                        }
                }
            }

            return output;
        }
        static SKRect FindRectangle(SKBitmap Pic, SKColor C, int MinSize)
        {
            bool IsSameColor(SKColor C1, SKColor C2)
            {
                return Math.Abs(C1.Red - C2.Red) < 10 && Math.Abs(C1.Green - C2.Green) < 10 && Math.Abs(C1.Blue - C2.Blue) < 10;
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
                            return new SKRect(x, y, a - x - 1, b - y - 1);
                    }

            return new SKRect();
        }
        static SKBitmap ApplyKernel(SKBitmap bmp, int[,] kernel, float factor = 1, bool grayscale = false)
        {
            float[,] fKernel = new float[kernel.GetLength(0), kernel.GetLength(1)];
            for (int x = 0; x < kernel.GetLength(0); x++)
                for (int y = 0; y < kernel.GetLength(1); y++)
                    fKernel[x, y] = kernel[x, y];
            return ApplyKernel(bmp, fKernel, factor, grayscale);
        }
        static SKBitmap ApplyKernel(SKBitmap input, float[,] kernel, float factor = 1, bool grayscale = false)
        {
            int kernelW = kernel.GetLength(0);
            int kernelH = kernel.GetLength(1);
            SKBitmap output = new SKBitmap(input.Width, input.Height);

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
                using (var outputPixmap = output.PeekPixels())
                {
                    Span<byte> outputPixels = outputPixmap.GetPixelSpan();
                    using (var inputPixmap = input.PeekPixels())
                    {
                        Span<byte> inputPixels = inputPixmap.GetPixelSpan();
                        for (int x = 0; x < input.Width; x++)
                            for (int y = 0; y < input.Height; y++)
                            {
                                float activation = 0;
                                for (int xk = 0; xk < kernelW; xk++)
                                    for (int yk = 0; yk < kernelH; yk++)
                                        activation += kernel[xk, yk] * inputPixels.GetPixel(inBounds(x + xk - kernelW / 2, input.Width), inBounds(y + yk - kernelH / 2, input.Height), inputPixmap.RowBytes).GetGrayScale();
                                activation = (int)Math.Abs(activation * factor);
                                if (activation > 255)
                                    activation = 255;
                                if (activation < 0)
                                    activation = 0;
                                outputPixels.SetPixel(x, y, outputPixmap.RowBytes, new SKColor((byte)activation, (byte)activation, (byte)activation));
                            }
                    }
                }
            }
            else
            {
                using (var outputPixmap = output.PeekPixels())
                {
                    Span<byte> outputPixels = outputPixmap.GetPixelSpan();
                    using (var inputPixmap = input.PeekPixels())
                    {
                        Span<byte> inputPixels = inputPixmap.GetPixelSpan();
                        for (int x = 0; x < input.Width; x++)
                            for (int y = 0; y < input.Height; y++)
                            {
                                float[] activation = new float[3] { 0, 0, 0 };
                                for (int i = 0; i < activation.Length; i++)
                                {
                                    for (int xk = 0; xk < kernelW; xk++)
                                        for (int yk = 0; yk < kernelH; yk++)
                                            activation[i] += kernel[xk, yk] * (i == 0 ?
                                                inputPixels.GetPixel(inBounds(x + xk - kernelW / 2, input.Width), inBounds(y + yk - kernelH / 2, input.Height), inputPixmap.RowBytes).Red : (i == 1 ?
                                                inputPixels.GetPixel(inBounds(x + xk - kernelW / 2, input.Width), inBounds(y + yk - kernelH / 2, input.Height), inputPixmap.RowBytes).Green :
                                                inputPixels.GetPixel(inBounds(x + xk - kernelW / 2, input.Width), inBounds(y + yk - kernelH / 2, input.Height), inputPixmap.RowBytes).Blue));
                                    activation[i] = (int)Math.Abs(activation[i] * factor);
                                    if (activation[i] > 255)
                                        activation[i] = 255;
                                    if (activation[i] < 0)
                                        activation[i] = 0;
                                }
                                outputPixels.SetPixel(x, y, outputPixmap.RowBytes, new SKColor((byte)activation[0], (byte)activation[1], (byte)activation[2]));
                            }
                    }
                }
            }

            input.Dispose();
            return output;
        }
        static SKBitmap FlagColor(SKColor[] Cs, SKBitmap bitmap, bool Horz = false)
        {
            using (var pixmap = bitmap.PeekPixels())
            {
                Span<byte> pixels = pixmap.GetPixelSpan();

                SKColor[] edges = new SKColor[] { pixels.GetPixel(0, 0, pixmap.RowBytes),
                    pixels.GetPixel(0, bitmap.Height - 1, pixmap.RowBytes), pixels.GetPixel(bitmap.Width - 1, 0, pixmap.RowBytes),
                    pixels.GetPixel(bitmap.Width - 1, bitmap.Height - 1, pixmap.RowBytes) };

                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        SKColor C = pixels.GetPixel(x, y, pixmap.RowBytes);
                        if (C.Alpha > 5 && edges.All(a => a.GetColorDist(C) < 30))
                            if (Horz)
                                pixels.SetPixel(x, y, pixmap.RowBytes, Cs[x * Cs.Length / bitmap.Width]);
                            else
                                pixels.SetPixel(x, y, pixmap.RowBytes, Cs[y * Cs.Length / bitmap.Height]);
                    }
            }

            return bitmap;
        }
        static SKRect IncreaseSize(SKRect r, int x, int y)
        {
            return new SKRect(r.Left, r.Top, r.Width + x, r.Height + y);
        }
    }
}
