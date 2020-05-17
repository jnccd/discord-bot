using Discord.WebSocket;
using MEE7.Backend;
using System;
using System.Collections.Generic;
using System.Text;

namespace MEE7.Commands
{
    class EditShortcut : Command
    {
        public EditShortcut() : base("-", "Shortcut to the edit command", isExperimental: false, isHidden: true)
        {

        }

        public override void Execute(SocketMessage message) => new Edit().Execute(message);
    }
}
