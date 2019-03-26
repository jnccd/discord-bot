using Discord;
using Discord.Commands;
using Discord.WebSocket;
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
        static int clearYcoords;
        public static bool ClientReady { get; private set; }
        static bool gotWorkingToken = false;
        static bool exitedNormally = false;
        static ulong[] ExperimentalChannels = new ulong[] { 473991188974927884 };
        static string buildDate;
        static ISocketMessageChannel CurrentChannel;
        static DiscordSocketClient client;
        static Command[] commands;
        static Type[] commandTypes = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                                      from assemblyType in domainAssembly.GetTypes()
                                      where assemblyType.IsSubclassOf(typeof(Command))
                                      select assemblyType).ToArray();
        static EmbedBuilder HelpMenu = new EmbedBuilder();
        static int Exectutions = 0;

        public static ulong OwnID
        {
            get
            {
                return GetSelf().Id;
            }
        }

        static void Main(string[] args)
        {
            try
            {
                ExecuteBot().Wait();
            }
            catch (Exception ex)
            {
                try { Config.Config.Save(); } catch { }

                string strPath = "Log.txt";
                if (!File.Exists(strPath))
                {
                    File.Create(strPath).Dispose();
                }
                using (StreamWriter sw = File.AppendText(strPath))
                {
                    sw.WriteLine();
                    sw.WriteLine("==========================Error Logging========================");
                    sw.WriteLine("============Start=============" + DateTime.Now);
                    sw.WriteLine("Error Message: " + ex.Message);
                    sw.WriteLine("Stack Trace: " + ex.StackTrace);
                    sw.WriteLine("=============End=============");
                }
            }
        }
        static async Task ExecuteBot()
        {
            #region startup
            Thread.CurrentThread.Name = "Main Console Input Reciever";
            ShowWindow(GetConsoleWindow(), 2);
            Console.ForegroundColor = ConsoleColor.White;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;

            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            try
            {
                buildDate = File.ReadAllText("BuildDate.txt").TrimEnd('\n');
                Global.ConsoleWriteLine("Build from: " + buildDate, ConsoleColor.Magenta);
            }
            catch
            {
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
                    if (Config.Config.Data.BotToken == "<INSERT BOT TOKEN HERE>")
                    {
                        ShowWindow(GetConsoleWindow(), 4);
                        SystemSounds.Exclamation.Play();
                        Console.CursorLeft = 0;
                        Console.Write("Give me a Bot Token: ");
                        Config.Config.Data.BotToken = Console.ReadLine();
                        Config.Config.Save();
                    }

                    await client.LoginAsync(TokenType.Bot, Config.Config.Data.BotToken);
                    await client.StartAsync();

                    gotWorkingToken = true;
                }
                catch { Config.Config.Data.BotToken = "<INSERT BOT TOKEN HERE>"; }
            }

            client.MessageReceived += MessageReceived;
            client.Ready += Client_Ready;
            client.ReactionAdded += Client_ReactionAdded;

            commands = new Command[commandTypes.Length];
            for (int i = 0; i < commands.Length; i++)
            {
                commands[i] = (Command)Activator.CreateInstance(commandTypes[i]);
                if (commands[i].CommandLine.Contains(" ") || commands[i].Prefix.Contains(" "))
                    throw new IllegalCommandException("Commands and Prefixes mustn't contain spaces!\nOn command: \"" + commands[i].Prefix + commands[i].CommandLine + "\" in " + commands[i]);
            }

            commands = commands.OrderBy(x => x.CommandLine).ToArray(); // Sort commands in alphabetical order

            while (!ClientReady) { Thread.Sleep(20); }
#if DEBUG
            await client.SetGameAsync("[DEBUG-MODE] Type " + Global.prefix + "help");
#else
            await client.SetGameAsync("Type " + Global.prefix + "help");
#endif
            Global.Master = client.GetUser(300699566041202699);

            // Build HelpMenu
            HelpMenu.WithColor(0, 128, 255);
            for (int i = 0; i < commands.Length; i++)
            {
                if (commands[i].CommandLine != "" && !commands[i].IsHidden)
                {
                    string desc = ((commands[i].Desc == null ? "" : commands[i].Desc + "   ")).Trim(' ');
                    HelpMenu.AddField(commands[i].Prefix + commands[i].CommandLine + (commands[i].IsExperimental ? " [EXPERIMENTAL]" : ""),
                        desc == null || desc == "" ? "-" : desc, true);
                }
            }
            HelpMenu.WithDescription("I was made by " + Global.Master.Mention + "\nYou can find my source-code [here](https://github.com/niklasCarstensen/Discord-Bot).\n\nCommands:");
            HelpMenu.WithFooter("Current Build from: " + buildDate);
            HelpMenu.WithThumbnailUrl("https://openclipart.org/image/2400px/svg_to_png/280959/1496637751.png");

            // Startup Console Display
            CurrentChannel = (ISocketMessageChannel)client.GetChannel(473991188974927884);
            Console.CursorLeft = 0;
            Global.ConsoleWriteLine("Active on the following Servers: ", ConsoleColor.Yellow);
            try
            {
                foreach (SocketGuild g in client.Guilds)
                    Global.ConsoleWriteLine(g.Name + "\t" + g.Id, ConsoleColor.Yellow);
            }
            catch { Global.ConsoleWriteLine("Error Displaying all servers!", ConsoleColor.Red); }
            Console.CursorLeft = 0;
            Console.Write("Default channel is: ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(CurrentChannel);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" on " + GetGuildFromChannel(CurrentChannel).Name);
            Console.WriteLine("Awaiting your commands: ");
            clearYcoords = Console.CursorTop;
            foreach (Command c in commands)
            {
                await Task.Run(() => {
                    try
                    {
                        c.OnConnected();
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
                            M = ((ISocketMessageChannel)GetChannelFromID(Config.Config.Data.ChannelsWrittenOn[i])).GetMessageAsync(Convert.ToUInt64(splits[1])).GetAwaiter().GetResult();

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
                    foreach (ulong ChannelID in Config.Config.Data.ChannelsWrittenOn)
                    {
                        IEnumerable<IMessage> messages = ((ISocketMessageChannel)client.GetChannel(ChannelID)).GetMessagesAsync(int.MaxValue).Flatten().GetAwaiter().GetResult();
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
                    Console.WriteLine(Config.Config.ToString());
                }
                else if (input == "/restart")
                {
                    Process.Start("TestDiscordBot.exe");
                    break;
                }
                else if (input == "/test")
                {
                    // TODO: Test
                    try
                    {
                        
                    }
                    catch (Exception e) { Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/roles")) // ServerID
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        Global.ConsoleWriteLine(String.Join("\n", GetGuildFromID(Convert.ToUInt64(split[1])).Roles.Select(x => x.Name)), ConsoleColor.Cyan);
                    }
                    catch (Exception e) { Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/rolePermissions")) // ServerID RoleName
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        Global.ConsoleWriteLine(GetGuildFromID(Convert.ToUInt64(split[1])).Roles.First(x => x.Name == split[2]).Permissions.ToList().
                            Select(x => x.ToString()).Aggregate((x, y) => x + "\n" + y), ConsoleColor.Cyan);
                    }
                    catch (Exception e) { Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/assignRole")) // ServerID UserID RoleName
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        await GetGuildFromID(Convert.ToUInt64(split[1])).GetUser(Convert.ToUInt64(split[2])).
                                AddRoleAsync(GetGuildFromID(Convert.ToUInt64(split[1])).Roles.First(x => x.Name == split[3]));
                        Global.ConsoleWriteLine("That worked!", ConsoleColor.Cyan);
                    }
                    catch (Exception e) { Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/channels")) // ChannelID
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        Global.ConsoleWriteLine(String.Join("\n", GetGuildFromID(Convert.ToUInt64(split[1])).Channels.Select(x => x.Name + "\t" + x.Id + "\t" + x.GetType())), ConsoleColor.Cyan);
                    }
                    catch (Exception e) { Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/read")) // ChannelID
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        var messages = (GetChannelFromID(Convert.ToUInt64(split[1])) as ISocketMessageChannel).GetMessagesAsync(100).Flatten().GetAwaiter().GetResult();
                        Global.ConsoleWriteLine(String.Join("\n", messages.Reverse().Select(x => x.Author + ": " + x.Content)), ConsoleColor.Cyan);
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
                    c.OnExit();
                }
                catch (Exception e) { Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
            }
            Config.Config.Save();
            exitedNormally = true;

            await client.SetGameAsync("Im actually closed but discord doesnt seem to notice...");
            await client.SetStatusAsync(UserStatus.DoNotDisturb);
            await client.LogoutAsync();
            Environment.Exit(0);
        }
        private static async Task Client_JoinedGuild(SocketGuild arg)
        {
            try
            {
                bool hasWrite = false, hasRead = false, hasReadHistory = false, hasFiles = false;
                SocketGuild g = client.GetGuild(479950092938248193);
                IUser u = g.Users.FirstOrDefault(x => x.Id == GetSelf().Id);
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
                    await g.TextChannels.ElementAt(0).SendMessageAsync("Whoever added me has big gay and didn't give me all the usual permissions.");
                    return;
                }
            }
            catch (Exception e) { Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
        }
        private static Task Client_Ready()
        {
            ClientReady = true;
            return Task.FromResult(0);
        }
        private static Task Log(LogMessage msg)
        {
            Global.ConsoleWriteLine(msg.ToString(), ConsoleColor.White);
            return Task.FromResult(0);
        }
        private static Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            Task.Run(() => {
                foreach (Command c in commands)
                {
                    try
                    {
                        c.OnEmojiReaction(arg1, arg2, arg3);
                    }
                    catch (Exception e) { Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
            });
            return Task.FromResult(default(object));
        }
        private static Task MessageReceived(SocketMessage message)
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
                    Task.Run(() => {
                        foreach (Command c in commands)
                        {
                            try
                            {
                                c.OnNonCommandMessageRecieved(message);
                            }
                            catch (Exception e) { Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                        }
                    });
                }
            }
            return Task.FromResult(default(object));
        }
        private static async void ThreadedMessageReceived(object o)
        {
            SocketMessage message = (SocketMessage)o;

            // Add server
            if (message.Channel is SocketGuildChannel)
            {
                ulong serverID = message.GetServerID();
                if (!Config.Config.Data.ServerList.Exists(x => x.ServerID == serverID))
                    Config.Config.Data.ServerList.Add(new DiscordServer(serverID));
            }

            if (message.Content == Global.prefix + "help")
            {
                await Global.SendEmbed(HelpMenu, message.Channel);
            }
            else
            {
                // Find command
                string[] split = message.Content.Split(new char[] { ' ', '\n' });
                Command called = commands.FirstOrDefault(x => (x.Prefix + x.CommandLine).ToLower() == split[0].ToLower());
                if (called != null)
                {
                    await ExecuteCommand(called, message);
                }
                else
                {
                    // No command found
                    int[] distances = new int[commands.Length];
                    for (int i = 0; i < commands.Length; i++)
                        if (commands[i].CommandLine != "" && !commands[i].IsHidden)
                            distances[i] = Global.LevenshteinDistance((commands[i].Prefix + commands[i].CommandLine).ToLower(), split[0].ToLower());
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
                        await Global.SendText("I don't know that command, but " + commands[minIndex].Prefix + commands[minIndex].CommandLine + " is pretty close:", message.Channel);
                        await ExecuteCommand(commands[minIndex], message);
                    }
                }
            }

            DiscordUser user = Config.Config.Data.UserList.FirstOrDefault(x => x.UserID == message.Author.Id);
            if (user != null)
                user.TotalCommandsUsed++;
        }
        private static async Task ExecuteCommand(Command command, SocketMessage message)
        {
            if (command.IsExperimental && !ExperimentalChannels.Contains(message.Channel.Id))
            {
                await Global.SendText("Experimental commands cant be used here!", message.Channel);
                return;
            }

            try
            {
                var typingState = message.Channel.EnterTypingState();
                Exectutions++;
                UpdateWorkState();

                Global.SaveUser(message.Author.Id);
                await command.Execute(message);

                if (message.Channel is SocketGuildChannel)
                    Global.ConsoleWriteLine("Send " + command.GetType().Name + " at " + DateTime.Now.ToShortTimeString() + "\tin " + 
                        ((SocketGuildChannel)message.Channel).Guild.Name + "\tin " + message.Channel.Name + "\tfor " + message.Author.Username, ConsoleColor.Green);
                else
                    Global.ConsoleWriteLine("Send " + command.GetType().Name + " at " + DateTime.Now.ToShortTimeString() + "\tin " +
                        "DMs\tin " + message.Channel.Name + "\tfor " + message.Author.Username, ConsoleColor.Green);

                typingState.Dispose();
                Exectutions--;
                UpdateWorkState();
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
        static void UpdateWorkState()
        {
            if (Exectutions > 0)
                client.SetStatusAsync(UserStatus.DoNotDisturb);
            else
                client.SetStatusAsync(UserStatus.Online);
        }
        
        public static SocketUser GetUserFromId(ulong UserId)
        {
            return client.GetUser(UserId);
        }
        public static SocketChannel GetChannelFromID(ulong ChannelID)
        {
            return client.GetChannel(ChannelID);
        }
        public static SocketGuild GetGuildFromChannel(IChannel Channel)
        {
            return ((SocketGuildChannel)Channel).Guild;
        }
        public static SocketSelfUser GetSelf()
        {
            return client.CurrentUser;
        }
        public static SocketGuild[] GetGuilds()
        {
            return client.Guilds.ToArray();
        }
        public static SocketGuild GetGuildFromID(ulong GuildID)
        {
            return client.GetGuild(GuildID);
        }

        // Closing Event
        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2 && !exitedNormally)
            {
                Console.WriteLine();
                Console.WriteLine("Closing... Files are being saved");
                Config.Config.Save();
            }
            Thread.Sleep(250);
            return false;
        }
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
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
