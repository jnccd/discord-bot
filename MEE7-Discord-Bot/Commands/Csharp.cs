using Discord.WebSocket;
using MEE7.Backend;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using MEE7.Backend.HelperFunctions;

namespace MEE7.Commands
{
    public class Csharp : Command
    {
        public Csharp() : base("charp", "Run csharp code", isExperimental: false, isHidden: true)
        {

        }

        public override void Execute(SocketMessage message)
        {
            string code = message.Content;
            string[] badWords = { "Console", "System.Runtime", "System.Reflection", "System.IO" };

            foreach (var badWord in badWords)
                if (code.Contains(badWord))
                {
                    DiscordNETWrapper.SendText("Your csharp code shouldn't use " + badWord, message.Channel).Wait();
                    return;
                }

            object re;
            try {
                re = CSharpScript.EvaluateAsync(@"using System;" + code).Result;
            } catch (Exception e) { re = e; }

            DiscordNETWrapper.SendText(re.ToString(), message.Channel).Wait();
        }
    }
}
