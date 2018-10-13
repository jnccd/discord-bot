using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot
{
    public static class Global
    {
        public static Random RDM = new Random();
        public static Program P;
        public static string commandString = "!";
        public static string CurrentExecutablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static async Task SendFile(string path, ISocketMessageChannel Channel)
        {
            await Channel.SendFileAsync(path);

            SaveChannel(Channel);
        }
        public static async Task SendFile(string path, string text, ISocketMessageChannel Channel)
        {
            await Channel.SendFileAsync(path, text);

            SaveChannel(Channel);
        }
        public static async Task SendText(string text, ISocketMessageChannel Channel)
        {
            await Channel.SendMessageAsync(text);

            SaveChannel(Channel);
        }
        public static async Task SendText(string text, ulong ChannelID)
        {
            ISocketMessageChannel Channel = (ISocketMessageChannel)P.client.GetChannel(ChannelID);
            await Channel.SendMessageAsync(text);

            SaveChannel(Channel);
        }
        public static async Task SendEmbed(EmbedBuilder Embed, ISocketMessageChannel Channel)
        {
            await Channel.SendMessageAsync("", false, Embed.Build());

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

        // Extensions
        public static List<int> AllIndexesOf(this string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }
        public static string GetEverythingBetween(this string str, string left, string right)
        {
            int leftIndex = str.IndexOf(left);
            int rightIndex = str.IndexOf(right, leftIndex == -1 ? 0 : leftIndex);

            if (leftIndex == -1 || rightIndex == -1 || leftIndex > rightIndex)
            {
                //throw new Exception("String doesnt contain left or right borders!");
                return "";
            }

            try
            {
                string re = str.Remove(0, leftIndex + left.Length);
                re = re.Remove(rightIndex - leftIndex - left.Length);
                return re;
            }
            catch
            {
                return "";
            }
        }
    }
}
