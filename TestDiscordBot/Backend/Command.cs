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
        private string descP;
        private string commandP;
        private bool isExperimentalP;
        private bool isHiddenP;

        public string desc
        {
            get
            {
                return descP;
            }
        }
        public string prefix
        {
            get
            {
                return Global.prefix;
            }
        }
        public string command
        {
            get
            {
                return commandP;
            }
        }
        public bool isExperimental
        {
            get
            {
                return isExperimentalP;
            }
        }
        public bool isHidden
        {
            get
            {
                return isHiddenP;
            }
        }

        public Command(string command, string desc, bool isExperimental)
        {
            descP = desc;
            commandP = command;
            isExperimentalP = isExperimental;
            isHiddenP = false;
        }
        public Command(string command, string desc, bool isExperimental, bool isHidden)
        {
            descP = desc;
            commandP = command;
            isExperimentalP = isExperimental;
            isHiddenP = isHidden;
        }

        public virtual async Task execute(SocketMessage message)
        {

        }

        public virtual void onNonCommandMessageRecieved(SocketMessage message)
        {

        }
        public virtual void onConnected()
        {

        }
        public virtual void onExit()
        {

        }
    }
}
