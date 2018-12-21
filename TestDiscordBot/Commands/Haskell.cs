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

        public override async Task execute(SocketMessage message)
        {
            lock (this)
            {
                try
                {
                    bool exited = false;

                    string[] split = message.Content.Split(new char[] { ' ', '\n' });
                    foreach (string s in split)
                        if (s.ContainsOneOf(new string[] { "Write", "write", "read", "Read", "Path", "path", "Directory", "directory", "open", "Open", "import",
                        "File", "file", "Handle", "handle", "System", "system", "unsafe", "seq", "Profunctor", "Windows", "windows" }) || s.Length > 1 && s[0] == ':' && char.IsLetter(s[1]))
                        {
                            Global.SendText("Your code contains commands you don't have permission to use!\nAt: " + s, message.Channel);
                            return;
                        }

                    string haskellInput = split.Skip(1).Aggregate((x, y) => { return x + " " + y; }).Trim('`');
                    if (haskellInput.StartsWith("haskell"))
                        haskellInput.Remove(0, "haskell".Length);
                    File.WriteAllText("input.hs", haskellInput);
                    Process compiler = new Process();
                    compiler.StartInfo.FileName = "haskell.bat";
                    compiler.StartInfo.CreateNoWindow = true;
                    compiler.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    compiler.Start();

                    DateTime start = DateTime.Now;

                    Task.Factory.StartNew(async () => {
                        compiler.WaitForExit();
                        if (exited)
                            return;
                        try
                        {
                            if (File.Exists("haskellCompile.txt"))
                            {
                                string[] output = File.ReadAllLines("haskellCompile.txt");

                                Debug.WriteLine("Raw Haskell Output: ");
                                Debug.WriteLine(output.Aggregate((x, y) => x + "\n" + y));

                                if (output.Length <= 2)
                                    await Global.SendText("Your code didn't create any output or an error occured!", message.Channel);
                                else
                                {
                                    string shortenedOutput = output.ToList().GetRange(1, output.Length - 2).Select((x) => (x.Split('>').Last())).
                                    Aggregate((x, y) => { return x + "\n" + y; }).Remove(0, 1);

                                    shortenedOutput = output.Aggregate((x, y) => x + "\n" + y);

                                    shortenedOutput = shortenedOutput.Insert(0, "```");
                                    shortenedOutput = shortenedOutput.Insert(shortenedOutput.Length, "```");
                                    if (shortenedOutput.Length < 2000)
                                        await Global.SendText(shortenedOutput, message.Channel);
                                    else
                                        await Global.SendText("That output was a little too long for Discords 2000 character limit.", message.Channel);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red);
                        }
                        exited = true;
                    });

                    while (!exited && (DateTime.Now - start).TotalSeconds < 3)
                        Thread.Sleep(100);
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
                        Global.SendText("Haskell timeout!", message.Channel);
                    }
                }
                catch (Exception e) { Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
            }
        }
    }
}
