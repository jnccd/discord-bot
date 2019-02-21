using Discord;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TestDiscordBot.Config;

namespace TestDiscordBot
{
    public static class Global
    {
        public static SocketUser Master {
            get { return Master; }
            set
            {
                if (Master == null)
                    Master = value;
                else
                    throw new FieldAccessException("The Master may only be set once!");
            }
        }
        public static Random RDM { get; private set; } = new Random();
        public static Program P { get; private set; } = new Program();
        public static string CurrentExecutablePath { get; private set; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public const string prefix = "$";
        
        public static ulong OwnID
        {
            get
            {
                return P.GetSelf().Id;
            }
        } 
        
        public static async Task SendFile(string path, ISocketMessageChannel Channel, string text = "")
        {
            await Channel.SendFileAsync(path, text);
            SaveChannel(Channel);
        }
        public static async Task SendFile(Stream stream, ISocketMessageChannel Channel, string fileEnd, string fileName = "", string text = "")
        {
            if (fileName == "")
                fileName = DateTime.Now.ToBinary().ToString();

            stream.Position = 0;
            await Channel.SendFileAsync(stream, fileName + "." + fileEnd, text);
            SaveChannel(Channel);
        }
        public static async Task SendBitmap(Bitmap bmp, ISocketMessageChannel Channel, string text = "")
        {
            MemoryStream stream = new MemoryStream();
            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            await SendFile(stream, Channel, "png", "", text);
        }
        public static async Task SendText(string text, ISocketMessageChannel Channel)
        {
            if (text.Length < 2000)
                await Channel.SendMessageAsync(text);
            else
            {
                while (text.Length > 0)
                {
                    int subLength = Math.Min(1999, text.Length);
                    string sub = text.Substring(0, subLength);
                    await Channel.SendMessageAsync(sub);
                    text = text.Remove(0, subLength);
                }
            }
            SaveChannel(Channel);
        }
        public static async Task SendText(string text, ulong ChannelID)
        {
            await SendText(text, (ISocketMessageChannel)P.GetChannelFromID(ChannelID));
        }
        public static async Task SendEmbed(EmbedBuilder Embed, ISocketMessageChannel Channel)
        {
            if (Embed.Fields.Count < 25)
                await Channel.SendMessageAsync("", false, Embed.Build());
            else
            {
                while (Embed.Fields.Count > 0)
                {
                    EmbedBuilder eb = new EmbedBuilder();
                    eb.Color = Embed.Color;
                    eb.Description = Embed.Description;
                    eb.Author = Embed.Author;
                    eb.Footer = Embed.Footer;
                    eb.ImageUrl = Embed.ImageUrl;
                    eb.ThumbnailUrl = Embed.ThumbnailUrl;
                    eb.Timestamp = Embed.Timestamp;
                    eb.Title = Embed.Title;
                    eb.Url = Embed.Title;
                    eb.Url = Embed.Url;
                    for (int i = 0; i < 25 && Embed.Fields.Count > 0; i++)
                    {
                        eb.Fields.Add(Embed.Fields[0]);
                        Embed.Fields.RemoveAt(0);
                    }
                    await Channel.SendMessageAsync("", false, eb.Build());
                }
            }
            SaveChannel(Channel);
        }
        
        public static void SaveChannel(ISocketMessageChannel Channel)
        {
            if (config.Data.ChannelsWrittenOn == null)
                config.Data.ChannelsWrittenOn = new List<ulong>();
            if (!config.Data.ChannelsWrittenOn.Contains(Channel.Id))
            {
                config.Data.ChannelsWrittenOn.Add(Channel.Id);
                config.Save();
            }
        }
        public static void SaveUser(ulong UserID)
        {
            if (!config.Data.UserList.Exists(x => x.UserID == UserID))
                config.Data.UserList.Add(new DiscordUser(UserID));
        }

        public static Bitmap GetBitmapFromURL(string url)
        {
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            return new Bitmap(responseStream);
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

        public static void ConsoleWriteLine(string text, ConsoleColor Color)
        {
            lock (Console.Title)
            {
                Console.CursorLeft = 0;
                Console.ForegroundColor = Color;
                Console.WriteLine(text);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("$");
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
        public static bool ContainsOneOf(this string str, string[] tests)
        {
            foreach (string s in tests)
                if (str.Contains(s))
                    return true;
            return false;
        }
        public static bool ContainsAllOf(this string str, string[] tests)
        {
            foreach (string s in tests)
                if (!str.Contains(s))
                    return false;
            return true;
        }
        public static string GetEverythingBetween(this string str, string left, string right)
        {
            int leftIndex = str.IndexOf(left);
            int rightIndex = str.IndexOf(right, leftIndex == -1 ? 0 : leftIndex + 1);

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
        public static ulong GetServerID(this SocketMessage m)
        {
            return P.GetGuildFromChannel(m.Channel).Id;
        }
        public static string ContainsPictureLink(this string str)
        {
            string[] split = str.Split(' ');
            foreach (string s in split)
                if (s.StartsWith("https://cdn.discordapp.com/") && s.Contains(".png") ||
                    s.StartsWith("https://cdn.discordapp.com/") && s.Contains(".jpg"))
                    return s;
            return null;
        }
        private static float ComputeGaussian(this float n, float theta)
        {
            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }
        public static double ConvertToDouble(this string s)
        {
            return Convert.ToDouble(s.Replace('.', ','));
        }
        public static string ToCapital(this string s)
        {
            string o = "";
            for (int i = 0; i < s.Length; i++)
                if (i == 0)
                    o += char.ToUpper(s[i]);
                else
                    o += char.ToLower(s[i]);
            return o;
        }
        public static EmbedBuilder ToEmbed(this IMessage m)
        {
            EmbedBuilder Embed = new EmbedBuilder();
            Embed.WithColor(0, 128, 255);
            Embed.WithAuthor(m.Author);
            Embed.WithTitle(string.IsNullOrWhiteSpace(m.Content) ?
                m.Attachments.Select(x => x.Url).
                Where(x => !x.EndsWith(".png") && !x.EndsWith(".jpg")).
                Union(new string[] { "-" }).
                Aggregate((x, y) => y == "-" ? x : x + " " + y) : m.Content);
            try
            {
                if (m.Attachments.Count > 0)
                    Embed.WithThumbnailUrl(m.Attachments.ElementAt(0).Url);
            }
            catch { }
            return Embed;
        }
    }

    // Approximate Math Functions
    public class Approximate
    {
        public static float Sqrt(float z)
        {
            if (z == 0) return 0;
            FloatIntUnion u;
            u.tmp = 0;
            u.f = z;
            u.tmp -= 1 << 23; /* Subtract 2^m. */
            u.tmp >>= 1; /* Divide by 2. */
            u.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */
            return u.f;
        }
        public static double Exp(double val)
        {
            long tmp = (long)(1512775 * val + 1072632447);
            return BitConverter.Int64BitsToDouble(tmp << 32);
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct FloatIntUnion
        {
            [FieldOffset(0)]
            public float f;

            [FieldOffset(0)]
            public int tmp;
        }
    }
}
