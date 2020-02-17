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

namespace MEE7.Commands
{
    public class Csharp : Command
    {
        public Csharp() : base("csharp", "Run csharp code", isExperimental: false, isHidden: true)
        {

        }

        public object RunCode(string code)
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
                return script.RunAsync().Result.ReturnValue;
            }
        }

        public override void Execute(SocketMessage message)
        {
            string code = message.Content.Split(" ").Skip(1).Combine(" ").Trim('`', ' ');
            string[] badWords = { "Console", "System.Runtime", "GC.", "System.Reflection", "System.IO" };

            foreach (var badWord in badWords)
                if (code.Contains(badWord))
                {
                    DiscordNETWrapper.SendText("Your csharp code shouldn't use " + badWord, message.Channel).Wait();
                    return;
                }

            object re;
            try {
                var imports = ScriptOptions.Default;
                re = RunCode("using System;using System.Linq;" + code);
            } 
            catch (Exception e) { re = e; }

            DiscordNETWrapper.SendText("```csharp\n" + re.ToString() + "```", message.Channel).Wait();
        }
    }
}
