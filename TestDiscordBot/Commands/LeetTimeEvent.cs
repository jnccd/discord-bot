using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class LeetTimeEvent : Command
    {
        Timer leetTimer;

        public LeetTimeEvent() : base("toggleLeetEvents", "Dooms this channel for all eternity (can only be used by server owners)", false)
        {
            TimerCallback callback = new TimerCallback(OnLeetTime);
            DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 37, 0);
            if (DateTime.Now > dt)
                dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1, 13, 37, 0);
            leetTimer = new Timer(callback, null, dt - DateTime.Now, TimeSpan.FromHours(24));
        }

        public override async Task execute(SocketMessage commandmessage)
        {
            try
            {
                if (commandmessage.Author.Id == 300699566041202699 || ((SocketGuildUser)commandmessage.Author) == ((SocketGuildUser)commandmessage.Author).Guild.Owner)
                {
                    if (config.Data.LeetEventChannels.Contains(commandmessage.Channel.Id))
                    {
                        config.Data.LeetEventChannels.Remove(commandmessage.Channel.Id);
                        await Global.SendText("This Channel isn't Leet anymore!", commandmessage.Channel);
                    }
                    else
                    {
                        config.Data.LeetEventChannels.Add(commandmessage.Channel.Id);
                        await Global.SendText("Set as Leet Channel!", commandmessage.Channel);
                    }
                    config.Save();
                }
                else
                {
                    await Global.SendText("You are not authorized!", commandmessage.Channel);
                }

                Console.CursorLeft = 0;
                Console.WriteLine("Leet switch in " + commandmessage.Channel.Name + " for " + commandmessage.Author.Username);
                Console.Write("$");
            }
            catch (Exception e)
            {
                await Global.SendText("Uwu We made a fucky wucky!! A wittle fucko boingo! The code monkeys at our headquarters are working VEWY HAWD to fix this!", commandmessage.Channel);

                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("$");
            }
        }

        public void OnLeetTime(object obj)
        {
            for (int i = 0; i < config.Data.LeetEventChannels.Count; i++)
                Global.SendText("LEET TIME!", config.Data.LeetEventChannels[i]);
        }
    }
}
