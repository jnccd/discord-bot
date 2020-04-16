using Discord.WebSocket;
using MEE7.Backend;

namespace MEE7.Commands
{
    class AC : Command
    {
        public override void Execute(SocketMessage message) => new AnimalCrossing().Execute(message);
    }
}
