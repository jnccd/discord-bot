using Discord;
using MEE7.Backend.HelperFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEE7.Commands.Edit
{
    class MessageCommands : EditCommandProvider
    {
        public string getMediaLinksDesc = "Gets the messages attachment links";
        public string GetMediaLinks(IMessage inM, IMessage m)
        {
            return $"`{(inM.Attachments.Count == 0 ? "There weren't any :/" : inM.Attachments.Select(x => x.Url).Combine("\n"))}`";
        }
    }
}
