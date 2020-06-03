using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using MEE7.Backend.HelperFunctions.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MEE7.Commands
{
    public class Haskell : Command
    {
        readonly string inputPath = $"Commands{Path.DirectorySeparatorChar}Haskell{Path.DirectorySeparatorChar}input.hs";

        public Haskell() : base("ghc", "Haskell Interpreter", false) { }

        public override void Execute(IMessage message)
        {
            lock (this)
            {
                try
                {
                    string[] lines = message.Content.Split('\n');
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string[] words = lines[i].Split(' ');
                        foreach (string word in words)
                            if (word.ContainsOneOf(new string[] { "write", "read", "append", "File", "Handle", "System.", "unsafe", "Windows",
                                "windows", "TCP", "Socket", "socket", "Network.", "Process.", "Compiler.", "Debug.", "Distribution.",
                                "Foreign.", "GHC.", "Trace.", "Type.", "Marshal", ":!" }) || word.Length > 1 && word[0] == ':' && char.IsLetter(word[1]))
                            {
                                DiscordNETWrapper.SendText("Your code contains commands you don't have permission to use!\nAt: " + word + " in line " + i, message.Channel).Wait();
                                return;
                            }
                    }

                    string haskellInput = message.Content.Remove(0, 5).Trim(' ').Trim('`');
                    if (haskellInput.StartsWith("haskell"))
                        haskellInput = haskellInput.Remove(0, "haskell".Length);
                    if (!haskellInput.Contains("main = "))
                        haskellInput = "main = " + haskellInput;
                    File.WriteAllText(inputPath, haskellInput);

                    $"runhaskell {inputPath}".RunAsConsoleCommand(3, () => {
                        DiscordNETWrapper.SendText("Haskell timeout!", message.Channel).Wait();
                    }, (s, e) => {
                        string output = string.IsNullOrWhiteSpace(s) ? e : s;
                        output = output.Insert(0, "```haskell\n").Insert(output.Length + "```haskell\n".Length, "```");
                        
                        if (output.Length >= 2000)
                            DiscordNETWrapper.SendText("That output was a little too long for Discords 2000 character limit.", message.Channel).Wait();
                        else if (output.Trim('`').Trim('\n') == "haskell")
                            DiscordNETWrapper.SendText("Your code didn't create any output!", message.Channel).Wait();
                        else
                            DiscordNETWrapper.SendText(output, message.Channel).Wait();
                    });
                }
                catch (Exception e) { ConsoleWrapper.ConsoleWriteLine(e, ConsoleColor.Red); }
            }
        }
    }
}
