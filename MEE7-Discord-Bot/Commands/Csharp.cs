using Discord.WebSocket;
using MEE7.Backend;
using System;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using MEE7.Backend.HelperFunctions;
using System.Linq;
using MEE7.Backend.HelperFunctions.Extensions;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using System.Reflection;
using System.Threading;
using System.Diagnostics;

namespace MEE7.Commands
{
    public class Csharp : Command
    {
        public Csharp() : base("csharp", "Run csharp code", isExperimental: true, isHidden: true)
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

        public override void Execute(SocketMessage message)
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
            Thread runner = new Thread(() => {
                object re = null;
                try
                { 
                    re = RunCode("using System;using System.Linq;" + code, cancelCulture.Token); 
                }
                catch (Exception e) { re = e.Message; }
                DiscordNETWrapper.SendText("```csharp\n" + (re == null ? "" : re.ToString()) + "```", message.Channel).Wait();
            });
            runner.Priority = ThreadPriority.Lowest;
            runner.Start();
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(500);
                if (Process.GetCurrentProcess().PrivateMemorySize64 > 500L * 1024 * 1024)
                {
                    cancelCulture.Cancel();
                    DiscordNETWrapper.SendText("```csharp\nCsharp Runner used up too much memory!```", message.Channel).Wait();
                }
            }

            if (runner.IsAlive)
            {
                cancelCulture.Cancel();
                DiscordNETWrapper.SendText("```csharp\nCsharp Runner timed out!```", message.Channel).Wait(); 
            }
        }
    }
}
