using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using MEE7.Backend.HelperFunctions.Extensions;
using MEE7.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MEE7.Commands.SoftwareProjektLatexCD
{
    public class SPLatexCD : Command
    {
        bool run = true;
        readonly static string 
            gitdir = $"{Program.ExePath}Commands\\SoftwareProjektLatexCD\\", 
            batchPath = gitdir + "run.bat",
            latexOutDir = gitdir + "SoSe19_HRS3_105b\\pflichten\\LaTeX\\";
        IMessageChannel channel;

        public SPLatexCD() : base("SPLatexCD", "", false, true)
        {
            Program.OnConnected += OnConnected;
            
            Task.Run(() => {
                while (run)
                {
                    PullBuildSend();
                    Thread.Sleep(1 * 60 * 1000);
                }
            });
        }

        public void OnConnected()
        {
            channel = (IMessageChannel)Program.GetChannelFromID(500759857205346304);
        }

        public void PullBuildSend()
        {
            lock (this)
            {
                string gitput; bool error = false;
                

                if (string.IsNullOrWhiteSpace(Config.Data.ExtraStuff))
                    return;

                Directory.CreateDirectory(gitdir);
                Process P = new Process();
                if (Directory.EnumerateFileSystemEntries(gitdir).Count() == 0)
                    $"git clone https://{Config.Data.ExtraStuff}@git.informatik.uni-kiel.de/sopro/SoSe19_HRS3_105b.git".
                        RunAsConsoleCommand(50, () => { error = true; }, (string o, string e) => { gitput = o + e; }, (StreamWriter w) => { },
                        gitdir, P);
                else
                    $"git pull https://{Config.Data.ExtraStuff}@git.informatik.uni-kiel.de/sopro/SoSe19_HRS3_105b.git".
                        RunAsConsoleCommand(50, () => { error = true; }, (string o, string e) => { gitput = o + e; }, (StreamWriter w) => { },
                        gitdir, P);

                if (error)
                {
                    DiscordNETWrapper.SendText("Error pulling the data from git", channel).Wait();
                    return;
                }

                P = Process.Start(new ProcessStartInfo() { FileName = batchPath, UseShellExecute = false, CreateNoWindow = true });
                P.WaitForExit();

                DiscordNETWrapper.SendFile(latexOutDir + "output.pdf", channel).Wait();

                File.Delete(latexOutDir + "output.pdf");
                File.Delete(latexOutDir + "output.out");
                File.Delete(latexOutDir + "output.aux");
                File.Delete(latexOutDir + "output.toc");
                File.Delete(latexOutDir + "output.log");
            }
        }

        public override void Execute(SocketMessage message)
        {
            string[] input = message.Content.Split(new char[] { ' ', '\n' });
            if (input.Length > 1)
            {
                if (input[1] == "-stop" && message.Author.Id == Program.Master.Id)
                {
                    run = false;
                }
            }

            PullBuildSend();
        }
    }
}
