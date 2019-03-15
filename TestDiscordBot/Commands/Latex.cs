using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class Latex : Command
    {
        readonly string inputPath = "Commands\\Latex\\input.tex";
        readonly string batchPath = "Commands\\Latex\\latex.bat";
        readonly string outputPath = "Commands\\Latex\\output.png";

        public Latex() : base("latex", "Renders latex strings", false)
        {

        }

        public override Task Execute(SocketMessage message)
        {
            lock (this)
            {
                try
                {
                    string latex = message.Content.Split(new char[] { ' ', '\n' }).Skip(1).Aggregate((x, y) => x + " " + y);

                    if (!latex.StartsWith("\\documentclass["))
                        latex = "\\documentclass[preview,border=12pt]{standalone}\n\\usepackage{amsmath}\n\\usepackage{tikz}\n\\begin{document}\n" + latex + "\n\\end{document}";
                    
                    if (!File.Exists(inputPath))
                        File.Create(inputPath).Dispose();
                    File.WriteAllText(inputPath, latex);

                    Process converter = new Process();
                    converter.StartInfo.FileName = batchPath;
                    converter.StartInfo.UseShellExecute = false;
                    converter.StartInfo.RedirectStandardInput = true;

                    converter.Start();
                    Thread.Sleep(500);
                    converter.StandardInput.WriteLine("return");
                    converter.WaitForExit();

                    Global.SendFile(outputPath, message.Channel).Wait();
                }
                catch (Exception e) { Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
            }

            return Task.FromResult(default(object));
        }
    }
}
