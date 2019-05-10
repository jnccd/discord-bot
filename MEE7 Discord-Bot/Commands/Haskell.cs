using Discord.WebSocket;
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
        readonly string inputPath = "Commands\\Haskell\\input.hs";

        public Haskell() : base("ghc", "Compiles Haskell Code", false)
        {

        }

        public override Task Execute(SocketMessage message)
        {
            lock (this)
            {
                try
                {
                    bool exited = false;

                    string[] lines = message.Content.Split('\n');
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string[] words = lines[i].Split(' ');
                        foreach (string word in words)
                            if (word.ContainsOneOf(new string[] { "write", "read", "append", "File", "Handle", "System.", "unsafe", "Windows",
                                "windows", "TCP", "Socket", "socket", "Network.", "Process.", "Compiler.", "Debug.", "Distribution.",
                                "Foreign.", "GHC.", "Trace.", "Type.", "Marshal", ":!" }) || word.Length > 1 && word[0] == ':' && char.IsLetter(word[1]))
                            {
                                Program.SendText("Your code contains commands you don't have permission to use!\nAt: " + word + " in line " + i, message.Channel).Wait();
                                return Task.FromResult(default(object));
                            }
                    }

                    string haskellInput = message.Content.Remove(0, 5).Trim(' ').Trim('`');
                    if (haskellInput.StartsWith("haskell"))
                        haskellInput = haskellInput.Remove(0, "haskell".Length);
                    if (!haskellInput.Contains("main = "))
                        haskellInput = "main = " + haskellInput;
                    File.WriteAllText(inputPath, haskellInput);
                    Process compiler = new Process();
                    compiler.StartInfo.FileName = "runhaskell";
                    compiler.StartInfo.Arguments = inputPath;
                    compiler.StartInfo.CreateNoWindow = true;
                    compiler.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    compiler.StartInfo.RedirectStandardOutput = true;
                    compiler.StartInfo.RedirectStandardError = true;
                    compiler.Start();
                    
                    DateTime start = DateTime.Now;

                    Task.Factory.StartNew(async () => {
                        Thread.CurrentThread.Name = "Haskell Compiler";
                        compiler.WaitForExit();
                        try
                        {
                            string s = compiler.StandardOutput.ReadToEnd();
                            string e = compiler.StandardError.ReadToEnd();

                            Debug.WriteLine("Raw Haskell Output: ");
                            Debug.WriteLine(s + " | " + e);

                            string output = string.IsNullOrWhiteSpace(s) ? e : s;
                            output = output.Insert(0, "```ruby\n").Insert(output.Length + "```ruby\n".Length, "```");

                            exited = true;
                            if (output.Length >= 2000)
                                await Program.SendText("That output was a little too long for Discords 2000 character limit.", message.Channel);
                            else if (string.IsNullOrWhiteSpace(output.Trim('`')))
                                await Program.SendText("Your code didn't create any output!", message.Channel);
                            else
                                await Program.SendText(output, message.Channel);
                        }
                        catch (Exception e)
                        {
                            Program.ConsoleWriteLine(e, ConsoleColor.Red);
                        }
                        exited = true;
                    });

                    while (!exited && (DateTime.Now - start).TotalSeconds < 10)
                        Thread.Sleep(100);
                    if (!exited)
                    {
                        exited = true;
                        try
                        {
                            compiler.Close();
                        }
                        catch { }
                        Program.SendText("Haskell timeout!", message.Channel).Wait();
                    }
                }
                catch (Exception e) { Program.ConsoleWriteLine(e, ConsoleColor.Red); }
            }

            return Task.FromResult(default(object));
        }
    }
}
