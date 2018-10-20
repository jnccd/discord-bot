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
        public LeetTimeEvent() : base("LeetTime", "Be the first one to leet after 13:37!", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            if (config.Data.LastLeetedDay.DayOfYear != DateTime.Now.DayOfYear && DateTime.Now.TimeOfDay > new TimeSpan(13, 37, 0))
            {
                await Global.SendText("", commandmessage.Channel);
            }
            else
            {
                //await Global.SendText("Someone already leeted!", commandmessage.Channel);
            }
        }
    }
}
