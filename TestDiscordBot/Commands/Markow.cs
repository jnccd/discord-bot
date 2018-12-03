using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TDict = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, uint>>;

namespace TestDiscordBot.Commands
{
    public class Markow : Command
    {
        public Markow() : base("markow", "Generates text", false)
        {

        }

        public override async void onConnected()
        {
            DateTime start = DateTime.Now;
            
            string input = "";

            // Load from Discord
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

            MarkovHelper.AddToDict(input);
            input = "";

            // Load from Files
            string[] files = Directory.GetFiles(Global.CurrentExecutablePath + "\\Resources\\MarkowSources\\");
            foreach (string file in files)
            {
                string[] lines = File.ReadAllLines(file);
                foreach (string line in lines)
                {
                    string trimmed = line.Trim(' ').Trim('\t');
                    if (trimmed != "")
                        input += trimmed + "\n";
                }
            }

            MarkovHelper.AddToDict(input);

            Global.ConsoleWriteLine("Loaded markow in " + (DateTime.Now - start).TotalSeconds + "s", ConsoleColor.Cyan);
        }
        public override void onNonCommandMessageRecieved(string message)
        {
            MarkovHelper.AddToDict(message);
        }

        public override async Task execute(SocketMessage message)
        {
            string[] split = message.Content.Split(' ');

            if (split.Length > 1 && split[1] == "\\learn")
            {
                MarkovHelper.AddToDict(split.Skip(2).Aggregate((x, y) => { return x + " " + y; }));
                await Global.SendText("Successfully added text to database!", message.Channel);
            }
            else
            {
                string output = MarkovHelper.GetString(split.Length > 1 ? split.Last() : "", 5, 2000);
                await Global.SendText(output, message.Channel);
            }
        }
    }
}
