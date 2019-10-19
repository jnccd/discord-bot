using Discord;
using Discord.Audio;
using Discord.WebSocket;
using MEE7.Backend.HelperFunctions;
using MEE7.Backend.HelperFunctions.Extensions;
using MEE7.Configuration;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MEE7.Backend
{
    public class Tests
    {
        private delegate void TestHandler();
        private static event TestHandler OnTest;

        public static void Run(int index)
        {
            Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.Name = "TestThread";
                index = index == -1 ? CurrentlyActiveTestIndex : index;
                ConsoleWrapper.WriteLine($"Running test {index}");
                try { TestFunctions[index].Invoke(); }
                catch (Exception e) { ConsoleWrapper.WriteLine(e.ToString(), ConsoleColor.Red); }
                ConsoleWrapper.Write("$");
            });
        }

        private static readonly int CurrentlyActiveTestIndex = 0;
        private static readonly Action[] TestFunctions = new Action[] {
            // 0 - Hello World
            () => {
                ConsoleWrapper.WriteLine("Hello world!");
            },
            // 1 - Video Playing
            () => {
                string videoPath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + MultiMediaHelper.DownloadVideoFromYouTube("https://www.youtube.com/watch?v=Y15Pkxk99h0");
                ISocketAudioChannel channel = Program.GetChannelFromID(479951814217826305) as ISocketAudioChannel;
                IAudioClient client = channel.ConnectAsync().Result;
                MultiMediaHelper.SendAudioAsync(client, videoPath).Wait();
                channel.DisconnectAsync().Wait();
            },
            // 2 - Uni Module Crawler
            () => {
                string url = "https://mdb.ps.informatik.uni-kiel.de/show.cgi?Category/show/Category91";
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.KeepAlive = false;
                req.AllowAutoRedirect = true;
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0";
                WebResponse W = req.GetResponse();
                using (StreamReader sr = new StreamReader(W.GetResponseStream()))
                {
                    string html = sr.ReadToEnd();
                    foreach (string s in html.GetEverythingBetweenAll("class=\"btn btn-link\"><span class=\"type_string\">", ":"))
                        Program.GetGuildFromID(479950092938248193).CreateTextChannelAsync(s, (TextChannelProperties t) => { t.CategoryId = 562233500963438603; }).Wait();
                }
            },
            // 3 - Events
            () => {
                OnTest += () => {
                    ConsoleWrapper.WriteLine("lul1");
                };
                OnTest += () => {
                    throw new Exception();
                };
                OnTest += () => {
                    ConsoleWrapper.WriteLine("lul2");
                };
                OnTest += () => {
                    ConsoleWrapper.WriteLine("lul3");
                };
                OnTest += () => {
                    throw new Exception();
                };
                OnTest.InvokeParallel();
            },
            // 4 - Floating Numbers
            () => {
                ConsoleWrapper.WriteLine(1 / 6f);
            },
            // 5 - Give everyone Likes Spam
            () => {
                var uniServer = Program.GetGuildFromID(479950092938248193);
                foreach (var user in uniServer.Users)
                    try {user.AddRoleAsync(uniServer.GetRole(552459506895028225)).Wait();} catch{}
            },
            // 6 - OwO
            () => {
                
            },
        };
    }
}
