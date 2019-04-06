using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class ExceptionThrower : Command
    {
        public ExceptionThrower() : base("exception", "", false, true)
        {

        }

        public override Task Execute(SocketMessage message)
        {
            throw new Exception();
        }
    }
}
