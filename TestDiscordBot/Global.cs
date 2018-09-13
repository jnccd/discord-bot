using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot
{
    public static class Global
    {
        public static Random RDM = new Random();
        public static Program P;
        public static string commandString = "!";

        public static async Task SendFile(string path, ISocketMessageChannel Channel)
        {
            await Channel.SendFileAsync(path);

            SaveChannel(Channel);
        }
        public static async Task SendText(string text, ISocketMessageChannel Channel)
        {
            await Channel.SendMessageAsync(text);

            SaveChannel(Channel);
        }
        public static void SaveChannel(ISocketMessageChannel Channel)
        {
            if (config.Default.ChannelsWrittenOn == null)
                config.Default.ChannelsWrittenOn = new ulong[0];
            List<ulong> longs = config.Default.ChannelsWrittenOn.ToList();
            if (!longs.Contains(Channel.Id))
            {
                longs.Add(Channel.Id);
                config.Default.ChannelsWrittenOn = longs.ToArray();
                config.Default.Save();
            }
        }
    }
}
