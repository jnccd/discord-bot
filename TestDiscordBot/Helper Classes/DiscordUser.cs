using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot
{
    public class DiscordUser
    {
        public ulong UserID;
        public int currency;

        public DiscordUser()
        {
            UserID = 0;
            currency = 0;
        }
        public DiscordUser(ulong UserID)
        {
            this.UserID = UserID;
            currency = 0;
        }

        public bool Equals(SocketUser User)
        {
            return User.Id == UserID;
        }
    }
}
