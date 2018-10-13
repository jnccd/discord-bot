using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class LeetTimeEvent : Command
    {
        public LeetTimeEvent() : base("toggleLeetEvents", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            try
            {
                if (commandmessage.Author.Id == 300699566041202699)
                {
                    if (config.Default.LeetEventChannels.Contains(commandmessage.Channel.Id))
                    {
                        List<ulong> channelList = new List<ulong>();
                        channelList.Remove(commandmessage.Channel.Id);
                        config.Default.LeetEventChannels = channelList.ToArray();
                    }
                    else
                    {
                        List<ulong> channelList = new List<ulong>();
                        channelList.Add(commandmessage.Channel.Id);
                        config.Default.LeetEventChannels = channelList.ToArray();
                    }
                    config.Default.Save();

                    await Global.SendText("Set new Leet Server!", commandmessage.Channel);
                }
                else
                {
                    await Global.SendText("You are not authorized!", commandmessage.Channel);
                }

                Console.CursorLeft = 0;
                Console.WriteLine("Literally nothing in " + commandmessage.Channel.Name + " for " + commandmessage.Author.Username);
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
            for (int i = 0; i < config.Default.LeetEventChannels.Length; i++)
                Global.SendText("LEET TIME!", config.Default.LeetEventChannels[i]);
        }
    }
}
