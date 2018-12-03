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
using TestDiscordBot.XML;

namespace TestDiscordBot
{
    public class IllegalCommandException : Exception { public IllegalCommandException(string message) : base (message) { } }

    public class Program
    {
        int clearYcoords;
        bool clientReady = false;
        bool gotWorkingToken = false;
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
            client.Disconnected += Client_Disconnected;
            
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
#pragma warning disable CS4014 // Da dieser Aufruf nicht abgewartet wird, wird die Ausführung der aktuellen Methode fortgesetzt, bevor der Aufruf abgeschlossen ist
            foreach (Command c in commands)
            {
                Task.Factory.StartNew(() => {
                    try
                    {
                        c.onConnected();
                    }
                    catch (Exception e) { Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                });
            }
#pragma warning restore CS4014 // Da dieser Aufruf nicht abgewartet wird, wird die Ausführung der aktuellen Methode fortgesetzt, bevor der Aufruf abgeschlossen ist
            Console.CursorLeft = 0;
            Console.Write("Default channel is: ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(CurrentChannel);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Awaiting your commands: ");
            clearYcoords = Console.CursorTop;
            ShowWindow(GetConsoleWindow(), 2);
#endregion
            
            #region commands
            while (true)
            {
                Console.Write("$");
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
                    try
                    {
                        // TODO: Insert Testing Code here
                    }
                    catch (Exception e) { Console.WriteLine(e); }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("I dont know that command.");
                    Console.ForegroundColor = ConsoleColor.White;
                }
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

            await client.SetGameAsync("Im actually closed but discord doesnt seem to notice...");
            await client.SetStatusAsync(UserStatus.DoNotDisturb);
            await client.LogoutAsync();
            Environment.Exit(0);
        }

        private async Task Client_JoinedGuild(SocketGuild arg)
        {
            try
            {
                await Global.SendText("Yall gay", arg.DefaultChannel);
            } catch { }
        }
        private async Task Client_Disconnected(Exception arg)
        {

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

                if (char.IsLetter(message.Content[0]))
                {
                    Task.Factory.StartNew(() => {
                        foreach (Command c in commands)
                        {
                            try
                            {
                                c.onNonCommandMessageRecieved(message.Content);
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
            
            if (message.Content == Global.prefix + "help")
            {
                #region post Help
                List<Command> commandsLeft = commands.ToList();
                while (commandsLeft.Count > 0)
                {
                    EmbedBuilder Embed = new EmbedBuilder();
                    Embed.WithColor(0, 128, 255);
                    for (int i = 0; i < 24 && commandsLeft.Count > 0; i++)
                    {
                        if (commandsLeft[0].command != "" && !commandsLeft[0].isHidden)
                        {
                            string desc = ((commandsLeft[0].desc == null ? "" : commandsLeft[0].desc + " ") + (commandsLeft[0].isExperimental ? "(EXPERIMENTAL)" : "")).Trim(' ');
                            Embed.AddField(commandsLeft[0].prefix + commandsLeft[0].command, desc == null || desc == "" ? "-" : desc, true);
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
                Command called = commands.FirstOrDefault(x => (x.prefix + x.command).ToLower() == (message.Content.Split(' ')[0]).ToLower());
                if (called != null)
                {
                    await executeCommand(called, message);
                    return;
                }

                // No command found
                int[] distances = new int[commands.Length];
                for (int i = 0; i < commands.Length; i++)
                    distances[i] = Global.LevenshteinDistance((commands[i].prefix + commands[i].command).ToLower(), (message.Content.Split(' ')[0]).ToLower());
                int minIndex = 0;
                int min = int.MaxValue;
                for (int i = 0; i < commands.Length; i++)
                    if (distances[i] < min)
                    {
                        minIndex = i;
                        min = distances[i];
                    }
                if (min < 5)
                {
                    await Global.SendText("I don't know that command, but " + commands[minIndex].prefix + commands[minIndex].command + " is pretty close:", message.Channel);
                    await executeCommand(commands[minIndex], message);
                    return;
                }
            }
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

                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Send " + command.GetType().Name + " at " + DateTime.Now.ToShortTimeString() + "\tin " + ((SocketGuildChannel)message.Channel).Guild.Name + "\tin " + message.Channel.Name + "\tfor " + message.Author.Username);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("$");
            }
            catch (Exception e)
            {
                await Global.SendText("Uwu We made a fucky wucky!! A wittle fucko boingo! The code monkeys at our headquarters are working VEWY HAWD to fix this!", message.Channel);

                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("$");
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
        
        // Imports
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
