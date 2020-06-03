using Discord;
using Discord.Audio;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MEE7.Commands
{
    public class Play : Command
    {
        public Play() : base("play", "Plays youtube videos in voice chats", false, false)
        {
            HelpMenu = new EmbedBuilder();
            HelpMenu.WithTitle("Give it a YoutTube link and it'll ~~maybe~~ work.");
        }

        public override async void Execute(IMessage message)
        {
            if (!message.Content.Contains(" "))
            {
                DiscordNETWrapper.SendEmbed(HelpMenu, message.Channel).Wait();
                return;
            }

            SocketGuild g = Program.GetGuildFromChannel(message.Channel);
            ISocketAudioChannel channel = g.VoiceChannels.FirstOrDefault(x => x.Users.Select(y => y.Id).Contains(message.Author.Id));

            string videoURL = message.Content.Split(' ')[1];
            if (!videoURL.StartsWith("https://www.youtube.com/watch?"))
            {
                DiscordNETWrapper.SendText("That doesn't look like a youtube video link :thinking:", message.Channel).Wait();
                return;
            }

            if (channel != null)
            {
                try { channel.DisconnectAsync().Wait(); } catch { }

                IAudioClient client = await channel.ConnectAsync();
                using (Process P = MultiMediaHelper.GetAudioStreamFromYouTubeVideo(videoURL, "mp3"))
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
                        MultiMediaHelper.SendAudioAsync(client, naudioStream).Wait();
                }

                try { channel.DisconnectAsync().Wait(); } catch { }
            }
            else
            {
                DiscordNETWrapper.SendText("You are not in an AudioChannel on this server!", message.Channel).Wait();
                return;
            }
        }
    }
}
