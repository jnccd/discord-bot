using Discord;
using MEE7.Backend;

namespace MEE7.Commands
{
    class EditShortcut : Command
    {
        public EditShortcut() : base("-", "Shortcut to the edit command", isExperimental: false, isHidden: true)
        {

        }

        public override void Execute(IMessage message) => Program.GetCommandInstance("edit").Execute(message);
    }
}
