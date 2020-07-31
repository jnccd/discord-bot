using Discord;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace MEE7.Commands
{
    public class Csharp : Command
    {
        public Csharp() : base("c#", "Run csharp code", isExperimental: false, isHidden: true)
        {

        }

        public object RunCode(string code, CancellationToken cancelled)
        {
            using (var interactiveLoader = new InteractiveAssemblyLoader())
            {
                var mscorlib = typeof(object).GetTypeInfo().Assembly;
                var systemCore = typeof(Enumerable).GetTypeInfo().Assembly;
                var references = new[] { mscorlib, systemCore };
                foreach (var reference in references)
                    interactiveLoader.RegisterDependency(reference);
                var scriptOptions = ScriptOptions.Default;
                scriptOptions = scriptOptions.AddReferences(references);
                scriptOptions = scriptOptions.AddImports("System");
                scriptOptions = scriptOptions.AddImports("System.Linq");
                scriptOptions = scriptOptions.AddImports("System.Collections.Generic");

                var script = CSharpScript.Create(code, scriptOptions, null, interactiveLoader);
                return script.RunAsync(null, null, cancelled).Result.ReturnValue;
            }
        }

        public override void Execute(IMessage message)
        {
            string code = message.Content;
            code = code.Replace("\n", " ");
            if (!code.Trim(' ').Contains(" "))
            {
                DiscordNETWrapper.SendText("I can't find the code D:", message.Channel).Wait();
                return;
            }
            code = code.Split(" ").Skip(1).Combine(" ").Trim('`', ' ');
            string[] badWords = { "Console", "System.Runtime", "GC.", "System.Reflection", "System.IO", "Environment.Exit", "System.Threading" };

            foreach (var badWord in badWords)
                if (code.Contains(badWord))
                {
                    DiscordNETWrapper.SendText("Your csharp code shouldn't use " + badWord, message.Channel).Wait();
                    return;
                }

            CancellationTokenSource cancelCulture = new CancellationTokenSource();
            Thread runner = new Thread(() =>
            {
                object re = null;
                try
                {
                    re = RunCode("using System;using System.Linq;" + code, cancelCulture.Token);
                }
                catch (Exception e) { re = e.Message; }
                DiscordNETWrapper.SendText("```csharp\n" + (re == null ? "null" : re.ToString().Replace("`", "")) + "```", message.Channel).Wait();
            })
            {
                Priority = ThreadPriority.Lowest
            };
            long startMem = Process.GetCurrentProcess().PrivateMemorySize64;
            runner.Start();
            for (int i = 0; i < 50; i++)
            {
                Thread.Sleep(100);
                if (Process.GetCurrentProcess().PrivateMemorySize64 > startMem + 128L * 1024 * 1024)
                {
                    cancelCulture.Cancel();
                    DiscordNETWrapper.SendText("```csharp\nCsharp Runner used up too much memory!```", message.Channel).Wait();
                    return;
                }
            }

            if (runner.IsAlive)
            {
                cancelCulture.Cancel();
                DiscordNETWrapper.SendText("```csharp\nCsharp Runner timed out!```", message.Channel).Wait();
                return;
            }
        }
    }
}
