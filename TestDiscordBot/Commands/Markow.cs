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
        bool loadedDict;

        public Markow() : base("markow", "Generates text", false)
        {

        }

        public override async void OnConnected()
        {
            DateTime start = DateTime.Now;
            string input = "";

            MarkovHelper.LoadDict();

            // Check for reset
            if (!MarkovHelper.SaveFileExists())
            {
                config.Data.LoadedMarkovTextFiles.Clear();

                // Load from Discord
                foreach (SocketGuild guild in Global.P.GetGuilds())
                    if (guild.Id != 473991188974927882)
                        foreach (SocketChannel channel in guild.Channels)
                            if (channel.GetType().GetInterfaces().Contains(typeof(ISocketMessageChannel)))
                            {
                                IEnumerable<IMessage> messages = await ((ISocketMessageChannel)channel).GetMessagesAsync().Flatten();
                                foreach (IMessage m in messages)
                                    if (!m.Author.IsBot && !string.IsNullOrWhiteSpace(m.Content) && !m.Content.StartsWith(Global.prefix) && m.Content[0] != '!')
                                        input += m.Content + "\n";
                            }
            }

            // Load from text Files
            string[] files = Directory.GetFiles(Global.CurrentExecutablePath + "\\Resources\\MarkowSources\\");
            foreach (string file in files)
            {
                if (!config.Data.LoadedMarkovTextFiles.Contains(Path.GetFileName(file)))
                {
                    string[] lines = File.ReadAllLines(file);
                    foreach (string line in lines)
                    {
                        string trimmed = line.Trim('\n').Trim('\t').Trim(' ');
                        if (!string.IsNullOrWhiteSpace(trimmed))
                            input += trimmed + "\n";
                    }
                    config.Data.LoadedMarkovTextFiles.Add(Path.GetFileName(file));
                }
            }
            
            MarkovHelper.AddToDict(input);

            loadedDict = true;
            Global.ConsoleWriteLine("Loaded markow in " + (DateTime.Now - start).TotalSeconds + "s", ConsoleColor.Cyan);
        }
        public override async void OnNonCommandMessageRecieved(SocketMessage message)
        {
            IEnumerable<IMessage> messages = await message.Channel.GetMessagesAsync(2).Flatten();
            if (messages.Count() == 2)
                MarkovHelper.AddToDict(messages.ElementAt(1).Content, message.Content);
            else
                MarkovHelper.AddToDict(message.Content);
        }
        public override void OnExit()
        {
            if (loadedDict)
                MarkovHelper.SaveDict();
        }

        public override async Task Execute(SocketMessage message)
        {
            string[] split = message.Content.Split(new char[] { ' ', '\n' });

            try
            {
                string output = MarkovHelper.GetString(split.Length > 1 ? split.Skip(1).Aggregate((x, y) => { return x + " " + y; }) : "", 5, 2000);
                await Global.SendText(output, message.Channel);
            }
            catch (NoEmptyElementException)
            {
                await Global.SendText("Markow isn't ready yet!", message.Channel);
            }
        }
    }
}
