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
        //private class SubCommand
        //{
        //    public SubCommand[] SubCommands;
        //    public string Command;
        //    public string Desc;
        //}

        public Command()
        {
            Desc = "-";
            CommandLine = this.GetType().Name;
            IsExperimental = true;
            IsHidden = true;
        }
        public Command(string command, string desc, bool isExperimental = false, bool isHidden = false)
        {
            Desc = desc;
            CommandLine = command;
            IsExperimental = isExperimental;
            IsHidden = isHidden;
        }
        
        public virtual void Execute(SocketMessage message)
        {
            
        }
    }
}
