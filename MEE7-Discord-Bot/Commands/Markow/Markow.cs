using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using MEE7.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MEE7.Commands
{
    public class Markow : Command
    {
        bool loadedDict;
        readonly string saucePath = $"Commands{Path.DirectorySeparatorChar}MarkowSources{Path.DirectorySeparatorChar}";

        public Markow() : base("markow", "Generates text", false)
        {
            Program.OnConnected += OnConnected;
            Program.OnNonCommandMessageRecieved += OnNonCommandMessageRecieved;
            Program.OnExit += OnExit;
        }

        public void OnConnected()
        {
            DateTime start = DateTime.Now;
            string input = "";

            MarkovHelper.LoadDict();

            // Check for reset
            if (!MarkovHelper.SaveFileExists())
            {
                Config.Data.LoadedMarkovTextFiles.Clear();

                // Load from Discord
                foreach (SocketGuild guild in Program.GetGuilds())
                    if (guild.Id != 473991188974927882)
                        foreach (SocketChannel channel in guild.Channels)
                            if (channel.GetType().GetInterfaces().Contains(typeof(ISocketMessageChannel)))
                                try
                                {
                                    IEnumerable<IMessage> messages = ((ISocketMessageChannel)channel).GetMessagesAsync().FlattenAsync().Result;
                                    foreach (IMessage m in messages)
                                        if (!m.Author.IsBot && !string.IsNullOrWhiteSpace(m.Content) && !m.Content.StartsWith(Program.Prefix) && m.Content[0] != '!')
                                            input += m.Content + "\n";
                                }
                                catch { }
            }

            // Load from text Files
            if (Directory.Exists(saucePath))
            {
                string[] files = Directory.GetFiles(saucePath);
                foreach (string file in files)
                {
                    if (!Config.Data.LoadedMarkovTextFiles.Contains(Path.GetFileName(file)))
                    {
                        string[] lines = File.ReadAllLines(file);
                        foreach (string line in lines)
                        {
                            string trimmed = line.Trim('\n').Trim('\t').Trim(' ');
                            if (!string.IsNullOrWhiteSpace(trimmed))
                                input += trimmed + "\n";
                        }
                        Config.Data.LoadedMarkovTextFiles.Add(Path.GetFileName(file));
                    }
                }
            }

            MarkovHelper.AddToDict(input);

            loadedDict = true;
            ConsoleWrapper.WriteLine("Loaded markow in " + (DateTime.Now - start).TotalSeconds + "s", ConsoleColor.Cyan);
        }
        public void OnNonCommandMessageRecieved(IMessage message)
        {
            try
            {
                IEnumerable<IMessage> messages = message.Channel.GetMessagesAsync(2).FlattenAsync().Result;
                if (messages.Count() == 2)
                    MarkovHelper.AddToDict(messages.ElementAt(1).Content, message.Content);
                else
                    MarkovHelper.AddToDict(message.Content);
            }
            catch { }
        }
        public void OnExit()
        {
            if (loadedDict)
                MarkovHelper.SaveDict();
        }

        public override void Execute(SocketMessage message)
        {
            string[] split = message.Content.Split(new char[] { ' ', '\n' });

            try
            {
                string output = MarkovHelper.GetString(split.Length > 1 ? split.Skip(1).Aggregate((x, y) => { return x + " " + y; }) : "", 5, 2000);
                DiscordNETWrapper.SendText(output, message.Channel).Wait();
            }
            catch (NoEmptyElementException)
            {
                DiscordNETWrapper.SendText("Markow isn't ready yet!", message.Channel).Wait();
            }
        }
    }
}
