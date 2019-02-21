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
        public string Desc { get; private set; }
        public string CommandLine { get; private set; }
        public bool IsExperimental { get; private set; }
        public bool IsHidden { get; private set; }

        public string Prefix
        {
            get
            {
                return Global.prefix;
            }
        }
        public string PrefixAndCommand
        {
            get
            {
                return Prefix + CommandLine;
            }
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

        public virtual async Task execute(SocketMessage message)
        {
            
        }

        public virtual void OnNonCommandMessageRecieved(SocketMessage message)
        {

        }
        public virtual void OnConnected()
        {

        }
        public virtual void OnExit()
        {

        }
    }
}
