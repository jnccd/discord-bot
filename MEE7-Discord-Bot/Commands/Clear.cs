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

            var toDelete = 0;
            try
            {
                toDelete = Convert.ToInt32(message.Content.Split()[1]);
            } 
            catch
            {
                DiscordNETWrapper.SendText("I need an amount of messages to delete!", message.Channel).Wait();
            }

            var toSkip = 0;
            try
            {
                toSkip = Convert.ToInt32(message.Content.Split()[2]);
            }
            catch { }

            try
            {
                ((ITextChannel)message.Channel).DeleteMessagesAsync(
                new IMessage[] { message }.Concat(
                DiscordNETWrapper.EnumerateMessages(message.Channel).
                Where(x => x.Id != message.Id).
                Skip(toSkip).
                Take(toDelete))).Wait();
            }
            catch
            {
                DiscordNETWrapper.SendText("Could not do the deleeto :/", message.Channel).Wait();
            }
        }
    }
}
