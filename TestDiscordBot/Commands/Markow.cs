using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TDict = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, uint>>;

namespace TestDiscordBot.Commands
{
    public class Markow : Command
    {
        TDict t = null; // dataset

        public Markow() : base("markow", "Uses sick markow chain autocompletion to create text.", false)
        {

        }

        public override async void onConnected()
        {
            DateTime start = DateTime.Now;

            string input = "";
            foreach (SocketGuild guild in Global.P.getGuilds())
                if (guild.Id != 473991188974927882)
                    foreach (SocketChannel channel in guild.Channels)
                        if (channel.GetType().GetInterfaces().Contains(typeof(ISocketMessageChannel)))
                        {
                            IEnumerable<IMessage> messages = await ((ISocketMessageChannel)channel).GetMessagesAsync().Flatten();
                            foreach (IMessage m in messages)
                                input += m.Content + " \n";
                            input = Regex.Replace(input, @"\s+", " ").TrimEnd(' ');
                        }
            t = MarkovHelper.BuildTDict(input, 2);

            Global.ConsoleWriteLine("Loaded markow in " + (DateTime.Now - start).TotalSeconds + "s", ConsoleColor.Cyan);
        }

        public override async Task execute(SocketMessage message)
        {
            if (t == null)
            {
                await Global.SendText("This component has't been loaded yet!", message.Channel);
                return;
            }

            string output = "";
            string[] split = message.Content.Split(' ');
            if (split.Length == 1)
                output = MarkovHelper.BuildString(t, 25, false);
            else
                output = MarkovHelper.BuildString(t, 25, false, split.Skip(1).Aggregate((x, y) => x + " " + y));
            await Global.SendText(output, message.Channel);
        }
    }
}
