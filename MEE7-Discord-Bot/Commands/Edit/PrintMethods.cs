using AnimatedGif;
using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using MEE7.Backend.HelperFunctions.Extensions;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace MEE7.Commands
{
    public partial class Edit : Command
    {
        static readonly PrintMethod[] PrintMethods = new PrintMethod[]
        {
            new PrintMethod(typeof(EmbedBuilder), (SocketMessage m, object o) => {
                DiscordNETWrapper.SendEmbed(o as EmbedBuilder, m.Channel).Wait();

            }),
            new PrintMethod(typeof(Tuple<string, EmbedBuilder>), (SocketMessage m, object o) => {
                var t = o as Tuple<string, EmbedBuilder>;
                DiscordNETWrapper.SendEmbed(t.Item2, m.Channel).Wait();
                DiscordNETWrapper.SendText(t.Item1, m.Channel).Wait();

            }),
            new PrintMethod(typeof(Bitmap), (SocketMessage m, object o) => {
                var b = o as Bitmap;
                DiscordNETWrapper.SendBitmap(b, m.Channel).Wait();
                b.Dispose();

            }),
            new PrintMethod(typeof(Bitmap[]), (SocketMessage m, object o) => {
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
            new PrintMethod(typeof(Gif), (SocketMessage m, object o) => {
                using (MemoryStream s = new MemoryStream())
                {
                    Gif gif = o as Gif;
                    int maxWidth = gif.Item1.Select(x => x.Width).Max();
                    int maxHeight = gif.Item1.Select(x => x.Height).Max();
                    using (AnimatedGifCreator c = new AnimatedGifCreator(s, -1))
                        for (int i = 0; i < gif.Item1.Length; i++)
                            c.AddFrame(gif.Item1[i].CropImage(new Rectangle(0, 0, maxWidth, maxHeight)), gif.Item2[i], GifQuality.Bit8);

                    DiscordNETWrapper.SendFile(s, m.Channel, "gif").Wait();

                    foreach (Bitmap b in gif.Item1)
                        b.Dispose();
                }

            }),
            new PrintMethod(typeof(WaveStream), (SocketMessage m, object o) => {
                Stream s = new MemoryStream();
                WaveFileWriter.WriteWavFileToStream(s, o as WaveStream);
                DiscordNETWrapper.SendFile(s, m.Channel, ".mp3").Wait();

            }),
            new PrintMethod(typeof(IWaveProvider), (SocketMessage m, object o) => {
                Stream s = new MemoryStream();
                WaveFileWriter.WriteWavFileToStream(s, o as IWaveProvider);
                DiscordNETWrapper.SendFile(s, m.Channel, ".mp3").Wait();

            }),
            new PrintMethod(typeof(object), (SocketMessage m, object o) => {
                DiscordNETWrapper.SendText(o.ToString(), m.Channel).Wait();

            }),
        };
    }
}
