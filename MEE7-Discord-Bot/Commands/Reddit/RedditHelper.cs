using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MEE7.Commands
{
    public static class RedditHelper
    {
        public static string GetPostJsonFromSubreddit(string subUrl)
        {
            string postJson;

            // Getting a post
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(subUrl + ".json?limit=25");
            req.KeepAlive = false;
            WebResponse W = req.GetResponse();
            using (StreamReader sr = new StreamReader(W.GetResponseStream()))
            {
                string html = sr.ReadToEnd();

                List<int> Posts = html.AllIndexesOf("{\"kind\": \"t3\",");
                string cuthtml = "";
                if (Posts.Count >= 100)
                    cuthtml = html.Remove(0, Posts[Posts.Count - Program.RDM.Next(99) - 1]);
                else
                    cuthtml = html.Remove(0, Posts[Program.RDM.Next(Posts.Count)]);
                List<int> index = cuthtml.AllIndexesOf("{\"kind\": \"t3\",");
                if (index.Count > 1)
                    postJson = cuthtml.Remove(index[1]);
                else
                    postJson = cuthtml;
            }

            // Convert those weird ass binary chars to normal chars
            while (postJson.Contains("\\u"))
            {
                int index = postJson.IndexOf("\\u");
                string bytes = postJson.Remove(0, index + "\\u".Length);
                bytes = bytes.Remove(4);
                string hexChar = BitConverter.ToChar(new byte[] { Convert.ToByte(bytes[0]), Convert.ToByte(bytes[1]), Convert.ToByte(bytes[2]), Convert.ToByte(bytes[3]) }, 0).ToString();
                postJson = postJson.Remove(index, 6);
                postJson = postJson.Insert(index, hexChar);
            }

            return postJson;
        }

        public static async Task SendPostJsonToDiscordChannel(string postJson, string subUrl, ISocketMessageChannel Channel, SocketUser Author)
        {
            // Resutls
            string ResultURL = "", ResultPicURL = "", ResultTitle = "", ResultTimestamp = "0", ResultPoints = "";
            bool IsVideo;

            // Looking into the postjson
            ResultURL = "https://www.reddit.com" + postJson.GetEverythingBetween("\"permalink\": \"", "\", ");
            ResultTitle = WebUtility.HtmlDecode(postJson.GetEverythingBetween("\"title\": \"", "\", "));
            ResultTimestamp = postJson.GetEverythingBetween("\"created\": ", ", ");
            ResultPoints = postJson.GetEverythingBetween("\"score\": ", ", ");
            string temp = postJson.GetEverythingBetween(", \"is_video\": ", "}");
            try { IsVideo = Convert.ToBoolean(temp); }
            catch { IsVideo = false; }

            // Getting full res image url from the post site html
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(ResultURL);
            req.KeepAlive = false;
            WebResponse W = req.GetResponse();
            using (StreamReader sr = new StreamReader(W.GetResponseStream()))
                ResultPicURL = sr.ReadToEnd().GetEverythingBetween("\"content\":\"", "\",\"");
            if (ResultPicURL == "" || !IsReachable(ResultPicURL))
                ResultPicURL = postJson.GetEverythingBetween("\"images\": [{\"source\": {\"url\": \"", "\", ");
            if (ResultPicURL == "" || !IsReachable(ResultPicURL))
                ResultPicURL = postJson.GetEverythingBetween("\"variants\": {\"gif\": {\"source\": {\"url\": \"", "\"");
            if (ResultPicURL == "" || !IsReachable(ResultPicURL))
                ResultPicURL = postJson.GetEverythingBetween("\"thumbnail\": \"", "\", ");
            if (ResultPicURL == "" || !IsReachable(ResultPicURL))
                ResultPicURL = postJson.GetEverythingBetween("\"url\": \"", "\",");
            if (ResultPicURL == "" || !IsReachable(ResultPicURL))
                throw new Exception("Faulty URL: " + ResultURL, new Exception(postJson));

            if (IsVideo)
            {
                await DiscordNETWrapper.SendText("Sending video post. Please wait...", Channel);

                // downlaod video
                string videofile = "Downloads\\RedditVideo.mp4";
                Directory.CreateDirectory(Path.GetDirectoryName(videofile));
                if (File.Exists(videofile))
                    File.Delete(videofile);
                WebClient client = new WebClient();
                client.DownloadFile(postJson.GetEverythingBetween("\"media\": {\"reddit_video\": {\"fallback_url\": \"", "\","), videofile);

                if (new FileInfo(videofile).Length > 8 * 1024 * 1024)
                {
                    await DiscordNETWrapper.SendText("That video is too big for discords puny 8MB limit!", Channel);
                }
                else
                {
                    // send post
                    await DiscordNETWrapper.SendFile(videofile, Channel, ResultTitle);
                    await DiscordNETWrapper.SendText(ResultPoints + (ResultPoints == "1" ? " fake internet point" : " fake internet points on " + subUrl.Remove(0, "https://www.reddit.com".Length)), Channel);
                }
                
                // delete "Sending video post. Please wait..." message
                IEnumerable<IMessage> messages = await Channel.GetMessagesAsync().FlattenAsync();
                foreach (IMessage m in messages)
                    if (m.Author.Id == Program.GetSelf().Id && m.Content == "Sending video post. Please wait...")
                    {
                        await m.DeleteAsync();
                        break;
                    }
            }
            else // Is Pic or Gif
            {
                EmbedBuilder Embed = new EmbedBuilder();

                if (Uri.IsWellFormedUriString(ResultURL, UriKind.RelativeOrAbsolute))
                    Embed.WithUrl(ResultURL);
                else
                    throw new Exception("Faulty URL: " + ResultURL, new Exception(postJson));
                Embed.WithTitle(ResultTitle);
                Embed.WithImageUrl(ResultPicURL);
                Embed.WithColor(0, 128, 255);
                if (ResultTimestamp != "0" && ResultTimestamp != "")
                {
                    DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dtDateTime = dtDateTime.AddSeconds(Convert.ToDouble(ResultTimestamp.Replace('.', ','))).AddHours(-10);
                    Embed.WithTimestamp(new DateTimeOffset(dtDateTime));
                }
                Embed.WithFooter(ResultPoints + (ResultPoints == "1" ? " fake internet point on " : " fake internet points on ") + subUrl.Remove(0, "https://www.reddit.com".Length));

                await DiscordNETWrapper.SendEmbed(Embed, Channel);
            }
        }

        public static bool IsReachable(string url)
        {
            // from https://stackoverflow.com/questions/5378429/check-if-a-url-is-reachable-help-in-optimizing-a-class
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 15000;
            request.Method = "HEAD"; // As per Lasse's comment
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (WebException)
            {
                return false;
            }
        }
    }
}
