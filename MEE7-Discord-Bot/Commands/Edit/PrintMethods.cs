using AnimatedGif;
using Discord;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using NAudio.Wave;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using static MEE7.Commands.Edit.Edit;
using Color = System.Drawing.Color;

namespace MEE7.Commands.Edit
{
    class PrintMethod
    {
        public Type Type;
        public Action<IMessage, object> Function;

        public PrintMethod(Type Type, Action<IMessage, object> Function)
        {
            this.Function = Function;
            this.Type = Type;
        }

        public static readonly PrintMethod[] PrintMethods = new PrintMethod[]
        {
            new PrintMethod(typeof(EmbedBuilder), (IMessage m, object o) => {
                DiscordNETWrapper.SendEmbed(o as EmbedBuilder, m.Channel).Wait();

            }),
            new PrintMethod(typeof(Tuple<string, EmbedBuilder>), (IMessage m, object o) => {
                var t = o as Tuple<string, EmbedBuilder>;
                DiscordNETWrapper.SendEmbed(t.Item2, m.Channel).Wait();
                DiscordNETWrapper.SendText(t.Item1, m.Channel).Wait();

            }),
            new PrintMethod(typeof(Bitmap), (IMessage m, object o) => {
                var b = o as Bitmap;
                DiscordNETWrapper.SendBitmap(b, m.Channel).Wait();
                b.Dispose();

            }),
            new PrintMethod(typeof(Bitmap[]), (IMessage m, object o) => {
                using (MemoryStream s = new MemoryStream())
                {
                    Bitmap[] bs = o as Bitmap[];
                    int maxWidth = bs.Select(x => x.Width).Max();
                    int maxHeight = bs.Select(x => x.Height).Max();
                    using (AnimatedGifCreator c = new AnimatedGifCreator(s, 33))
                        foreach (Bitmap b in bs)
                            c.AddFrame(b.CropImage(new Rectangle(0, 0, maxWidth, maxHeight)), -1, GifQuality.Bit8);

                    DiscordNETWrapper.SendFile(s, m.Channel, "gif").Wait();

                    foreach (Bitmap b in bs)
                        b.Dispose();
                }

            }),
            new PrintMethod(typeof(Gif), (IMessage m, object o) => {
                using MemoryStream s = new MemoryStream(); Gif gif = o as Gif;
                int maxWidth = gif.Item1.Select(x => x.Width).Max();
                int maxHeight = gif.Item1.Select(x => x.Height).Max();
                using (AnimatedGifCreator c = new AnimatedGifCreator(s, -1))
                    for (int i = 0; i < gif.Item1.Length; i++)
                        c.AddFrame(gif.Item1[i].CropImage(new Rectangle(0, 0, maxWidth, maxHeight)),
                            gif.Item2[i], GifQuality.Bit8);

                DiscordNETWrapper.SendFile(s, m.Channel, "gif").Wait();

                foreach (Bitmap b in gif.Item1)
                    b.Dispose();

            }),
            new PrintMethod(typeof(Color), (IMessage m, object o) => {
                Color c = (o as Color?).Value;
                using Bitmap b = new Bitmap(30, 30);
                using Graphics g = Graphics.FromImage(b);
                g.FillRectangle(new SolidBrush(c), 0, 0, 30, 30);
                DiscordNETWrapper.SendBitmap(b, m.Channel, $"{c.R} {c.G} {c.B}").Wait();
            }),
            new PrintMethod(typeof(WaveStream), (IMessage m, object o) => {
                Stream s = new MemoryStream();
                WaveFileWriter.WriteWavFileToStream(s, o as WaveStream);
                DiscordNETWrapper.SendFile(s, m.Channel, ".mp3").Wait();

            }),
            new PrintMethod(typeof(IWaveProvider), (IMessage m, object o) => {
                Stream s = new MemoryStream();
                WaveFileWriter.WriteWavFileToStream(s, o as IWaveProvider);
                DiscordNETWrapper.SendFile(s, m.Channel, ".mp3").Wait();

            }),
            new PrintMethod(typeof(Array), (IMessage m, object o) => {
                Array arr = o as Array;
                DiscordNETWrapper.SendText(
                    $"[{Enumerable.Range(0, arr.Length).Select(x => arr.GetValue(x).ToString()).Combine(", ")}]",
                    m.Channel).Wait();

            }),
            new PrintMethod(typeof(object), (IMessage m, object o) => {
                DiscordNETWrapper.SendText(o.ToString(), m.Channel).Wait();

            }),
        };
    }
}
