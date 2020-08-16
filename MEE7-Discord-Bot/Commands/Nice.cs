using Discord;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System.Linq;

namespace MEE7.Commands
{
    class Nice : Command
    {
        public Nice() : base("nice", "Check if a user is nice", isHidden: false)
        {

        }

        public override void Execute(IMessage message)
        {
            string[] split = message.Content.Split(' ');

            if (split.Length != 2)
                return;

            if (split[1] == "nicest")
            {
                var users = message.Channel.GetUsersAsync().FlattenAsync().Result;
                var nicest = users.MaxElement(x => x.Id.ToString().AllIndexesOf("69").Count);
                DiscordNETWrapper.SendText("The nicest user here is " + nicest.Username, message.Channel).Wait();
                return;
            }

            var user = DiscordNETWrapper.ParseUser(split.Skip(1).Combine(" "), message.Channel);

            if (user == null)
            {
                DiscordNETWrapper.SendText("Couldn't find that user", message.Channel).Wait();
                return;
            }

            var id = user.Id.ToString();
            var nices = id.AllIndexesOf("69").Count;

            if (nices == 0)
                DiscordNETWrapper.SendText("This user is not nice", message.Channel).Wait();
            else if (nices == 1)
                DiscordNETWrapper.SendText("This user is nice", message.Channel).Wait();
            else if (nices == 2)
                DiscordNETWrapper.SendText("This user double nice!", message.Channel).Wait();
            else
                DiscordNETWrapper.SendText("This user is INCREDIBLY nice! Give them a hug", message.Channel).Wait();
        }
    }
}
