using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot
{
    public static class Reddit
    {
        public static string GetPostJsonFromSubreddit(string subUrl)
        {
            string postJson;

            // Getting a post
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(subUrl + ".json?limit=100");
            req.KeepAlive = false;
            WebResponse W = req.GetResponse();
            using (StreamReader sr = new StreamReader(W.GetResponseStream()))
            {
                string html = sr.ReadToEnd();

                List<int> Posts = html.AllIndexesOf("{\"kind\": \"t3\",");
                string cuthtml = "";
                if (Posts.Count >= 100)
                    cuthtml = html.Remove(0, Posts[Posts.Count - Global.RDM.Next(99) - 1]);
                else
                    cuthtml = html.Remove(0, Posts[Global.RDM.Next(Posts.Count)]);
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
                postJson.Remove(index, 6);
                postJson.Insert(index, hexChar);
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
            ResultPicURL = postJson.GetEverythingBetween("\"images\": [{\"source\": {\"url\": \"", "\", ");
            if (ResultPicURL == "")
                ResultPicURL = postJson.GetEverythingBetween("\"variants\": {\"gif\": {\"source\": {\"url\": \"", "\"");
            if (ResultPicURL == "")
                ResultPicURL = postJson.GetEverythingBetween("\"url\": \"", "\",");
            if (ResultPicURL == "")
                throw new Exception("Faulty URL: " + ResultURL, new Exception(postJson));
            string temp = postJson.GetEverythingBetween(", \"is_video\": ", "}");
            try { IsVideo = Convert.ToBoolean(temp); }
            catch { IsVideo = false; }

            if (IsVideo)
            {
                await Global.SendText("Sending video post. Please wait...", Channel);

                // downlaod video
                string videofile = Global.CurrentExecutablePath + "\\Downloads\\Video.mp4";
                Directory.CreateDirectory(Path.GetDirectoryName(videofile));
                if (File.Exists(videofile))
                    File.Delete(videofile);
                WebClient client = new WebClient();
                client.DownloadFile(postJson.GetEverythingBetween("\"media\": {\"reddit_video\": {\"fallback_url\": \"", "\","), videofile);

                // send post
                await Global.SendFile(videofile, ResultTitle, Channel);
                await Global.SendText(ResultPoints + (ResultPoints == "1" ? " fake internet point" : " fake internet points on " + subUrl.Remove(0, "https://www.reddit.com".Length)), Channel);

                // delete "Sending video post. Please wait..." message
                IEnumerable<IMessage> messages = await Channel.GetMessagesAsync().Flatten();
                foreach (IMessage m in messages)
                    if (m.Author.Id == Global.P.client.CurrentUser.Id && m.Content == "Sending video post. Please wait...")
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

                await Global.SendEmbed(Embed, Channel);
            }
        }
    }
}
