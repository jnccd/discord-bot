using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using MEE7.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MEE7.Commands
{
    public class Timer : Command
    {
        public Timer() : base("timer", "Posts a continually updated message that shows the time until some event", isExperimental: false, isHidden: false)
        {
            Task.Factory.StartNew(() =>
            {
                string getTimer(TimeSpan t) => $"{t.Days} Days, {t.Hours} Hours, {t.Minutes} Minutes, {t.Seconds} Seconds";

                while (true)
                {
                    for (int i = 0; i < Config.Data.timers.Count; i++)
                    {
                        var timer = Config.Data.timers[i];

                        try
                        {
                            var eventName = timer.Item1;
                            var eventMessage = (IUserMessage)(Program.GetChannelFromID(timer.Item2) as IMessageChannel).GetMessageAsync(timer.Item3).Result;
                            var eventTime = timer.Item4;

                            if (DateTime.Now < eventTime)
                                eventMessage.ModifyAsync(m => m.Content = $"```fix\n{eventName} in {getTimer(eventTime - DateTime.Now)}```");
                            else
                            {
                                eventMessage.ModifyAsync(m => m.Content = $"```fix\n{eventName} happened at {eventTime}```");
                                Config.Data.timers.RemoveAt(i);
                                i--;
                            }

                            Thread.Sleep(1000);
                        }
                        catch { }
                    }

                    Thread.Sleep(200);
                }
            });
        }

        public override void Execute(SocketMessage message)
        {
            string[] split = message.Content.Split(' ');
            if (split.Length != 3)
            {
                DiscordNETWrapper.SendText("You didn't give me proper arguments to work with :angery:", message.Channel).Wait();
                return;
            }
            string eventName = split[2];
            IUserMessage eventMessage = DiscordNETWrapper.SendText($"```fix\n{eventName} in *Please wait*```", message.Channel).Result[0];
            Config.Data.timers.Add(new Tuple<string, ulong, ulong, DateTime>(eventName, message.Channel.Id, eventMessage.Id, DateTime.Parse(split[1])));
        }
    }
}
