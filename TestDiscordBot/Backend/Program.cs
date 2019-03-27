using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

    public static partial class Extensions
    {
        // String
        public static List<int> AllIndexesOf(this string str, string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("the string to find may not be empty", "value");
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }
        public static bool ContainsOneOf(this string str, string[] tests)
        {
            foreach (string s in tests)
                if (str.Contains(s))
                    return true;
            return false;
        }
        public static bool ContainsAllOf(this string str, string[] tests)
        {
            foreach (string s in tests)
                if (!str.Contains(s))
                    return false;
            return true;
        }
        public static string GetEverythingBetween(this string str, string left, string right)
        {
            int leftIndex = str.IndexOf(left);
            int rightIndex = str.IndexOf(right, leftIndex == -1 ? 0 : leftIndex + 1);

            if (leftIndex == -1 || rightIndex == -1 || leftIndex > rightIndex)
            {
                //throw new Exception("String doesnt contain left or right borders!");
                return "";
            }

            try
            {
                string re = str.Remove(0, leftIndex + left.Length);
                re = re.Remove(rightIndex - leftIndex - left.Length);
                return re;
            }
            catch
            {
                return "";
            }
        }
        public static List<string> GetEverythingBetweenAll(this string str, string left, string right)
        {
            List<string> re = new List<string>();

            int leftIndex = str.IndexOf(left);
            int rightIndex = str.IndexOf(right, leftIndex == -1 ? 0 : leftIndex + 1);

            if (leftIndex == -1 || rightIndex == -1 || leftIndex > rightIndex)
            {
                return re;
            }

            while (leftIndex != -1 && rightIndex != -1)
            {
                try
                {
                    str = str.Remove(0, leftIndex + left.Length);
                    re.Add(str.Remove(rightIndex - leftIndex - left.Length));
                }
                catch { break; }

                leftIndex = str.IndexOf(left);
                rightIndex = str.IndexOf(right, leftIndex == -1 ? 0 : leftIndex + 1);
            }

            return re;
        }
        public static bool StartsWith(this string str, string[] values)
        {
            foreach (string s in values)
                if (str.StartsWith(s))
                    return true;
            return false;
        }
        public static string ContainsPictureLink(this string str)
        {
            string[] split = str.Split(' ');
            foreach (string s in split)
                if (s.StartsWith("https://cdn.discordapp.com/") && s.Contains(".png") ||
                    s.StartsWith("https://cdn.discordapp.com/") && s.Contains(".jpg"))
                    return s;
            return null;
        }
        public static double ConvertToDouble(this string s)
        {
            return Convert.ToDouble(s.Replace('.', ','));
        }
        public static string ToCapital(this string s)
        {
            string o = "";
            for (int i = 0; i < s.Length; i++)
                if (i == 0)
                    o += char.ToUpper(s[i]);
                else
                    o += char.ToLower(s[i]);
            return o;
        }
        public static Bitmap GetBitmapFromURL(this string url)
        {
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            return new Bitmap(responseStream);
        }
        public static int LevenshteinDistance(this string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }
        public static void ConsoleWriteLine(this string text, ConsoleColor Color)
        {
            lock (Console.Title)
            {
                Console.CursorLeft = 0;
                Console.ForegroundColor = Color;
                Console.WriteLine(text);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("$");
            }
        }

        // Discord
        public static EmbedBuilder ToEmbed(this IMessage m)
        {
            EmbedBuilder Embed = new EmbedBuilder();
            Embed.WithColor(0, 128, 255);
            Embed.WithAuthor(m.Author);
            Embed.WithTitle(string.IsNullOrWhiteSpace(m.Content) ?
                m.Attachments.Select(x => x.Url).
                Where(x => !x.EndsWith(".png") && !x.EndsWith(".jpg")).
                Union(new string[] { "-" }).
                Aggregate((x, y) => y == "-" ? x : x + " " + y) : m.Content);
            try
            {
                if (m.Attachments.Count > 0)
                    Embed.WithThumbnailUrl(m.Attachments.ElementAt(0).Url);
            }
            catch { }
            return Embed;
        }
        public static ulong GetServerID(this SocketMessage m)
        {
            return Program.GetGuildFromChannel(m.Channel).Id;
        }
        
        // Linq Extensions
        public static b Foldr<a, b>(this IEnumerable<a> xs, b y, Func<a, b, b> f)
        {
            foreach (a x in xs)
                y = f(x, y);
            return y;
        }
        public static b Foldr<a, b>(this IEnumerable<a> xs, Func<a, b, b> f)
        {
            return xs.Foldr(default(b), f);
        }
        public static a GetRandomValue<a>(this IEnumerable<a> xs)
        {
            a[] arr = xs.ToArray();
            return arr[Program.RDM.Next(arr.Length)];
        }
        public static string RemoveLastGroup(this string s, char seperator)
        {
            string[] split = s.Split(seperator);
            return split.Take(split.Length - 1).Reverse().Foldr("", (a, b) => a + seperator + b);
        }
    }

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
        
        static readonly string lockject = "";

        // ------------------------------------------------------------------------------------------------------------

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
                Program.ConsoleWriteLine("Build from: " + buildDate, ConsoleColor.Magenta);
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
            await client.SetGameAsync("[DEBUG-MODE] Type " + Program.prefix + "help");
#else
            await client.SetGameAsync("Type " + prefix + "help");
#endif
            Master = client.GetUser(300699566041202699);

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
            HelpMenu.WithDescription("I was made by " + Master.Mention + "\nYou can find my source-code [here](https://github.com/niklasCarstensen/Discord-Bot).\n\nCommands:");
            HelpMenu.WithFooter("Current Build from: " + buildDate);
            HelpMenu.WithThumbnailUrl("https://openclipart.org/image/2400px/svg_to_png/280959/1496637751.png");

            // Startup Console Display
            CurrentChannel = (ISocketMessageChannel)client.GetChannel(473991188974927884);
            Console.CursorLeft = 0;
            Program.ConsoleWriteLine("Active on the following Servers: ", ConsoleColor.Yellow);
            try
            {
                foreach (SocketGuild g in client.Guilds)
                    Program.ConsoleWriteLine(g.Name + "\t" + g.Id, ConsoleColor.Yellow);
            }
            catch { Program.ConsoleWriteLine("Error Displaying all servers!", ConsoleColor.Red); }
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
                    catch (Exception e) { Program.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
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
                            await Program.SendText(input, CurrentChannel);
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
                        await Program.SendFile(path.Trim('\"'), CurrentChannel);
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
                    catch (Exception e) { Program.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/roles")) // ServerID
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        Program.ConsoleWriteLine(String.Join("\n", GetGuildFromID(Convert.ToUInt64(split[1])).Roles.Select(x => x.Name)), ConsoleColor.Cyan);
                    }
                    catch (Exception e) { Program.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/rolePermissions")) // ServerID RoleName
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        Program.ConsoleWriteLine(GetGuildFromID(Convert.ToUInt64(split[1])).Roles.First(x => x.Name == split[2]).Permissions.ToList().
                            Select(x => x.ToString()).Aggregate((x, y) => x + "\n" + y), ConsoleColor.Cyan);
                    }
                    catch (Exception e) { Program.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/assignRole")) // ServerID UserID RoleName
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        await GetGuildFromID(Convert.ToUInt64(split[1])).GetUser(Convert.ToUInt64(split[2])).
                                AddRoleAsync(GetGuildFromID(Convert.ToUInt64(split[1])).Roles.First(x => x.Name == split[3]));
                        Program.ConsoleWriteLine("That worked!", ConsoleColor.Cyan);
                    }
                    catch (Exception e) { Program.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/channels")) // ChannelID
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        Program.ConsoleWriteLine(String.Join("\n", GetGuildFromID(Convert.ToUInt64(split[1])).Channels.Select(x => x.Name + "\t" + x.Id + "\t" + x.GetType())), ConsoleColor.Cyan);
                    }
                    catch (Exception e) { Program.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else if (input.StartsWith("/read")) // ChannelID
                {
                    string[] split = input.Split(' ');
                    try
                    {
                        var messages = (GetChannelFromID(Convert.ToUInt64(split[1])) as ISocketMessageChannel).GetMessagesAsync(100).Flatten().GetAwaiter().GetResult();
                        Program.ConsoleWriteLine(String.Join("\n", messages.Reverse().Select(x => x.Author + ": " + x.Content)), ConsoleColor.Cyan);
                    }
                    catch (Exception e) { Program.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
                else
                    Program.ConsoleWriteLine("I dont know that command.", ConsoleColor.Red);
            }
            #endregion

            foreach (Command c in commands)
            {
                try
                {
                    c.OnExit();
                }
                catch (Exception e) { Program.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
            }
            Config.Config.Save();
            exitedNormally = true;

            await client.SetGameAsync("Im actually closed but discord doesnt seem to notice...");
            await client.SetStatusAsync(UserStatus.DoNotDisturb);
            await client.LogoutAsync();
            Environment.Exit(0);
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
            catch (Exception e) { Program.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
        }
        private static Task Client_Ready()
        {
            ClientReady = true;
            return Task.FromResult(0);
        }
        private static Task Client_Log(LogMessage msg)
        {
            Program.ConsoleWriteLine(msg.ToString(), ConsoleColor.White);
            return Task.FromResult(0);
        }
        private static Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            Task.Run(async () => {
                Tuple<RestUserMessage, Exception> error = CachedErrorMessages.FirstOrDefault(x => x.Item1.Id == arg1.Id);
                if (error != null)
                {
                    var reacts = (await arg1.GetOrDownloadAsync()).Reactions;
                    reacts.TryGetValue(ErrorEmoji, out var react);
                    if (react.ReactionCount > 1)
                        await error.Item1.ModifyAsync(m => m.Content = ErrorMessage + "\n\n```" + error.Item2 + "```");
                    else
                        await error.Item1.ModifyAsync(m => m.Content = ErrorMessage);
                }
            });
            Task.Run(() => {
                foreach (Command c in commands)
                {
                    try
                    {
                        c.OnEmojiReactionAdded(arg1, arg2, arg3);
                    }
                    catch (Exception e) { Program.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
            });
            return Task.FromResult(default(object));
        }
        private static Task Client_ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            Task.Run(async () => {
                Tuple<RestUserMessage, Exception> error = CachedErrorMessages.FirstOrDefault(x => x.Item1.Id == arg1.Id);
                if (error != null)
                {
                    var reacts = (await arg1.GetOrDownloadAsync()).Reactions;
                    reacts.TryGetValue(ErrorEmoji, out var react);
                    if (react.ReactionCount > 1)
                        await error.Item1.ModifyAsync(m => m.Content = ErrorMessage + "\n\n```" + error.Item2 + "```");
                    else
                        await error.Item1.ModifyAsync(m => m.Content = ErrorMessage);
                }
            });
            Task.Run(() => {
                foreach (Command c in commands)
                {
                    try
                    {
                        c.OnEmojiReactionRemoved(arg1, arg2, arg3);
                    }
                    catch (Exception e) { Program.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                }
            });
            return Task.FromResult(default(object));
        }
        private static Task MessageReceived(SocketMessage message)
        {
            if (!message.Author.IsBot)
            {
                if (message.Content.StartsWith(Program.prefix))
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
                            catch (Exception e) { Program.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
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

            if (message.Content == Program.prefix + "help")
            {
                await Program.SendEmbed(HelpMenu, message.Channel);
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
                            distances[i] = Program.LevenshteinDistance((commands[i].Prefix + commands[i].CommandLine).ToLower(), split[0].ToLower());
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
                        await Program.SendText("I don't know that command, but " + commands[minIndex].Prefix + commands[minIndex].CommandLine + " is pretty close:", message.Channel);
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
            if (command.GetType() == typeof(Template) && !ExperimentalChannels.Contains(message.Channel.Id))
                return;
            if (command.IsExperimental && !ExperimentalChannels.Contains(message.Channel.Id))
            {
                await Program.SendText("Experimental commands cant be used here!", message.Channel);
                return;
            }

            IDisposable typingState = null;
            try
            {
                typingState = message.Channel.EnterTypingState();
                lock (lockject)
                {
                    ConcurrentCommandExecutions++;
                    UpdateWorkState();
                }

                Program.SaveUser(message.Author.Id);
                await command.Execute(message);

                if (message.Channel is SocketGuildChannel)
                    Program.ConsoleWriteLine("Send " + command.GetType().Name + " at " + DateTime.Now.ToShortTimeString() + "\tin " + 
                        ((SocketGuildChannel)message.Channel).Guild.Name + "\tin " + message.Channel.Name + "\tfor " + message.Author.Username, ConsoleColor.Green);
                else
                    Program.ConsoleWriteLine("Send " + command.GetType().Name + " at " + DateTime.Now.ToShortTimeString() + "\tin " +
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
                
                Program.ConsoleWriteLine(e.ToString(), ConsoleColor.Red);
            }
            finally
            {
                typingState.Dispose();
                lock (lockject)
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

        // Send Wrappers
        public static async Task SendFile(string path, ISocketMessageChannel Channel, string text = "")
        {
            await Channel.SendFileAsync(path, text);
            SaveChannel(Channel);
        }
        public static async Task SendFile(Stream stream, ISocketMessageChannel Channel, string fileEnd, string fileName = "", string text = "")
        {
            if (fileName == "")
                fileName = DateTime.Now.ToBinary().ToString();

            stream.Position = 0;
            await Channel.SendFileAsync(stream, fileName + "." + fileEnd, text);
            SaveChannel(Channel);
        }
        public static async Task SendBitmap(Bitmap bmp, ISocketMessageChannel Channel, string text = "")
        {
            MemoryStream stream = new MemoryStream();
            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            await SendFile(stream, Channel, "png", "", text);
        }
        public static async Task SendText(string text, ISocketMessageChannel Channel)
        {
            if (text.Length < 2000)
                await Channel.SendMessageAsync(text);
            else
            {
                while (text.Length > 0)
                {
                    int subLength = Math.Min(1999, text.Length);
                    string sub = text.Substring(0, subLength);
                    await Channel.SendMessageAsync(sub);
                    text = text.Remove(0, subLength);
                }
            }
            SaveChannel(Channel);
        }
        public static async Task SendText(string text, ulong ChannelID)
        {
            await SendText(text, (ISocketMessageChannel)Program.GetChannelFromID(ChannelID));
        }
        public static async Task SendEmbed(EmbedBuilder Embed, ISocketMessageChannel Channel)
        {
            if (Embed.Fields.Count < 25)
                await Channel.SendMessageAsync("", false, Embed.Build());
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
                    await Channel.SendMessageAsync("", false, eb.Build());
                }
            }
            SaveChannel(Channel);
        }

        // Save
        public static void SaveChannel(ISocketMessageChannel Channel)
        {
            if (Config.Config.Data.ChannelsWrittenOn == null)
                Config.Config.Data.ChannelsWrittenOn = new List<ulong>();
            if (!Config.Config.Data.ChannelsWrittenOn.Contains(Channel.Id))
            {
                Config.Config.Data.ChannelsWrittenOn.Add(Channel.Id);
                Config.Config.Save();
            }
        }
        public static void SaveUser(ulong UserID)
        {
            if (!Config.Config.Data.UserList.Exists(x => x.UserID == UserID))
                Config.Config.Data.UserList.Add(new DiscordUser(UserID));
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
