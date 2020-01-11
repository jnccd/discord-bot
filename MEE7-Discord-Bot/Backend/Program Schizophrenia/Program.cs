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
using Environment = System.Environment;

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
        public static readonly string ExePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar;
        public static bool RunningOnCI { get; private set; }
        public static bool RunningOnLinux { get; private set; }

        public static readonly ulong logChannel = 665219921692852271;
        public static readonly bool logToDiscord = true;
        public static readonly string instanceIdentifier = "" + Environment.OSVersion + Environment.TickCount64 + Environment.CurrentDirectory;
        public static readonly string logStartupMessagePräfix = "new instance who dis?";
        public static readonly string logStartupMessage = logStartupMessagePräfix + " I am here: " + instanceIdentifier;
        
        // Client 
        static DiscordSocketClient client;
        public static bool ClientReady { get; private set; }

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
        static void Main()
        {
            try
            {
                ExecuteBot();
            }
            catch (Exception ex)
            {
                try { Config.Save(); } catch (Exception e) { Console.WriteLine("Config Save Error: " + e); }
                ConsoleWrapper.WriteLine("Error Message: " + ex.Message + "\nStack Trace: " + ex.StackTrace);
                Saver.SaveToLog("Error Message: " + ex.Message + "\nStack Trace: " + ex.StackTrace);

                Environment.Exit(1);
            }
        }

        static void ExecuteBot()
        {
            StartUp();

            if (!RunningOnCI)
                try { HandleConsoleCommandsLoop(); }
                catch (Exception e) { CILimbo(); e.ToString(); }
            else
                CILimbo();

            BeforeClose();
        }

        static void StartUp()
        {
            RunningOnCI = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CI_SERVER"));
            RunningOnLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            if (RunningOnLinux)
                ConsoleWrapper.WriteLine("Linux Environment detected!");

            Directory.SetCurrentDirectory(Path.GetDirectoryName(ExePath));

            if (RunningOnCI)
                ConsoleWrapper.WriteLine("CI Environment detected!");
            else
            {
                Console.Title = "MEE7";
                if (!RunningOnLinux)
                    ShowWindow(GetConsoleWindow(), 2);
                Thread.CurrentThread.Name = "Main";
                Console.ForegroundColor = ConsoleColor.White;
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;

                handler = new ConsoleEventDelegate(ConsoleEventCallback);
                if (!RunningOnLinux)
                    SetConsoleCtrlHandler(handler, true);
            }

            LoadBuildDate();
            UpdateYTDL();

            client = new DiscordSocketClient();
            SetClientEvents();

            Login();

            CreateCommandInstances();

            while (!ClientReady) { Thread.Sleep(20); }

            SetState();
            Master = client.GetUser(300699566041202699);

            BuildHelpMenu();

            CurrentChannel = (ISocketMessageChannel)client.GetChannel(473991188974927884);

            DiscordNETWrapper.SendText(logStartupMessage, (IMessageChannel)GetChannelFromID(logChannel)).Wait();
            Config.Load();

            StartAutosaveLoop();

            CallOnConnected();
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
        static void UpdateYTDL()
        {
            try
            {
                Process ytdlUpdater = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = "youtube-dl.exe",
                        Arguments = "-U",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    }
                };
                ytdlUpdater.Start();
                ytdlUpdater.WaitForExit();
                ytdlUpdater.Dispose();
            }
            catch
            {
                ConsoleWrapper.WriteLine("Couldn't update youtube-dl :C", ConsoleColor.Red);
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
            client.UserJoined += Client_UserJoined;
        }
        static void Login()
        {
            try
            {
                client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("BotToken")).Wait();
                client.StartAsync().Wait();
            }
            catch (Exception e1)
            {
                try
                {
                    client.LoginAsync(TokenType.Bot, Config.Data.BotToken).Wait();
                    client.StartAsync().Wait();
                }
                catch (Exception e2)
                {
                    Console.WriteLine($"Wrong Bot Tokens!\n\n{e1}\n{e2}");
                    Environment.Exit(0);
                }
            }
        }
        static void CreateCommandInstances()
        {
            List<Command> commandsList = new List<Command>();
            for (int i = 0; i < commandTypes.Length; i++)
            {
                try
                {
                    Command commandInstance = (Command)Activator.CreateInstance(commandTypes[i]);
                    if (commandInstance.CommandLine.Contains(" ") || commandInstance.Prefix.Contains(" "))
                        throw new IllegalCommandException($"Commands and Prefixes mustn't contain spaces!\n" +
                            $"On command: \"{commandInstance.Prefix}{commandInstance.CommandLine}\" in {commandInstance}");
                    commandsList.Add(commandInstance);
                }
                catch
                {
                    ConsoleWrapper.WriteLine($"Error on instance creation of {commandTypes[i].Name}!", ConsoleColor.Red);
                }
            }
            commands = commandsList.OrderBy(x => {
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
                $", eg. **{Prefix}help {commands.First(x => x.HelpMenu != null).CommandLine}**" : "") +
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
                        ConsoleWrapper.WriteLine($"{DateTime.Now.ToLongTimeString()} Autosaved!", ConsoleColor.Cyan);
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine($"{DateTime.Now.ToLongTimeString()} Autosaved! [Nothing to save]", ConsoleColor.Cyan);
                    }
                }
            });
        }

        static void HandleConsoleCommandsLoop()
        {
            PrintConsoleStartup();

            while (true)
            {
                string input = "";
                if (RunningOnCI)
                    CILimbo();
                else
                    try { input = Console.ReadLine(); }
                    catch (Exception e) { CILimbo(); e.ToString(); }

                if (input == "exit")
                    break;

                if (!input.StartsWith("/"))
                {
                    if (CurrentChannel == null)
                        ConsoleWrapper.WriteLine("No channel selected!");
                    else if (!string.IsNullOrWhiteSpace(input))
                    {
                        try
                        {
                            DiscordNETWrapper.SendText(input, CurrentChannel).Wait();
                        }
                        catch (Exception e)
                        {
                            ConsoleWrapper.WriteLine(e, ConsoleColor.Red);
                        }
                    }
                }
                else if (input.StartsWith("/file "))
                {
                    if (CurrentChannel == null)
                        ConsoleWrapper.WriteLine("No channel selected!");
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
                            ConsoleWrapper.WriteLine("Succsessfully set new channel!", ConsoleColor.Green);
                            ConsoleWrapper.Write("Current channel is: ");
                            ConsoleWrapper.WriteLine(CurrentChannel, ConsoleColor.Magenta);
                        }
                        else
                            ConsoleWrapper.WriteLine("Couldn't set new channel!", ConsoleColor.Red);
                    }
                    catch
                    {
                        ConsoleWrapper.WriteLine("Couldn't set new channel!", ConsoleColor.Red);
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
                        ConsoleWrapper.WriteLine(e, ConsoleColor.Red);
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
                        ConsoleWrapper.WriteLine(String.Join("\n", GetGuildFromID(Convert.ToUInt64(split[1])).Roles.Select(x => x.Name)), ConsoleColor.Cyan);
                    }
                    catch (Exception e) { ConsoleWrapper.WriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/rolePermissions")) // ServerID RoleName
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        ConsoleWrapper.WriteLine(GetGuildFromID(Convert.ToUInt64(split[1])).Roles.First(x => x.Name == split[2]).Permissions.ToList().
                            Select(x => x.ToString()).Aggregate((x, y) => x + "\n" + y), ConsoleColor.Cyan);
                    }
                    catch (Exception e) { ConsoleWrapper.WriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/assignRole")) // ServerID UserID RoleName
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        GetGuildFromID(Convert.ToUInt64(split[1])).GetUser(Convert.ToUInt64(split[2])).
                            AddRoleAsync(GetGuildFromID(Convert.ToUInt64(split[1])).Roles.First(x => x.Name == split[3])).Wait();
                        ConsoleWrapper.WriteLine("That worked!", ConsoleColor.Cyan);
                    }
                    catch (Exception e) { ConsoleWrapper.WriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/channels")) // ChannelID
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        ConsoleWrapper.WriteLine(String.Join("\n", GetGuildFromID(Convert.ToUInt64(split[1])).Channels.Select(x => x.Name + "\t" + x.Id + "\t" + x.GetType())), ConsoleColor.Cyan);
                    }
                    catch (Exception e) { ConsoleWrapper.WriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/read")) // ChannelID
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        var messages = (GetChannelFromID(Convert.ToUInt64(split[1])) as ISocketMessageChannel).GetMessagesAsync(100).FlattenAsync().GetAwaiter().GetResult();
                        ConsoleWrapper.WriteLine(String.Join("\n", messages.Reverse().Select(x => x.Author + ": " + x.Content)), ConsoleColor.Cyan);
                    }
                    catch (Exception e) { ConsoleWrapper.WriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else
                    ConsoleWrapper.WriteLine("I dont know that command.", ConsoleColor.Red);
                ConsoleWrapper.Write("$");
            }
        }
        static void PrintConsoleStartup()
        {
            lock (ConsoleWrapper.lockject) 
            {
                Console.CursorLeft = 0;
                ConsoleWrapper.WriteLine("Active on the following Servers: ", ConsoleColor.White);
                try
                {
                    foreach (SocketGuild g in client.Guilds)
                    {
                        ConsoleWrapper.Write($"  {g.Name}", ConsoleColor.Magenta);
                        ConsoleWrapper.WriteLine($"{new string(Enumerable.Repeat(' ', client.Guilds.Max(x => x.Name.Length) - g.Name.Length + 2).ToArray())}{g.Id}",
                            ConsoleColor.White);
                    }
                }
                catch { ConsoleWrapper.WriteLine("Error Displaying all servers!", ConsoleColor.Red); }
                ConsoleWrapper.Write("Default channel is: ");
                ConsoleWrapper.Write(CurrentChannel, ConsoleColor.Magenta);
                ConsoleWrapper.Write(" on ");
                ConsoleWrapper.WriteLine(GetGuildFromChannel(CurrentChannel).Name, ConsoleColor.Magenta);
                ConsoleWrapper.WriteLine("Awaiting your commands: ");
                clearYcoords = Console.CursorTop;
            }
        }
        static void CILimbo() 
        {
            while (true) 
            { 
                Thread.Sleep(int.MaxValue); 
            } 
        }

        static void BeforeClose()
        {
            lock (exitlock)
            {
                ConsoleWrapper.WriteLine("Closing... Command Exit events are being executed");
                try { OnExit(); }
                catch (Exception e) { ConsoleWrapper.WriteLine(e.ToString(), ConsoleColor.Red); }
                ConsoleWrapper.WriteLine("Closing... Files are being saved");
                Config.Save();
                ConsoleWrapper.WriteLine("Closing... Removing Error Emojis");
                DisposeErrorMessages();

                ConsoleWrapper.WriteLine("Closing... Logging out");
                client.SetStatusAsync(UserStatus.DoNotDisturb).Wait();
                client.StopAsync().Wait();
                client.LogoutAsync().Wait();

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
                ConsoleWrapper.WriteLine(DateTime.Now.ToLongTimeString() + " Long log message has been saved to file.", color);
            }
            else
                ConsoleWrapper.WriteLine(msg.ToString(), color);

            return Task.FromResult(default(object));
        }

        public static void Exit(int exitCode)
        {
            BeforeClose();
            Environment.Exit(exitCode);
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
        static ConsoleEventDelegate handler;
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
