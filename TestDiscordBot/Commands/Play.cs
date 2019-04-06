using Discord;
using Discord.Audio;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot
{
    public class Play : Command
    {
        public Play() : base("play", "Plays youtube videos", false)
        {
            HelpMenu = new EmbedBuilder();
            HelpMenu.WithTitle("Give it a YoutTube link and it'll work.");
        }

        public override Task Execute(SocketMessage message)
        {
            if (!message.Content.Contains(" "))
            {
                Program.SendEmbed(HelpMenu, message.Channel).Wait();
                return Task.FromResult(default(object));
            }
            string videoURL = message.Content.Split(' ')[1];
            if (!videoURL.StartsWith("https://www.youtube.com/watch?"))
            {
                Program.SendText("That doesn't look like a youtube video link :thinking:", message.Channel).Wait();
                return Task.FromResult(default(object));
            }
            SocketGuild g = Program.GetGuildFromChannel(message.Channel);
            ISocketAudioChannel channel = g.VoiceChannels.FirstOrDefault(x => x.Users.Select(y => y.Id).Contains(message.Author.Id));
            if (channel != null)
            {
                string path = Program.DownloadVideoFromYouTube(videoURL);
                IAudioClient client = channel.ConnectAsync().Result;
                Program.SendAudioAsync(client, path).Wait();
                channel.DisconnectAsync().Wait();
            }
            else
            {
                Program.SendText("You are not in an AudioChannel on this server!", message.Channel).Wait();
                return Task.FromResult(default(object));
            }
            return Task.FromResult(default(object));
        }
    }
}
