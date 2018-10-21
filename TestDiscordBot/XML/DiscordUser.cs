using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.XML
{
    public class DiscordUser
    {
        public ulong UserID;
        public int Credits;

        public DiscordUser()
        {
            UserID = 0;
            Credits = 0;
        }
        public DiscordUser(ulong UserID)
        {
            this.UserID = UserID;
            Credits = 0;
        }

        public bool Equals(SocketUser User)
        {
            return User.Id == UserID;
        }
    }
}
