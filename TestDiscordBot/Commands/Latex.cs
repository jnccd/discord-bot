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
        public Latex() : base("latex", "Renders latex strings", false)
        {

        }

        public override async Task execute(SocketMessage message)
        {
            lock (this)
            {
                try
                {
                    string latex = message.Content.Split(new char[] { ' ', '\n' }).Skip(1).Aggregate((x, y) => x + " " + y);

                    if (!latex.StartsWith("\\documentclass["))
                        latex = "\\documentclass[preview,border=12pt]{standalone}\n\\usepackage{amsmath}\n\\usepackage{tikz}\n\\begin{document}\n" + latex + "\n\\end{document}";

                    File.WriteAllText("input.tex", latex);

                    Process converter = new Process();
                    converter.StartInfo.FileName = "latex.bat";
                    converter.StartInfo.UseShellExecute = false;
                    converter.StartInfo.RedirectStandardInput = true;
                    //converter.StartInfo.CreateNoWindow = true;
                    //converter.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    converter.Start();
                    Thread.Sleep(500);
                    converter.StandardInput.WriteLine("return");
                    converter.WaitForExit();

                    Global.SendFile(Global.CurrentExecutablePath + "\\output.png", message.Channel);
                }
                catch (Exception e) { Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red); }
            }
        }
    }
}
