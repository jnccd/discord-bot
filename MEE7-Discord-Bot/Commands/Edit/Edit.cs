using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using Color = System.Drawing.Color;
using BumpKit;
using System.Reflection;
using System.Linq.Expressions;
using AnimatedGif;
using MEE7.Backend.HelperFunctions.Extensions;
using MEE7.Backend.HelperFunctions;

namespace MEE7.Commands
{
    public partial class Edit : Command
    {
        public Edit() : base("edit", "Edit stuff using various functions")
        {
            Commands = InputCommands.Union(TextCommands.Union(PictureCommands.Union(AudioCommands)));

            HelpMenu = new EmbedBuilder();
            HelpMenu.WithDescription("Operators:\n" +
                "> Concatinates functions\n" +
                "() Let you add additional arguments for the command (optional)\n" +
               $"\neg. {PrefixAndCommand} thisT(omegaLUL) > swedish > Aestheticify\n" +
                "\nEdit Commands:");
            AddToHelpmenu("Input Commands", InputCommands);
            AddToHelpmenu("Text Commands", TextCommands);
            AddToHelpmenu("Picture Commands", PictureCommands);
            AddToHelpmenu("Audio Commands", AudioCommands);
        }
        void AddToHelpmenu(string Name, EditCommand[] editCommands)
        {
            string CommandToCommandTypeString(EditCommand c) => $"**{c.Command}**: " +
                  $"`{(c.InputType == null ? "_" : c.InputType.ToReadableString())}` -> " +
                  $"`{(c.OutputType == null ? "_" : c.OutputType.ToReadableString())}`" +
                $"";
            int maxlength = editCommands.
                Select(CommandToCommandTypeString).
                Select(x => x.Length).
                Max();
            HelpMenu.AddFieldDirectly(Name, "" + editCommands.
                Select(c => CommandToCommandTypeString(c) +
                $"{new string(Enumerable.Repeat(' ', maxlength - c.Command.Length - 1).ToArray())}{c.Desc}\n").
                Combine() + "");
        }

        public override void Execute(SocketMessage message)
        {
            if (message.Content.Length <= PrefixAndCommand.Length + 1)
                DiscordNETWrapper.SendEmbed(HelpMenu, message.Channel).Wait();
            else
                PrintResult(RunCommands(message), message);
        }
        object RunCommands(SocketMessage message)
        {
            string input = message.Content.Remove(0, PrefixAndCommand.Length + 1);
            IEnumerable<string> commands = input.Split('|').First().Split(" > ").Select(x => x.Trim(' '));
            object currentData = null;

            if (commands.Count() > 50)
            {
                DiscordNETWrapper.SendText($"That's too many commands for one message.", message.Channel).Wait();
                return null;
            }

            foreach (string c in commands)
            {
                string cwoargs = new string(c.TakeWhile(x => x != '(').ToArray());
                string args = c.GetEverythingBetween("(", ")");

                EditCommand command = Commands.FirstOrDefault(x => x.Command.ToLower() == cwoargs.ToLower());
                if (command == null)
                {
                    DiscordNETWrapper.SendText($"I don't know a command called {cwoargs}", message.Channel).Wait();
                    return null;
                }

                if (command.InputType != null && (currentData == null || !command.InputType.IsAssignableFrom(currentData.GetType())))
                {
                    DiscordNETWrapper.SendText($"Wrong Input Data Type Error in {c}\nExpected: {command.InputType}\nGot: {currentData.GetType()}", message.Channel).Wait();
                    return null;
                }

                try { currentData = command.Function(message, args, currentData); }
                catch (Exception e)
                {
                    DiscordNETWrapper.SendText($"[{c}] {e.Message} " + 
                        $"{e.StackTrace.Split('\n').FirstOrDefault(x => x.Contains(":line "))?.Split('\\').Last().Replace(":", ", ")}",
                        message.Channel).Wait();
                    return null;
                }

                if (command.OutputType != null && (currentData == null || !command.OutputType.IsAssignableFrom(currentData.GetType())))
                {
                    DiscordNETWrapper.SendText($"Wrong Output Data Type Error in {c}\nExpected: {command.OutputType}\nReturned: {currentData.GetType()}", message.Channel).Wait();
                    return null;
                }
            }

            return currentData;
        }
        void PrintResult(object currentData, SocketMessage message)
        {
            foreach (PrintMethod m in PrintMethods)
                if (m.Type == currentData.GetType())
                    m.Function(message, currentData);
        }

        public class PrintMethod
        {
            public Type Type;
            public Action<SocketMessage, object> Function;

            public PrintMethod(Type Type, Action<SocketMessage, object> Function)
            {
                this.Function = Function;
                this.Type = Type;
            }
        }
        public class EditCommand
        {
            public string Command, Desc;
            public Type InputType, OutputType;
            public Func<SocketMessage, string, object, object> Function;

            public EditCommand(string Command, string Desc, Func<SocketMessage, string, object, object> Function, Type InputType, Type OutputType)
            {
                if (Command.ContainsOneOf(new string[] { "|", ">", "<", "." }))
                    throw new IllegalCommandException("Illegal Symbol!");

                this.Command = Command;
                this.Desc = Desc;
                this.Function = Function;
                this.InputType = InputType;
                this.OutputType = OutputType;
            }
        }
        readonly IEnumerable<EditCommand> Commands;
    }
}
