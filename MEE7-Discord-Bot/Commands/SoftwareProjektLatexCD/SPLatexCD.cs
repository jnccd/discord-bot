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
        readonly static string 
            gitdir = $"{Program.ExePath}Commands\\SoftwareProjektLatexCD\\", 
            batchPath = gitdir + "run.bat",
            latexOutDir = gitdir + "SoSe19_HRS3_105b\\pflichten\\LaTeX\\output.pdf";

        public SPLatexCD() : base("", "", false, true)
        {
            Task.Run(() => {
                while (true)
                {
                    PullBuildSend();
                    Thread.Sleep(15 * 60 * 1000);
                }
            });
        }

        public void PullBuildSend()
        {
            lock (this)
            {
                string gitput; bool error = false;
                IMessageChannel channel = (IMessageChannel)Program.GetChannelFromID(500759857205346304);

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

                Process.Start(new ProcessStartInfo() { FileName = batchPath, UseShellExecute = false, CreateNoWindow = true });

                DiscordNETWrapper.SendFile(latexOutDir, channel).Wait();
            }
        }

        public override void Execute(SocketMessage message)
        {
            PullBuildSend();
        }
    }
}
