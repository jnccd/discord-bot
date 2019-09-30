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
    public static partial class Program
    {
        public const string Prefix = "$";
        static Command[] commands;
        static EmbedBuilder helpMenu = new EmbedBuilder();
        static Type[] commandTypes = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                                      from assemblyType in domainAssembly.GetTypes()
                                      where assemblyType.IsSubclassOf(typeof(Command))
                                      select assemblyType).ToArray();

        static int concurrentCommandExecutions = 0;
        static readonly string commandExecutionLock = "";
        static ulong[] experimentalChannels = new ulong[] { 473991188974927884 };
        static List<DiscordUser> usersWithRunningCommands = new List<DiscordUser>();
        
        public delegate void NonCommandMessageRecievedHandler(SocketMessage message);
        public static event NonCommandMessageRecievedHandler OnNonCommandMessageRecieved;
        public delegate void EmojiReactionAddedHandler(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3);
        public static event EmojiReactionAddedHandler OnEmojiReactionAdded;
        public delegate void EmojiReactionRemovedHandler(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3);
        public static event EmojiReactionRemovedHandler OnEmojiReactionRemoved;
        public delegate void EmojiReactionUpdatedHandler(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3);
        public static event EmojiReactionUpdatedHandler OnEmojiReactionUpdated;
        public delegate void UserJoinedHandler(SocketGuildUser arg);
        public static event UserJoinedHandler OnUserJoined;

        static List<Tuple<RestUserMessage, Exception>> cachedErrorMessages = new List<Tuple<RestUserMessage, Exception>>();
        static readonly string errorMessage = "Uwu We made a fucky wucky!! A wittle fucko boingo! " +
            "The code monkeys at our headquarters are working VEWY HAWD to fix this!";
        static readonly Emoji errorEmoji = new Emoji("🤔");
        
        static void UpdateWorkState()
        {
            if (concurrentCommandExecutions > 1)
                Program.SetStatus(UserStatus.DoNotDisturb);
            else if (concurrentCommandExecutions > 0)
                Program.SetStatus(UserStatus.AFK);
            else
                Program.SetStatus(UserStatus.Online);
        }
        public static void DisposeErrorMessages()
        {
            foreach (Tuple<RestUserMessage, Exception> err in cachedErrorMessages)
            {
                err.Item1.RemoveAllReactionsAsync().Wait();
                err.Item1.ModifyAsync(m => m.Content = errorMessage).Wait();
            }
        }

        // Events
        private static async Task Client_JoinedGuild(SocketGuild arg)
        {
            try
            {
                bool hasWrite = false, hasRead = false, hasReadHistory = false, hasFiles = false;
                SocketGuild g = Program.GetGuildFromID(479950092938248193);
                IUser u = g.Users.FirstOrDefault(x => x.Id == Program.GetSelf().Id);
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
            catch (Exception e) { ConsoleWrapper.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
        }
        private static Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            Task.Run(async () =>
            {
                Tuple<RestUserMessage, Exception> error = cachedErrorMessages.FirstOrDefault(x => x.Item1.Id == arg1.Id);
                if (error != null)
                {
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                    var reacts = (await arg1.GetOrDownloadAsync()).Reactions;
                    reacts.TryGetValue(errorEmoji, out var react);
                    if (react.ReactionCount > 1)
                        await error.Item1.ModifyAsync(m => m.Content = errorMessage + "\n\n```" + error.Item2 + "```");
                    else
                        await error.Item1.ModifyAsync(m => m.Content = errorMessage);
                }
            });
            if (arg3.UserId != OwnID)
                Task.Run(() =>
                {
                    try
                    {
                        OnEmojiReactionAdded?.InvokeParallel(arg1, arg2, arg3);
                        OnEmojiReactionUpdated?.InvokeParallel(arg1, arg2, arg3);
                    }
                    catch (Exception e) { ConsoleWrapper.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                });

            return Task.FromResult(default(object));
        }
        private static Task Client_ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            Task.Run(async () =>
            {
                Tuple<RestUserMessage, Exception> error = cachedErrorMessages.FirstOrDefault(x => x.Item1.Id == arg1.Id);
                if (error != null)
                {
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                    var reacts = (await arg1.GetOrDownloadAsync()).Reactions;
                    reacts.TryGetValue(errorEmoji, out var react);
                    if (react.ReactionCount > 1)
                        await error.Item1.ModifyAsync(m => m.Content = errorMessage + "\n\n```" + error.Item2 + "```");
                    else
                        await error.Item1.ModifyAsync(m => m.Content = errorMessage);
                }
            });
            if (arg3.UserId != OwnID)
                Task.Run(() =>
                {
                    try
                    {
                        OnEmojiReactionRemoved?.InvokeParallel(arg1, arg2, arg3);
                        OnEmojiReactionUpdated?.InvokeParallel(arg1, arg2, arg3);
                    }
                    catch (Exception e) { ConsoleWrapper.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                });

            return Task.FromResult(default(object));
        }
        private static Task Client_UserJoined(SocketGuildUser arg)
        {
            OnUserJoined?.InvokeParallel(arg);
            return Task.FromResult(default(object));
        }
        private static Task MessageReceived(SocketMessage message)
        {
            if (!message.Author.IsBot && message.Content.StartsWith(Prefix))
                Task.Run(() => ParallelMessageReceived(message));
            if (message.Content.Length > 0 && (char.IsLetter(message.Content[0]) || message.Content[0] == '<' || message.Content[0] == ':'))
                Task.Run(() =>
                {
                    try { OnNonCommandMessageRecieved.InvokeParallel(message); }
                    catch (Exception e) { ConsoleWrapper.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
                });
            return Task.FromResult(default(object));
        }
        private static void ParallelMessageReceived(SocketMessage message)
        {
            if (message.Channel is SocketGuildChannel)
                Saver.SaveServer(message.GetServerID());
            DiscordUser user = Saver.SaveUser(message.Author.Id);
            user.TotalCommandsUsed++;

            if (message.Content.StartsWith(Prefix + "help"))
            {
                string[] split = message.Content.Split(' ');
                if (split.Length < 2)
                    DiscordNETWrapper.SendEmbed(helpMenu, message.Channel).Wait();
                else
                {
                    foreach (Command c in commands)
                        if (c.CommandLine == split[1])
                        {
                            DiscordNETWrapper.SendEmbed(c.HelpMenu, message.Channel).Wait();
                            return;
                        }
                    DiscordNETWrapper.SendText("That command doesn't implement a HelpMenu", message.Channel).Wait();
                }
            }
            else
            {
                if (!usersWithRunningCommands.Contains(user))
                {
                    usersWithRunningCommands.Add(user);

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
                                distances[i] = StringExtensions.ModifiedLevenshteinDistance((commands[i].Prefix + commands[i].CommandLine).ToLower(), split[0].ToLower());
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
                            DiscordNETWrapper.SendText("I don't know that command, but " + commands[minIndex].Prefix + commands[minIndex].CommandLine + " is pretty close:", message.Channel).Wait();
                            ExecuteCommand(commands[minIndex], message);
                        }
                    }

                    usersWithRunningCommands.RemoveAll(x => x.UserID == user.UserID);
                }
                else
                    DiscordNETWrapper.SendText("You are already executing a command, wait for the current one to finish.", message.Channel).Wait();
            }
        }
        private static void ExecuteCommand(Command command, SocketMessage message)
        {
            if (command.GetType() == typeof(Template) && !experimentalChannels.Contains(message.Channel.Id))
                return;
            if (command.IsExperimental && !experimentalChannels.Contains(message.Channel.Id))
            {
                DiscordNETWrapper.SendText("Experimental commands cant be used here!", message.Channel).Wait();
                return;
            }

            IDisposable typingState = null;
            try
            {
                typingState = message.Channel.EnterTypingState();
                lock (commandExecutionLock)
                {
                    concurrentCommandExecutions++;
                    UpdateWorkState();
                }

                Saver.SaveUser(message.Author.Id);
                command.Execute(message);

                if (message.Channel is SocketGuildChannel)
                    ConsoleWrapper.ConsoleWriteLine($"{DateTime.Now.ToLongTimeString()} Send {command.GetType().Name}\tin " +
                        $"{((SocketGuildChannel)message.Channel).Guild.Name} \tin {message.Channel.Name} \tfor {message.Author.Username}", ConsoleColor.Green);
                else
                    ConsoleWrapper.ConsoleWriteLine($"{DateTime.Now.ToLongTimeString()} Send {command.GetType().Name}\tin " +
                       $"DMs \tfor {message.Author.Username}", ConsoleColor.Green);
            }
            catch (Exception e)
            {
                try // Try in case I dont have the permissions to write at all
                {
                    RestUserMessage m = message.Channel.SendMessageAsync(errorMessage).Result;

                    m.AddReactionAsync(errorEmoji).Wait();
                    cachedErrorMessages.Add(new Tuple<RestUserMessage, Exception>(m, e));
                }
                catch { }

                ConsoleWrapper.ConsoleWriteLine($"{DateTime.Now.ToLongTimeString()} [{command.GetType().Name}] {e.Message}\n  " +
                    $"{e.StackTrace.Split('\n').FirstOrDefault(x => x.Contains(":line "))?.Split('\\').Last().Replace(":", ", ")}", ConsoleColor.Red);
                Saver.SaveToLog(e.ToString());
            }
            finally
            {
                typingState.Dispose();
                lock (commandExecutionLock)
                {
                    concurrentCommandExecutions--;
                    UpdateWorkState();
                }
            }
        }
    }
}
