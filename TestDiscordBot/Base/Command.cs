using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestDiscordBot.Commands;

namespace TestDiscordBot
{
    public class Command
    {
        public string desc;
        public string prefix;
        public string command;
        public bool isExperimental;
        
        public Command(string command, string desc, bool isExperimental)
        {
            this.desc = desc;
            this.command = command;
            this.isExperimental = isExperimental;
            prefix = Global.prefix;
        }
        
        public virtual async Task execute(SocketMessage commandmessage)
        {

        }
    }
}
