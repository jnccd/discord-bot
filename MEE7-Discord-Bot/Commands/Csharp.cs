using Discord.WebSocket;
using MEE7.Backend;
using System;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using MEE7.Backend.HelperFunctions;
using System.Linq;
using MEE7.Backend.HelperFunctions.Extensions;

namespace MEE7.Commands
{
    public class Csharp : Command
    {
        public Csharp() : base("csharp", "Run csharp code", isExperimental: false, isHidden: true)
        {

        }

        public override void Execute(SocketMessage message)
        {
            string code = message.Content.Split(" ").Skip(1).Combine(" ");
            string[] badWords = { "Console", "System.Runtime", "GC.", "System.Reflection", "System.IO" };

            foreach (var badWord in badWords)
                if (code.Contains(badWord))
                {
                    DiscordNETWrapper.SendText("Your csharp code shouldn't use " + badWord, message.Channel).Wait();
                    return;
                }

            object re;
            try {
                re = CSharpScript.EvaluateAsync(@"using System;using System.Linq;" + code).Result;
            } catch (Exception e) { re = e; }

            DiscordNETWrapper.SendText(re.ToString(), message.Channel).Wait();
        }
    }
}
