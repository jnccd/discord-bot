using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEE7.Commands
{
    class FunctionalEditCommands : EditCommandProvider
    {
        public string AddeDesc = "It adds E";
        public string Adde(string s, SocketMessage m, int count = 1)
        {
            return s + new string(Enumerable.Repeat('a', count).ToArray());
        }
    }
}
