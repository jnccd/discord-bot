using AnimatedGif;
using BumpKit;
using Discord;
using MEE7.Backend.HelperFunctions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using static MEE7.Commands.Edit.Edit;
using Color = System.Drawing.Color;

namespace MEE7.Commands.Edit
{
    class GifCommands : EditCommandProvider
    {
        public string rainbowDesc = "I'll try spinning colors that's a good trick";
        public Gif Rainbow(Bitmap b, IMessage m)
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
                using UnsafeBitmapContext c = ImageExtensions.CreateUnsafeContext(re[i]);
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
        public Gif SpinToWin(Bitmap b, IMessage m)
        {
            Vector2 middle = new Vector2(b.Width / 2, b.Height / 2);

            int steps = 20;
            int stepWidth = 360 / steps;
            Bitmap[] re = new Bitmap[steps];
            for (int i = 0; i < steps; i++)
                re[i] = b.RotateImage(-stepWidth * i, middle);

            return new Gif(re, Enumerable.Repeat(33, re.Length).ToArray());
        }

        public string PatDesc = "Pat the picture";
        public Gif Pat(Bitmap b, IMessage m)
        {
            Bitmap[] pats = new Bitmap[5];
            for (int i = 1; i <= 5; i++)
            {
                var patDesignPath = $"Commands{s}Edit{s}Resources{s}pat{s}{i}d.png";
                var patOverlayPath = $"Commands{s}Edit{s}Resources{s}pat{s}{i}o.png";
                pats[i - 1] = PictureCommands.InsertIntoRect(b, m, (Bitmap)Bitmap.FromFile(patDesignPath), (Bitmap)Bitmap.FromFile(patOverlayPath));
            }
            int[] patTimings = new int[] { 40, 40, 40, 40, 40 };

            pats = pats.Select(x => (Bitmap)x.Stretch(new Size(512, 512))).ToArray();

            return new Gif(pats, patTimings);
        }

        public string backAndForthDesc = "Make the gif go backward after it went forward and " +
                "then it goes forward again because it loops and its all very fancy n stuff";
        public Gif BackAndForth(Gif gif, IMessage m)
        {
            return new Gif(gif.Item1.Concat(gif.Item1.Skip(1).Reverse().Select(x => (Bitmap)x.Clone())).ToArray(),
                           gif.Item2.Concat(gif.Item2.Skip(1).Reverse()).ToArray());
        }

        public string getPicDesc = "Get single picture from a gif";
        public Bitmap GetPic(Gif gif, IMessage m, int index = 0)
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
        public Gif ToGif(Bitmap[] input, IMessage m)
        {
            return new Gif(input, Enumerable.Repeat(33, input.Length).ToArray());
        }

        public string toBitmapArrayDesc = "Converts a gif to a bitmap array";
        public Bitmap[] ToBitmapArray(Gif input, IMessage m)
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
            List<Bitmap> re = new List<Bitmap>();
            foreach (Bitmap b in gif.Item1)
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
            int maxWidth = gif.Item1.Select(x => x.Width).Max();
            int maxHeight = gif.Item1.Select(x => x.Height).Max();
            using (AnimatedGifCreator c = new AnimatedGifCreator(s, -1))
                for (int i = 0; i < gif.Item1.Length; i++)
                    c.AddFrame(gif.Item1[i].CropImage(new Rectangle(0, 0, maxWidth, maxHeight)), gif.Item2[i], GifQuality.Bit8);

            guild.CreateEmoteAsync(name, new Discord.Image(s)).Wait();

            foreach (Bitmap b in gif.Item1)
                b.Dispose();
        }

        public string MapGDesc = "Map for gifs, because gifs are special now";
        public Gif MapG(Gif gif, IMessage m, string pipe, string varName = "i", float startValue = 0, float endValue = int.MinValue)
        {
            if (endValue == int.MinValue) endValue = gif.Item1.Length;
            for (int i = 0; i < gif.Item1.Length; i++)
                gif.Item1[i] = (Bitmap)Pipe.Parse(m, pipe.Replace("%" + varName, 
                    (i / (float)gif.Item1.Length * endValue + (1 - (i / (float)gif.Item1.Length)) * startValue).ToString().Replace(",", "."))).
                    Apply(m, gif.Item1[i]);
            return gif;
        }

        static readonly char s = Path.DirectorySeparatorChar;
    }
}
