using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEE7.Commands
{
    public class ProfilePicture : Command
    {
        public ProfilePicture() : base("profilePicture", "Gets the profile picture source", false)
        {

        }

        public override async Task Execute(SocketMessage commandmessage)
        {
            SocketUser target;
            if (commandmessage.MentionedUsers.Count > 0)
                target = commandmessage.MentionedUsers.ElementAt(0);
            else
                target = commandmessage.Author;

            ushort size = 512;
            try
            {
                size = Convert.ToUInt16(commandmessage.Content.Split(new char[] { ' ', '\n' })[1]);
            }
            catch { }
            try
            {
                size = Convert.ToUInt16(commandmessage.Content.Split(new char[] { ' ', '\n' })[2]);
            }
            catch { }

            try
            {
                await Program.SendText(target.GetAvatarUrl(Discord.ImageFormat.Auto, size), commandmessage.Channel);
            }
            catch
            {
                await Program.SendText("That guy doesn't have a profile Picture UwU", commandmessage.Channel);
            }
        }
    }
}
