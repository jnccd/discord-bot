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
using System.Threading.Tasks;
using System.Threading;

namespace MEE7.Commands
{
    public partial class Edit : Command
    {
        readonly IEnumerable<EditCommand> Commands;
        class PrintMethod
        {
            public Type Type;
            public Action<SocketMessage, object> Function;

            public PrintMethod(Type Type, Action<SocketMessage, object> Function)
            {
                this.Function = Function;
                this.Type = Type;
            }
        }
        class EditCommand
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
                PrintPipeOutput(RunPipe(CheckPipe(GetExecutionPipe(message), message.Channel), message), message);
        }
        List<Tuple<string, EditCommand>> GetExecutionPipe(SocketMessage message)
        {
            List<Tuple<string, EditCommand>> re = new List<Tuple<string, EditCommand>>();

            string input = message.Content.Remove(0, PrefixAndCommand.Length + 1);
            IEnumerable<string> commands = input.
                Split(" > ").
                Select(x => x.Trim(' '));

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

                re.Add(new Tuple<string, EditCommand>(args, command));
            }

            return re;
        }
        List<Tuple<string, EditCommand>> CheckPipe(List<Tuple<string, EditCommand>> pipe, ISocketMessageChannel channel)
        {
            if (pipe == null)
                return null;
            if (pipe.Count > 50)
            {
                DiscordNETWrapper.SendText($"Only 50 instructions are allowed per pipe", channel).Wait();
                return null;
            }
            if (pipe.First().Item2.InputType != null)
            {
                DiscordNETWrapper.SendText($"The first function has to be a input function", channel).Wait();
                return null;
            }
            for (int i = 1; i < pipe.Count; i++)
                if (!pipe[i].Item2.InputType.IsAssignableFrom(pipe[i - 1].Item2.OutputType))
                {
                    DiscordNETWrapper.SendText($"Type Error: {i + 1}. Command, {pipe[i].Item2.Command} should recieve a " +
                        $"{pipe[i].Item2.InputType.ToReadableString()} but gets a {pipe[i - 1].Item2.OutputType.ToReadableString()} from {pipe[i - 1].Item2.Command}", channel).Wait();
                    return null;
                }
            return pipe;
        }
        object RunPipe(List<Tuple<string, EditCommand>> pipe, SocketMessage message)
        {
            if (pipe == null)
                return null;

            object currentData = null;

            foreach (Tuple<string, EditCommand> p in pipe)
            {
                try { currentData = p.Item2.Function(message, p.Item1, currentData); }
                catch (Exception e)
                {
                    DiscordNETWrapper.SendText($"[{p.Item2.Command}] {e.Message} " + 
                        $"{e.StackTrace.Split('\n').FirstOrDefault(x => x.Contains(":line "))?.Split('\\').Last().Replace(":", ", ")}",
                        message.Channel).Wait();
                    return null;
                }

                if (p.Item2.OutputType != null && (currentData == null || !p.Item2.OutputType.IsAssignableFrom(currentData.GetType())))
                {
                    DiscordNETWrapper.SendText($"Corrupt Function Error: {p.Item2.Command} was supposed to give me a " +
                        $"{p.Item2.OutputType} but actually gave me a {currentData.GetType().ToReadableString()}", message.Channel).Wait();
                    return null;
                }
            }

            return currentData;
        }
        void PrintPipeOutput(object output, SocketMessage message)
        {
            if (output == null) return;
            Type outType = output.GetType();
            PrintMethod m = PrintMethods.FirstOrDefault(x => x.Type.IsAssignableFrom(outType));
            if (m == null)
                DiscordNETWrapper.SendText($"Unprintable Output Error: I wasn't taught how to print {outType.ToReadableString()}", message.Channel).Wait();
            else
                m.Function(message, output);
        }
    }
}
