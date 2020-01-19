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

namespace MEE7.Backend.HelperFunctions.Extensions
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
                    req.Method = "HEAD";
                    using (var resp = req.GetResponse())
                        if (resp.ContentType.ToLower(CultureInfo.InvariantCulture)
                                .StartsWith("image/"))
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
                Select(x => {
                    gif.SelectActiveFrame(dimension, x);
                    return new Bitmap(gif);
                }).
                ToArray();
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

                Task.Run(() => {
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
            string googleImgCookie = "CONSENT=YES+DE.de+20151207-13-0; SLG_GWPT_Show_Hide_tmp=1; SLG_wptGlobTipTmp=1; BLPROV=Google; HSID=A-vqZZg8vQI3Q-EIq; SSID=A6kkf51j7vY_Ztcdt; APISID=CkhOuBICTgvR89NU/AoEgG4Va-p_0wiAQQ; SAPISID=VR6q92ueupcUlkcE/A-PsY1KJoxTZ4N23R; SID=pQd9ALQj3Wyie2H57TNx-5gfOVHws-na2PDdVj87rBbos2pv0ULd8f_GDMinApOqb5zsNA.; __Secure-3PSID=pQd9ALQj3Wyie2H57TNx-5gfOVHws-na2PDdVj87rBbos2pv3kFPx5H75oEpCwy9bPRRLw.; __Secure-3PAPISID=VR6q92ueupcUlkcE/A-PsY1KJoxTZ4N23R; __Secure-HSID=A-vqZZg8vQI3Q-EIq; __Secure-SSID=A6kkf51j7vY_Ztcdt; __Secure-APISID=CkhOuBICTgvR89NU/AoEgG4Va-p_0wiAQQ; SEARCH_SAMESITE=CgQI2o4B; ANID=AHWqTUmZ9PEkbS6012PBtaBthjQGSls0OoRkwQJ_w0DLBMeu2uhleyt-EUa5mbXt; NID=196=aRCKdxbW0za8uy_0QXRmMIiuqwa2j7jY0889LaQll0zUHrqcM2UUs8qNBQzn4ceO60ahnW64xunjI7xLZwcVpEcDnbdd_0pRVyu9StT-CRciM_mJQMlLKMYnZDkM7gwAZ_8QWMIPxdd6WXMnNlixoYE9RNd0pgPwaAKpTVUa-RrY9IRAkVH7AixUTFEQ6CzjAZ9svYMkbU7xUauwaSsRaGt9tfrp382HoCCxXv3FzqMSZDPqjwO0yuMn5MS2aAwkrtDncPk3AIsNjf0m-3edLq0DlMa1zrO9MM4rDR6_exeRuBOIfmtc3hI4rd_XrZaV; DV=c8Wi7yZ7UvM6wOHOSNGMdZA1YIn_-5bs_5BZ6oTAAAAAALC-OcS5CfPkFQAAAAA; 1P_JAR=2020-01-19-23";

            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URL);
                req.KeepAlive = false;
                req.Timeout = 3000;
                req.AllowAutoRedirect = true;
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0";

                CookieContainer c = new CookieContainer();
                googleImgCookie.Split("; ").Select(x => {
                    c.Add(new Cookie(x.Split("=")[0], x.Split("=")[1]));
                    return 1;
                });
                req.CookieContainer = c;
                return req.GetResponse();
            }
            catch (Exception) { return null; }
        }
    }
}
