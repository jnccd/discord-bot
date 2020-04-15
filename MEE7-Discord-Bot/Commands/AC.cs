using Discord.WebSocket;
using MEE7.Backend;
using System;
using System.Collections.Generic;
using System.Text;

namespace MEE7.Commands
{
    class AC : Command
    {
        public override void Execute(SocketMessage message) => new AnimalCrossing().Execute(message);
    }
}
