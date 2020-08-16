using Discord;
using Discord.WebSocket;
using IronPython.Modules;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MEE7.Commands.Edit
{
    public class EditCommandProvider { }
    public class Edit : Command
    {
        public struct Argument
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
        public abstract class SubCommand
        {
            public string Command;
            public Type InputType, OutputType;

            public int FunctionCalls() => 1;
        }
        public abstract class SubPipeCommand : SubCommand
        {
#pragma warning disable CS0649
            public static new string Command;
#pragma warning restore CS0649

            public string[] RawCommands;
            public List<Tuple<object[], SubCommand>>[] Pipes;
        }
        public class ForCommand : SubPipeCommand
        {
            public static new string Command { get => "for"; set { } }
            public string VarName;
            public double Start, End, StepWidth;

            public ForCommand(string varName, double start, double end, double stepWidth, string[] rawCommands, List<Tuple<object[], SubCommand>>[] pipes)
            {
                VarName = varName;
                Start = start;
                End = end;
                StepWidth = stepWidth;
                RawCommands = rawCommands;
                Pipes = pipes;

                InputType = Pipes[0].First().Item2.InputType;
                OutputType = Pipes[0].Last().Item2.OutputType.MakeArrayType();

                if (Pipes == null || Pipes.Length != 1 || Pipes.Any(x => x == null))
                    throw new Exception($"Something went wrong during parsing :/");

                if (Pipes[0].Count < 1)
                    throw new Exception($"A for loops sub pipe needs to be not empty!");
            }

            public int Steps() => (int)((End - Start) / StepWidth);
            public new int FunctionCalls() => Steps() * Pipes[0].Select(x => x.Item2.FunctionCalls()).Aggregate((x, y) => x + y);
        }
        public class ForeachCommand : SubPipeCommand
        {
            public static new string Command { get => "foreach"; set { } }
            public string VarName;
            public double Start, End;

            public ForeachCommand(string varName, double start, double end, string[] rawCommands, List<Tuple<object[], SubCommand>>[] pipes)
            {
                VarName = varName;
                Start = start;
                End = end;
                RawCommands = rawCommands;
                Pipes = pipes;

                InputType = Pipes[0].First().Item2.InputType.MakeArrayType();
                OutputType = Pipes[0].Last().Item2.OutputType.MakeArrayType();

                if (Pipes == null || Pipes.Length != 1 || Pipes.Any(x => x == null))
                    throw new Exception($"Something went wrong during parsing :/");

                if (Pipes[0].Count < 1)
                    throw new Exception($"A for loops sub pipe needs to be not empty!");
            }

            public new int FunctionCalls() => Pipes[0].Select(x => x.Item2.FunctionCalls()).Aggregate((x, y) => x + y);
        }
        public class EditCommand : SubCommand
        {
            public string Desc;
            public Argument[] Arguments;
            public Func<IMessage, object[], object, object> Function;
            public MethodInfo sourceMethod;

            public EditCommand(string Command, string Desc, Type InputType, Type OutputType, Argument[] Arguments,
                Func<IMessage, object[], object, object> Function, MethodInfo sourceMethod = null)
            {
                if (Command.ContainsOneOf(new string[] { "|", ">", "<", "." }))
                    throw new IllegalCommandException("Illegal Symbol in the name!");

                //foreach (Argument arg in Arguments)
                //    if (ArgumentParseMethod.ArgumentParseMethods.FirstOrDefault(x => x.Type == arg.Type) == null)
                //        throw new IllegalCommandException($"Argument {arg.Name} doesn't have a corresponding Parse Method! {arg.Type.ToReadableString()}");

                this.Command = Command;
                this.Desc = Desc;
                this.Function = Function;
                this.InputType = InputType;
                this.OutputType = OutputType;
                this.Arguments = Arguments;
                this.sourceMethod = sourceMethod;
            }
        }
        public class Pipe : List<Tuple<object[], SubCommand>>
        {
            public bool isArgsPipe = false;
            public string rawPipe;
            public Type InputType() => this.First().Item2.InputType;
            public Type OutputType() => this.Last().Item2.OutputType;

            public static Pipe Parse(IMessage message, string rawPipe, Type InputType = null, Type OutputType = null)
            {
                return CheckPipe(GetExecutionPipe(message, rawPipe), true, InputType, OutputType);
            }
            public object Apply(IMessage message, object inputData, Dictionary<string, object> vars = null)
            {
                return RunPipe(this, message, inputData, vars);
            }
        }
        public class Gif : Tuple<Bitmap[], int[]> { public Gif(Bitmap[] item1, int[] item2) : base(item1, item2) { } }
        public class EditNull { }
        public class EditVariable { public string VarName; }

        private static IEnumerable<EditCommand> Commands;
        private static Dictionary<string, EmbedBuilder> groupHelpMenus = new Dictionary<string, EmbedBuilder>();

        public Edit() : base("edit", "This is a little more advanced command which allows you to chain together functions that were made specific for this command. " +
            $"Shortcut: **{Program.Prefix}-**\nFor more information just type **{Program.Prefix}edit**.")
        {
            Commands = new List<EditCommand>();
            HelpMenu = new EmbedBuilder();
            HelpMenu.WithDescription("Operators:\n" +
                "\\> Concatinates functions\n" +
                "() Let you add additional arguments for the command (optional unless the command requires arguments)\n" +
                "\"\" Automatically choose a input function for your input\n" +
               $"\neg. {PrefixAndCommand} \"omegaLUL\" > swedish > Aestheticify\n" +
               $"\nIf you want to find more commands you can write \"{PrefixAndCommand} help [groupName]\"" +
                "The following command groups are currently loaded:");

            // Load Functions
            Type[] classesWithEditCommands = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                                              from assemblyType in domainAssembly.GetTypes()
                                              where assemblyType.IsSubclassOf(typeof(EditCommandProvider))
                                              select assemblyType).ToArray();
            foreach (Type t in classesWithEditCommands)
            {
                var curCommands = new List<EditCommand>();
                var methods = t.GetMethods().Where(x => x.DeclaringType == t);
                var fields = t.GetFields();
                var tInstance = Activator.CreateInstance(t);

                foreach (var method in methods)
                {
                    try
                    {
                        var descVar = fields.First(x => x.Name.ToLower() == method.Name.ToLower() + "desc" && x.FieldType == typeof(string));
                        string desc = (string)descVar.GetValue(tInstance);
                        var param = method.GetParameters();
                        if (param[1].ParameterType == typeof(IMessage))
                        {
                            var command = new EditCommand(method.Name, desc,
                                param.First().ParameterType == typeof(EditNull) ? null : param.First().ParameterType,
                                method.ReturnType == typeof(void) ? null : method.ReturnType,
                                param.Skip(2).Select(x => new Argument(x.Name, x.ParameterType, x.DefaultValue)).ToArray(),
                                (IMessage m, object[] args, object o) =>
                                {
                                    if (param.First().ParameterType == typeof(EditNull))
                                        o = new EditNull();

                                    var completeArgs = new object[] { o, m }.ToList();
                                    completeArgs.AddRange(args);

                                    for (int i = 0; i < completeArgs.Count; i++)
                                        if (completeArgs[i] is IConvertible)
                                            completeArgs[i] = Convert.ChangeType(completeArgs[i], param[i].ParameterType);

                                    if (method.IsGenericMethod)
                                    {
                                        try
                                        {
                                            var type = typeof(object);
                                            if (o != null) type = o.GetType();
                                            if (type.IsArray) type = type.GetElementType();
                                            var meth = method.MakeGenericMethod(new Type[] { type }.
                                                Concat(Enumerable.Repeat(typeof(object), method.GetGenericArguments().Length - 1)).ToArray());
                                            return meth.Invoke(tInstance, completeArgs.ToArray());
                                        }
                                        catch
                                        {
                                            var meth = method.MakeGenericMethod(Enumerable.Repeat(typeof(object), method.GetGenericArguments().Length).ToArray());
                                            return meth.Invoke(tInstance, completeArgs.ToArray());
                                        }
                                    }
                                    else
                                        return method.Invoke(tInstance, completeArgs.ToArray());
                                });
                            curCommands.Add(command);
                        }
                        else
                            throw new Exception(method.Name + " does not have proper arguments!");
                    }
                    catch
                    {
                        ConsoleWrapper.WriteLine("[Edit] Failed to load: " + method.Name);
                    }
                }

                Commands = Commands.Union(curCommands);
                HelpMenu.Description += $"\n{t.Name}";
                AddToHelpmenus(t.Name, curCommands.ToArray());
            }
        }
        string CommandToCommandTypeString(EditCommand c) => $"**{c.Command}**" +
                  $"({c.Arguments.Select(x => $"{x.Name} : {x.Type.ToReadableString()}").Combine(", ")}): " +
                  $"`{(c.InputType == null ? "_" : c.InputType.ToReadableString())}` -> " +
                  $"`{(c.OutputType == null ? "_" : c.OutputType.ToReadableString())}`" +
                $"";
        void AddToHelpmenus(string Name, EditCommand[] editCommands)
        {
            EmbedBuilder helpMenu = DiscordNETWrapper.CreateEmbedBuilder(Name);
            groupHelpMenus.Add(Name, helpMenu);
            if (editCommands.Length > 0)
            {
                int maxlength = editCommands.
                    Select(CommandToCommandTypeString).
                    Select(x => x.Length).
                    Max();
                helpMenu.AddFieldDirectly(Name, "" + editCommands.
                    Select(c => CommandToCommandTypeString(c) +
                    $"{new string(Enumerable.Repeat(' ', maxlength - c.Command.Length - 1).ToArray())}{c.Desc}\n").
                    Combine() + "");
            }
        }
        public override void Execute(IMessage message)
        {
            var split = message.Content.Split(' ');
            if (split.Length <= 1)
                DiscordNETWrapper.SendEmbed(HelpMenu, message.Channel).Wait();
            else if (split[1] == "help")
            {
                EditCommand command; EmbedBuilder groupHelpMenu;
                if (split.Length == 2)
                    DiscordNETWrapper.SendEmbed(HelpMenu, message.Channel).Wait();
                else if ((command = Commands.FirstOrDefault(x => x.Command.ToLower() == split[2].ToLower())) != null)
                    DiscordNETWrapper.SendEmbed(DiscordNETWrapper.CreateEmbedBuilder($"**{command.Command}**",
                        $"{command.Desc}\n{CommandToCommandTypeString(command)}"), message.Channel).Wait();
                else if ((groupHelpMenu = groupHelpMenus.Where(x => x.Key.ToLower().StartsWith(split[2].ToLower())).Select(x => x.Value).FirstOrDefault()) != null)
                    DiscordNETWrapper.SendEmbed(groupHelpMenu, message.Channel).Wait();
            }
            else if (split[1] == "search" && split.Length == 3)
            {
                var hits = Commands.OrderBy(x => x.Command.LevenshteinDistance(split[2]));
                var topHits = hits.Take(3).ToArray();
                topHits = topHits.Union(Commands.Where(x => x.Command.Contains(split[2], StringComparison.OrdinalIgnoreCase)).Take(2)).ToArray();
                DiscordNETWrapper.SendEmbed(DiscordNETWrapper.CreateEmbedBuilder("Search hits:", topHits.Length == 0 ? "-" :
                    topHits.Select(x => $"{Array.IndexOf(topHits, x) + 1}. {x.Command}\n{x.Desc}\n{CommandToCommandTypeString(x)}").
                    Combine("\n\n")), message.Channel).Wait();
            }
            else
            {
                try
                {
                    PrintPipeOutput(
                        RunPipe(
                            CheckPipe(
                                GetExecutionPipe(message, split.Skip(1).Combine(" "))),
                            message),
                    message);
                }
                catch (Exception e)
                {
                    string text = e.Message;
                    if (e.InnerException != null)
                        text += $" ||{e.InnerException.StackTrace.Split('\\').Last()}||";
                    DiscordNETWrapper.SendText(text, message.Channel).Wait();
                }
            }
        }

        static Pipe GetExecutionPipe(IMessage message, string rawPipe, bool argumentParsing = true)
        {
            Pipe re = new Pipe() { rawPipe = rawPipe };

            if (rawPipe.Contains(""))
            {
                DiscordNETWrapper.SendText($"Edit commands can't contain  symbols!", message.Channel).Wait();
                return null;
            }

            string input = rawPipe.Trim(' ');
            int k = 0, j = 0;
            string[] commands = new string(input.Select(x =>
            {
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
                    commands[0] = $"thisG({pipeInput})";
                else if (pipeInput.EndsWith(".png") || pipeInput.EndsWith(".jpg"))
                    commands[0] = $"thisP({pipeInput})";
                else
                    commands[0] = $"thisT({pipeInput})";
            }

            foreach (string c in commands)
            {
                string cwoargs = new string(c.TakeWhile(x => x != '(').ToArray()).Trim(' ');
                string arg = new string((c + "").GetEverythingBetween("(", "").SkipLast(1).ToArray());

                object[] parsedArgs = new object[0];
                SubCommand reCommand = null;

                string[] rawPipes = c.GetEverythingBetweenAll("{", "}").Select(x => x.Trim(' ')).ToArray();
                if (rawPipes.Length > 0)
                {
                    arg = c.GetEverythingBetween("(", ")");
                    string[] args = arg.Split(':', ';');

                    if (cwoargs == ForCommand.Command)
                    {
                        if (args.Length != 4)
                            throw new Exception($"A for loop needs 4 parameters!");

                        reCommand = new ForCommand(varName: args[0], start: args[1].ConvertToDouble(),
                            end: args[2].ConvertToDouble(), stepWidth: args[3].ConvertToDouble(),
                            rawCommands: rawPipes, rawPipes.Select(x => GetExecutionPipe(message, x, false)).ToArray());
                    }
                    else if (cwoargs == ForeachCommand.Command)
                    {
                        if (args.Length != 3)
                            throw new Exception($"A foreach loop needs 3 parameters!");

                        reCommand = new ForeachCommand(varName: args[0], start: args[1].ConvertToDouble(),
                            end: args[2].ConvertToDouble(), rawCommands: rawPipes, rawPipes.Select(x => GetExecutionPipe(message, x, false)).ToArray());
                    }
                }
                else
                {
                    EditCommand command = Commands.FirstOrDefault(x => x.Command.ToLower() == cwoargs.ToLower());

                    if (command == null)
                        throw new Exception($"I don't know a command called {cwoargs}");

                    if (argumentParsing)
                    {
                        k = 0; j = 0;
                        string[] args = new string(arg.Select(x =>
                        {
                            if (x == '(')
                                k++;
                            if (x == '{')
                                j++;
                            if (x == ')')
                                k--;
                            if (x == '}')
                                j--;
                            if (k == 0 && j == 0 && x == ',')
                                x = '';
                            return x;
                        }).
                        ToArray()).
                        Split('').
                        ToArray();

                        if (args.Length == 1 && args[0] == "") args = new string[0];
                        parsedArgs = new object[command.Arguments.Length];
                        if (args.Length > parsedArgs.Length && parsedArgs.Length != 0) args[parsedArgs.Length - 1] = args.Skip(parsedArgs.Length - 1).Combine(",");
                        for (int i = 0; i < command.Arguments.Length; i++)
                            if (i < args.Length)
                            {
                                args[i] = args[i].Trim(' ');

                                // Args preprocessing
                                if (args[i].StartsWith('%')) // If it starts with % its a var
                                    parsedArgs[i] = new EditVariable() { VarName = args[i].Trim('%') };
                                else
                                    try // Try the ArgumentParseMethods on it
                                    {
                                        parsedArgs[i] = ArgumentParseMethod.ArgumentParseMethods.First(x => x.Type == command.Arguments[i].Type).Function(message, args[i]);
                                    }
                                    catch
                                    {
                                        try // Assume the argument is a pipe and that the pipe takes null as input and uses no vars, try to run it to preprocess the arg
                                        {
                                            parsedArgs[i] = Convert.ChangeType(Pipe.Parse(message, args[i]).Apply(message, null), command.Arguments[i].Type);
                                        }
                                        catch
                                        {
                                            try // Assume the argument is a pipe again, only parse it and run it in runtime
                                            {
                                                parsedArgs[i] = Pipe.Parse(message, args[i]);
                                                (parsedArgs[i] as Pipe).isArgsPipe = true;
                                            }
                                            catch
                                            {
                                                throw new Exception($"I couldn't decipher the argument \"{args[i]}\" that you gave to {cwoargs}");
                                            }
                                        }

                                    }
                            }
                            else if (command.Arguments[i].StandardValue == null)
                                throw new Exception($"[{cwoargs}] {command.Arguments[i].Name} requires a value!");
                            else
                                parsedArgs[i] = command.Arguments[i].StandardValue;
                    }

                    reCommand = command;
                }

                if (reCommand == null) throw new Exception($"I can't read :/");

                re.Add(new Tuple<object[], SubCommand>(parsedArgs, reCommand));
            }

            return re;
        }
        static Pipe CheckPipe(Pipe pipe, bool subPipe = false, Type InputType = null, Type OutputType = null)
        {
            if (pipe.Select(x => x.Item2.FunctionCalls()).Sum() >= 100)
                throw new Exception($"Only 100 instructions are allowed per pipe.");

            if (pipe.First().Item2.InputType != null && !subPipe)
                throw new Exception($"The first function has to be a input function");

            for (int i = 1; i < pipe.Count; i++)
                try
                {
                    if (!pipe[i].Item2.InputType.IsAssignableFrom(pipe[i - 1].Item2.OutputType) &&
                    !pipe[i].Item2.InputType.ContainsGenericParameters && !pipe[i - 1].Item2.OutputType.ContainsGenericParameters)
                        throw new Exception($"Type Error: {i + 1}. Command, {pipe[i].Item2.Command} should recieve a " +
                            $"{pipe[i].Item2.InputType.ToReadableString()} but gets a {pipe[i - 1].Item2.OutputType.ToReadableString()} " +
                            $"from {pipe[i - 1].Item2.Command}");
                }
                catch (Exception e) { if (e.Message.StartsWith("Type Error")) throw e; }
            for (int i = 1; i < pipe.Count - 1; i++)
                try
                {
                    if (pipe[i].Item2.InputType.ContainsGenericParameters && i < pipe.Count - 1 &&
                        !(pipe[i].Item2 as EditCommand).sourceMethod.MakeGenericMethod(pipe[i - 1].Item2.OutputType).ReturnType.
                        IsAssignableFrom(pipe[i + 1].Item2.InputType))
                        throw new Exception($"Generic Type Error: {i + 1}. Command, {pipe[i].Item2.Command} should receive a " +
                            $"{pipe[i].Item2.InputType.ToReadableString()} but gets a {pipe[i - 1].Item2.OutputType.ToReadableString()} " +
                            $"from {pipe[i - 1].Item2.Command}");
                }
                catch (Exception e) { if (e.Message.StartsWith("Generic Type Error")) throw e; }


            foreach (ForCommand f in pipe.Select(x => x.Item2).Where(x => x is ForCommand).Select(x => x as ForCommand))
            {
                if (f.StepWidth == 0 || f.Steps() < 0)
                    throw new Exception($"Man you must have accidentaly dropped a infinite for loop into me.\n" +
                        $"No one would do this on purpose, that would be evil.\n" +
                        $"But don't worry I was programmed to ignore something like this.");
            }

            if (!subPipe && pipe.Last().Item2.OutputType != null && PrintMethod.PrintMethods.FirstOrDefault(x => x.Type.IsAssignableFrom(pipe.Last().Item2.OutputType)) == null)
                throw new Exception($"Unprintable Output Error: I wasn't taught how to print {pipe.Last().Item2.OutputType.ToReadableString()}");

            return pipe;
        }
        static object RunPipe(Pipe pipe, IMessage message, object initialData = null, Dictionary<string, object> vars = null)
        {
            object currentData = initialData;

            foreach (Tuple<object[], SubCommand> p in pipe)
            {
                try
                {
                    object[] args = p.Item1.Select(arg =>
                    {
                        if (arg is EditVariable)
                            if (vars == null || !vars.ContainsKey((arg as EditVariable).VarName))
                                throw new Exception("you fowgot to define vawiabwes uwu");
                            else
                                return vars[(arg as EditVariable).VarName];
                        else if (arg is Pipe && (arg as Pipe).isArgsPipe)
                            return (arg as Pipe).Apply(message, null, vars);
                        else
                            return arg;
                    }).ToArray();

                    if (p.Item2 is EditCommand)
                        currentData = (p.Item2 as EditCommand).Function(message, args, currentData);
                    else if (p.Item2 is ForCommand)
                    {
                        ForCommand forCommand = p.Item2 as ForCommand;
                        List<Task> threads = new List<Task>();
                        object[] array = (object[])Activator.CreateInstance(forCommand.OutputType, forCommand.Steps());
                        object oldData = currentData;

                        for (int i = 0; i < forCommand.Steps(); i++)
                        {
                            int j = i;
                            threads.Add(Task.Run(() =>
                            {
                                object usableData;
                                lock (currentData)
                                {
                                    if (currentData is ICloneable)
                                        usableData = (currentData as ICloneable).Clone();
                                    else
                                        usableData = currentData;
                                }

                                string rawCommandThisLoop = forCommand.RawCommands[0].Replace($"%{forCommand.VarName}",
                                    (forCommand.Start + j * forCommand.StepWidth).ToString().Replace(",", "."));
                                Pipe parsedLoopedPipe =
                                    CheckPipe(GetExecutionPipe(message, rawCommandThisLoop), subPipe: true);

                                array[j] = RunPipe(parsedLoopedPipe, message, usableData);
                            }));
                        }
                        threads.ForEach(x => x.Wait());

                        currentData = array;
                        if (oldData is IDisposable)
                            (oldData as IDisposable).Dispose();
                    }
                    else if (p.Item2 is ForeachCommand)
                    {
                        List<Task> threads = new List<Task>();
                        object[] arraydCurrentData = currentData as object[];

                        ForeachCommand foreachCommand = p.Item2 as ForeachCommand;
                        object[] array = (object[])Activator.CreateInstance(foreachCommand.OutputType, arraydCurrentData.Length);

                        for (int i = 0; i < arraydCurrentData.Length; i++)
                        {
                            int j = i;
                            threads.Add(Task.Run(() =>
                            {
                                double varValue = foreachCommand.Start + ((foreachCommand.End - foreachCommand.Start) * (j / (double)arraydCurrentData.Length));
                                string rawCommandThisLoop = foreachCommand.RawCommands[0].Replace($"%{foreachCommand.VarName}", varValue.ToString().Replace(",", "."));
                                Pipe parsedLoopedPipe =
                                    CheckPipe(GetExecutionPipe(message, rawCommandThisLoop), subPipe: true);

                                array[j] = RunPipe(parsedLoopedPipe, message, arraydCurrentData[j]);
                            }));
                        }

                        threads.ForEach(x => x.Wait());
                        currentData = array;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"[{p.Item2.Command}] {e.InnerException.Message} ", e.InnerException);
                }

                if ((p.Item2.OutputType != null && (currentData == null || !p.Item2.OutputType.IsAssignableFrom(currentData.GetType()))) &&
                    !p.Item2.OutputType.ContainsGenericParameters)
                    throw new Exception($"Corrupt Function Error: {p.Item2.Command} was supposed to give me a " +
                        $"{p.Item2.OutputType} but actually gave me a {currentData.GetType().ToReadableString()}");
            }

            return currentData;
        }

        static void PrintPipeOutput(object output, IMessage message)
        {
            if (output == null || (output is string && string.IsNullOrWhiteSpace(output as string))) return;
            object[] arr;
            if (output.GetType().IsArray && (arr = output as object[]).All(x => x.GetType() == typeof(Bitmap)))
            {
                output = new Bitmap[arr.Length];
                for (int i = 0; i < arr.Length; i++)
                    (output as Bitmap[])[i] = (Bitmap)arr[i];
            }
            PrintMethod.PrintMethods.FirstOrDefault(x => x.Type.IsAssignableFrom(output.GetType())).Function(message, output);
        }
    }
}
