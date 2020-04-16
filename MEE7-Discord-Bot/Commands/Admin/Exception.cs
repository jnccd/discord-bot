using Discord.WebSocket;
using MEE7.Backend;
using System;

namespace MEE7.Commands
{
    public class ExceptionThrower : Command
    {
        public ExceptionThrower() : base("exception", "", false, true)
        {

        }

        public override void Execute(SocketMessage message)
        {
            if (message.Author.Id == Program.Master.Id)
                throw new Exception("Command failed successfully");
        }
    }
}
