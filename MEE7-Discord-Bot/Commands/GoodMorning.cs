using Discord;
using HtmlAgilityPack;
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
                if (!Config.Data.saidGoodMorningAlready && DateTime.Now.Hour >= 9)
                {
                    Config.Data.saidGoodMorningAlready = true;
                    Config.Save();
                    FeelsAsynchronousMan().Wait();
                }
                if (Config.Data.lastGoodMorningDate < DateOnly.FromDateTime(DateTime.Now))
                {
                    Config.Data.saidGoodMorningAlready = false;
                    Config.Save();
                }

                Thread.Sleep(updateIntervalMin * 60000);
            }
        }

        public async Task FeelsAsynchronousMan()
        {
            new BrowserFetcher().DownloadAsync().Wait();
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            using var page = await browser.NewPageAsync();

            await page.GoToAsync("https://www.instagram.com/fire.scoop/");
            await page.WaitForSelectorAsync("a div div");

            string html = await page.GetContentAsync();

            string postId = html.GetEverythingBetween("href=\"/fire.scoop/reel/", "/\"");

            DiscordNETWrapper.SendText($"https://www.kkinstagram.com/p/{postId}/", goodMorningChannelId).Wait();
        }
    }
}
