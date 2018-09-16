using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot
{
    public class Command
    {
        public string desc;
        public string command;
        public bool isExperimental;

        public Command(string command, bool isExperimental)
        {
            this.command = command;
            this.isExperimental = isExperimental;
        }
        public Command(string command, string desc, bool isExperimental)
        {
            this.desc = desc;
            this.command = command;
            this.isExperimental = isExperimental;
        }

#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        public virtual async Task execute(SocketMessage commandmessage)
#pragma warning restore CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        {

        }
    }
}
