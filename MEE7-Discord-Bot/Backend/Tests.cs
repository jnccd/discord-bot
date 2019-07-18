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
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

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
                ConsoleWrapper.ConsoleWriteLine($"Running test {index}");
                try { TestFunctions[index].Invoke(); }
                catch (Exception e) { ConsoleWrapper.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                ConsoleWrapper.ConsoleWrite("$");
            });
        }

        private static readonly int CurrentlyActiveTestIndex = 0;
        private static readonly Action[] TestFunctions = new Action[] {
            // 0 - Hello World
            () => {
                ConsoleWrapper.ConsoleWriteLine("Hello world!");
            },
            // 1 - Video Playing
            () => {
                string videoPath = Directory.GetCurrentDirectory() + "\\" + MultiMediaHelper.DownloadVideoFromYouTube("https://www.youtube.com/watch?v=Y15Pkxk99h0");
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
            // 3 - Twtich
            () => {
                var client = new TwitchClient();
                client.Initialize(new ConnectionCredentials(Config.Data.TwtichBotUsername, Config.Data.TwtichAccessToken));
                client.OnLog += (object o, OnLogArgs arg) => { ConsoleWrapper.ConsoleWriteLine($"{arg.BotUsername} - {arg.DateTime}: {arg.Data}", ConsoleColor.Magenta); };
                client.OnMessageReceived += (object sender, OnMessageReceivedArgs e) => {
                    ConsoleWrapper.ConsoleWriteLine($"Message: {e.ChatMessage}", ConsoleColor.Magenta);
                    if (e.ChatMessage.Message.StartsWith("hi"))
                        client.SendMessage(e.ChatMessage.Channel, "Hello there");
                };
                client.OnFailureToReceiveJoinConfirmation += (object sender, OnFailureToReceiveJoinConfirmationArgs e) => { ConsoleWrapper.ConsoleWriteLine($"Exception: {e.Exception}\n{e.Exception.Details}", ConsoleColor.Magenta); };
                client.OnJoinedChannel += (object sender, OnJoinedChannelArgs e) => { ConsoleWrapper.ConsoleWriteLine($"{e.BotUsername} - joined {e.Channel}", ConsoleColor.Magenta); };
                client.OnConnectionError += (object sender, OnConnectionErrorArgs e) => { ConsoleWrapper.ConsoleWriteLine($"Error: {e.Error}", ConsoleColor.Magenta); };
                client.Connect();

                client.JoinChannel(Config.Data.TwtichChannelName);

                Task.Factory.StartNew(() => { Thread.Sleep(15000); client.Disconnect(); ConsoleWrapper.ConsoleWriteLine("Disconnected Twitch"); });

                var api = new TwitchAPI();
                api.Settings.ClientId = Config.Data.TwitchAPIClientID;
                api.Settings.AccessToken = Config.Data.TwitchAPIAccessToken;
                var res = api.Helix.Users.GetUsersFollowsAsync("42111676").Result;
            },
            // 4 - Events
            () => {
                OnTest += () => {
                    ConsoleWrapper.ConsoleWriteLine("lul1");
                };
                OnTest += () => {
                    throw new Exception();
                };
                OnTest += () => {
                    ConsoleWrapper.ConsoleWriteLine("lul2");
                };
                OnTest += () => {
                    ConsoleWrapper.ConsoleWriteLine("lul3");
                };
                OnTest += () => {
                    throw new Exception();
                };
                OnTest.InvokeParallel();
            },
            // 5 - Floating Numbers
            () => {
                ConsoleWrapper.ConsoleWriteLine(1 / 6f);
            }
        };
    }
}
