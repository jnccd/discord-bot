using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestDiscordBot
{
    public class Program
    {
        ISocketMessageChannel CurrentChannel;
        bool clientReady = false;
        DiscordSocketClient client;
        Type[] commandTypes = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                               from assemblyType in domainAssembly.GetTypes()
                               where assemblyType.IsSubclassOf(typeof(Command))
                               select assemblyType).ToArray();
        Command[] commands;
        int clearYcoords;

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        async Task MainAsync()
        {
            #region startup
            Global.P = this;
            try {
                Console.WriteLine("Build from: " + File.ReadAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\BuildDate.txt").TrimEnd('\n'));
            } catch { }
            client = new DiscordSocketClient();
            client.Log += Log;

            if (config.Default.BotToken.StartsWith("<INSERT BOT TOKEN HERE>"))
            {
                config.Default.BotToken = config.Default.BotToken + "*";
                config.Default.Save();
                MessageBox.Show("Oi! I cant start without a bot token ya cunt.");
                return;
            }
            
            await client.LoginAsync(TokenType.Bot, config.Default.BotToken);
            await client.StartAsync();
            client.MessageReceived += MessageReceived;
            client.Ready += Client_Ready;

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
                    foreach (ulong ChannelID in config.Default.ChannelsWrittenOn)
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
                else if(input == "/test")
                {
                    IEnumerable<IMessage> messages = await CurrentChannel.GetMessagesAsync(int.MaxValue).Flatten();
                    foreach (IMessage m in messages)
                    {
                        if (m.Author.Id == client.CurrentUser.Id)
                            GetHashCode();
                    }
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
            if (message.Content.StartsWith(Global.commandString))
            {
                if (message.Content == Global.commandString + "help")
                {
                    string output = "Current comamnds:\n";
                    for (int i = 0; i < commands.Length; i++)
                        output += (commands[i].command == "" ? "" : Global.commandString) + commands[i].command + 
                            (commands[i].desc == null ? "" : " | " + commands[i].desc) + (commands[i].isExperimental ? " | (EXPERIMENTAL)" : "") + "\n";
                    await Global.SendText(output, message.Channel);
                }
                // Experimental
                else if (message.Channel.Id == 473991188974927884) // Channel ID from my server
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
    }
}
