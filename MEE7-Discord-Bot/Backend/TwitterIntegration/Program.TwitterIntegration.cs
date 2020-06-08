using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TweetSharp;

namespace MEE7
{
    public static partial class Program
    {
        static TwitterService service;

        public static void BootTwitterModule()
        {
            ConsoleWrapper.WriteLineAndDiscordLog($"{DateTime.Now.ToLongTimeString()} BootTwitterModule!");

            try
            {
                service = new TwitterService(
                    Environment.GetEnvironmentVariable("customer_key"),
                    Environment.GetEnvironmentVariable("customer_key_secret"),
                    Environment.GetEnvironmentVariable("access_token"),
                    Environment.GetEnvironmentVariable("access_token_secret"));

                ConsoleWrapper.WriteLineAndDiscordLog($"{DateTime.Now.ToLongTimeString()} Connected to Twitter!");
            }
            catch (Exception e)
            {
                ConsoleWrapper.WriteLineAndDiscordLog($"{DateTime.Now.ToLongTimeString()} Failed to connect to Twitter!\n" + e);
                return;
            }

            try
            {
                StartTwitterLoop();
            }
            catch (Exception e)
            {
                ConsoleWrapper.WriteLineAndDiscordLog($"{DateTime.Now.ToLongTimeString()} Error during twitter loop!\n" + e);
                return;
            }
        }
        static void StartTwitterLoop()
        {
            long lastMentionId = service.ListTweetsMentioningMe(new ListTweetsMentioningMeOptions() { Count = 1 }).Last().Id;
            TweetSharpWrapper.PrintTwitterRateLimitStatus(service);

            while (true)
            {
                var mentions = service.ListTweetsMentioningMe(new ListTweetsMentioningMeOptions() { SinceId = lastMentionId });
                if (mentions != null)
                    foreach (var m in mentions.Reverse())
                    {
                        ConsoleWrapper.WriteLine($"{DateTime.Now.ToLongTimeString()} Recieved Tweet: " + m.Text);

                        try
                        {
                            TwitterChannel c = new TwitterChannel(m, service);
                            TweetSharpWrapper.PrintTwitterRateLimitStatus(service);
                            if (c.InitialMessage.Content.StartsWith(Program.Prefix))
                                Task.Run(() => Program.ParallelMessageReceived(c.InitialMessage));
                        }
                        catch (Exception e)
                        {
                            ConsoleWrapper.WriteLine("Error handling Tweet: " + e);
                        }

                        lastMentionId = m.Id;
                    }

                Thread.Sleep(50000); // 48 seconds times 75 hourly allowed hits would exactly be one hour
            }
        }
    }
}
