using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Discord.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MEE7.Commands;
using System.Globalization;
using MEE7.Configuration;
using System.Reflection;
using System.Runtime.Versioning;
using NAudio.Wave;

namespace MEE7
{
    public class IllegalCommandException : Exception { public IllegalCommandException(string message) : base (message) { } }

    public class Program
    {
        #if DEBUG
            static readonly string runConfig = "Debug";
        #else
            static readonly string runConfig = "Release";
        #endif

        // Console / Execution
        static int clearYcoords;
        static bool exitedNormally = false;
        static string buildDate;
        static int ConcurrentCommandExecutions = 0;
        public static Random RDM { get; private set; } = new Random();
        static readonly int AutoSaveIntervalInMinutes = 60;
        public static readonly string ExePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\";
        readonly static string LogPath = "Log.txt";

        // Client 
        static DiscordSocketClient client;
        public static bool ClientReady { get; private set; }
        static bool gotWorkingToken = false;

        // Commands
        public const string prefix = "$";
        static Command[] commands;
        static EmbedBuilder HelpMenu = new EmbedBuilder();
        static Type[] commandTypes = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                                      from assemblyType in domainAssembly.GetTypes()
                                      where assemblyType.IsSubclassOf(typeof(Command))
                                      select assemblyType).ToArray();

        // Command Events
        public delegate void NonCommandMessageRecievedHandler(SocketMessage message);
        public static event NonCommandMessageRecievedHandler OnNonCommandMessageRecieved;
        public delegate void EmojiReactionAddedHandler(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3);
        public static event EmojiReactionAddedHandler OnEmojiReactionAdded;
        public delegate void EmojiReactionRemovedHandler(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3);
        public static event EmojiReactionRemovedHandler OnEmojiReactionRemoved;
        public delegate void EmojiReactionUpdatedHandler(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3);
        public static event EmojiReactionUpdatedHandler OnEmojiReactionUpdated;
        public delegate void ConnectedHandler();
        public static event ConnectedHandler OnConnected;
        public delegate void ExitHandler();
        public static event ExitHandler OnExit;

        // Discord
        private static SocketUser Pmaster;
        public static SocketUser Master
        {
            get { return Pmaster; }
            set
            {
                if (Pmaster == null)
                    Pmaster = value;
                else
                    throw new FieldAccessException("The Master may only be set once!");
            }
        }
        static ulong[] ExperimentalChannels = new ulong[] { 473991188974927884 };
        static ISocketMessageChannel CurrentChannel;
        static List<Tuple<RestUserMessage, Exception>> CachedErrorMessages =
            new List<Tuple<RestUserMessage, Exception>>();
        static readonly string ErrorMessage = "Uwu We made a fucky wucky!! A wittle fucko boingo! " +
            "The code monkeys at our headquarters are working VEWY HAWD to fix this!";
        static readonly Emoji ErrorEmoji = new Emoji("🤔");

        static readonly string commandExecutionLock = "";
        static readonly string youtubeDownloadLock = "";
        static readonly string exitlock = "";
        
        // --- Main ---------------------------------------------------------------------------------------------------------
        static void Main(string[] args)
        {
            try
            {
                ExecuteBot();
            }
            catch (Exception ex)
            {
                try { Config.Save(); } catch { }
                SaveToLog("Error Message: " + ex.Message  + "\nStack Trace: " + ex.StackTrace);
            }
        }

        static void ExecuteBot()
        {
            StartUp();
            HandleConsoleCommandsLoop();
            
            BeforeClose();
            
            client.SetStatusAsync(UserStatus.DoNotDisturb).Wait();
            client.StopAsync().Wait();
            client.LogoutAsync().Wait();
        }
        
        static void StartUp()
        {
            Console.Title = "MEE7";
            ShowWindow(GetConsoleWindow(), 2);
            Thread.CurrentThread.Name = "Main";
            Console.ForegroundColor = ConsoleColor.White;
            Directory.SetCurrentDirectory(Path.GetDirectoryName(ExePath));
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;
            
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            LoadBuildDate();

            "youtube-dl -U".RunAsConsoleCommand(360, () => { }, (string o, string e) => { ConsoleWrite(o + e); });

            client = new DiscordSocketClient();
            SetClientEvents();

            Login();

            CreateCommandInstances();

            while (!ClientReady) { Thread.Sleep(20); }

            SetState();
            Master = client.GetUser(300699566041202699);

            BuildHelpMenu();

            CurrentChannel = (ISocketMessageChannel)client.GetChannel(473991188974927884);
            PrintConsoleStartup();

            CallOnConnected();

            StartAutosaveLoop();
        }
        static void LoadBuildDate()
        {
            try
            {
                buildDate = File.ReadAllText("BuildDate.txt").TrimEnd('\n');
            }
            catch
            {
                if (buildDate == null)
                    buildDate = "Error: Couldn't read build date!";
            }
        }
        static void SetClientEvents()
        {
            client.Log += Client_Log;
            client.JoinedGuild += Client_JoinedGuild;
            client.MessageReceived += MessageReceived;
            client.Ready += Client_Ready;
            client.ReactionAdded += Client_ReactionAdded;
            client.ReactionRemoved += Client_ReactionRemoved;
        }
        static void Login()
        {
            while (!gotWorkingToken)
            {
                try
                {
                    if (Config.Data.BotToken == "<INSERT BOT TOKEN HERE>")
                    {
                        ShowWindow(GetConsoleWindow(), 4);
                        //SystemSounds.Exclamation.Play(); Support for SystemSounds is planned for .Net Core 3.0
                        ConsoleWrite("Give me a Bot Token: ");
                        Config.Data.BotToken = Console.ReadLine();
                        Config.Save();
                    }

                    client.LoginAsync(TokenType.Bot, Config.Data.BotToken).Wait();
                    client.StartAsync().Wait();

                    gotWorkingToken = true;
                }
                catch { Config.Data.BotToken = "<INSERT BOT TOKEN HERE>"; }
            }
        }
        static void CreateCommandInstances()
        {
            commands = new Command[commandTypes.Length];
            for (int i = 0; i < commands.Length; i++)
            {
                object test = Activator.CreateInstance(commandTypes[i]);
                commands[i] = (Command)test;
                if (commands[i].CommandLine.Contains(" ") || commands[i].Prefix.Contains(" "))
                    throw new IllegalCommandException("Commands and Prefixes mustn't contain spaces!\nOn command: \"" + commands[i].Prefix + commands[i].CommandLine + "\" in " + commands[i]);
            }
            commands = commands.OrderBy(x => x.CommandLine).ToArray();
        }
        static void SetState()
        {
#if DEBUG
            client.SetGameAsync($"{prefix}help [DEBUG-MODE]", "", ActivityType.Listening).Wait();
#else
            client.SetGameAsync($"{prefix}help", "", ActivityType.Listening).Wait();
#endif
        }
        static void BuildHelpMenu()
        {
            HelpMenu.WithColor(0, 128, 255);
            HelpMenu.AddFieldDirectly($"{prefix}help", $"Prints the HelpMenu for a Command" +
                (commands.Where(x => x.HelpMenu != null).ToList().Count != 0 ?
                $", eg. {prefix}help {commands.First(x => x.HelpMenu != null).CommandLine}" : "") +
                "\nCommands with a HelpMenu are marked with a (h)");
            for (int i = 0; i < commands.Length; i++)
            {
                if (commands[i].CommandLine != "" && !commands[i].IsHidden)
                {
                    string desc = ((commands[i].Desc == null ? "" : commands[i].Desc + "   ")).Trim(' ');
                    HelpMenu.AddFieldDirectly(commands[i].Prefix + commands[i].CommandLine +
                        (commands[i].IsExperimental ? " [EXPERIMENTAL]" : "") + (commands[i].HelpMenu == null ? "" : " (h)"),
                        string.IsNullOrWhiteSpace(desc) ? "-" : desc, true);
                }
            }
            HelpMenu.WithDescription($"I was made by {Master.Mention}\nYou can find my source-code " +
                $"[here](https://github.com/niklasCarstensen/Discord-Bot).\n\nCommands:");
            HelpMenu.WithFooter($"Running {runConfig} build from {buildDate} on {Environment.OSVersion.VersionString} / " +
                $"{Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName}\n");
            HelpMenu.WithThumbnailUrl("https://openclipart.org/image/2400px/svg_to_png/280959/1496637751.png");
        }
        static void PrintConsoleStartup()
        {
            Console.CursorLeft = 0;
            ConsoleWriteLine("Active on the following Servers: ", ConsoleColor.White);
            try
            {
                foreach (SocketGuild g in client.Guilds)
                {
                    ConsoleWrite($"  {g.Name}", ConsoleColor.Magenta);
                    ConsoleWriteLine($"{new string(Enumerable.Repeat(' ', client.Guilds.Max(x => x.Name.Length) - g.Name.Length + 2).ToArray())}{g.Id}", 
                        ConsoleColor.White);
                } 
            }
            catch { ConsoleWriteLine("Error Displaying all servers!", ConsoleColor.Red); }
            ConsoleWrite("Default channel is: ");
            ConsoleWrite(CurrentChannel, ConsoleColor.Magenta);
            ConsoleWrite(" on ");
            ConsoleWriteLine(GetGuildFromChannel(CurrentChannel).Name, ConsoleColor.Magenta);
            ConsoleWriteLine("Awaiting your commands: ");
            clearYcoords = Console.CursorTop;
        }
        static void CallOnConnected()
        {
            Task.Run(() => {
                try { OnConnected(); }
                catch (Exception e) { ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
            });
        }
        static void StartAutosaveLoop()
        {
            Task.Run(() => {
                while (true)
                {
                    Thread.Sleep(AutoSaveIntervalInMinutes * 60000);
                    if (Config.UnsavedChanges)
                    {
                        Config.Save();
                        ConsoleWriteLine($"{DateTime.Now.ToLongTimeString()} Autosaved!", ConsoleColor.Cyan);
                    }
                    else
                    {
                        ConsoleWriteLine($"{DateTime.Now.ToLongTimeString()} Autosaved! [Nothing to save]", ConsoleColor.Cyan);
                    }
                }
            });
        }

        static void HandleConsoleCommandsLoop()
        {
            while (true)
            {
                string input = Console.ReadLine();
                
                if (input == "exit")
                    break;

                if (!input.StartsWith("/"))
                {
                    if (CurrentChannel == null)
                        ConsoleWriteLine("No channel selected!");
                    else if (!string.IsNullOrWhiteSpace(input))
                    {
                        try
                        {
                            SendText(input, CurrentChannel).Wait();
                        }
                        catch (Exception e)
                        {
                            ConsoleWriteLine(e, ConsoleColor.Red);
                        }
                    }
                }
                else if (input.StartsWith("/file "))
                {
                    if (CurrentChannel == null)
                        ConsoleWriteLine("No channel selected!");
                    else
                    {
                        string[] splits = input.Split(' ');
                        string path = splits.Skip(1).Aggregate((x, y) => x + " " + y);
                        SendFile(path.Trim('\"'), CurrentChannel).Wait();
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
                            ConsoleWriteLine("Succsessfully set new channel!", ConsoleColor.Green);
                            ConsoleWrite("Current channel is: ");
                            ConsoleWriteLine(CurrentChannel, ConsoleColor.Magenta);
                        }
                        else
                            ConsoleWriteLine("Couldn't set new channel!", ConsoleColor.Red);
                    }
                    catch
                    {
                        ConsoleWriteLine("Couldn't set new channel!", ConsoleColor.Red);
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
                            try
                            {
                                M = ((ISocketMessageChannel)GetChannelFromID(Config.Data.ChannelsWrittenOn[i])).
                                    GetMessageAsync(Convert.ToUInt64(splits[1])).GetAwaiter().GetResult();

                                if (M != null)
                                {
                                    M.DeleteAsync().Wait();
                                    DeletionComplete = true;
                                } 
                            }
                            catch { }
                        }
                    }
                    catch (Exception e)
                    {
                        ConsoleWriteLine(e, ConsoleColor.Red);
                    }
                    #endregion
                }
                else if (input == "/PANIKDELETE")
                {
                    foreach (ulong ChannelID in Config.Data.ChannelsWrittenOn)
                    {
                        IEnumerable<IMessage> messages = ((ISocketMessageChannel)client.GetChannel(ChannelID)).GetMessagesAsync(int.MaxValue).FlattenAsync().GetAwaiter().GetResult();
                        foreach (IMessage m in messages)
                        {
                            if (m.Author.Id == client.CurrentUser.Id)
                                m.DeleteAsync().Wait();
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
                    Console.WriteLine(Config.ToString());
                }
                else if (input == "/restart")
                {
                    Process.Start("MEE7.exe");
                    break;
                }
                else if (input.StartsWith("/test"))
                {
                    string[] split = input.Split(' ');
                    int index = -1;
                    if (split.Length > 0)
                        try { index = Convert.ToInt32(split[1]); } catch { }
                    Tests.Run(index);
                }
                else if (input.StartsWith("/roles")) // ServerID
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        ConsoleWriteLine(String.Join("\n", GetGuildFromID(Convert.ToUInt64(split[1])).Roles.Select(x => x.Name)), ConsoleColor.Cyan);
                    }
                    catch (Exception e) { ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/rolePermissions")) // ServerID RoleName
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        ConsoleWriteLine(GetGuildFromID(Convert.ToUInt64(split[1])).Roles.First(x => x.Name == split[2]).Permissions.ToList().
                            Select(x => x.ToString()).Aggregate((x, y) => x + "\n" + y), ConsoleColor.Cyan);
                    }
                    catch (Exception e) { ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/assignRole")) // ServerID UserID RoleName
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        GetGuildFromID(Convert.ToUInt64(split[1])).GetUser(Convert.ToUInt64(split[2])).
                            AddRoleAsync(GetGuildFromID(Convert.ToUInt64(split[1])).Roles.First(x => x.Name == split[3])).Wait();
                        ConsoleWriteLine("That worked!", ConsoleColor.Cyan);
                    }
                    catch (Exception e) { ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/channels")) // ChannelID
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        ConsoleWriteLine(String.Join("\n", GetGuildFromID(Convert.ToUInt64(split[1])).Channels.Select(x => x.Name + "\t" + x.Id + "\t" + x.GetType())), ConsoleColor.Cyan);
                    }
                    catch (Exception e) { ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/read")) // ChannelID
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        var messages = (GetChannelFromID(Convert.ToUInt64(split[1])) as ISocketMessageChannel).GetMessagesAsync(100).FlattenAsync().GetAwaiter().GetResult();
                        ConsoleWriteLine(String.Join("\n", messages.Reverse().Select(x => x.Author + ": " + x.Content)), ConsoleColor.Cyan);
                    }
                    catch (Exception e) { ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else
                    ConsoleWriteLine("I dont know that command.", ConsoleColor.Red);
                ConsoleWrite("$");
            }
        }

        static void BeforeClose()
        {
            lock (exitlock)
            {
                ConsoleWriteLine("Closing... Files are being saved");
                Config.Save();
                ConsoleWriteLine("Closing... Command Exit events are being executed");
                try { OnExit(); }
                catch (Exception e) { ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                ConsoleWriteLine("Closing... Remove Error Emojis");
                foreach (Tuple<RestUserMessage, Exception> err in CachedErrorMessages)
                {
                    err.Item1.RemoveAllReactionsAsync().Wait();
                    err.Item1.ModifyAsync(m => m.Content = ErrorMessage).Wait();
                }
                exitedNormally = true;
            }
        }
        
        // Events
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
                        if (r.Permissions.ViewChannel)
                            hasRead = true;
                        if (r.Permissions.ReadMessageHistory)
                            hasReadHistory = true;
                        if (r.Permissions.AttachFiles)
                            hasFiles = true;
                    }
                }

                if (!hasWrite)
                {
                    IDMChannel c = g.Owner.GetOrCreateDMChannelAsync().Result;
                    await c.SendMessageAsync("How can one be on your server and not have the right to write messages!? This is outrageous, its unfair!");
                    return;
                }

                if (!hasRead || !hasReadHistory || !hasFiles)
                {
                    await g.TextChannels.ElementAt(0).SendMessageAsync("Whoever added me has big gay and didn't give me all the usual permissions.");
                    return;
                }
            }
            catch (Exception e) { ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
        }
        private static Task Client_Ready()
        {
            ClientReady = true;
            return Task.FromResult(default(object));
        }
        private static Task Client_Log(LogMessage msg)
        {
            ConsoleColor color = ConsoleColor.White;
            if (msg.Severity == LogSeverity.Critical ||
                msg.Severity == LogSeverity.Error)
                color = ConsoleColor.Red;
            else if (msg.Severity == LogSeverity.Warning)
                color = ConsoleColor.Yellow;
            else if (msg.Severity == LogSeverity.Debug)
                color = ConsoleColor.Magenta;

            string log = msg.ToString();
            if (log.Length > Console.BufferWidth || log.Contains("\n"))
            {
                SaveToLog(log.ToString());
                ConsoleWriteLine(DateTime.Now.ToLongTimeString() + " Long log message has been saved to file.", color);
            }
            else
                ConsoleWriteLine(msg.ToString(), color);
            return Task.FromResult(default(object));
        }
        private static Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            Task.Run(async () => {
                Tuple<RestUserMessage, Exception> error = CachedErrorMessages.FirstOrDefault(x => x.Item1.Id == arg1.Id);
                if (error != null)
                {
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                    var reacts = (await arg1.GetOrDownloadAsync()).Reactions;
                    reacts.TryGetValue(ErrorEmoji, out var react);
                    if (react.ReactionCount > 1)
                        await error.Item1.ModifyAsync(m => m.Content = ErrorMessage + "\n\n```" + error.Item2 + "```");
                    else
                        await error.Item1.ModifyAsync(m => m.Content = ErrorMessage);
                }
            });
            if (arg3.UserId != OwnID)
                Task.Run(() => {
                    try { OnEmojiReactionAdded?.InvokeParallel(arg1, arg2, arg3);
                          OnEmojiReactionUpdated?.InvokeParallel(arg1, arg2, arg3); }
                    catch (Exception e) { ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                });

            return Task.FromResult(default(object));
        }
        private static Task Client_ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            Task.Run(async () => {
                Tuple<RestUserMessage, Exception> error = CachedErrorMessages.FirstOrDefault(x => x.Item1.Id == arg1.Id);
                if (error != null)
                {
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                    var reacts = (await arg1.GetOrDownloadAsync()).Reactions;
                    reacts.TryGetValue(ErrorEmoji, out var react);
                    if (react.ReactionCount > 1)
                        await error.Item1.ModifyAsync(m => m.Content = ErrorMessage + "\n\n```" + error.Item2 + "```");
                    else
                        await error.Item1.ModifyAsync(m => m.Content = ErrorMessage);
                }
            });
            if (arg3.UserId != OwnID)
                Task.Run(() => {
                    try { OnEmojiReactionRemoved?.InvokeParallel(arg1, arg2, arg3);
                          OnEmojiReactionUpdated?.InvokeParallel(arg1, arg2, arg3); }
                    catch (Exception e) { ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                });

            return Task.FromResult(default(object));
        }
        private static Task MessageReceived(SocketMessage message)
        {
            if (!message.Author.IsBot && message.Content.StartsWith(prefix))
                Task.Run(() => ParallelMessageReceived(message));
            if (message.Content.Length > 0 && (char.IsLetter(message.Content[0]) || message.Content[0] == '<' || message.Content[0] == ':'))
                Task.Run(() => {
                    try { OnNonCommandMessageRecieved.InvokeParallel(message); }
                    catch (Exception e) { ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                });
            return Task.FromResult(default(object));
        }
        private static void ParallelMessageReceived(SocketMessage message)
        {
            // Add server
            if (message.Channel is SocketGuildChannel)
            {
                ulong serverID = message.GetServerID();
                if (!Config.Data.ServerList.Exists(x => x.ServerID == serverID))
                    Config.Data.ServerList.Add(new DiscordServer(serverID));
            }

            if (message.Content.StartsWith(prefix + "help"))
            {
                string[] split = message.Content.Split(' ');
                if (split.Length < 2)
                    SendEmbed(HelpMenu, message.Channel).Wait();
                else
                {
                    foreach (Command c in commands)
                        if (c.CommandLine == split[1])
                        {
                            SendEmbed(c.HelpMenu, message.Channel).Wait();
                            return;
                        }
                    SendText("That command doesn't implement a HelpMenu", message.Channel).Wait();
                }
            }
            else
            {
                // Find command
                string[] split = message.Content.Split(new char[] { ' ', '\n' });
                Command called = commands.FirstOrDefault(x => (x.Prefix + x.CommandLine).ToLower() == split[0].ToLower());
                if (called != null)
                {
                    ExecuteCommand(called, message);
                }
                else
                {
                    // No command found
                    float[] distances = new float[commands.Length];
                    for (int i = 0; i < commands.Length; i++)
                        if (commands[i].CommandLine != "" && !commands[i].IsHidden)
                            distances[i] = Extensions.ModifiedLevenshteinDistance((commands[i].Prefix + commands[i].CommandLine).ToLower(), split[0].ToLower());
                        else
                            distances[i] = int.MaxValue;
                    int minIndex = 0;
                    float min = float.MaxValue;
                    for (int i = 0; i < commands.Length; i++)
                        if (distances[i] < min)
                        {
                            minIndex = i;
                            min = distances[i];
                        }
                    if (min < Math.Min(4, split[0].Length - 1))
                    {
                        SendText("I don't know that command, but " + commands[minIndex].Prefix + commands[minIndex].CommandLine + " is pretty close:", message.Channel).Wait();
                        ExecuteCommand(commands[minIndex], message);
                    }
                }
            }

            DiscordUser user = Config.Data.UserList.FirstOrDefault(x => x.UserID == message.Author.Id);
            if (user != null)
                user.TotalCommandsUsed++;
        }
        private static void ExecuteCommand(Command command, SocketMessage message)
        {
            if (command.GetType() == typeof(Template) && !ExperimentalChannels.Contains(message.Channel.Id))
                return;
            if (command.IsExperimental && !ExperimentalChannels.Contains(message.Channel.Id))
            {
                SendText("Experimental commands cant be used here!", message.Channel).Wait();
                return;
            }

            IDisposable typingState = null;
            try
            {
                typingState = message.Channel.EnterTypingState();
                lock (commandExecutionLock)
                {
                    ConcurrentCommandExecutions++;
                    UpdateWorkState();
                }

                SaveUser(message.Author.Id);
                command.Execute(message);

                if (message.Channel is SocketGuildChannel)
                    ConsoleWriteLine($"{DateTime.Now.ToLongTimeString()} Send {command.GetType().Name}\tin " + 
                        $"{((SocketGuildChannel)message.Channel).Guild.Name} \tin {message.Channel.Name} \tfor {message.Author.Username}", ConsoleColor.Green);
                else
                    ConsoleWriteLine($"{DateTime.Now.ToLongTimeString()} Send {command.GetType().Name}\tin " +
                       $"DMs \tfor {message.Author.Username}", ConsoleColor.Green);
            }
            catch (Exception e)
            {
                try // Try in case I dont have the permissions to write at all
                {
                    RestUserMessage m = message.Channel.SendMessageAsync(ErrorMessage).Result;

                    m.AddReactionAsync(ErrorEmoji).Wait();
                    CachedErrorMessages.Add(new Tuple<RestUserMessage, Exception>(m, e));
                }
                catch { }
                
                ConsoleWriteLine($"{DateTime.Now.ToLongTimeString()} [{command.GetType().Name}] {e.Message}\n  " +
                    $"{e.StackTrace.Split('\n').FirstOrDefault(x => x.Contains(":line "))?.Split('\\').Last().Replace(":", ", ")}", ConsoleColor.Red);
                SaveToLog(e.ToString());
            }
            finally
            {
                typingState.Dispose();
                lock (commandExecutionLock)
                {
                    ConcurrentCommandExecutions--;
                    UpdateWorkState();
                }
            }
        }
        static void UpdateWorkState()
        {
            if (ConcurrentCommandExecutions > 0)
                client.SetStatusAsync(UserStatus.DoNotDisturb);
            else
                client.SetStatusAsync(UserStatus.Online);
        }
        
        // Client Getters
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
        public static ulong OwnID
        {
            get
            {
                return GetSelf().Id;
            }
        }

        // Audio / Video
        private static Process CreateFFMPEGProcess(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -filter:a \"volume = 0.05\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
        public static string DownloadVideoFromYouTube(string YoutubeURL)
        {
            if (!YoutubeURL.StartsWith("https://www.youtube.com/watch?"))
                return "";

            lock (youtubeDownloadLock)
            {
                string videofile = "Downloads\\YoutubeVideo.mp4";
                Directory.CreateDirectory(Path.GetDirectoryName(videofile));
                if (File.Exists(videofile))
                {
                    int i = 0;
                    while (true)
                    {
                        if (File.Exists(videofile) && new FileInfo(videofile).IsFileLocked())
                            videofile = $"Downloads\\YoutubeVideo{++i}.mp4";
                        else
                        {
                            File.Delete(videofile);
                            break;
                        }
                    }
                }

                bool worked = false;
                $"youtube-dl.exe -f mp4 -o \"{videofile}\" {YoutubeURL}".RunAsConsoleCommand(25, () => { }, 
                    (s, e) => { if (s != null) worked = true; }, (StreamWriter w) => w.Write("e"));

                if (worked)
                    return videofile;
                else
                    return "";
            }
        }
        public static Process GetAudioStreamFromYouTubeVideo(string YoutubeURL, string audioFormat)
        {
            if (!YoutubeURL.StartsWith("https://www.youtube.com/watch?"))
                return null;

            Process P = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "youtube-dl",
                    Arguments = $"--audio-format {audioFormat} -o - {YoutubeURL}",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true
                }
            };
            P.Start();
            return P;
        }
        public static async Task SendAudioAsync(IAudioClient audioClient, Stream stream)
        {
            Exception ex = null;
            using (AudioOutStream audioStream = audioClient.CreatePCMStream(AudioApplication.Music))
            {
                try { await stream.CopyToAsync(audioStream); }
                catch (Exception e) { ex = e; }
                finally { await audioStream.FlushAsync(); }
            }

            if (ex != null)
                throw ex;
        }
        public static async Task SendAudioAsync(IAudioClient audioClient, string path)
        {
            Exception ex = null;
            using (Process ffmpeg = CreateFFMPEGProcess(path))
            using (AudioOutStream stream = audioClient.CreatePCMStream(AudioApplication.Music))
            {
                try { await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream); }
                catch (Exception e) { ex = e; }
                finally { await stream.FlushAsync(); }
            }

            if (ex != null)
                throw ex;
        }

        // Send Wrappers
        public static async Task<IUserMessage> SendFile(string path, IMessageChannel Channel, string text = "")
        {
            SaveChannel(Channel);
            return await Channel.SendFileAsync(path, text);
        }
        public static async Task<IUserMessage> SendFile(Stream stream, IMessageChannel Channel, string fileEnd, string fileName = "", string text = "")
        {
            SaveChannel(Channel);
            if (fileName == "")
                fileName = DateTime.Now.ToBinary().ToString();
            stream.Position = 0;
            return await Channel.SendFileAsync(stream, fileName + "." + fileEnd.TrimStart('.'), text);
        }
        public static async Task<IUserMessage> SendBitmap(Bitmap bmp, IMessageChannel Channel, string text = "")
        {
            SaveChannel(Channel);
            MemoryStream stream = new MemoryStream();
            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return await SendFile(stream, Channel, "png", "", text);
        }
        public static async Task<List<IUserMessage>> SendText(string text, IMessageChannel Channel)
        {
            List<IUserMessage> sendMessages = new List<IUserMessage>();
            SaveChannel(Channel);
            if (text.Length < 2000)
                sendMessages.Add(await Channel.SendMessageAsync(text));
            else
            {
                while (text.Length > 0)
                {
                    int subLength = Math.Min(1999, text.Length);
                    string sub = text.Substring(0, subLength);
                    sendMessages.Add(await Channel.SendMessageAsync(sub));
                    text = text.Remove(0, subLength);
                }
            }
            return sendMessages;
        }
        public static async Task<List<IUserMessage>> SendText(string text, ulong ChannelID)
        {
            return await SendText(text, (ISocketMessageChannel)Program.GetChannelFromID(ChannelID));
        }
        public static async Task<List<IUserMessage>> SendEmbed(EmbedBuilder Embed, IMessageChannel Channel, string text = "")
        {
            if (Embed == null)
                return new List<IUserMessage>();

            if (Embed.Color == null && !(Channel is IDMChannel))
                Embed.Color = GetGuildFromChannel(Channel).GetUser(GetSelf().Id).GetDisplayColor();

            List<IUserMessage> sendMessages = new List<IUserMessage>();
            if (Embed.Fields == null || Embed.Fields.Count < 25)
                sendMessages.Add(await Channel.SendMessageAsync(text, false, Embed.Build()));
            else
            {
                List<EmbedFieldBuilder> Fields = new List<EmbedFieldBuilder>(Embed.Fields);
                while (Fields.Count > 0)
                {
                    EmbedBuilder eb = new EmbedBuilder
                    {
                        Color = Embed.Color,
                        Description = Embed.Description,
                        Author = Embed.Author,
                        Footer = Embed.Footer,
                        ImageUrl = Embed.ImageUrl,
                        ThumbnailUrl = Embed.ThumbnailUrl,
                        Timestamp = Embed.Timestamp,
                        Title = Embed.Title,
                        Url = Embed.Url
                    };
                    for (int i = 0; i < 25 && Fields.Count > 0; i++)
                    {
                        eb.Fields.Add(Fields[0]);
                        Fields.RemoveAt(0);
                    }
                    sendMessages.Add(await Channel.SendMessageAsync(text, false, eb.Build()));
                }
            }
            SaveChannel(Channel);
            return sendMessages;
        }

        // Save
        public static void SaveChannel(IChannel Channel)
        {
            if (Config.Data.ChannelsWrittenOn == null)
                Config.Data.ChannelsWrittenOn = new List<ulong>();
            if (!Config.Data.ChannelsWrittenOn.Contains(Channel.Id))
            {
                Config.Data.ChannelsWrittenOn.Add(Channel.Id);
                Config.Save();
            }
        }
        public static void SaveUser(ulong UserID)
        {
            if (!Config.Data.UserList.Exists(x => x.UserID == UserID))
                Config.Data.UserList.Add(new DiscordUser(UserID));
        }
        public static void SaveToLog(string message)
        {
            using (StreamWriter sw = File.AppendText(LogPath))
            {
                sw.WriteLine();
                sw.WriteLine("==========================Logging========================");
                sw.WriteLine("============Start=============" + DateTime.Now);
                sw.WriteLine(message);
                sw.WriteLine("=============End=============");
            }
        }
        
        // Closing Event
        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2 && !exitedNormally)
            {
                Console.WriteLine();
                BeforeClose();
            }
            Thread.Sleep(250);
            return false;
        }
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
        private delegate bool ConsoleEventDelegate(int eventType);

        // Wrappers
        public static void ConsoleWriteLine(object text, ConsoleColor Color)
        {
            lock (Console.Title)
            {
                if (Console.CursorLeft == 1)
                    Console.CursorLeft = 0;
                Console.ForegroundColor = Color;
                Console.WriteLine(text);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("$");
            }
        }
        public static void ConsoleWriteLine(object text)
        {
            lock (Console.Title)
            {
                if (Console.CursorLeft == 1)
                    Console.CursorLeft = 0;
                Console.WriteLine(text);
                Console.Write("$");
            }
        }
        public static void ConsoleWrite(object text, ConsoleColor Color)
        {
            lock (Console.Title)
            {
                if (Console.CursorLeft == 1)
                    Console.CursorLeft = 0;
                Console.ForegroundColor = Color;
                Console.Write(text);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        public static void ConsoleWrite(object text)
        {
            lock (Console.Title)
            {
                if (Console.CursorLeft == 1)
                    Console.CursorLeft = 0;
                Console.Write(text);
            }
        }
        public static EmbedBuilder CreateEmbedBuilder(string TitleText = "", string DescText = "", string ImgURL = "", IUser Author = null, string ThumbnailURL = "")
        {
            EmbedBuilder e = new EmbedBuilder();
            e.WithDescription(DescText);
            e.WithImageUrl(ImgURL);
            e.WithTitle(TitleText);
            if (Author != null)
                e.WithAuthor(Author);
            e.WithThumbnailUrl(ThumbnailURL);
            return e;
        }

        // Imports
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
