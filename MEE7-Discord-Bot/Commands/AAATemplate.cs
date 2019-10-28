using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEE7.Backend;

namespace MEE7.Commands
{
    public class Template : Command
    {
        public Template() : base("", "", isExperimental: false, isHidden: true)
        {

        }

        public override void Execute(SocketMessage message)
        {
            // <Insert command code here>
        }
    }
}
