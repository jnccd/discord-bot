using Discord;
using MEE7.Backend.HelperFunctions;
using System.Linq;

namespace MEE7.Commands.Edit
{
    class MessageCommands : EditCommandProvider
    {
        public string getContentDesc = "Gets the messages text";
        public string GetContent(IMessage inM, IMessage m)
        {
            return inM.Content;
        }

        public string getMediaLinksDesc = "Gets the messages attachment links";
        public string GetMediaLinks(IMessage inM, IMessage m)
        {
            return $"`{(inM.Attachments.Count == 0 ? "There weren't any :/" : inM.Attachments.Select(x => x.Url).Combine("\n"))}`";
        }

        public string getVideoLinksDesc = "Gets the messages video links";
        public string GetVideoLinks(IMessage inM, IMessage m)
        {
            return inM.Attachments.Where(x => x.Url.Contains(".mp4")).Count() == 0 ?
                "There weren't any :/" : inM.Attachments.Select(x => x.Url).Where(x => x.Contains(".mp4")).Combine("\n");
        }
    }
}
