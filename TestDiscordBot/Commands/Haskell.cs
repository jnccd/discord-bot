using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class Haskell : Command
    {
        public Haskell() : base("ghci", "Compiles Haskell Code", false)
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
                    File.WriteAllText("Commands\\Haskell\\input.hs", haskellInput);
                    Process compiler = new Process();
                    compiler.StartInfo.FileName = "Commands\\Haskell\\haskell.bat";
                    compiler.StartInfo.CreateNoWindow = true;
                    compiler.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    compiler.Start();

                    DateTime start = DateTime.Now;

                    Task.Factory.StartNew(async () => {
                        Thread.CurrentThread.Name = "Haskell Compiler";
                        compiler.WaitForExit();
                        try
                        {
                            if (File.Exists("Commands\\Haskell\\haskellCompile.txt"))
                            {
                                string[] output = File.ReadAllLines("Commands\\Haskell\\haskellCompile.txt");

                                Debug.WriteLine("Raw Haskell Output: ");
                                Debug.WriteLine(output.Aggregate((x, y) => x + "\n" + y));

                                string parsedOutput = "";

                                string workOutput = output.Aggregate((x, y) => x + "\n" + y);
                                while (workOutput.Contains(">"))
                                {
                                    string o = workOutput.GetEverythingBetween("> ", "Pre");
                                    if (!string.IsNullOrWhiteSpace(o))
                                        parsedOutput += o;
                                    workOutput = workOutput.Remove(0, workOutput.IndexOf('>') + 1);
                                }

                                parsedOutput = parsedOutput.Insert(0, "```ruby\n");
                                parsedOutput = parsedOutput.Insert(parsedOutput.Length, "```");

                                exited = true;
                                if (parsedOutput.Length >= 2000)
                                    await Program.SendText("That output was a little too long for Discords 2000 character limit.", message.Channel);
                                else if (string.IsNullOrWhiteSpace(parsedOutput.Trim('`')))
                                    await Program.SendText("Your code didn't create any output or an error occured!", message.Channel);
                                else
                                    await Program.SendText(parsedOutput, message.Channel);
                            }
                        }
                        catch (Exception e)
                        {
                            e.ToString().ConsoleWriteLine(ConsoleColor.Red);
                        }
                        exited = true;
                    });

                    while (!exited && (DateTime.Now - start).TotalSeconds < 30)
                        Task.Delay(100);
                    if (!exited)
                    {
                        exited = true;
                        try
                        {
                            compiler.Close();
                        }
                        catch { }
                        try
                        {
                            Process[] ghcs = Process.GetProcessesByName("ghc");
                            foreach (Process p in ghcs)
                            {
                                if ((DateTime.Now - p.StartTime).TotalSeconds < 5)
                                    p.Kill();
                            }
                        }
                        catch { }
                        Program.SendText("Haskell timeout!", message.Channel).Wait();
                    }
                }
                catch (Exception e) { e.ToString().ConsoleWriteLine(ConsoleColor.Red); }
            }

            return Task.FromResult(default(object));
        }
    }
}
