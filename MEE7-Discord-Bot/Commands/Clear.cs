using Discord;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System;
using System.Linq;

namespace MEE7.Commands
{
    class Clear : Command
    {
        public override void Execute(IMessage message)
        {
            var g = Program.GetGuildFromChannel(message.Channel);
            if (message.Author.Id != g.Owner.Id && message.Author.Id != Program.Master.Id
                && !g.Roles.Where(x => x.Members.FirstOrDefault(x => x.Id == message.Author.Id) != null).Any(x => x.Permissions.ManageMessages))
            {
                DiscordNETWrapper.SendText("You do not have permissions to use this command!", message.Channel).Wait();
                return;
            }

            var split = message.Content.Split();

            if (split.Length <= 1)
            {
                DiscordNETWrapper.SendText("I need an amount of messages to delete!", message.Channel).Wait();
                return;
            }

            var toDelete = 0;
            try
            {
                toDelete = Convert.ToInt32(message.Content.Split()[1]) + 1;
            } 
            catch
            {
                DiscordNETWrapper.SendText("I need an amount of messages to delete!", message.Channel).Wait();
            }

            ((ITextChannel)message.Channel).DeleteMessagesAsync(DiscordNETWrapper.EnumerateMessages(message.Channel).Take(toDelete)).Wait();
        }
    }
}
