using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using MEE7.Backend.HelperFunctions.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MEE7.Commands
{
    public class EditCommandProvider { }
    public partial class Edit : Command
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
        class ArgumentParseMethod
        {
            public Type Type;
            public Func<SocketMessage, string, object> Function;

            public ArgumentParseMethod(Type Type, Func<SocketMessage, string, object> Function)
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
        public abstract class SubCommand 
        {
            public string Command;
            public Type InputType, OutputType;

            public int FunctionCalls() => 1;
        }
        public abstract class SubPipeCommand: SubCommand
        {
#pragma warning disable CS0649
            public static new string Command;
#pragma warning restore CS0649

            public string[] RawCommands;
            public List<Tuple<object[], SubCommand>>[] Pipes;
        }
        public class ForCommand: SubPipeCommand
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
            public new int FunctionCalls() => Steps() * Pipes[0].Select(x => x.Item2.FunctionCalls()).Aggregate((x,y) => x + y);
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
        public class EditCommand: SubCommand
        {
            public string Desc;
            public Argument[] Arguments;
            public Func<SocketMessage, object[], object, object> Function;
            public MethodInfo sourceMethod;

            public EditCommand(string Command, string Desc, Type InputType, Type OutputType, Argument[] Arguments,
                Func<SocketMessage, object[], object, object> Function, MethodInfo sourceMethod = null)
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
                this.sourceMethod = sourceMethod;
            }
        }
        public class Pipe : List<Tuple<object[], SubCommand>> 
        { 
            public string rawPipe;
            public Type InputType() => this.First().Item2.InputType;
            public Type OutputType() => this.Last().Item2.OutputType;

            public static Pipe Parse(SocketMessage message, string rawPipe, Type InputType = null, Type OutputType = null)
            {
                return CheckPipe(GetExecutionPipe(message, rawPipe), true, InputType, OutputType);
            }
            public object Apply(SocketMessage message, object inputData)
            {
                return RunPipe(this, message, inputData);
            }
        }
        public class Null { }
        private static IEnumerable<EditCommand> Commands;

        public Edit() : base("edit", "This is a little more advanced command which allows you to edit data using a set of functions which can be executed in a pipe." +
            "\nFor more information just type **$edit**.")
        {
            Commands = new List<EditCommand>();
            HelpMenu = new EmbedBuilder();
            HelpMenu.WithDescription("Operators:\n" +
                "\\> Concatinates functions\n" +
                "() Let you add additional arguments for the command (optional unless the command requires arguments)\n" +
                "\"\" Automatically choose a input function for your input\n" +
               $"\neg. {PrefixAndCommand} \"omegaLUL\" > swedish > Aestheticify\n" +
                "\nEdit Commands:");

            // Load Functions
            Type[] classesWithEditCommands = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                                              from assemblyType in domainAssembly.GetTypes()
                                              where assemblyType.IsSubclassOf(typeof(EditCommandProvider))
                                              select assemblyType).ToArray();
            foreach (Type t in classesWithEditCommands) {
                var curCommands = new List<EditCommand>();
                var methods = t.GetMethods();
                var fields = t.GetFields();

                foreach (var method in methods)
                {
                    try
                    {
                        var descVar = fields.First(x => x.Name.ToLower() == method.Name.ToLower() + "desc" && x.FieldType == typeof(string));
                        var tInstance = Activator.CreateInstance(t);
                        string desc = (string)descVar.GetValue(tInstance);
                        var param = method.GetParameters();
                        if (param[1].ParameterType == typeof(SocketMessage))
                        {
                            var command = new EditCommand(method.Name, desc, 
                                param.First().ParameterType == typeof(Null)? null : param.First().ParameterType, 
                                method.ReturnType == typeof(void)? null : method.ReturnType,
                                param.Skip(2).Select(x => new Argument(x.Name, x.ParameterType, x.DefaultValue)).ToArray(),
                                (SocketMessage m, object[] args, object o) =>
                                {
                                    if (param.First().ParameterType == typeof(Null))
                                        o = new Null();

                                    var completeArgs = new object[] { o, m }.ToList();
                                    completeArgs.AddRange(args);

                                    if (method.IsGenericMethod)
                                    {
                                        var meth = method.MakeGenericMethod(o == null ? typeof(object) : o.GetType());
                                        var re = meth.Invoke(tInstance, completeArgs.ToArray());
                                        return re;
                                    }
                                    else
                                        return method.Invoke(tInstance, completeArgs.ToArray());
                                });
                            curCommands.Add(command);
                        }
                    } catch { }
                }

                Commands = Commands.Union(curCommands);
                AddToHelpmenu(t.Name, curCommands.ToArray());
            }
        }
        void AddToHelpmenu(string Name, EditCommand[] editCommands)
        {
            string CommandToCommandTypeString(EditCommand c) => $"**{c.Command}**" +
                  $"({c.Arguments.Select(x => $"{x.Name} : {x.Type.ToReadableString()}").Combine(", ")}): " +
                  $"`{(c.InputType == null ? "_" : c.InputType.ToReadableString())}` -> " +
                  $"`{(c.OutputType == null ? "_" : c.OutputType.ToReadableString())}`" +
                $"";
            if (editCommands.Length > 0)
            {
                int maxlength = editCommands.
                    Select(CommandToCommandTypeString).
                    Select(x => x.Length).
                    Max();
                HelpMenu.AddFieldDirectly(Name, "" + editCommands.
                    Select(c => CommandToCommandTypeString(c) +
                    $"{new string(Enumerable.Repeat(' ', maxlength - c.Command.Length - 1).ToArray())}{c.Desc}\n").
                    Combine() + "");
            }
        }
        public override void Execute(SocketMessage message)
        {
            if (message.Content.Length <= PrefixAndCommand.Length + 1)
                DiscordNETWrapper.SendEmbed(HelpMenu, message.Channel).Wait();
            else
            {
                try
                {
                    PrintPipeOutput(
                        RunPipe(
                            CheckPipe(
                                GetExecutionPipe(message, message.Content.Remove(0, PrefixAndCommand.Length + 1))),
                            message),
                    message);
                }
                catch (Exception e)
                {
                    var s = e.StackTrace.GetEverythingBetweenAll(Path.DirectorySeparatorChar.ToString(), "\n");
                    DiscordNETWrapper.SendText($"{e.Message}" +
                        $"{(s.Count > 0 ? $", {s.Last()}" : "")}", 
                        message.Channel).Wait();
                    return;
                }
            }
        }

        static Pipe GetExecutionPipe(SocketMessage message, string rawPipe, bool argumentParsing = true)
        {
            Pipe re = new Pipe() { rawPipe = rawPipe };

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
                        string[] args = new string(arg.Select(x => {
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
                        if (args.Length > parsedArgs.Length) args[parsedArgs.Length - 1] = args.Skip(parsedArgs.Length - 1).Combine(",");
                        for (int i = 0; i < command.Arguments.Length; i++)
                            if (i < args.Length)
                            {
                                args[i] = args[i].Trim(' ');
                                try
                                {
                                    parsedArgs[i] = Convert.ChangeType(Pipe.Parse(message, args[i]).Apply(message, null), command.Arguments[i].Type);
                                }
                                catch
                                {
                                    try { parsedArgs[i] = ArgumentParseMethods.First(x => x.Type == command.Arguments[i].Type).Function(message, args[i]); }
                                    catch { throw new Exception($"I couldn't decipher the argument \"{args[i]}\" that you gave to {cwoargs}"); }
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
                        throw new Exception($"Generic Type Error: {i + 1}. Command, {pipe[i].Item2.Command} should recieve a " +
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

            if (!subPipe && pipe.Last().Item2.OutputType != null && PrintMethods.FirstOrDefault(x => x.Type.IsAssignableFrom(pipe.Last().Item2.OutputType)) == null)
                throw new Exception($"Unprintable Output Error: I wasn't taught how to print {pipe.Last().Item2.OutputType.ToReadableString()}");

            return pipe;
        }
        static object RunPipe(Pipe pipe, SocketMessage message, object initialData = null)
        {
            object currentData = initialData;

            foreach (Tuple<object[], SubCommand> p in pipe)
            {
                try 
                {
                    if (p.Item2 is EditCommand)
                        currentData = (p.Item2 as EditCommand).Function(message, p.Item1, currentData);
                    else if (p.Item2 is ForCommand)
                    {
                        ForCommand forCommand = p.Item2 as ForCommand;
                        List<Task> threads = new List<Task>();
                        object[] array = (object[])Activator.CreateInstance(forCommand.OutputType, forCommand.Steps());
                        object oldData = currentData;

                        for (int i = 0; i < forCommand.Steps(); i++)
                        {
                            int j = i;
                            threads.Add(Task.Run(() => {
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
                            threads.Add(Task.Run(() => {
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
                catch (Exception e) { throw new Exception($"[{p.Item2.Command}] {e.Message} " + 
                    $"{e.StackTrace.Split('\n').FirstOrDefault(x => x.Contains(":line "))?.Split(Path.DirectorySeparatorChar).Last().Replace(":", ", ")}"); }

                if ((p.Item2.OutputType != null && (currentData == null || !p.Item2.OutputType.IsAssignableFrom(currentData.GetType()))) && 
                    !p.Item2.OutputType.ContainsGenericParameters)
                    throw new Exception($"Corrupt Function Error: {p.Item2.Command} was supposed to give me a " +
                        $"{p.Item2.OutputType} but actually gave me a {currentData.GetType().ToReadableString()}");
            }

            return currentData;
        }

        static void PrintPipeOutput(object output, SocketMessage message)
        {
            if (output == null)
                throw new Exception("I can't print `null` :/");

            //if (output is Bitmap[] && (output as Bitmap[]).Length > 50)
            //    throw new Exception($"My Internet is too slow to upload gifs this long");

            PrintMethods.FirstOrDefault(x => x.Type.IsAssignableFrom(output.GetType())).Function(message, output);
        }
    }
}
