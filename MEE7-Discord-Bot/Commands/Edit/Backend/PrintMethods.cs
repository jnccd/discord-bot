using AnimatedGif;
using Discord;
using MEE7.Backend.HelperFunctions;
using NAudio.Wave;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using static MEE7.Commands.Edit.Edit;

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
            new PrintMethod(typeof(SKBitmap), (IMessage m, object o) => {
                var b = o as SKBitmap;
                DiscordNETWrapper.SendBitmap(b, m.Channel).Wait();
                b.Dispose();

            }),
            new PrintMethod(typeof(SKBitmap[]), (IMessage m, object o) => {
                using MemoryStream s = new MemoryStream();
                SKBitmap[] bs = o as SKBitmap[];
                int maxWidth = bs.Max(x => x.Width);
                int maxHeight = bs.Max(x => x.Height);
                MultiMediaHelper.SaveBitmaps(bs, s);

                DiscordNETWrapper.SendFile(s, m.Channel, "gif").Wait();

                foreach (SKBitmap b in bs)
                    b.Dispose();

            }),
            new PrintMethod(typeof(Gif), (IMessage m, object o) => {
                using MemoryStream s = new MemoryStream();
                Gif gif = o as Gif;
                int maxWidth = gif.Item1.Max(x => x.Width);
                int maxHeight = gif.Item1.Max(x => x.Height);
                MultiMediaHelper.SaveGif(gif, s);

                DiscordNETWrapper.SendFile(s, m.Channel, "gif").Wait();

                foreach (SKBitmap b in gif.Item1)
                    b.Dispose();

            }),
            new PrintMethod(typeof(SKColor), (IMessage m, object o) => {
                SKColor c = (o as SKColor?).Value;
                using SKBitmap b = new(30, 30);
                using SKCanvas g = new(b);
                g.Clear(new SKColor(c.Red, c.Green, c.Blue));
                c.ToHsv(out float h, out float s, out float v);
                DiscordNETWrapper.SendBitmap(b, m.Channel, $"R:{c.Red} G:{c.Green} B:{c.Blue}\n" +
                    $"H:{h} S:{s} B:{v}\n" +
                    $"{c}").Wait();
            }),
            new PrintMethod(typeof(Video), (IMessage m, object o) => {
                Video v = o as Video;
                if (string.IsNullOrWhiteSpace(v.name))
                    DiscordNETWrapper.SendFile(v.filePath, m.Channel).Wait();
                else
                {
                    FileInfo file = new FileInfo(Path.Combine(Program.ExePath, v.filePath));
                    file.Rename(new string(v.name.Where(x => char.IsLetterOrDigit(x) || x == ' ').ToArray()) + ".mp4");
                    DiscordNETWrapper.SendFile(file.FullName, m.Channel).Wait();
                    File.Delete(file.FullName);
                }
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
