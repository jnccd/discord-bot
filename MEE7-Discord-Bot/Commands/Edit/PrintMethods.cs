using AnimatedGif;
using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace MEE7.Commands
{
    public partial class Edit : Command
    {
        readonly PrintMethod[] PrintMethods = new PrintMethod[]
        {
            new PrintMethod(typeof(string), (SocketMessage m, object o) => {
                DiscordNETWrapper.SendText(o as string, m.Channel).Wait();

            }),
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
                    using (AnimatedGifCreator c = new AnimatedGifCreator(s, 33))
                        foreach (Bitmap b in bs)
                            c.AddFrame(b, -1, GifQuality.Bit8);

                    DiscordNETWrapper.SendFile(s, m.Channel, "gif").Wait();

                    foreach (Bitmap b in bs)
                        b.Dispose();
                }

            }),
            new PrintMethod(typeof(WaveStream), (SocketMessage m, object o) => {
                Stream s = new MemoryStream();
                WaveFileWriter.WriteWavFileToStream(s, o as WaveStream);
                DiscordNETWrapper.SendFile(s, m.Channel, ".mp3").Wait();

            }),
        };
    }
}
