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
        public static SocketUser Master;
        public static Random RDM = new Random();
        public static Program P = new Program();
        public const string prefix = "!";
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
            await SendText(text, (ISocketMessageChannel)P.getChannelFromID(ChannelID));
        }
        public static async Task SendEmbed(EmbedBuilder Embed, ISocketMessageChannel Channel)
        {
            await Channel.SendMessageAsync("", false, Embed.Build());
            SaveChannel(Channel);
        }

        public static void SaveChannel(ISocketMessageChannel Channel)
        {
            if (config.Data.ChannelsWrittenOn == null)
                config.Data.ChannelsWrittenOn = new List<ulong>();
            if (!config.Data.ChannelsWrittenOn.Contains(Channel.Id))
                config.Data.ChannelsWrittenOn.Add(Channel.Id);
            config.Save();
        }
        public static void SaveUser(ulong UserID)
        {
            if (!config.Data.UserList.Exists(x => x.UserID == UserID))
                config.Data.UserList.Add(new DiscordUser(UserID));
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
        public static bool StartsWith(this string str, string[] values)
        {
            foreach (string s in values)
                if (str.StartsWith(s))
                    return true;
            return false;
        }
    }
}
