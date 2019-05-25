using Discord;
using Discord.Audio;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
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
            HelpMenu.WithTitle("Give it a YoutTube link and it'll work.");
        }
        
        class PlayInstance
        {
            public Task playThread;
            public IAudioClient client;
            public ISocketAudioChannel channel;
            public string VideoPath;
        }
        List<PlayInstance> instances = new List<PlayInstance>();

        public override void Execute(SocketMessage message)
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
                PlayInstance instance = instances.FirstOrDefault(x => x.channel.Id == channel.Id);
                if (instance == null)
                {
                    Task thread;
                    lock (this)
                    {
                        string path = Program.DownloadVideoFromYouTube(videoURL);
                        IAudioClient client = channel.ConnectAsync().Result;
                        thread = Program.SendAudioAsync(client, path);
                        instance = new PlayInstance { playThread = thread, channel = channel, client = client, VideoPath = path };
                        instances.Add(instance);
                    }
                    thread.Wait();
                    channel.DisconnectAsync().Wait();
                    instances.Remove(instance);
                }
                else
                {
                    Task thread;
                    lock (this)
                    {
                        instance.channel.DisconnectAsync().Wait();
                        instances.Remove(instance);

                        string path = Program.DownloadVideoFromYouTube(videoURL);
                        IAudioClient client = channel.ConnectAsync().Result;
                        thread = Program.SendAudioAsync(client, path);
                        instance = new PlayInstance { playThread = thread, channel = channel, client = client, VideoPath = path };
                        instances.Add(instance);
                    }

                    thread.Wait();
                    channel.DisconnectAsync().Wait();
                    instances.Remove(instance);
                }
            }
            else
            {
                Program.SendText("You are not in an AudioChannel on this server!", message.Channel).Wait();
                return;
            }
            return;
        }
    }
}
