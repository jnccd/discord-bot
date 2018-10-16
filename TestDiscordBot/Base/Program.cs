using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestDiscordBot.Commands;

namespace TestDiscordBot
{
    public class Program
    {
        int clearYcoords;
        bool clientReady = false;
        bool gotWorkingToken = false;
        ulong[] ExperimentalChannels = new ulong[] { 473991188974927884 };
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
            try
            {
                Console.WriteLine("Build from: " + File.ReadAllText(Global.CurrentExecutablePath + "\\BuildDate.txt").TrimEnd('\n'));
            }
            catch { }
            client = new DiscordSocketClient();
            client.Log += Log;
            client.JoinedGuild += Client_JoinedGuild;
            
            while (!gotWorkingToken)
            {
                try
                {
                    if (config.Data.BotToken == "<INSERT BOT TOKEN HERE>")
                    {
                        stringDialog dialog = new stringDialog("Gimme a Bot Token!", "");
                        dialog.ShowDialog();
                        config.Data.BotToken = dialog.result;
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
                commands[i] = (Command)Activator.CreateInstance(commandTypes[i]);
            
            while (!clientReady) { Thread.Sleep(20); }

            await client.SetGameAsync("Type " + Global.commandString + "help");
            CurrentChannel = (ISocketMessageChannel)client.GetChannel(473991188974927884);
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

                        var M = await CurrentChannel.GetMessageAsync(Convert.ToUInt64(splits[1]));
                        await M.DeleteAsync();
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
                else if (input == "/test")
                {
                    
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("I dont know that command.");
                    Console.ForegroundColor = ConsoleColor.White;
                }

            }
            #endregion

            await client.SetGameAsync("Im actually closed but discord doesnt seem to notice...");
            await client.LogoutAsync();
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
            Console.WriteLine("Disconeect");
            if (arg.Message.Contains("Server missed last heartbeat"))
            {
                try
                {
                    await client.LogoutAsync();
                } catch { }
                await client.LoginAsync(TokenType.Bot, config.Data.BotToken);
                await client.StartAsync();
            }
        }
        private Task Client_Ready()
        {
            clientReady = true;
            return Task.FromResult(0);
        }
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.FromResult(0);
        }
        private async Task MessageReceived(SocketMessage message)
        {
            if (message.Content.StartsWith(Global.commandString) && !message.Author.IsBot)
            {
                if (message.Content == Global.commandString + "help")
                {
                    List<Command> commandsLeft = commands.ToList();

                    while (commandsLeft.Count > 0)
                    {
                        EmbedBuilder Embed = new EmbedBuilder();
                        Embed.WithColor(0, 128, 255);
                        for (int i = 0; i < 24 && i < commandsLeft.Count; i++)
                        {
                            if (commandsLeft[i].command != "")
                            {
                                string desc = ((commandsLeft[i].desc == null ? "" : commandsLeft[i].desc + " ") + (commandsLeft[i].isExperimental ? "(EXPERIMENTAL)" : "")).Trim(' ');
                                Embed.AddField(Global.commandString + commandsLeft[i].command, desc == null || desc == "" ? "-" : desc);
                            }
                            commandsLeft.RemoveAt(i);
                            i--;
                        }
                        Embed.WithDescription("Made by <@300699566041202699>\n\nCommands:");
                        Embed.WithFooter("Commands flagged as \"(EXPERIMENTAL)\" can only be used on channels approved by the dev!");
                        await Global.SendEmbed(Embed, message.Channel);
                    }
                }
                // Experimental
                else if (ExperimentalChannels.Contains(message.Channel.Id)) // Channel ID from my server
                {
                    for (int i = 0; i < commands.Length; i++)
                        if (Global.commandString + commands[i].command == message.Content.Split(' ')[0])
                            try { await commands[i].execute(message); } catch {  }
                }
                // Default Channels
                else
                {
                    for (int i = 0; i < commands.Length; i++)
                        if (Global.commandString + commands[i].command == message.Content.Split(' ')[0])
                        {
                            if (commands[i].isExperimental)
                                await Global.SendText("Experimental commands cant be used here!", message.Channel);
                            else
                                await commands[i].execute(message);
                        }
                }
            }
        }

        public SocketChannel getChannelFromID(ulong ChannelID)
        {
            return client.GetChannel(ChannelID);
        }
        public SocketSelfUser getSelf()
        {
            return client.CurrentUser;
        }

        // Imports
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
