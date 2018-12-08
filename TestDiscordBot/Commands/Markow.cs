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
using TestDiscordBot.Config;

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

            MarkovHelper.LoadDict();

            // Load from text Files
            if (!MarkovHelper.SaveFileExists())
                config.Data.loadedMarkovTextFiles.Clear();
            string[] files = Directory.GetFiles(Global.CurrentExecutablePath + "\\Resources\\MarkowSources\\");
            foreach (string file in files)
            {
                if (!config.Data.loadedMarkovTextFiles.Contains(Path.GetFileName(file)))
                {
                    string[] lines = File.ReadAllLines(file);
                    foreach (string line in lines)
                    {
                        string trimmed = line.Trim('\n').Trim('\t').Trim(' ');
                        if (trimmed != "")
                            input += trimmed + "\n";
                    }
                    config.Data.loadedMarkovTextFiles.Add(Path.GetFileName(file));
                }
            }

            // Load from Discord
            foreach (SocketGuild guild in Global.P.getGuilds())
                if (guild.Id != 473991188974927882)
                    foreach (SocketChannel channel in guild.Channels)
                        if (channel.GetType().GetInterfaces().Contains(typeof(ISocketMessageChannel)))
                        {
                            IEnumerable<IMessage> messages = await ((ISocketMessageChannel)channel).GetMessagesAsync().Flatten();
                            foreach (IMessage m in messages)
                                if (!m.Author.IsBot && m.Content.Trim('\n').Trim('\t').Trim(' ') != "")
                                    input += m.Content.Trim(' ') + " ";
                        }

            MarkovHelper.AddToDict(input);

            Global.ConsoleWriteLine("Loaded markow in " + (DateTime.Now - start).TotalSeconds + "s", ConsoleColor.Cyan);
        }
        public override void onNonCommandMessageRecieved(SocketMessage message)
        {
            MarkovHelper.AddToDict(message.Content);
        }
        public override void onExit()
        {
            MarkovHelper.SaveDict();
        }

        public override async Task execute(SocketMessage message)
        {
            string[] split = message.Content.Split(' ');

            try
            {
                string output = MarkovHelper.GetString(split.Length > 1 ? split.Skip(1).Aggregate((x, y) => { return x + " " + y; }) : "", 2, 2000);
                await Global.SendText(output, message.Channel);
            }
            catch (NoEmptyElementException e)
            {
                await Global.SendText("Markow isn't ready yet!", message.Channel);
            }
        }
    }
}
