using Discord;
using Discord.Audio;
using Discord.Rest;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using MEE7.Backend.HelperFunctions.Extensions;
using MEE7.Commands;
using MEE7.Configuration;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace MEE7
{
    public partial class Program
    {
        #if DEBUG
            public static readonly string runConfig = "Debug";
        #else
            public static readonly string runConfig = "Release";
        #endif

        static string buildDate;
        static int clearYcoords;
        static bool exitedNormally = false;

        static ISocketMessageChannel CurrentChannel;
        static readonly int AutoSaveIntervalInMinutes = 60;
        public static Random RDM { get; private set; } = new Random();
        public static readonly string ExePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\";
        
        // Client 
        static DiscordSocketClient client;
        public static bool ClientReady { get; private set; }
        static bool gotWorkingToken = false;
        
        static readonly string exitlock = "";

        private static SocketUser pmaster;
        public static SocketUser Master
        {
            get { return pmaster; }
            set
            {
                if (pmaster == null)
                    pmaster = value;
                else
                    throw new FieldAccessException("The Master may only be set once!");
            }
        }

        public delegate void ConnectedHandler();
        public static event ConnectedHandler OnConnected;
        public delegate void ExitHandler();
        public static event ExitHandler OnExit;

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
                Saver.SaveToLog("Error Message: " + ex.Message + "\nStack Trace: " + ex.StackTrace);
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

            "youtube-dl -U".RunAsConsoleCommand(360, () => { }, (string o, string e) => { ConsoleWrapper.ConsoleWrite(o + e); });

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
            client.Ready += Client_Ready;

            client.JoinedGuild += Client_JoinedGuild;
            client.MessageReceived += MessageReceived;
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
                        //SystemSounds.Exclamation.Play(); TODO: Support for SystemSounds is planned for .Net Core 3.0
                        ConsoleWrapper.ConsoleWrite("Give me a Bot Token: ");
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
            commands = commands.OrderBy(x => {
                if (x.CommandLine == "edit")
                    return "00000";
                else
                    return x.CommandLine;
            }).ToArray();
        }
        static void SetState()
        {
            if (runConfig == "Debug")
                client.SetGameAsync($"{Prefix}help [DEBUG-MODE]", "", ActivityType.Listening).Wait();
            else
                client.SetGameAsync($"{Prefix}help", "", ActivityType.Listening).Wait();
        }
        static void BuildHelpMenu()
        {
            helpMenu.WithColor(0, 128, 255);
            helpMenu.AddFieldDirectly($"{Prefix}help", $"Prints the HelpMenu for a Command" +
                (commands.Where(x => x.HelpMenu != null).ToList().Count != 0 ?
                $", eg. {Prefix}help {commands.First(x => x.HelpMenu != null).CommandLine}" : "") +
                "\nCommands with a HelpMenu are marked with a (h)");
            for (int i = 0; i < commands.Length; i++)
            {
                if (commands[i].CommandLine != "" && !commands[i].IsHidden)
                {
                    string desc = ((commands[i].Desc == null ? "" : commands[i].Desc + "   ")).Trim(' ');
                    helpMenu.AddFieldDirectly(commands[i].Prefix + commands[i].CommandLine +
                        (commands[i].IsExperimental ? " [EXPERIMENTAL]" : "") + (commands[i].HelpMenu == null ? "" : " (h)"),
                        string.IsNullOrWhiteSpace(desc) ? "-" : desc, true);
                }
            }
            helpMenu.WithDescription($"I was made by {Master.Mention}\nYou can find my source-code " +
                $"[here](https://github.com/niklasCarstensen/Discord-Bot).\n\nCommands:");
            helpMenu.WithFooter($"Running {runConfig} build from {buildDate} on {Environment.OSVersion.VersionString} / " +
                $"{Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName}\n");
            helpMenu.WithThumbnailUrl("https://openclipart.org/image/2400px/svg_to_png/280959/1496637751.png");
        }
        static void PrintConsoleStartup()
        {
            Console.CursorLeft = 0;
            ConsoleWrapper.ConsoleWriteLine("Active on the following Servers: ", ConsoleColor.White);
            try
            {
                foreach (SocketGuild g in client.Guilds)
                {
                    ConsoleWrapper.ConsoleWrite($"  {g.Name}", ConsoleColor.Magenta);
                    ConsoleWrapper.ConsoleWriteLine($"{new string(Enumerable.Repeat(' ', client.Guilds.Max(x => x.Name.Length) - g.Name.Length + 2).ToArray())}{g.Id}",
                        ConsoleColor.White);
                }
            }
            catch { ConsoleWrapper.ConsoleWriteLine("Error Displaying all servers!", ConsoleColor.Red); }
            ConsoleWrapper.ConsoleWrite("Default channel is: ");
            ConsoleWrapper.ConsoleWrite(CurrentChannel, ConsoleColor.Magenta);
            ConsoleWrapper.ConsoleWrite(" on ");
            ConsoleWrapper.ConsoleWriteLine(GetGuildFromChannel(CurrentChannel).Name, ConsoleColor.Magenta);
            ConsoleWrapper.ConsoleWriteLine("Awaiting your commands: ");
            clearYcoords = Console.CursorTop;
        }
        static void CallOnConnected()
        {
            OnConnected.InvokeParallel();
        }
        static void StartAutosaveLoop()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(AutoSaveIntervalInMinutes * 60000);
                    if (Config.UnsavedChanges)
                    {
                        Config.Save();
                        ConsoleWrapper.ConsoleWriteLine($"{DateTime.Now.ToLongTimeString()} Autosaved!", ConsoleColor.Cyan);
                    }
                    else
                    {
                        ConsoleWrapper.ConsoleWriteLine($"{DateTime.Now.ToLongTimeString()} Autosaved! [Nothing to save]", ConsoleColor.Cyan);
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
                        ConsoleWrapper.ConsoleWriteLine("No channel selected!");
                    else if (!string.IsNullOrWhiteSpace(input))
                    {
                        try
                        {
                            DiscordNETWrapper.SendText(input, CurrentChannel).Wait();
                        }
                        catch (Exception e)
                        {
                            ConsoleWrapper.ConsoleWriteLine(e, ConsoleColor.Red);
                        }
                    }
                }
                else if (input.StartsWith("/file "))
                {
                    if (CurrentChannel == null)
                        ConsoleWrapper.ConsoleWriteLine("No channel selected!");
                    else
                    {
                        string[] splits = input.Split(' ');
                        string path = splits.Skip(1).Aggregate((x, y) => x + " " + y);
                        DiscordNETWrapper.SendFile(path.Trim('\"'), CurrentChannel).Wait();
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
                            ConsoleWrapper.ConsoleWriteLine("Succsessfully set new channel!", ConsoleColor.Green);
                            ConsoleWrapper.ConsoleWrite("Current channel is: ");
                            ConsoleWrapper.ConsoleWriteLine(CurrentChannel, ConsoleColor.Magenta);
                        }
                        else
                            ConsoleWrapper.ConsoleWriteLine("Couldn't set new channel!", ConsoleColor.Red);
                    }
                    catch
                    {
                        ConsoleWrapper.ConsoleWriteLine("Couldn't set new channel!", ConsoleColor.Red);
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
                        ConsoleWrapper.ConsoleWriteLine(e, ConsoleColor.Red);
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
                        ConsoleWrapper.ConsoleWriteLine(String.Join("\n", GetGuildFromID(Convert.ToUInt64(split[1])).Roles.Select(x => x.Name)), ConsoleColor.Cyan);
                    }
                    catch (Exception e) { ConsoleWrapper.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/rolePermissions")) // ServerID RoleName
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        ConsoleWrapper.ConsoleWriteLine(GetGuildFromID(Convert.ToUInt64(split[1])).Roles.First(x => x.Name == split[2]).Permissions.ToList().
                            Select(x => x.ToString()).Aggregate((x, y) => x + "\n" + y), ConsoleColor.Cyan);
                    }
                    catch (Exception e) { ConsoleWrapper.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/assignRole")) // ServerID UserID RoleName
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        GetGuildFromID(Convert.ToUInt64(split[1])).GetUser(Convert.ToUInt64(split[2])).
                            AddRoleAsync(GetGuildFromID(Convert.ToUInt64(split[1])).Roles.First(x => x.Name == split[3])).Wait();
                        ConsoleWrapper.ConsoleWriteLine("That worked!", ConsoleColor.Cyan);
                    }
                    catch (Exception e) { ConsoleWrapper.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/channels")) // ChannelID
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        ConsoleWrapper.ConsoleWriteLine(String.Join("\n", GetGuildFromID(Convert.ToUInt64(split[1])).Channels.Select(x => x.Name + "\t" + x.Id + "\t" + x.GetType())), ConsoleColor.Cyan);
                    }
                    catch (Exception e) { ConsoleWrapper.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/read")) // ChannelID
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        var messages = (GetChannelFromID(Convert.ToUInt64(split[1])) as ISocketMessageChannel).GetMessagesAsync(100).FlattenAsync().GetAwaiter().GetResult();
                        ConsoleWrapper.ConsoleWriteLine(String.Join("\n", messages.Reverse().Select(x => x.Author + ": " + x.Content)), ConsoleColor.Cyan);
                    }
                    catch (Exception e) { ConsoleWrapper.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else
                    ConsoleWrapper.ConsoleWriteLine("I dont know that command.", ConsoleColor.Red);
                ConsoleWrapper.ConsoleWrite("$");
            }
        }

        static void BeforeClose()
        {
            lock (exitlock)
            {
                ConsoleWrapper.ConsoleWriteLine("Closing... Files are being saved");
                Config.Save();
                ConsoleWrapper.ConsoleWriteLine("Closing... Command Exit events are being executed");
                try { OnExit(); }
                catch (Exception e) { ConsoleWrapper.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                ConsoleWrapper.ConsoleWriteLine("Closing... Removing Error Emojis");
                DisposeErrorMessages();
                exitedNormally = true;
            }
        }
        // ------------------------------------------------------------------------------------------------------------------
        
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
                Saver.SaveToLog(log.ToString());
                ConsoleWrapper.ConsoleWriteLine(DateTime.Now.ToLongTimeString() + " Long log message has been saved to file.", color);
            }
            else
                ConsoleWrapper.ConsoleWriteLine(msg.ToString(), color);
            return Task.FromResult(default(object));
        }

        // Client Getters / Wrappers
        public static SocketUser GetUserFromId(ulong userId)
        {
            return client.GetUser(userId);
        }
        public static SocketChannel GetChannelFromID(ulong channelID)
        {
            return client.GetChannel(channelID);
        }
        public static SocketGuild GetGuildFromChannel(IChannel channel)
        {
            return ((SocketGuildChannel)channel).Guild;
        }
        public static SocketSelfUser GetSelf()
        {
            return client.CurrentUser;
        }
        public static SocketGuild[] GetGuilds()
        {
            return client.Guilds.ToArray();
        }
        public static SocketGuild GetGuildFromID(ulong guildID)
        {
            return client.GetGuild(guildID);
        }
        public static void SetStatus(UserStatus usa)
        {
            client.SetStatusAsync(usa);
        }
        public static ulong OwnID
        {
            get
            {
                return GetSelf().Id;
            }
        }
        
        // Execute BeforeClose before closing
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
        
        // Imports
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
