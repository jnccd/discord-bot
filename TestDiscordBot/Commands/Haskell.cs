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
        public Haskell() : base("ghci", "Compiles Haskell Code", true)
        {

        }

        public override async Task execute(SocketMessage message)
        {
            lock (this)
            {
                bool exited = false;

                if (message.Content.Contains("IO") || message.Content.Contains("Write") || message.Content.Contains("write") || message.Content.Contains("import") ||
                message.Content.Contains("open") || message.Content.Contains("File") || message.Content.Contains("handle") || message.Content.Contains("System"))
                {
                    Global.SendText("Your code contains commands you don't have permission to use!", message.Channel);
                    return;
                }

                File.WriteAllText("input.hs", message.Content.Split(' ').Skip(1).Aggregate((x, y) => { return x + " " + y; }));
                Process compiler = new Process();
                compiler.StartInfo.FileName = "haskell.bat";
                compiler.StartInfo.CreateNoWindow = true;
                compiler.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                compiler.Start();

                DateTime start = DateTime.Now;
                
                Task.Factory.StartNew(async () => {
                    compiler.WaitForExit();
                    if (File.Exists("haskellCompile.txt"))
                    {
                        string output = File.ReadAllText("haskellCompile.txt");
                        await Global.SendText(output.Split('\n').Skip(1).Aggregate((x, y) => { return x + "\n" + y; }), message.Channel);
                    }
                    exited = true;
                });
                
                while (!exited && (DateTime.Now - start).TotalSeconds < 3)
                    Thread.Sleep(100);
                if (!exited)
                {
                    try {
                        compiler.Close();
                    } catch { }
                    try
                    {
                        Process[] ghcs = Process.GetProcessesByName("ghc");
                        foreach (Process p in ghcs)
                        {
                            if ((DateTime.Now - p.StartTime).TotalSeconds < 5)
                                p.Kill();
                        }
                    } catch { }
                    Global.SendText("Haskell timeout!", message.Channel);
                }
            }
        }
    }
}
