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
            new EditCommand("audioFromVoice[WIP]", "Records audio from the voice chat you are currently in", null, typeof(WaveStream),
                new Argument[] { new Argument("Recording Time in Seconds", typeof(int), 5) },
                (SocketMessage m, object[] args, object o) => {

                    int recordingTime = (args[0] as int?).Value * 1000;
                    MemoryStream mem = new MemoryStream();

                    SocketGuild g = Program.GetGuildFromChannel(m.Channel);
                    ISocketAudioChannel channel = g.VoiceChannels.FirstOrDefault(x => x.Users.Select(y => y.Id).Contains(m.Author.Id));
                    if (channel != null)
                    {
                        try { channel.DisconnectAsync().Wait(); } catch { }

                        bool doneListening = false;

                        new Action(async () => {
                            Console.WriteLine("Action was called");

                            IAudioClient client = await channel.ConnectAsync();
                            client.StreamCreated += async (ulong id, AudioInStream stream) => {
                                Console.WriteLine("StreamCreated was called");
                                await Task.Delay(recordingTime);
                                await channel.DisconnectAsync();
                                stream.CopyTo(mem);
                                doneListening = true;
                            };
                            using (WaveStream naudioStream = WaveFormatConversionStream.CreatePcmStream(
                                new StreamMediaFoundationReader(
                                    new FileStream($"Commands{Path.DirectorySeparatorChar}Edit{Path.DirectorySeparatorChar}StartListeningSoundEffect.mp3", FileMode.Open))))
                                await MultiMediaHelper.SendAudioAsync(client, naudioStream);
                        }).Invoke();

                        while (!doneListening)
                            Thread.Sleep(100);

                        Console.WriteLine("Action call ended");
                    }
                    else
                        throw new Exception("You are not in an AudioChannel on this server!");

                    return WaveFormatConversionStream.CreatePcmStream(new StreamMediaFoundationReader(mem));
            }),
            new EditCommand("turingDrawing", "Creates a random turing machine which operates on looped 2D tape", null, typeof(Bitmap[]),
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
        };
        
        enum Direction { Up, Down, Left, Right }
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
