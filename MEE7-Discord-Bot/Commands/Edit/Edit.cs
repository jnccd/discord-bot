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
        struct Argument
        {
            public string Name;
            public Type Type;
            public object StandardValue;

            public Argument(string Name, Type Type, object StandardValue)
            {
                this.Name = Name;
                this.Type = Type;
                this.StandardValue = StandardValue;
            }
        }
        class ArgumentParseMethod
        {
            public Type Type;
            public Func<string, object> Function;

            public ArgumentParseMethod(Type Type, Func<string, object> Function)
            {
                this.Function = Function;
                this.Type = Type;
            }
        }
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
        class SubCommand 
        {
            public string Command, Desc;
            public Type InputType, OutputType;
        }
        class ForCommand: SubCommand
        {
            public string VarName, RawCommands;
            public double Start, End, StepWidth;
            public List<Tuple<object[], SubCommand>> Commands;

            public ForCommand(string varName, string rawCommands, double start, double end, double stepWidth)
            {
                Command = "For Loop";
                Desc = "For Loop";

                VarName = varName;
                RawCommands = rawCommands;
                Start = start;
                End = end;
                StepWidth = stepWidth;
            }

            public int Steps() => (int)((End - Start) / StepWidth);
        }
        class EditCommand: SubCommand
        {
            public Argument[] Arguments;
            public Func<SocketMessage, object[], object, object> Function;

            public EditCommand(string Command, string Desc, Type InputType, Type OutputType, Argument[] Arguments,
                Func<SocketMessage, object[], object, object> Function)
            {
                if (Command.ContainsOneOf(new string[] { "|", ">", "<", "." }))
                    throw new IllegalCommandException("Illegal Symbol in the name!");

                foreach (Argument arg in Arguments)
                    if (ArgumentParseMethods.FirstOrDefault(x => x.Type == arg.Type) == null)
                        throw new IllegalCommandException($"Argument {arg.Name} doesn't have a corresponding Parse Method! {arg.Type.ToReadableString()}");

                this.Command = Command;
                this.Desc = Desc;
                this.Function = Function;
                this.InputType = InputType;
                this.OutputType = OutputType;
                this.Arguments = Arguments;
            }
        }
        private readonly IEnumerable<EditCommand> Commands;

        public Edit() : base("edit", "This is a little more advanced command which allows you to edit data using a set of functions which can be executed in a pipe." +
            "\nFor more information just type **$edit**.")
        {
            Commands = new List<EditCommand>();
            FieldInfo[] CommandLists = GetType().GetRuntimeFields().
                Where(x => x.Name.EndsWith("Commands") && x.Name != "Commands" && x.Name != "for" && x.FieldType == typeof(EditCommand[])).
                OrderBy(x => {
                    if (x.Name.StartsWith("Input"))
                        return "0000";
                    else
                        return x.Name;
                }).
                ToArray();
            foreach (FieldInfo f in CommandLists)
                Commands = Commands.Union((EditCommand[])f.GetValue(this));

            HelpMenu = new EmbedBuilder();
            HelpMenu.WithDescription("Operators:\n" +
                "\\> Concatinates functions\n" +
                "() Let you add additional arguments for the command (optional unless the command requires arguments)\n" +
                "\"\" Automatically choose a input function for your input\n" +
               $"\neg. {PrefixAndCommand} \"omegaLUL\" > swedish > Aestheticify\n" +
                "\nEdit Commands:");
            foreach (FieldInfo f in CommandLists)
                AddToHelpmenu(f.Name, (EditCommand[])f.GetValue(this));
        }
        void AddToHelpmenu(string Name, EditCommand[] editCommands)
        {
            string CommandToCommandTypeString(EditCommand c) => $"**{c.Command}**" +
                  $"({c.Arguments.Select(x => $"{x.Name} : {x.Type.ToReadableString()}").Combine(", ")}): " +
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
                PrintPipeOutput(
                    RunPipe(
                        CheckPipe(
                            GetExecutionPipe(message, message.Content.Remove(0, PrefixAndCommand.Length + 1)), 
                            message.Channel), 
                        message), 
                    message);
        }
        List<Tuple<object[], SubCommand>> GetExecutionPipe(SocketMessage message, string rawPipe, bool argumentParsing = true)
        {
            List<Tuple<object[], SubCommand>> re = new List<Tuple<object[], SubCommand>>();

            if (rawPipe.Contains(""))
            {
                DiscordNETWrapper.SendText($"Edit commands can't contain  symbols!", message.Channel).Wait();
                return null;
            }

            string input = rawPipe.Trim(' ');
            int k = 0, j = 0;
            string[] commands = new string(input.Select(x => {
                if (x == '(')
                    k++;
                if (x == '{')
                    j++;
                if (x == ')')
                    k--;
                if (x == '}')
                    j--;
                if (k == 0 && j == 0 && x == '>')
                    x = '';
                return x;
            }).
            ToArray()).
            Split('').
            Select(x => x.Trim(' ')).
            ToArray();

            if (input[0] == '"')
            {
                string pipeInput = input.GetEverythingBetween("\"", "\"");
                if (message.Attachments.Count > 0 && (message.Attachments.First().Url.EndsWith(".mp3") || message.Attachments.First().Url.EndsWith(".wav")))
                    commands[0] = $"thisA";
                else if (pipeInput.EndsWith(".gif") || message.Attachments.Count > 0 && message.Attachments.First().Url.EndsWith(".gif"))
                    commands[0] = $"thisG";
                else if (pipeInput.EndsWith(".png") || pipeInput.EndsWith(".jpg"))
                    commands[0] = $"thisP({pipeInput})";
                else
                    commands[0] = $"thisT({pipeInput})";
            }

            foreach (string c in commands)
            {
                string cwoargs = new string(c.TakeWhile(x => x != '(').ToArray()).Trim(' ');
                string arg = c.GetEverythingBetween("(", ")");

                object[] parsedArgs = new object[0];
                SubCommand reCommand;

                if (cwoargs == "for")
                {
                    string[] args = arg.Split(':');

                    if (args.Length != 4)
                    {
                        DiscordNETWrapper.SendText($"A for loop needs 4 parameters!", message.Channel).Wait();
                        return null;
                    }

                    ForCommand forCommand = new ForCommand(varName: args[0], rawCommands: c.GetEverythingBetween("{", "}").Trim(' '), 
                        start: args[1].ConvertToDouble(), end: args[2].ConvertToDouble(), stepWidth: args[3].ConvertToDouble());
                    forCommand.Commands = GetExecutionPipe(message, forCommand.RawCommands, false);
                    if (forCommand.Commands == null) return null;

                    if (forCommand.Commands.Count < 1)
                    {
                        DiscordNETWrapper.SendText($"A for loops sub pipe needs to be not empty!", message.Channel).Wait();
                        return null;
                    }
                    forCommand.InputType = forCommand.Commands.First().Item2.InputType;
                    forCommand.OutputType = forCommand.Commands.Last().Item2.OutputType.MakeArrayType();

                    reCommand = forCommand;
                }
                else
                {
                    EditCommand command = Commands.FirstOrDefault(x => x.Command.ToLower() == cwoargs.ToLower());

                    if (command == null)
                    {
                        DiscordNETWrapper.SendText($"I don't know a command called {cwoargs}", message.Channel).Wait();
                        return null;
                    }

                    if (argumentParsing)
                    {
                        string[] args = arg.Split(',').Select(x => x.Trim(' ')).ToArray();
                        if (args.Length == 1 && args[0] == "") args = new string[0];
                        parsedArgs = new object[command.Arguments.Length];
                        for (int i = 0; i < command.Arguments.Length; i++)
                            if (i < args.Length)
                            {
                                try { parsedArgs[i] = ArgumentParseMethods.First(x => x.Type == command.Arguments[i].Type).Function(args[i]); }
                                catch
                                {
                                    DiscordNETWrapper.SendText($"I couldn't decipher the argument \"{args[i]}\" that you gave to {cwoargs}", message.Channel).Wait();
                                    return null;
                                }
                            }
                            else if (command.Arguments[i].StandardValue == null)
                            {
                                DiscordNETWrapper.SendText($"[{cwoargs}] {command.Arguments[i].Name} requires a value!", message.Channel).Wait();
                                return null;
                            }
                            else
                                parsedArgs[i] = command.Arguments[i].StandardValue;
                    }
                    
                    reCommand = command;
                }

                re.Add(new Tuple<object[], SubCommand>(parsedArgs, reCommand));
            }

            return re;
        }
        List<Tuple<object[], SubCommand>> CheckPipe(List<Tuple<object[], SubCommand>> pipe, ISocketMessageChannel channel, bool subPipe = false)
        {
            if (pipe == null) return null;
            if (pipe.Select(x => {
                if (x.Item2 is ForCommand)
                    return ((x.Item2 as ForCommand).RawCommands.AllIndexesOf(">").Count + 1) * (x.Item2 as ForCommand).Steps();
                return 1;
            }).Aggregate((x,y) => x+y) >= 100) // TODO: Improve performance limit check
            {
                DiscordNETWrapper.SendText($"Only 100 instructions are allowed per pipe.", channel).Wait();
                return null;
            }
            if (pipe.First().Item2.InputType != null && !subPipe)
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

            foreach (ForCommand f in pipe.Select(x => x.Item2).Where(x => x is ForCommand).Select(x => x as ForCommand))
                if (f.StepWidth == 0 || f.Steps() < 0)
                {
                    DiscordNETWrapper.SendText($"Man you must have accidentaly dropped a infinite for loop into me.\n" +
                        $"No one would do this on purpose, that would be evil.\n" +
                        $"But don't worry I was programmed to ignore something like this.", channel).Wait();
                    return null;
                }

            if (pipe.Last().Item2.OutputType != null && PrintMethods.FirstOrDefault(x => x.Type.IsAssignableFrom(pipe.Last().Item2.OutputType)) == null)
            {
                DiscordNETWrapper.SendText($"Unprintable Output Error: I wasn't taught how to print {pipe.Last().Item2.OutputType.ToReadableString()}", channel).Wait();
                return null;
            }

            return pipe;
        }
        object RunPipe(List<Tuple<object[], SubCommand>> pipe, SocketMessage message, object initialData = null)
        {
            if (pipe == null) return null;
            object currentData = initialData;

            foreach (Tuple<object[], SubCommand> p in pipe)
            {
                try 
                {
                    if (p.Item2 is EditCommand)
                        currentData = (p.Item2 as EditCommand).Function(message, p.Item1, currentData);
                    else if (p.Item2 is ForCommand)
                    {
                        ForCommand forLoop = p.Item2 as ForCommand;
                        object[] array = (object[])Activator.CreateInstance(forLoop.OutputType, forLoop.Steps());
                        object oldData = currentData;

                        for (int i = 0; i < forLoop.Steps(); i++)
                        {
                            object usableData;
                            if (currentData is ICloneable)
                                usableData = (currentData as ICloneable).Clone();
                            else
                                usableData = currentData;

                            string rawCommandThisLoop = forLoop.RawCommands.Replace($"%{forLoop.VarName}", (forLoop.Start + i * forLoop.StepWidth).ToString());
                            forLoop.Commands = CheckPipe(GetExecutionPipe(message, rawCommandThisLoop),
                                    message.Channel, subPipe: true);
                            if (forLoop.Commands == null) return null;

                            array[i] = RunPipe(forLoop.Commands, message, usableData);
                            if (array[i] == null) return null;
                        }

                        currentData = array;
                        if (oldData is IDisposable)
                            (oldData as IDisposable).Dispose();
                    }
                }
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
            if (output is Bitmap[] && (output as Bitmap[]).Length > 50)
            {
                DiscordNETWrapper.SendText($"My Internet is too slow to upload gifs this long", message.Channel).Wait();
                return;
            }

            PrintMethods.FirstOrDefault(x => x.Type.IsAssignableFrom(output.GetType())).Function(message, output);
        }
    }
}
