using Discord;
using Discord.Audio;
using Discord.WebSocket;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEE7
{
    public class Play : Command
    {
        public Play() : base("play", "Plays youtube videos", false)
        {
            HelpMenu = new EmbedBuilder();
            HelpMenu.WithTitle("Give it a YoutTube link and it'll ~~maybe~~ work.");
        }

        public override async void Execute(SocketMessage message)
        {
            if (!message.Content.Contains(" "))
            {
                Program.SendEmbed(HelpMenu, message.Channel).Wait();
                return;
            }

            SocketGuild g = Program.GetGuildFromChannel(message.Channel);
            ISocketAudioChannel channel = g.VoiceChannels.FirstOrDefault(x => x.Users.Select(y => y.Id).Contains(message.Author.Id));

            string videoURL = message.Content.Split(' ')[1];
            if (!videoURL.StartsWith("https://www.youtube.com/watch?"))
            {
                Program.SendText("That doesn't look like a youtube video link :thinking:", message.Channel).Wait();
                return;
            }

            if (channel != null)
            {
                try { channel.DisconnectAsync().Wait(); } catch { }

                IAudioClient client = await channel.ConnectAsync();
                using (Process P = Program.GetAudioStreamFromYouTubeVideo(videoURL, "mp3"))
                using (MemoryStream mem = new MemoryStream())
                {
                    while (true)
                    {
                        Task.Delay(1001).Wait();
                        if (string.IsNullOrWhiteSpace(P.StandardError.ReadLine()))
                            break;
                    }
                    P.StandardOutput.BaseStream.CopyTo(mem);
                    using (WaveStream naudioStream = WaveFormatConversionStream.CreatePcmStream(new StreamMediaFoundationReader(mem)))
                        Program.SendAudioAsync(client, naudioStream).Wait();
                }

                try { channel.DisconnectAsync().Wait(); } catch { }
            }
            else
            {
                Program.SendText("You are not in an AudioChannel on this server!", message.Channel).Wait();
                return;
            }
        }
    }
}
