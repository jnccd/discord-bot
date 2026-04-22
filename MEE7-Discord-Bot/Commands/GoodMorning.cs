using Discord;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using MEE7.Configuration;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MEE7.Commands
{
    class GoodMorning : Command
    {
#if DEBUG
        private const int updateIntervalMin = 1;
        private const ulong goodMorningChannelId = 500759857205346304;
#else
        private const int updateIntervalMin = 60;
        private const ulong goodMorningChannelId = 479951853766049803;
#endif

        public GoodMorning() : base("goodMorning", "Wishes you a good morning", false, true)
        {
            Program.OnConnected += () => Task.Factory.StartNew(RunNotificationLoop);
        }

        public override void Execute(IMessage message)
        {
            DiscordNETWrapper.SendText("Good Morning!", message.Channel.Id).Wait();
        }

        void RunNotificationLoop()
        {
            Thread.CurrentThread.Name = "Good Morning Notification Loop";

            while (true)
            {
                try
                {
                    string postId = GetPostId();
                    if (postId != Config.Data.lastGoodMorningPostId)
                    {
                        Config.Data.lastGoodMorningPostId = postId;
                        Config.Save();
                        DiscordNETWrapper.SendText($"https://www.kkinstagram.com/p/{postId}/", goodMorningChannelId).Wait();
                    }
                }
                catch (Exception ex)
                {
                    ConsoleWrapper.WriteLineAndDiscordLog($"GoodMorning Error occurred: {ex.Message}");
                }

                Thread.Sleep(updateIntervalMin * 60000);
            }
        }

        public string GetPostId()
        {
            new BrowserFetcher().DownloadAsync().Wait();
            using var browser = Puppeteer.LaunchAsync(new LaunchOptions { Headless = true }).Result;
            using var page = browser.NewPageAsync().Result;

            page.GoToAsync("https://www.instagram.com/fire.scoop/").Wait();
            page.WaitForSelectorAsync("a div div").Wait();

            string html = page.GetContentAsync().Result;

            string postId = html.GetEverythingBetween("href=\"/fire.scoop/reel/", "/\"");

            return postId;
        }
    }
}
