using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TestDiscordBot.XML;

namespace TestDiscordBot
{
    public static class Global
    {
        static SocketUser MasterP;
        static Random RDMP = new Random();
        static Program PP = new Program();
        static string CurrentExecutablePathP = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public const string prefix = "$";

        public static string CurrentExecutablePath
        {
            get
            {
                return CurrentExecutablePathP;
            }
        }
        public static Program P
        {
            get
            {
                return PP;
            }
        }
        public static Random RDM
        {
            get
            {
                return RDMP;
            }
        }
        public static SocketUser Master
        {
            get
            {
                return MasterP;
            }
            set
            {
                if (MasterP == null)
                    MasterP = value;
                else
                    throw new FieldAccessException("The Master may only be set once!");
            }
        }

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

        public static int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
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
