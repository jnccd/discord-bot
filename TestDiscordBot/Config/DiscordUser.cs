using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Config
{
    public class DiscordUser
    {
        public ulong UserID;
        public int Credits;
        public int TotalCommandsUsed;

        public DiscordUser()
        {
            
        }
        public DiscordUser(ulong UserID)
        {
            this.UserID = UserID;
        }

        public bool Equals(SocketUser User)
        {
            return User.Id == UserID;
        }
    }
}
