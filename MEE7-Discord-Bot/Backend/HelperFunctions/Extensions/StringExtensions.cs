using NAudio.Wave;
using NVorbis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static MEE7.Commands.Edit.Edit;

namespace MEE7.Backend.HelperFunctions
{
    public static class StringExtensions
    {
        private static int RunAsConsoleCommandThreadIndex = 0;

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
            int rightIndex = str.IndexOf(right, leftIndex == -1 ? 0 : leftIndex + left.Length);

            if (right == "")
                rightIndex = str.Length - 1;

            if (left == "")
                leftIndex = 0;

            if (leftIndex == -1 || rightIndex == -1 || leftIndex + left.Length > rightIndex)
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
        public static List<string> GetEverythingBetweenAll(this string str, string left, string right)
        {
            List<string> re = new List<string>();

            int leftIndex = str.IndexOf(left);
            int rightIndex = str.IndexOf(right, leftIndex == -1 ? 0 : leftIndex + 1);

            if (leftIndex == -1 || rightIndex == -1 || leftIndex > rightIndex)
            {
                return re;
            }

            while (leftIndex != -1 && rightIndex != -1)
            {
                try
                {
                    str = str.Remove(0, leftIndex + left.Length);
                    re.Add(str.Remove(rightIndex - leftIndex - left.Length));
                    str = str.Remove(0, rightIndex - leftIndex - right.Length - left.Length);
                }
                catch { break; }

                leftIndex = str.IndexOf(left);
                rightIndex = str.IndexOf(right, leftIndex == -1 ? 0 : leftIndex + 1);
            }

            return re;
        }
        public static bool StartsWith(this string str, string[] values)
        {
            foreach (string s in values)
                if (str.StartsWith(s))
                    return true;
            return false;
        }
        public static string GetDiscordPictureLink(this string str)
        {
            string[] split = str.Split(' ');
            foreach (string s in split)
                if (s.StartsWith("https://cdn.discordapp.com/") && s.Contains(".png") ||
                    s.StartsWith("https://cdn.discordapp.com/") && s.Contains(".jpg"))
                    return s;
            return null;
        }
        public static string GetPictureLinkInMessage(this string str)
        {
            string[] split = str.Split(' ');
            foreach (string s in split)
            {
                Uri.TryCreate(s, UriKind.Absolute, out Uri uriResult);
                if (uriResult != null && uriResult.Scheme == Uri.UriSchemeHttps ||
                    uriResult != null && uriResult.Scheme == Uri.UriSchemeHttp)
                {
                    var req = (HttpWebRequest)HttpWebRequest.Create(s);
                    req.Method = "GET";
                    req.AllowAutoRedirect = true;
                    req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.142 Safari/537.36";
                    using var resp = req.GetResponse();
                    if (resp.ContentType.ToLower(CultureInfo.InvariantCulture).StartsWith("image/"))
                        return s;
                }
            }
            return null;
        }
        public static double ConvertToDouble(this string s)
        {
            if (CultureInfo.CurrentCulture.Name == "de-DE")
                return Convert.ToDouble(s.Replace('.', ','));
            else
                return Convert.ToDouble(s);
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
        public static Bitmap GetBitmapFromURL(this string url) => new Bitmap(WebRequest.Create(url).GetResponse().GetResponseStream());
        public static Bitmap[] GetBitmapsFromGIFURL(this string url)
        {
            Image gif = Image.FromStream(WebRequest.Create(url).GetResponse().GetResponseStream());
            FrameDimension dimension = new FrameDimension(gif.FrameDimensionsList[0]);
            return Enumerable.Range(0, gif.GetFrameCount(dimension)).
                Select(x =>
                {
                    gif.SelectActiveFrame(dimension, x);
                    return new Bitmap(gif);
                }).
                ToArray();
        }
        public static Gif GetBitmapsAndTimingsFromGIFURL(this string url)
        {
            Image gif = Image.FromStream(WebRequest.Create(url).GetResponse().GetResponseStream());
            return MultiMediaHelper.ImageToGif(gif);
        }
        public static Mp3FileReader Getmp3AudioFromURL(this string url)
        {
            Stream ms = new MemoryStream();
            using (Stream stream = WebRequest.Create(url).GetResponse().GetResponseStream())
                stream.CopyTo(ms);

            ms.Position = 0;
            return new Mp3FileReader(ms);
        }
        public static WaveFileReader GetwavAudioFromURL(this string url) => new WaveFileReader(WebRequest.Create(url).GetResponse().GetResponseStream());
        public static VorbisReader GetoggAudioFromURL(this string url) => new VorbisReader(WebRequest.Create(url).GetResponse().GetResponseStream(), true);
        public static int LevenshteinDistance(this string s, string t)
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
        public static float ModifiedLevenshteinDistance(this string smaller, string longer)
        {
            if (string.IsNullOrEmpty(smaller))
            {
                if (string.IsNullOrEmpty(longer))
                    return 0;
                return longer.Length;
            }

            if (string.IsNullOrEmpty(longer))
                return smaller.Length;

            // initialize the top and right of the table to 0, 1, 2, ...
            float[,] d = new float[smaller.Length + 1, longer.Length + 1];
            for (int i = 0; i <= smaller.Length; d[i, 0] = i++) ;
            for (int j = 1; j <= longer.Length; d[0, j] = j++) ;

            for (int i = 1; i <= smaller.Length; i++)
                for (int j = 1; j <= longer.Length; j++)
                {
                    float delete = d[i - 1, j] + 1;
                    float insert = d[i, j - 1] + 0.5f;
                    float replace = d[i - 1, j - 1] + ((longer[j - 1] == smaller[i - 1]) ? 0 : 1);
                    d[i, j] = Math.Min(Math.Min(delete, insert), replace);
                }
            return d[smaller.Length, longer.Length];
        }
        public static string Combine(this IEnumerable<string> s, string combinator = "")
        {
            return s.Count() == 0 ? "" : s.Foldl("", (x, y) => x + combinator + y).Remove(0, combinator.Length);
        }
        public static void RunAsConsoleCommand(this string command, int TimeLimitInSeconds, Action TimeoutEvent, Action<string, string> ExecutedEvent,
            Action<StreamWriter> RunEvent = null, string WorkingDir = "", Process compiler = null)
        {
            bool exited = false;
            string[] split = command.Split(' ');

            if (split.Length == 0)
                return;

            if (compiler == null)
                compiler = new Process();

            using (compiler)
            {
                compiler.StartInfo.FileName = split.First();
                compiler.StartInfo.Arguments = split.Skip(1).Foldl("", (x, y) => x + " " + y).Trim(' ');
                compiler.StartInfo.CreateNoWindow = true;
                compiler.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                compiler.StartInfo.RedirectStandardInput = true;
                compiler.StartInfo.RedirectStandardOutput = true;
                compiler.StartInfo.RedirectStandardError = true;
                if (!string.IsNullOrWhiteSpace(WorkingDir))
                    compiler.StartInfo.WorkingDirectory = WorkingDir;
                compiler.Start();

                Task.Run(() => { RunEvent?.Invoke(compiler.StandardInput); });

                DateTime start = DateTime.Now;

                Task.Run(() =>
                {
                    Thread.CurrentThread.Name = $"RunAsConsoleCommand Thread {RunAsConsoleCommandThreadIndex++}";
                    compiler.WaitForExit();

                    string o = "";
                    string e = "";

                    try { o = compiler.StandardOutput.ReadToEnd(); } catch { }
                    try { e = compiler.StandardError.ReadToEnd(); } catch { }

                    exited = true;
                    ExecutedEvent(o, e);
                });

                while (!exited && (DateTime.Now - start).TotalSeconds < TimeLimitInSeconds)
                    Thread.Sleep(100);
                if (!exited)
                {
                    exited = true;
                    try
                    {
                        compiler.Close();
                    }
                    catch { }
                    TimeoutEvent();
                }
            }
        }
        public static string GetHTMLfromURL(this string URL)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URL);
                req.KeepAlive = false;
                req.Timeout = 3000;
                req.AllowAutoRedirect = true;
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0";
                using (WebResponse w = req.GetResponse())
                using (Stream s = w.GetResponseStream())
                using (StreamReader sr = new StreamReader(s))
                    return sr.ReadToEnd();
            }
            catch (Exception e) { return $"Exception: {e}"; }
        }
        public static WebResponse GetWebResponsefromURL(this string URL)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URL);
                req.KeepAlive = false;
                req.Timeout = 3000;
                req.AllowAutoRedirect = true;
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0";

                return req.GetResponse();
            }
            catch (Exception) { return null; }
        }
        public static MemoryStream GetStreamFromUrl(this string url)
        {
            byte[] imageData = null;
            MemoryStream ms = null;

            try
            {
                using (var wc = new WebClient())
                    imageData = wc.DownloadData(url);
                ms = new MemoryStream(imageData);
            }
            catch { }

            return ms;
        }
        public static string RemoveLastGroup(this string s, char seperator)
        {
            string[] split = s.Split(seperator);
            return split.Take(split.Length - 1).Foldl("", (a, b) => a + seperator + b).Remove(0, 1);
        }
    }
}
