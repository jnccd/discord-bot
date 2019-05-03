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
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MEE7.Commands;
using System.Globalization;
using TwitchLib.Client.Models;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using MEE7.Configuration;

namespace MEE7
{
    public class IllegalCommandException : Exception { public IllegalCommandException(string message) : base (message) { } }
    
    public class Program
    {
        // Console / Execution
        static int clearYcoords;
        static bool exitedNormally = false;
        static string buildDate;
        static int ConcurrentCommandExecutions = 0;
        public static Random RDM { get; private set; } = new Random();

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

        // ------------------------------------------------------------------------------------------------------------

        static void Main(string[] args)
        {
            try
            {
                ExecuteBot();
            }
            catch (Exception ex)
            {
                try { Config.Save(); } catch { }

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
            Thread.CurrentThread.Name = "Main";
            Console.Title = "MEE7";
            ShowWindow(GetConsoleWindow(), 2);
            Console.ForegroundColor = ConsoleColor.White;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.Idle;

            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            try
            {
                buildDate = File.ReadAllText("BuildDate.txt").TrimEnd('\n');
                ConsoleWriteLine("Build from: " + buildDate, ConsoleColor.Magenta);
            }
            catch
            {
                if (buildDate == null)
                    buildDate = "Error: Couldn't read build date!";
            }
            client = new DiscordSocketClient();
            client.Log += Client_Log;
            client.JoinedGuild += Client_JoinedGuild;

            while (!gotWorkingToken)
            {
                try
                {
                    if (Config.Data.BotToken == "<INSERT BOT TOKEN HERE>")
                    {
                        ShowWindow(GetConsoleWindow(), 4);
                        SystemSounds.Exclamation.Play();
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

            client.MessageReceived += MessageReceived;
            client.Ready += Client_Ready;
            client.ReactionAdded += Client_ReactionAdded;
            client.ReactionRemoved += Client_ReactionRemoved;

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
            client.SetGameAsync($"{prefix}help [DEBUG-MODE]", "", ActivityType.Listening).Wait();
#else
            client.SetGameAsync($"{prefix}help", "", ActivityType.Listening).Wait();
#endif
            Master = client.GetUser(300699566041202699);

            // Build HelpMenu
            HelpMenu.WithColor(0, 128, 255);
            HelpMenu.AddField($"{prefix}help", $"Prints the HelpMenu for a Command" +
                (commands.Where(x => x.HelpMenu != null).ToList().Count != 0 ?
                $", eg. {prefix}help {commands.First(x => x.HelpMenu != null).CommandLine}" : "") +
                "\nCommands with a HelpMenu are marked with a (h)", true);
            for (int i = 0; i < commands.Length; i++)
            {
                if (commands[i].CommandLine != "" && !commands[i].IsHidden)
                {
                    string desc = ((commands[i].Desc == null ? "" : commands[i].Desc + "   ")).Trim(' ');
                    HelpMenu.AddField(commands[i].Prefix + commands[i].CommandLine +
                        (commands[i].IsExperimental ? " [EXPERIMENTAL]" : "") + (commands[i].HelpMenu == null ? "" : " (h)"),
                        string.IsNullOrWhiteSpace(desc) ? "-" : desc, true);
                }
            }
            HelpMenu.WithDescription("I was made by " + Master.Mention + "\nYou can find my source-code [here](https://github.com/niklasCarstensen/Discord-Bot).\n\nCommands:");
            HelpMenu.WithFooter("Current Build from: " + buildDate);
            HelpMenu.WithThumbnailUrl("https://openclipart.org/image/2400px/svg_to_png/280959/1496637751.png");

            // Startup Console Display
            CurrentChannel = (ISocketMessageChannel)client.GetChannel(473991188974927884);
            Console.CursorLeft = 0;
            ConsoleWriteLine("Active on the following Servers: ", ConsoleColor.Yellow);
            try
            {
                foreach (SocketGuild g in client.Guilds)
                    ConsoleWriteLine(g.Name + "\t" + g.Id, ConsoleColor.Yellow);
            }
            catch { ConsoleWriteLine("Error Displaying all servers!", ConsoleColor.Red); }
            ConsoleWrite("Default channel is: ");
            ConsoleWrite(CurrentChannel, ConsoleColor.Magenta);
            ConsoleWriteLine(" on " + GetGuildFromChannel(CurrentChannel).Name);
            ConsoleWriteLine("Awaiting your commands: ");
            clearYcoords = Console.CursorTop;
            foreach (Command c in commands)
            {
                Task.Run(() => {
                    try
                    {
                        c.OnConnected();
                    }
                    catch (Exception e) { ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                });
            }
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
                    else
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
                            M = ((ISocketMessageChannel)GetChannelFromID(Config.Data.ChannelsWrittenOn[i])).GetMessageAsync(Convert.ToUInt64(splits[1])).GetAwaiter().GetResult();

                            if (M != null)
                            {
                                try
                                {
                                    M.DeleteAsync().Wait();
                                    DeletionComplete = true;
                                }
                                catch { }
                            }
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
                else if (input == "/test")
                {
                    Task.Factory.StartNew(() =>
                    {
                        Thread.CurrentThread.Name = "TestThread";
                        try { Test(); }
                        catch (Exception e) { ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                        ConsoleWrite("$");
                    });
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
            }
        }
        static void Test()
        {
            // TODO: Test

            var client = new TwitchClient();
            client.Initialize(new ConnectionCredentials(Config.Data.TwtichBotUsername, Config.Data.TwtichAccessToken));
            client.OnLog += (object o, OnLogArgs arg) => { ConsoleWriteLine($"{arg.BotUsername} - {arg.DateTime}: {arg.Data}", ConsoleColor.Magenta); };
            client.OnMessageReceived += (object sender, OnMessageReceivedArgs e) => {
                ConsoleWriteLine($"Message: {e.ChatMessage}", ConsoleColor.Magenta);
                if (e.ChatMessage.Message.StartsWith("hi"))
                    client.SendMessage(e.ChatMessage.Channel, "Hello there");
            };
            client.OnFailureToReceiveJoinConfirmation += (object sender, OnFailureToReceiveJoinConfirmationArgs e) => { ConsoleWriteLine($"Exception: {e.Exception}\n{e.Exception.Details}", ConsoleColor.Magenta); };
            client.OnJoinedChannel += (object sender, OnJoinedChannelArgs e) => { ConsoleWriteLine($"{e.BotUsername} - joined {e.Channel}", ConsoleColor.Magenta); };
            client.OnConnectionError += (object sender, OnConnectionErrorArgs e) => { ConsoleWriteLine($"Error: {e.Error}", ConsoleColor.Magenta); };
            client.Connect();

            client.JoinChannel(Config.Data.TwtichChannelName);

            Task.Factory.StartNew(() => { Thread.Sleep(15000); client.Disconnect(); ConsoleWriteLine("Disconnected Twitch"); });

            //var api = new TwitchAPI();
            //api.Settings.ClientId = "3o7o8q658z2wy8dt61klsk3ycjlal7";
            //api.Settings.AccessToken = "xwvigbxrqz48uiq3i0zrli2wqfodku";
            //var res = api.Helix.Users.GetUsersFollowsAsync("42111676").Result;

            

            //string url = "https://mdb.ps.informatik.uni-kiel.de/show.cgi?Category/show/Category91";
            //HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            //req.KeepAlive = false;
            //req.AllowAutoRedirect = true;
            //req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0";
            //WebResponse W = req.GetResponse();
            //using (StreamReader sr = new StreamReader(W.GetResponseStream()))
            //{
            //    string html = sr.ReadToEnd();
            //    foreach (string s in html.GetEverythingBetweenAll("class=\"btn btn-link\"><span class=\"type_string\">", ":"))
            //        GetGuildFromID(479950092938248193).CreateTextChannelAsync(s, (TextChannelProperties t) => { t.CategoryId = 562233500963438603; }).Wait();
            //}
            
            //string videoPath = Directory.GetCurrentDirectory() + "\\" + DownloadVideoFromYouTube("https://www.youtube.com/watch?v=Y15Pkxk99h0");
            //ISocketAudioChannel channel = GetChannelFromID(479951814217826305) as ISocketAudioChannel;
            //IAudioClient client = channel.ConnectAsync().Result;
            //SendAudioAsync(client, videoPath).Wait();
            //channel.DisconnectAsync().Wait();
        }
        static void BeforeClose()
        {
            lock (exitlock)
            {
                ConsoleWriteLine("Closing... Files are being saved");
                Config.Save();
                ConsoleWriteLine("Closing... Command Exit events are being executed");
                foreach (Command c in commands)
                {
                    try
                    {
                        c.OnExit();
                    }
                    catch (Exception e) { ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
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
            catch (Exception e) { ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
        }
        private static Task Client_Ready()
        {
            ClientReady = true;
            return Task.FromResult(0);
        }
        private static Task Client_Log(LogMessage msg)
        {
            ConsoleWriteLine(msg.ToString(), ConsoleColor.White);
            return Task.FromResult(0);
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
                foreach (Command c in commands)
                    Task.Run(() => {
                        try
                        {
                            c.OnEmojiReactionAdded(arg1, arg2, arg3);
                            c.OnEmojiReactionUpdated(arg1, arg2, arg3);
                        }
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
                foreach (Command c in commands)
                    Task.Run(() => {
                        try
                        {
                            c.OnEmojiReactionRemoved(arg1, arg2, arg3);
                            c.OnEmojiReactionUpdated(arg1, arg2, arg3);
                        }
                        catch (Exception e) { ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                    });

            return Task.FromResult(default(object));
        }
        private static Task MessageReceived(SocketMessage message)
        {
            if (!message.Author.IsBot)
            {
                if (message.Content.StartsWith(Program.prefix))
                    Task.Run(() => ParallelMessageReceived(message));

                if (char.IsLetter(message.Content[0]) || message.Content[0] == '<' || message.Content[0] == ':')
                {
                    Task.Run(() => {
                        foreach (Command c in commands)
                        {
                            try
                            {
                                c.OnNonCommandMessageRecieved(message);
                            }
                            catch (Exception e) { ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                        }
                    });
                }
            }
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
                    ExecuteCommand(called, message).Wait();
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
                        ExecuteCommand(commands[minIndex], message).Wait();
                    }
                }
            }

            DiscordUser user = Config.Data.UserList.FirstOrDefault(x => x.UserID == message.Author.Id);
            if (user != null)
                user.TotalCommandsUsed++;
        }
        private static async Task ExecuteCommand(Command command, SocketMessage message)
        {
            if (command.GetType() == typeof(Template) && !ExperimentalChannels.Contains(message.Channel.Id))
                return;
            if (command.IsExperimental && !ExperimentalChannels.Contains(message.Channel.Id))
            {
                await SendText("Experimental commands cant be used here!", message.Channel);
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
                await command.Execute(message);

                if (message.Channel is SocketGuildChannel)
                    ConsoleWriteLine("Send " + command.GetType().Name + " at " + DateTime.Now.ToShortTimeString() + "\tin " + 
                        ((SocketGuildChannel)message.Channel).Guild.Name + "\tin " + message.Channel.Name + "\tfor " + message.Author.Username, ConsoleColor.Green);
                else
                    ConsoleWriteLine("Send " + command.GetType().Name + " at " + DateTime.Now.ToShortTimeString() + "\tin " +
                        "DMs\tin " + message.Channel.Name + "\tfor " + message.Author.Username, ConsoleColor.Green);
            }
            catch (Exception e)
            {
                try // Try in case I dont have the permissions to write at all
                {
                    RestUserMessage m = await message.Channel.SendMessageAsync(ErrorMessage);

                    await m.AddReactionAsync(ErrorEmoji);
                    CachedErrorMessages.Add(new Tuple<RestUserMessage, Exception>(m, e));
                }
                catch { }
                
                ConsoleWriteLine(e.ToString(), ConsoleColor.Red);
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

                Process P = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "youtube-dl.exe",
                        Arguments = "-f mp4 -o \"" + videofile + "\" " + YoutubeURL,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true
                    }
                };

                P.Start();
                P.StandardInput.Write("e");
                P.WaitForExit();

                return videofile;
            }
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
            return await Channel.SendFileAsync(stream, fileName + "." + fileEnd, text);
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

            List<IUserMessage> sendMessages = new List<IUserMessage>();
            if (Embed.Fields == null || Embed.Fields.Count < 25)
                sendMessages.Add(await Channel.SendMessageAsync(text, false, Embed.Build()));
            else
            {
                while (Embed.Fields.Count > 0)
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
                        Url = Embed.Title
                    };
                    eb.Url = Embed.Url;
                    for (int i = 0; i < 25 && Embed.Fields.Count > 0; i++)
                    {
                        eb.Fields.Add(Embed.Fields[0]);
                        Embed.Fields.RemoveAt(0);
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
        static void ConsoleWriteLine(object text)
        {
            lock (Console.Title)
            {
                if (Console.CursorLeft == 1)
                    Console.CursorLeft = 0;
                Console.WriteLine(text);
                Console.Write("$");
            }
        }
        static void ConsoleWrite(object text, ConsoleColor Color)
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
        static void ConsoleWrite(object text)
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
