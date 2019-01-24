using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestDiscordBot.Commands;
using TestDiscordBot.Config;

namespace TestDiscordBot
{
    public class IllegalCommandException : Exception { public IllegalCommandException(string message) : base (message) { } }

    public class Program
    {
        int clearYcoords;
        bool clientReady = false;
        bool gotWorkingToken = false;
        bool exitedNormally = false;
        ulong[] ExperimentalChannels = new ulong[] { 473991188974927884 };
        string buildDate;
        ISocketMessageChannel CurrentChannel;
        DiscordSocketClient client;
        Command[] commands;
        Type[] commandTypes = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                               from assemblyType in domainAssembly.GetTypes()
                               where assemblyType.IsSubclassOf(typeof(Command))
                               select assemblyType).ToArray();
        
        static void Main(string[] args)
            => Global.P.MainAsync().GetAwaiter().GetResult();

        async Task MainAsync()
        {
            #region startup
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;

            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            try
            {
                buildDate = File.ReadAllText(Global.CurrentExecutablePath + "\\BuildDate.txt").TrimEnd('\n');
                Console.WriteLine("Build from: " + buildDate);
            } catch {
                if (buildDate == null)
                    buildDate = "Error: Couldn't read build date!";
            }
            client = new DiscordSocketClient();
            client.Log += Log;
            client.JoinedGuild += Client_JoinedGuild;
            
            while (!gotWorkingToken)
            {
                try
                {
                    if (config.Data.BotToken == "<INSERT BOT TOKEN HERE>")
                    {
                        SystemSounds.Exclamation.Play();
                        Console.Write("Give me a Bot Token: ");
                        config.Data.BotToken = Console.ReadLine();
                        config.Save();
                    }

                    await client.LoginAsync(TokenType.Bot, config.Data.BotToken);
                    await client.StartAsync();

                    gotWorkingToken = true;
                }
                catch { config.Data.BotToken = "<INSERT BOT TOKEN HERE>"; }
            }
            
            client.MessageReceived += MessageReceived;
            client.Ready += Client_Ready;
            
            commands = new Command[commandTypes.Length];
            for (int i = 0; i < commands.Length; i++)
            {
                commands[i] = (Command)Activator.CreateInstance(commandTypes[i]);
                if (commands[i].command.Contains(" ") || commands[i].prefix.Contains(" "))
                    throw new IllegalCommandException("Commands and Prefixes mustn't contain spaces!\nOn command: \"" + commands[i].prefix + commands[i].command + "\" in " + commands[i]);
            }

            commands = commands.OrderBy(x => x.command).ToArray(); // Sort commands in alphabetical order
            
            while (!clientReady) { Thread.Sleep(20); }
#if DEBUG
            await client.SetGameAsync("[DEBUG-MODE] Type " + Global.prefix + "help");
#else
            await client.SetGameAsync("Type " + Global.prefix + "help");
#endif
            Global.Master = client.GetUser(300699566041202699);
            CurrentChannel = (ISocketMessageChannel)client.GetChannel(473991188974927884);
            Console.CursorLeft = 0;
            Console.WriteLine("Active on the following Servers: ");
            try
            {
                foreach (SocketGuild g in client.Guilds)
                    Console.WriteLine(g.Name);
            } catch { }
            Console.Write("Default channel is: ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(CurrentChannel);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" on " + getGuildFromChannel(CurrentChannel).Name);
            Console.WriteLine("Awaiting your commands: ");
            clearYcoords = Console.CursorTop;
            ShowWindow(GetConsoleWindow(), 2);
            foreach (Command c in commands)
            {
                await Task.Run(() => {
                    try
                    {
                        c.onConnected();
                    }
                    catch (Exception e) { Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                });
            }
            #endregion

            #region commands
            while (true)
            {
                lock (Console.Title)
                {
                    Console.CursorLeft = 0;
                    Console.Write("$");
                }
                string input = Console.ReadLine();

                if (input == "exit")
                    break;

                if (!input.StartsWith("/"))
                {
                    if (CurrentChannel == null)
                        Console.WriteLine("No channel selected!");
                    else
                    {
                        try
                        {
                            await Global.SendText(input, CurrentChannel);
                        }
                        catch (Exception e)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(e);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }
                else if (input.StartsWith("/file "))
                {
                    if (CurrentChannel == null)
                        Console.WriteLine("No channel selected!");
                    else
                    {
                        string[] splits = input.Split(' ');
                        string path = splits.Skip(1).Aggregate((x, y) => x + " " + y);
                        await Global.SendFile(path.Trim('\"'), CurrentChannel);
                    }
                }
                else if (input.StartsWith("/setchannel ") || input.StartsWith("/set "))
                {
                    #region set channel code
                    try
                    {
                        string[] splits = input.Split(' ');

                        SocketChannel channel = client.GetChannel((ulong)Convert.ToInt64(splits[1]));
                        IMessageChannel textChannel = (IMessageChannel)channel;
                        if (textChannel != null)
                        {
                            CurrentChannel = (ISocketMessageChannel)textChannel;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Succsessfully set new channel!");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("Current channel is: ");
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine(CurrentChannel);
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Couldn't set new channel!");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                    catch
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Couldn't set new channel!");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
#endregion
                }
                else if (input.StartsWith("/del "))
                {
                    #region deletion code
                    try
                    {
                        string[] splits = input.Split(' ');
                        IMessage M = null;
                        bool DeletionComplete = false;

                        for (int i = 0; !DeletionComplete; i++)
                        {
                            M = await ((ISocketMessageChannel)getChannelFromID(config.Data.ChannelsWrittenOn[i])).GetMessageAsync(Convert.ToUInt64(splits[1]));

                            if (M != null)
                            {
                                try
                                {
                                    await M.DeleteAsync();
                                    DeletionComplete = true;
                                }
                                catch { }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
#endregion
                }
                else if (input == "/PANIKDELETE")
                {
                    foreach (ulong ChannelID in config.Data.ChannelsWrittenOn)
                    {
                        IEnumerable<IMessage> messages = await ((ISocketMessageChannel)client.GetChannel(ChannelID)).GetMessagesAsync(int.MaxValue).Flatten();
                        foreach (IMessage m in messages)
                        {
                            if (m.Author.Id == client.CurrentUser.Id)
                                await m.DeleteAsync();
                        }
                    }
                }
                else if (input == "/clear")
                {
                    Console.CursorTop = clearYcoords;
                    Console.CursorLeft = 0;
                    string large = "";
                    for (int i = 0; i < (Console.BufferHeight - clearYcoords - 2) * Console.BufferWidth; i++)
                        large += " ";
                    Console.WriteLine(large);
                    Console.CursorTop = clearYcoords;
                    Console.CursorLeft = 0;
                }
                else if (input == "/config")
                {
                    Console.WriteLine(config.ToString());
                }
                else if (input == "/restart")
                {
                    Process.Start(Global.CurrentExecutablePath + "\\TestDiscordBot.exe");
                    Process.GetCurrentProcess().Kill();
                }
                else if (input == "/test")
                {
                    // TODO: Test
                    try
                    {
                        EmbedBuilder embed = new EmbedBuilder();
                        embed.WithDescription("Find my source-code [kek124](https://www.google.de).\n");
                        embed.AddField("[kek](https://www.google.de)", "[kek2](https://www.google.de)");
                        embed.WithFooter("[kek4](https://www.google.de)");
                        embed.WithTitle("[kek5](https://www.google.de)");
                        await Global.SendEmbed(embed, CurrentChannel);
                    }
                    catch (Exception e) { Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else
                    Global.ConsoleWriteLine("I dont know that command.", ConsoleColor.Red);
            }
            #endregion

            foreach (Command c in commands)
            {
                try
                {
                    c.onExit();
                }
                catch (Exception e) { Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
            }
            config.Save();
            exitedNormally = true;

            await client.SetGameAsync("Im actually closed but discord doesnt seem to notice...");
            await client.SetStatusAsync(UserStatus.DoNotDisturb);
            await client.LogoutAsync();
            Environment.Exit(0);
        }

        private async Task Client_JoinedGuild(SocketGuild arg)
        {
            try
            {
                bool hasWrite = false, hasRead = false, hasReadHistory = false, hasFiles = false;
                SocketGuild g = client.GetGuild(479950092938248193);
                IUser u = g.Users.FirstOrDefault(x => x.Id == getSelf().Id);
                if (u != null)
                {
                    IEnumerable<IRole> roles = (u as IGuildUser).RoleIds.Select(x => (u as IGuildUser).Guild.GetRole(x));
                    foreach (IRole r in roles)
                    {
                        if (r.Permissions.SendMessages)
                            hasWrite = true;
                        if (r.Permissions.ReadMessages)
                            hasRead = true;
                        if (r.Permissions.ReadMessageHistory)
                            hasReadHistory = true;
                        if (r.Permissions.AttachFiles)
                            hasFiles = true;
                    }
                }

                if (!hasWrite)
                {
                    IDMChannel c = await g.Owner.GetOrCreateDMChannelAsync();
                    await c.SendMessageAsync("How can one be on your server and not have the right to write messages!? This is outrageous, its unfair!");
                    return;
                }

                if (!hasRead || !hasReadHistory || !hasFiles)
                {
                    await g.TextChannels.ElementAt(0).SendMessageAsync("Whoever added me has big gay and didn't give me all the permissions.");
                    return;
                }
            }
            catch (Exception e) { Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
        }
        private async Task Client_Ready()
        {
            clientReady = true;
        }
        private async Task Log(LogMessage msg)
        {
            Console.CursorLeft = 0;
            Console.WriteLine(msg.ToString());
            if (clientReady)
                Console.Write("$");
        }
        private async Task MessageReceived(SocketMessage message)
        {
            if (!message.Author.IsBot)
            {
                if (message.Content.StartsWith(Global.prefix))
                {
                    Thread t = new Thread(new ParameterizedThreadStart(ThreadedMessageReceived));
                    t.Start(message);
                }

                if (char.IsLetter(message.Content[0]) || message.Content[0] == '<' || message.Content[0] == ':')
                {
                    Task.Factory.StartNew(() => {
                        foreach (Command c in commands)
                        {
                            try
                            {
                                c.onNonCommandMessageRecieved(message);
                            }
                            catch (Exception e) { Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                        }
                    });
                }
            }

        }
        private async void ThreadedMessageReceived(object o)
        {
            SocketMessage message = (SocketMessage)o;

            // Add server
            if (message.Channel is SocketGuildChannel)
            {
                ulong serverID = message.GetServerID();
                if (!config.Data.ServerList.Exists(x => x.ServerID == serverID))
                    config.Data.ServerList.Add(new DiscordServer(serverID));
            }

            if (message.Content == Global.prefix + "help")
            {
                #region get Help
                List<Command> commandsLeft = commands.ToList();
                while (commandsLeft.Count > 0)
                {
                    EmbedBuilder Embed = new EmbedBuilder();
                    Embed.WithColor(0, 128, 255);
                    for (int i = 0; i < 24 && commandsLeft.Count > 0; i++)
                    {
                        if (commandsLeft[0].command != "" && !commandsLeft[0].isHidden)
                        {
                            string desc = ((commandsLeft[0].desc == null ? "" : commandsLeft[0].desc + "   ")).Trim(' ');
                            Embed.AddField(commandsLeft[0].prefix + commandsLeft[0].command + (commandsLeft[0].isExperimental ? " [EXPERIMENTAL]" : ""), 
                                desc == null || desc == "" ? "-" : desc, true);
                        }
                        commandsLeft.RemoveAt(0);
                    }
                    
                    Embed.WithDescription("I was made by " + Global.Master.Mention + "\nYou can find my source-code [here](https://github.com/niklasCarstensen/Discord-Bot).\n\nCommands:");
                    Embed.WithFooter("Current Build from: " + buildDate);
                    Embed.WithThumbnailUrl("https://openclipart.org/image/2400px/svg_to_png/280959/1496637751.png");
                    await Global.SendEmbed(Embed, message.Channel);
                }
                #endregion
            }
            else
            {
                // Find command
                string[] split = message.Content.Split(new char[] { ' ', '\n' });
                Command called = commands.FirstOrDefault(x => (x.prefix + x.command).ToLower() == split[0].ToLower());
                if (called != null)
                {
                    await executeCommand(called, message);
                }
                else
                {
                    // No command found
                    int[] distances = new int[commands.Length];
                    for (int i = 0; i < commands.Length; i++)
                        if (commands[i].command != "" && !commands[i].isHidden)
                            distances[i] = Global.LevenshteinDistance((commands[i].prefix + commands[i].command).ToLower(), split[0].ToLower());
                        else
                            distances[i] = int.MaxValue;
                    int minIndex = 0;
                    int min = int.MaxValue;
                    for (int i = 0; i < commands.Length; i++)
                        if (distances[i] < min)
                        {
                            minIndex = i;
                            min = distances[i];
                        }
                    if (min < Math.Min(5, split[0].Length - 1))
                    {
                        await Global.SendText("I don't know that command, but " + commands[minIndex].prefix + commands[minIndex].command + " is pretty close:", message.Channel);
                        await executeCommand(commands[minIndex], message);
                    }
                }
            }

            DiscordUser user = config.Data.UserList.FirstOrDefault(x => x.UserID == message.Author.Id);
            if (user != null)
                user.TotalCommandsUsed++;
        }
        private async Task executeCommand(Command command, SocketMessage message)
        {
            if (command.isExperimental && !ExperimentalChannels.Contains(message.Channel.Id))
            {
                await Global.SendText("Experimental commands cant be used here!", message.Channel);
                return;
            }

            try
            {
                Global.SaveUser(message.Author.Id);
                await command.execute(message);

                if (message.Channel is SocketGuildChannel)
                    Global.ConsoleWriteLine("Send " + command.GetType().Name + " at " + DateTime.Now.ToShortTimeString() + "\tin " + 
                        ((SocketGuildChannel)message.Channel).Guild.Name + "\tin " + message.Channel.Name + "\tfor " + message.Author.Username, ConsoleColor.Green);
                else
                    Global.ConsoleWriteLine("Send " + command.GetType().Name + " at " + DateTime.Now.ToShortTimeString() + "\tin " +
                        "DMs\tin " + message.Channel.Name + "\tfor " + message.Author.Username, ConsoleColor.Green);
            }
            catch (Exception e)
            {
                try // Try in case I dont have the permissions to write at all
                {
                    await Global.SendText("Uwu We made a fucky wucky!! A wittle fucko boingo! The code monkeys at our headquarters are working VEWY HAWD to fix this!", message.Channel);
                } catch { }

                Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red);
            }
        }
        
        public SocketUser getUserFromId(ulong UserId)
        {
            return client.GetUser(UserId);
        }
        public SocketChannel getChannelFromID(ulong ChannelID)
        {
            return client.GetChannel(ChannelID);
        }
        public SocketGuild getGuildFromChannel(IChannel Channel)
        {
            return ((SocketGuildChannel)Channel).Guild;
        }
        public SocketSelfUser getSelf()
        {
            return client.CurrentUser;
        }
        public SocketGuild[] getGuilds()
        {
            return client.Guilds.ToArray();
        }
        public SocketGuild getGuildFromID(ulong GuildID)
        {
            return client.GetGuild(GuildID);
        }
        public bool isConnected
        {
            get
            {
                return clientReady;
            }
        }

        // Closing Event
        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2 && !Global.P.exitedNormally)
            {
                Console.WriteLine();
                Console.WriteLine("Closing... Files are being saved");
                File.Copy(config.getConfigPath(), Global.CurrentExecutablePath + "\\config_backup.json", true);
                config.Save();
            }
            return false;
        }
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        
        // Imports
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
