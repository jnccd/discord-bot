using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEE7.Commands;

namespace MEE7
{
    public class Command
    {
        public string Desc { get; private set; }
        public string CommandLine { get; private set; }
        public bool IsExperimental { get; private set; }
        public bool IsHidden { get; private set; }

        public string Prefix
        {
            get
            {
                return Program.prefix;
            }
        }
        public string PrefixAndCommand
        {
            get
            {
                return Prefix + CommandLine;
            }
        }

        public EmbedBuilder HelpMenu;
        public class SubCommand
        {
            public SubCommand[] SubCommands;
            public string Command;
            public string Desc;
        }

        public Command(string command, string desc, bool isExperimental)
        {
            Desc = desc;
            CommandLine = command;
            IsExperimental = isExperimental;
            IsHidden = false;
        }
        public Command(string command, string desc, bool isExperimental, bool isHidden)
        {
            Desc = desc;
            CommandLine = command;
            IsExperimental = isExperimental;
            IsHidden = isHidden;
        }
        
        public virtual Task Execute(SocketMessage message)
        {
            return Task.FromResult(default(object));
        }
    }
}
