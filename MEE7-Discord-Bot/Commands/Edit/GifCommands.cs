using AnimatedGif;
using AnimatedGif.ImageSharp;
using Discord;
using MEE7.Backend.HelperFunctions;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using static MEE7.Commands.Edit.Edit;

namespace MEE7.Commands.Edit
{
    class GifCommands : EditCommandProvider
    {
        public string rainbowDesc = "I'll try spinning colors that's a good trick";
        public Gif Rainbow(SKBitmap bitmap, IMessage m)
        {
            var HSVimage = new Vector3[bitmap.Width, bitmap.Height];
            var Alphas = new byte[bitmap.Width, bitmap.Height];

            using (var pixmap = bitmap.PeekPixels())
            {
                Span<byte> pixels = pixmap.GetPixelSpan();
                for (int y = 0; y < bitmap.Height; y++)
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        // Calculate the offset for the current pixel
                        int offset = y * pixmap.RowBytes + x * pixmap.BytesPerPixel;

                        // Read RGBA components (assuming SKColorType.Rgba8888)
                        byte red = pixels[offset + 0];
                        byte green = pixels[offset + 1];
                        byte blue = pixels[offset + 2];
                        byte alpha = pixels[offset + 3];

                        SkiaSharpExtensions.ColorToHSV(new SKColor(red, green, blue, alpha), out double hue, out double saturation, out double value);
                        Alphas[x, y] = alpha;
                        HSVimage[x, y] = new Vector3((float)hue, (float)saturation, (float)value);
                    }
            }

            int steps = 20;
            int stepWidth = 360 / steps;
            SKBitmap[] re = new SKBitmap[steps];
            for (int i = 0; i < steps; i++)
            {
                re[i] = new SKBitmap(bitmap.Width, bitmap.Height, SKColorType.Rgba8888, SKAlphaType.Premul);

                using (var pixmap = bitmap.PeekPixels())
                {
                    Span<byte> pixels = pixmap.GetPixelSpan();
                    for (int y = 0; y < bitmap.Height; y++)
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            var alphaless = HSVimage[x, y].HsvToRgb();
                            pixels.SetPixel(x, y, pixmap.RowBytes, new SKColor(alphaless.Red, alphaless.Green, alphaless.Blue, Alphas[x, y]));
                            HSVimage[x, y].X += stepWidth;
                            while (HSVimage[x, y].X > 360)
                                HSVimage[x, y].X -= 360;
                        }
                }
            }

            return new Gif(re, Enumerable.Repeat(33, re.Length).ToArray());
        }

        public string spinToWinDesc = "I'll try spinning that's a good trick";
        public Gif SpinToWin(SKBitmap b, IMessage m)
        {
            Vector2 middle = new Vector2(b.Width / 2, b.Height / 2);

            int steps = 20;
            int stepWidth = 360 / steps;
            SKBitmap[] re = new SKBitmap[steps];
            for (int i = 0; i < steps; i++)
                re[i] = b.RotateImage(-stepWidth * i, middle);

            return new Gif(re, Enumerable.Repeat(33, re.Length).ToArray());
        }

        public string PatDesc = "Pat the picture";
        public Gif Pat(SKBitmap b, IMessage m)
        {
            SKBitmap[] pats = new SKBitmap[5];
            for (int i = 1; i <= 5; i++)
            {
                var patDesignPath = $"Commands{s}Edit{s}Resources{s}pat{s}{i}d.png";
                var patOverlayPath = $"Commands{s}Edit{s}Resources{s}pat{s}{i}o.png";
                pats[i - 1] = PictureCommands.InsertIntoRect(b, m, SKBitmap.Decode(patDesignPath), SKBitmap.Decode(patOverlayPath), false);
            }
            int[] patTimings = new int[] { 40, 40, 40, 40, 40 };

            pats = pats.Select(x => x.Stretch(128, 128)).ToArray();

            return new Gif(pats, patTimings);
        }

        public string backAndForthDesc = "Make the gif go backward after it went forward and " +
                "then it goes forward again because it loops and its all very fancy n stuff";
        public Gif BackAndForth(Gif gif, IMessage m)
        {
            return new Gif(gif.Item1.Concat(gif.Item1.Skip(1).Reverse().Select(x => x.Copy())).ToArray(),
                           gif.Item2.Concat(gif.Item2.Skip(1).Reverse()).ToArray());
        }

        public string getPicDesc = "Get single picture from a gif";
        public SKBitmap GetPic(Gif gif, IMessage m, int index = 0)
        {
            for (int i = 0; i < gif.Item1.Length; i++)
                if (i != index)
                    gif.Item1[i].Dispose();
            return gif.Item1[index];
        }

        public string getNumFramesDesc = "Get the number of gif frames";
        public int GetNumFrames(Gif gif, IMessage m)
        {
            int num = gif.Item1.Count();
            for (int i = 0; i < gif.Item1.Length; i++)
                gif.Item1[i].Dispose();
            return num;
        }

        public string toGifDesc = "Converts a bitmap array to a gif";
        public Gif ToGif(SKBitmap[] input, IMessage m)
        {
            return new Gif(input, Enumerable.Repeat(33, input.Length).ToArray());
        }

        public string toBitmapArrayDesc = "Converts a gif to a bitmap array";
        public SKBitmap[] ToBitmapArray(Gif input, IMessage m)
        {
            return input.Item1;
        }

        public string ChangeSpeedDesc = "Change the gifs playback speed";
        public Gif ChangeSpeed(Gif gif, IMessage m, float multiplier)
        {
            return new Gif(gif.Item1, gif.Item2.Select(x => (int)(x * multiplier)).ToArray());
        }

        public string setDelayDesc = "Change the gifs playback speed";
        public Gif SetDelay(Gif gif, IMessage m, int delay)
        {
            return new Gif(gif.Item1, gif.Item2.Select(x => delay).ToArray());
        }

        public string MultiplyFramesDesc = "Copy each frame x times";
        public Gif MultiplyFrames(Gif gif, IMessage m, int x)
        {
            List<SKBitmap> re = new List<SKBitmap>();
            foreach (SKBitmap b in gif.Item1)
                for (int i = 0; i < x; i++)
                    re.Add(b);

            return new Gif(re.ToArray(), gif.Item2.Select(y => y / x > 0 ? y / x : 1).ToArray());
        }

        public string ReverseDesc = "Make the gif go backwards";
        public Gif Reverse(Gif gif, IMessage m)
        {
            return new Gif(gif.Item1.Reverse().ToArray(),
                           gif.Item2.Reverse().ToArray());
        }

        public string AddAsEmoteGDesc = "Add the gif to the server emotes [WIP]!";
        public void AddAsEmoteG(Gif gif, IMessage m, string name = "uwu")
        {
            var guild = Program.GetGuildFromChannel(m.Channel);
            var guildUser = guild.GetUser(m.Author.Id);

            if (!guildUser.GuildPermissions.AddReactions)
                throw new Exception("U no have permission for dis :c");

            using MemoryStream s = new MemoryStream();
            MultiMediaHelper.SaveGif(gif, s);

            guild.CreateEmoteAsync(name, new Discord.Image(s)).Wait();

            foreach (SKBitmap b in gif.Item1)
                b.Dispose();
        }

        public string ForBDesc = "foori foori for bitmaps, requires a color to color lambda pipe p";
        public SKBitmap[] ForB(SKBitmap bitmap, IMessage m, Pipe thresholder, Pipe col, string varName, float startValue, float endValue, int steps)
        {
            if (steps > 150)
                throw new Exception("Too many steps >:(");

            float mul = (endValue - startValue) / steps;
            int i = 0;
            return Enumerable.Repeat(bitmap, steps).
                Select(a =>
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
                                    pixels.SetPixel(x, y, pixmap.RowBytes, (SKColor)col.Apply(m, c, new Dictionary<string, object>() { { varName, i * mul + startValue } }));
                            }
                    }
                    return a;
                }).ToArray();
        }

        public string MapGDesc = "Map for gifs, because gifs are special now";
        public Gif MapG(Gif gif, IMessage m, string pipe, string varName = "i", float startValue = 0, float endValue = int.MinValue)
        {
            if (endValue == int.MinValue) endValue = gif.Item1.Length;
            for (int i = 0; i < gif.Item1.Length; i++)
                gif.Item1[i] = (SKBitmap)Pipe.Parse(m, pipe.Replace("%" + varName,
                    (i / (float)gif.Item1.Length * endValue + (1 - (i / (float)gif.Item1.Length)) * startValue).ToString().Replace(",", "."))).
                    Apply(m, gif.Item1[i]);
            return gif;
        }

        public string ApplyToLighthouseDesc = "";
        public void ApplyToLighthouse(Gif gif, IMessage m)
        {
            if (m.Author.Id != Program.Master.Id)
                throw new Exception("ApplyToLighthouse is admin only :P");

            var smallerImgs = gif.Item1.Select(b => b.Stretch(28, 14)).ToArray();
            var timings = gif.Item2;

            List<byte[,,]> smallerByteArrays = new();
            for (int i = 0; i < smallerImgs.Length; i++)
            {
                smallerByteArrays.Add(new byte[14, 28, 3]);
                for (int x = 0; x < smallerImgs[i].Width; x++)
                {
                    for (int y = 0; y < smallerImgs[i].Height; y++)
                    {
                        var c = smallerImgs[i].GetPixel(x, y);
                        smallerByteArrays[i][y, x, 0] = c.Red;
                        smallerByteArrays[i][y, x, 1] = c.Green;
                        smallerByteArrays[i][y, x, 2] = c.Blue;
                    }
                }
            }

            lock (Lighthouse.images)
            {
                Lighthouse.images = Enumerable.
                    Zip(smallerByteArrays, timings).
                    Select(x => new Tuple<byte[,,], int>(x.First, x.Second)).
                    ToList();
            }
        }

        static readonly char s = Path.DirectorySeparatorChar;
    }
}
