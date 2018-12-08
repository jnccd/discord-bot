using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Config
{
    public class DiscordServer
    {
        public ulong ServerID = 0;
        public Dictionary<string, uint> EmojiUsage = new Dictionary<string, uint>();

        public DiscordServer()
        {

        }
        public DiscordServer(ulong ServerID)
        {
            this.ServerID = ServerID;
        }
    }
}
