using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class LeetTimeEvent : Command
    {
        public LeetTimeEvent() : base("e", "Be the first one to leet on 13:37!", false)
        {
            prefix = "Leet tim";
        }

        public override async Task execute(SocketMessage commandmessage)
        {
            if (DateTime.Now.TimeOfDay > new TimeSpan(13, 37, 0) && DateTime.Now.TimeOfDay < new TimeSpan(13, 38, 0))
            {
                await Global.SendText("Succsessful Leet-Time Detected!", commandmessage.Channel);
            }
            else
            {
                await Global.SendText("Its not the right time for that.", commandmessage.Channel);
            }
        }
    }
}
