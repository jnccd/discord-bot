using Discord;
using Discord.WebSocket;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions.Extensions;
using MEE7.Backend.HelperFunctions;
using Color = System.Drawing.Color;
using BumpKit;

namespace MEE7.Commands
{
    public partial class Edit : Command
    {
        readonly EditCommand[] InputCommands = new EditCommand[] {
            new EditCommand("lastT", "Gets the last messages text", null, typeof(string), new Argument[0], (SocketMessage m, object[] args, object o) => {
                return m.Channel.GetMessagesAsync(2).FlattenAsync().Result.Last().Content;
            }),
            new EditCommand("lastP", "Gets the last messages picture", null, typeof(Bitmap), new Argument[0], (SocketMessage m, object[] args, object o) => {
                IMessage lm = m.Channel.GetMessagesAsync(2).FlattenAsync().Result.Last();
                string pic = null;
                if (lm.Attachments.Count > 0 && lm.Attachments.ElementAt(0).Size > 0)
                {
                    if (lm.Attachments.ElementAt(0).Filename.EndsWith(".png"))
                        pic = lm.Attachments.ElementAt(0).Url;
                    else if (lm.Attachments.ElementAt(0).Filename.EndsWith(".jpg"))
                        pic = lm.Attachments.ElementAt(0).Url;
                }
                string picLink = lm.Content.GetPictureLinkInMessage();
                if (string.IsNullOrWhiteSpace(pic) && picLink != null)
                    pic = picLink;
                return pic.GetBitmapFromURL();
            }),
            new EditCommand("thisT", "Outputs the given text", null, typeof(string), new Argument[] { new Argument("Text", typeof(string), null) }, 
                (SocketMessage m, object[] args, object o) => {
                
                    return args[0];
            }),
            new EditCommand("thisP", "Gets attatched picture / picture from url argument", null, typeof(Bitmap), 
                new Argument[] { new Argument("Picture URL", typeof(string), null) }, 
                (SocketMessage m, object[] args, object o) => {
                
                    return GetPictureLinkFromMessage(m, (string)args[0]).GetBitmapFromURL();
            }),
            new EditCommand("thisG", "Gets attatched gif / gif from url argument", null, typeof(Bitmap[]),
                new Argument[] { new Argument("Gif URL", typeof(string), null) },
                (SocketMessage m, object[] args, object o) => {

                    return GetPictureLinkFromMessage(m, (string)args[0]).GetBitmapsFromGIFURL();
            }),
            new EditCommand("thisA", "Gets mp3 or wav audio files attached to this message", null, typeof(WaveStream), new Argument[0],
                (SocketMessage m, object[] args, object o) => {
                    string url = m.Attachments.FirstOrDefault(x => x.Url.EndsWith(".mp3")).Url;
                    if (!string.IsNullOrWhiteSpace(url))
                        return url.Getmp3AudioFromURL();

                    url = m.Attachments.FirstOrDefault(x => x.Url.EndsWith(".wav")).Url;
                    if (!string.IsNullOrWhiteSpace(url))
                        return url.GetwavAudioFromURL();

                    url = m.Attachments.FirstOrDefault(x => x.Url.EndsWith(".ogg")).Url;
                    if (!string.IsNullOrWhiteSpace(url))
                        return url.GetoggAudioFromURL();

                    throw new Exception("No audio file found!");
            }),
            new EditCommand("profilePicture", "Gets a profile picture", null, typeof(Bitmap),
                new Argument[] { new Argument("User ID / User Mention", typeof(string), null) },
                (SocketMessage m, object[] args, object o) => {

                    SocketUser luser = Program.GetUserFromId(Convert.ToUInt64((args[0] as string).Trim(new char[] { ' ', '<', '>', '@', '!' })));
                    string avatarURL = luser.GetAvatarUrl(ImageFormat.Png, 512);
                    return (string.IsNullOrWhiteSpace(avatarURL) ? luser.GetDefaultAvatarUrl() : avatarURL).GetBitmapFromURL();
            }),
            new EditCommand("serverPicture", "Gets the server picture from a server id", null, typeof(Bitmap),
                new Argument[] { new Argument("Server ID", typeof(string), null) },
                (SocketMessage m, object[] args, object o) => {

                    if (args[0] as string == "")
                        return Program.GetGuildFromChannel(m.Channel).IconUrl.GetBitmapFromURL();
                    else
                        return Program.GetGuildFromID(Convert.ToUInt64((args[0] as string).Trim(new char[] { ' ', '<', '>', '@', '!' }))).IconUrl.GetBitmapFromURL();
            }),
            new EditCommand("emote", "Gets the picture of the emote", null, typeof(Bitmap),
                new Argument[] { new Argument("Emote", typeof(string), null) },
                (SocketMessage m, object[] args, object o) => {

                    Emote.TryParse((args[0] as string).Trim(' '), out Emote res);
                    if (res != null) return res.Url.GetBitmapFromURL();
                    return Program.GetGuildFromChannel(m.Channel).Emotes.
                        FirstOrDefault(x => x.Name.Contains((args[0] as string).Trim(' ', ':'))).Url.
                        GetBitmapFromURL();
            }),
            new EditCommand("gifEmote", "Gets the pictures of the emote", null, typeof(Bitmap[]),
                new Argument[] { new Argument("Emote", typeof(string), null) },
                (SocketMessage m, object[] args, object o) => {

                    Emote.TryParse((args[0] as string).Trim(' '), out Emote res);
                    if (res != null) return res.Url.GetBitmapsFromGIFURL();
                    return Program.GetGuildFromChannel(m.Channel).Emotes.
                        FirstOrDefault(x => x.Name.Contains((args[0] as string).Trim(' ', ':')) && x.Animated).Url.
                        GetBitmapsFromGIFURL();
            }),
            new EditCommand("AudioFromYT", "Gets the mp3 of an youtube video", null, typeof(WaveStream),
                new Argument[] { new Argument("YouTube Video URL", typeof(string), null) },
                (SocketMessage m, object[] args, object o) => {

                    MemoryStream mem = new MemoryStream();
                    using (Process P = MultiMediaHelper.GetAudioStreamFromYouTubeVideo((args[0] as string), "mp3"))
                    {
                        P.StandardOutput.BaseStream.CopyTo(mem);
                        return WaveFormatConversionStream.CreatePcmStream(new StreamMediaFoundationReader(mem));
                    }
            }),
            new EditCommand("TuringDrawing", "Creates a random turing machine which operates on looped 2D tape", null, typeof(Bitmap[]),
                new Argument[] {
                    new Argument("Num States", typeof(int), 6),
                    new Argument("Num Symbols", typeof(int), 3),
                },
                (SocketMessage m, object[] args, object o) => {

                    int states = (int)args[0];
                    int symbols = (int)args[1];
                    List<Tuple<int, Color, int, Color, int, int>> transitions = new List<Tuple<int, Color, int, Color, int, int>>();

                    for (int i = 0; i < states; i++)
                        for (int j = 0; j < symbols; j++)
                            transitions.Add(new Tuple<int, Color, int, Color, int, int>(
                                i,
                                TuringColors[j % TuringColors.Length],
                                Program.RDM.NextDouble() < 0.2 ? Program.RDM.Next(states) : i + 1 % states - 1,
                                TuringColors[Program.RDM.Next(Math.Min(symbols, TuringColors.Length))], 
                                Program.RDM.Next(5) - 2, 
                                Program.RDM.Next(5) - 2));

                    int curState = 0;
                    Bitmap[] re = new Bitmap[120];

                    int curX;
                    int curY;
                    Color curC;
                    re[0] = new Bitmap(100, 100);
                    using (UnsafeBitmapContext c = new UnsafeBitmapContext(re[0]))
                    {
                        curX = re[0].Width / 2;
                        curY = re[0].Height / 2;
                        for (int x = 0; x < re[0].Width; x++)
                            for (int y = 0; y < re[0].Height; y++)
                                c.SetPixel(x, y, Color.White);
                        curC = c.GetPixel(curX, curY);
                    }
                    
                    for (int i = 1; i < re.Length; i++)
                    {
                        re[i] = (Bitmap)re[i - 1].Clone();

                        for (int k = 0; k < 50; k++)
                        {
                            var trans = transitions.FirstOrDefault(x => x.Item1 == curState && x.Item2.ToArgb() == curC.ToArgb());
                            if (trans.Item3 >= states)
                                ;
                            curState = trans.Item3;
                            curC = trans.Item4;
                            curX += trans.Item5;
                            curY += trans.Item6;
                            if (curX >= re[i].Width) curX = 0;
                            if (curY >= re[i].Height) curY = 0;
                            if (curX < 0) curX = re[i].Width - 1;
                            if (curY < 0) curY = re[i].Height - 1;
                            using (UnsafeBitmapContext c = new UnsafeBitmapContext(re[i]))
                                c.SetPixel(curX, curY, curC);
                        }
                    }

                    return re.ToArray();
            }),
        };
        
        static Color[] TuringColors = new Color[] { Color.Black, Color.White, Color.Red, Color.Green, Color.Yellow, Color.Purple, Color.Blue, Color.Brown, Color.Gold, Color.Gray };
        private static string GetPictureLinkFromMessage(SocketMessage m, string arguments)
        {
            string pic = null;
            if (m.Attachments.Count > 0 && m.Attachments.ElementAt(0).Size > 0)
            {
                if (m.Attachments.ElementAt(0).Filename.EndsWith(".png"))
                    pic = m.Attachments.ElementAt(0).Url;
                else if (m.Attachments.ElementAt(0).Filename.EndsWith(".jpg"))
                    pic = m.Attachments.ElementAt(0).Url;
                else if (m.Attachments.ElementAt(0).Filename.EndsWith(".gif"))
                    pic = m.Attachments.ElementAt(0).Url;
            }
            string picLink = arguments.GetPictureLinkInMessage();
            if (string.IsNullOrWhiteSpace(pic) && picLink != null)
                pic = picLink;
            return pic;
        }
    }
}
