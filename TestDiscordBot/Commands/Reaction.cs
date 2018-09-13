using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class Reaction : Command
    {
        public Reaction() : base("reaction", true)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            try
            {
                string[] Files = Directory.GetFiles(@"D:\Eigene Dateien\Medien\Bilder\Reactions");
                List<string> SendableFiles = new List<string>();
                foreach (string s in Files)
                {
                    if (Path.GetExtension(s) == ".jpg" || Path.GetExtension(s) == ".png" || Path.GetExtension(s) == ".jpeg" ||
                        Path.GetExtension(s) == ".gif" || Path.GetExtension(s) == ".mp4")
                        SendableFiles.Add(s);
                }
                string filepath = SendableFiles[Global.RDM.Next(SendableFiles.Count)];
                await Global.SendFile(filepath, commandmessage.Channel);
                Console.CursorLeft = 0;
                Console.WriteLine("Send reaction in " + commandmessage.Channel.Name + " for " + commandmessage.Author.Username + " from " + filepath);
                Console.Write("$");
            }
            catch (Exception e)
            {
                Console.CursorLeft = 0;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("$");
            }
        }
    }
}
