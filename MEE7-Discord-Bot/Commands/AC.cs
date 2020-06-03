using Discord;
using MEE7.Backend;

namespace MEE7.Commands
{
    class AC : Command
    {
        public override void Execute(IMessage message) => new AnimalCrossing().Execute(message);
    }
}
