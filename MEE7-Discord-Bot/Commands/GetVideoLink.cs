using Discord;
using MEE7.Backend;

namespace MEE7.Commands
{
    class GetVideoLink : Command
    {
        public override void Execute(IMessage message) => Program.GetCommandInstance("edit").Execute(new SelfmadeMessage(message)
        {
            Content = $"{Program.Prefix}- lastM() > GetVideoLinks"
        });
    }
}
