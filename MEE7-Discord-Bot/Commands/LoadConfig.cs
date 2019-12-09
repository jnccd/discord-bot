using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEE7.Commands
{
    public class LoadConfig : Command
    {
        public LoadConfig() : base("loadConfig", "loads the attached config", isExperimental: true, isHidden: true)
        {

        }

        public override void Execute(SocketMessage message)
        {
            if (message.Author.Id == Program.Master.Id)
            {
                var url = message.Attachments.First().Url;
                var json = url.GetHTMLfromURL();
                Configuration.Config.LoadFrom(json);
            }
        }
    }
}
