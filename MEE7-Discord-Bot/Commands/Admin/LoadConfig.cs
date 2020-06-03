using Discord;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System.Linq;

namespace MEE7.Commands
{
    public class LoadConfig : Command
    {
        public LoadConfig() : base("loadConfig", "loads the attached config", isExperimental: true, isHidden: true)
        {

        }

        public override void Execute(IMessage message)
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
