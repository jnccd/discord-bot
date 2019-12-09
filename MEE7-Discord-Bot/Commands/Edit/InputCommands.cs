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
using Discord.Audio;
using System.Threading;
using Discord.Audio.Streams;
using System.Numerics;

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
            new EditCommand("thisP", "Gets attached picture / picture from url argument", null, typeof(Bitmap), 
                new Argument[] { new Argument("Picture URL", typeof(string), null) }, 
                (SocketMessage m, object[] args, object o) => {
                
                    return GetPictureLinkFromMessage(m, (string)args[0]).GetBitmapFromURL();
            }),
            new EditCommand("thisG", "Gets attached gif / gif from url argument", null, typeof(Bitmap[]),
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
            new EditCommand("profilePictureG", "Gets a profile picture gif", null, typeof(Bitmap[]),
                new Argument[] { new Argument("User ID / User Mention", typeof(string), null) },
                (SocketMessage m, object[] args, object o) => {

                    SocketUser luser = Program.GetUserFromId(Convert.ToUInt64((args[0] as string).Trim(new char[] { ' ', '<', '>', '@', '!' })));
                    string avatarURL = luser.GetAvatarUrl(ImageFormat.Gif, 512);
                    return (string.IsNullOrWhiteSpace(avatarURL) ? luser.GetDefaultAvatarUrl() : avatarURL).GetBitmapsFromGIFURL();
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
            new EditCommand("emoteG", "Gets the pictures of the emote", null, typeof(Bitmap[]),
                new Argument[] { new Argument("Emote", typeof(string), null) },
                (SocketMessage m, object[] args, object o) => {

                    Emote.TryParse((args[0] as string).Trim(' '), out Emote res);
                    if (res != null) return res.Url.GetBitmapsFromGIFURL();
                    return Program.GetGuildFromChannel(m.Channel).Emotes.
                        FirstOrDefault(x => x.Name.Contains((args[0] as string).Trim(' ', ':')) && x.Animated).Url.
                        GetBitmapsFromGIFURL();
            }),
            new EditCommand("mandelbrot", "Render a mandelbrot", null, typeof(Bitmap), 
                new Argument[] { 
                    new Argument("Zoom", typeof(double), 1),
                    new Argument("Camera", typeof(Vector2), new Vector2(0, 0)),
                    new Argument("Passes", typeof(int), 40),
                },
                (SocketMessage m, object[] a, object o) => {

                    Bitmap bmp = new Bitmap(500, 500);
                    double zoom = (a[0] as double?).GetValueOrDefault();
                    Vector2 cam = (a[1] as Vector2?).GetValueOrDefault();
                    int passes = (a[2] as int?).GetValueOrDefault();

                    if (passes > 200)
                        throw new Exception("200 passes should be enough, everything else would be spam and you dont wanna spam");

                    zoom = Math.Pow(2, -zoom);

                    using (UnsafeBitmapContext con = new UnsafeBitmapContext(bmp))
                        for (int x = 0; x < bmp.Width; x++)
                            for (int y = 0; y < bmp.Height; y++)
                            {
                                double cre = (2 * ((x / (double)bmp.Width) - 0.5)) * zoom - cam.X;
                                double cim = (2 * ((y / (double)bmp.Height) - 0.5)) * zoom - cam.Y;

                                double x2 = 0, y2 = 0;
                                int count = 0;

                                for (int i = 0; i < passes; i++)
	                            {
		                            if (x2* x2 + y2* y2 < 1)
                                         count++;

                                    double xtemp = x2 * x2 - y2 * y2 + cre;
                                    y2 = 2 * x2* y2 + cim;
		                            x2 = xtemp;
	                            }

                                var float4 = HSVtoRGB((float)((Math.Atan2(y2, x2) + Math.PI) / (Math.PI * 2) * 360), 1, count * 10);
                                con.SetPixel(x, y, Color.FromArgb((x2+y2 < 1000 ? 255 : 0), (int)(float4.X * 255), (int)(float4.Y * 255), (int)(float4.Z * 255)));
                            }

                    return bmp;
            }),
            new EditCommand("turingDrawing?", "Creates a random turing machine which operates on looped 2D tape", null, typeof(Bitmap[]),
                new Argument[] {
                    new Argument("Num States", typeof(int), 6),
                    new Argument("Num Symbols", typeof(int), 3),
                    new Argument("Draw Machine?", typeof(bool), false),
                },
                (SocketMessage m, object[] args, object o) => {

                    int states = (int)args[0];
                    int symbols = (int)args[1];

                    int sizeX = 300, sizeY = sizeX;

                    Tuple<int, int, Direction>[,] transitions = new Tuple<int, int, Direction>[states, symbols];

                    for (int i = 0; i < states; i++)
                        for (int j = 0; j < symbols; j++)
                            transitions[i, j] = new Tuple<int, int, Direction>(
                                Program.RDM.Next(states),
                                Program.RDM.Next(Math.Min(symbols, TuringColors.Length)),
                                (Direction)Program.RDM.Next(Enum.GetValues(typeof(Direction)).Length));

                    if ((bool)args[2])
                    {
                        string machine = $"States: {Enumerable.Range(0,states).Select(x => x.ToString()).Combine()}" +
                            $"Symbols: {Enumerable.Range(0,Math.Min(symbols, TuringColors.Length)).Select(x => TuringColors[x].ToString()).Combine()}" +
                            $"Transitions: ";
                        for (int i = 0; i < states; i++)
                            for (int j = 0; j < symbols; j++)
                                machine += $"\n({i}, {TuringColors[j]}) -> ({transitions[i, j].ToString()})";
                        DiscordNETWrapper.SendText(machine, m.Channel).Wait();

                        //Bitmap b = new Bitmap(1000, 1000);
                        //AdjacencyGraph<int, Edge<int>> a = new AdjacencyGraph<int, Edge<int>>();
                        //for (int i = 0; i < states; i++)
                        //    a.AddVertex(i);
                        //foreach (var trans in transitions)
                        //    a.AddEdge(new Edge<int>(trans.Item1, trans.Item3));
                        //GraphvizAlgorithm<int, Edge<int>> v = new GraphvizAlgorithm<int, Edge<int>>(a);
                        //v.Generate();
                    }

                    Bitmap[] re = new Bitmap[100];

                    int curState = 0;
                    int curSymbol = 0;
                    int[,] curTape = new int[sizeX, sizeY];
                    int curX = sizeX / 2;
                    int curY = sizeY / 2;

                    for (int i = 0; i < re.Length; i++)
                    {
                        for (int k = 0; k < 500; k++)
                        {
                            var trans = transitions[curState, curSymbol];
                            curState = trans.Item1;
                            curSymbol = trans.Item2;
                            curTape[curX,curY] = curSymbol;

                            if (trans.Item3 == Direction.Down) curY++;
                            else if (trans.Item3 == Direction.Up) curY--;
                            else if (trans.Item3 == Direction.Left) curX--;
                            else if (trans.Item3 == Direction.Right) curX++;

                            if (curX >= sizeX) curX = 0;
                            if (curY >= sizeY) curY = 0;
                            if (curX < 0) curX = sizeX - 1;
                            if (curY < 0) curY = sizeY - 1;
                        }

                        re[i] = new Bitmap(sizeX, sizeY);
                        using (UnsafeBitmapContext c = new UnsafeBitmapContext(re[i]))
                            for (int x = 0; x < sizeX; x++)
                                for (int y = 0; y < sizeY; y++)
                                    c.SetPixel(x,y,TuringColors[curTape[x,y]]);
                    }

                    return re;
            }),
            new EditCommand("audioFromYT", "Gets the mp3 of an youtube video", null, typeof(WaveStream),
                new Argument[] { new Argument("YouTube Video URL", typeof(string), null) },
                (SocketMessage m, object[] args, object o) => {

                    MemoryStream mem = new MemoryStream();
                    using (Process P = MultiMediaHelper.GetAudioStreamFromYouTubeVideo((args[0] as string), "mp3"))
                    {
                        P.StandardOutput.BaseStream.CopyTo(mem);
                        return WaveFormatConversionStream.CreatePcmStream(new StreamMediaFoundationReader(mem));
                    }
            }),
            new EditCommand("audioFromVoice[WIP]?", "Records audio from the voice chat you are currently in", null, typeof(WaveStream),
                new Argument[] { new Argument("User ID", typeof(ulong), 0), new Argument("Recording Time in Seconds", typeof(int), 5) },
                (SocketMessage m, object[] args, object o) => {

                    string filePath = $"Commands{Path.DirectorySeparatorChar}Edit{Path.DirectorySeparatorChar}audioFromVoice.bin";

                    ulong? userID = args[0] as ulong?;
                    int recordingTime = (args[1] as int?).Value;
                    MemoryStream memOut = new MemoryStream();

                    if (recordingTime > 10)
                        throw new Exception("Thats too long UwU");

                    using (MemoryStream mem = new MemoryStream()) {

                        SocketGuild g = Program.GetGuildFromChannel(m.Channel);
                        ISocketAudioChannel channel = g.VoiceChannels.FirstOrDefault(x => x.Users.Select(y => y.Id).Contains(m.Author.Id));

                        if (channel == null)
                            throw new Exception("You are not in an AudioChannel on this server!");

                        try { channel.DisconnectAsync().Wait(); } catch { }

                        bool doneListening = false;

                        new Action(async () => {
                            try
                            {
                                Thread.CurrentThread.Name = "Fuck";
                                IAudioClient client = await channel.ConnectAsync();

                                using (WaveStream naudioStream = WaveFormatConversionStream.CreatePcmStream(
                                    new StreamMediaFoundationReader(
                                        new FileStream($"Commands{Path.DirectorySeparatorChar}Edit{Path.DirectorySeparatorChar}StartListeningSoundEffect.mp3", FileMode.Open))))
                                    await MultiMediaHelper.SendAudioAsync(client, naudioStream);

                                var u = (SocketGuildUser)(await channel.GetUsersAsync().FlattenAsync()).FirstOrDefault(x => userID == 0 ? !x.IsBot : !x.IsBot && x.Id == userID);

                                if (u == null)
                                    throw new Exception("I cant find that user!");

                                var streamMeUpScotty = (InputStream)u.AudioStream;

                                var buffer = new byte[4096];
                                DateTime startListeningTime = DateTime.Now;
                                while (await streamMeUpScotty.ReadAsync(buffer, 0, buffer.Length) > 0 && (DateTime.Now - startListeningTime).TotalSeconds < recordingTime)
                                    mem.Write(buffer, 0, buffer.Length);

                                try { channel.DisconnectAsync().Wait(); } catch { }

                                mem.Position = 0;
                                using (FileStream f = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                                    mem.CopyTo(f);

                                using (Process P = MultiMediaHelper.CreateFfmpegOut(filePath))
                                    P.StandardOutput.BaseStream.CopyTo(memOut);

                                File.Delete(filePath);
                            }
                            catch { }
                            finally
                            {
                                try { channel.DisconnectAsync().Wait(); } catch { }
                                doneListening = true;
                            }
                        }).Invoke();

                        while (!doneListening)
                            Thread.Sleep(100);

                        memOut.Position = 0;
                        return WaveFormatConversionStream.CreatePcmStream(new StreamMediaFoundationReader(memOut));
                    }
            }),
        };
        
        enum Direction { Up, Down, Left, Right }
        static Color[] TuringColors = new Color[] { Color.Black, Color.White, Color.Red, Color.Green, Color.Yellow, Color.Purple, Color.Blue, Color.Brown, Color.Gold, Color.Gray };

        private static Vector4 HSVtoRGB(float H, float S, float V)
        {
            H %= 360;
            if (S > 1)
                S = 1;
            if (S < 0)
                S = 0;
            if (V > 1)
                V = 1;
            if (V < 0)
                V = 0;

            //using wikipedia formulas https://de.wikipedia.org/wiki/HSV-Farbraum
            int hi = (int)(H / 60);
            float f = (H / 60 - hi);
            float p = V * (1 - S);
            float q = V * (1 - S * f);
            float t = V * (1 - S * (1 - f));

            if (hi == 0 || hi == 6)
                return new Vector4(V, t, p, 1);
            else if (hi == 1)
                return new Vector4(q, V, p, 1);
            else if (hi == 2)
                return new Vector4(p, V, t, 1);
            else if (hi == 3)
                return new Vector4(p, q, V, 1);
            else if (hi == 4)
                return new Vector4(t, p, V, 1);
            else if (hi == 5)
                return new Vector4(V, p, q, 1);
            else
                return new Vector4(0, 0, 0, 0);
        }
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
