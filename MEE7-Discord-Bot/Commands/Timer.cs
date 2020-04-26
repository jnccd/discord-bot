using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using MEE7.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MEE7.Commands
{
    public class ConfigTimer
    {
        public string EventName;
        public ulong ChannelId;
        public ulong MessageId;
        public DateTime EventTime;
        public ulong AuthorId;

        public ConfigTimer(string eventName, ulong channelId, ulong messageId, DateTime eventTime, ulong authorId)
        {
            EventName = eventName;
            ChannelId = channelId;
            MessageId = messageId;
            EventTime = eventTime;
            AuthorId = authorId;
        }

        public IUserMessage GetMessage() => (IUserMessage)(Program.GetChannelFromID(ChannelId) as IMessageChannel).GetMessageAsync(MessageId).Result;
    }

    public class Timer : Command
    {
        Emote PingEmote;
        Emoji CancelEmote;

        public Timer() : base("timer", "Posts a continually updated message that shows the time until some event", isExperimental: false, isHidden: false)
        {
            Program.OnConnected += () => {
                PingEmote = Emote.Parse("<a:ping:703994951377092661>");
                CancelEmote = new Emoji("❌");
            };
            Program.OnEmojiReactionAdded += (Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3) => {
                if (arg3.Emote.Name == CancelEmote.Name)
                {
                    var timer = Config.Data.timers.FirstOrDefault(x => x.MessageId == arg1.Id);
                    if (timer != null && arg3.User.Value.Id == timer.AuthorId)
                    {
                        var eventMessage = timer.GetMessage();

                        eventMessage.ModifyAsync(m => m.Content = $"```fix\n{timer.EventName} was cancelled :c```");
                        Config.Data.timers.Remove(timer);
                    }
                }
            };
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
                            var eventMessage = (IUserMessage)(Program.GetChannelFromID(timer.ChannelId) as IMessageChannel).GetMessageAsync(timer.MessageId).Result;

                            if (DateTime.Now < timer.EventTime)
                                eventMessage.ModifyAsync(m => m.Content = $"```fix\n{timer.EventName} in {getTimer(timer.EventTime - DateTime.Now)}```");
                            else
                            {
                                eventMessage.ModifyAsync(m => m.Content = $"```fix\n{timer.EventName} happened at {timer.EventTime}```");
                                var pingUsers = eventMessage.GetReactionUsersAsync(PingEmote, 100).FlattenAsync().Result;
                                string mentions = pingUsers.Where(x => !x.IsBot).Select(x => x.Mention).Combine(" ");
                                if (pingUsers.Where(x => !x.IsBot).Count() > 0)
                                    DiscordNETWrapper.SendText($"{mentions} {eventMessage.GetJumpUrl()}", eventMessage.Channel).Wait();

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
            if (split.Length != 4)
            {
                DiscordNETWrapper.SendText("You didn't give me proper arguments to work with :angery:", message.Channel).Wait();
                return;
            }

            string inAt = split[1];
            string time = split[2];
            string eventName = split[3];

            IUserMessage eventMessage = DiscordNETWrapper.SendText($"```fix\n{eventName} in *Please wait*```", message.Channel).Result[0];
            DateTime eventTime = DateTime.Now;
            try
            {
                if (inAt == "at")
                    eventTime = DateTime.Parse(time);
                else if (inAt == "in")
                    eventTime = DateTime.Now.Add(TimeSpan.Parse(time));
                else
                    throw new Exception("uwu");
            }
            catch
            {
                eventMessage.ModifyAsync(m => m.Content = $"```fix\nme no understando ur langaugo```");
                return;
            }
            eventMessage.AddReactionsAsync(new IEmote[] { PingEmote, CancelEmote });
            Config.Data.timers.Add(new ConfigTimer(eventName, message.Channel.Id, eventMessage.Id, eventTime, message.Author.Id));
        }
    }
}
