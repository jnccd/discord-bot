using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class ProfilePicture : Command
    {
        public ProfilePicture() : base("profilePicture", "Gets the profile picture of a mentioned user.", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            SocketUser Target;
            if (commandmessage.MentionedUsers.Count > 0)
                Target = commandmessage.MentionedUsers.ElementAt(0);
            else
                Target = commandmessage.Author;

            ushort size = 512;
            try
            {
                size = Convert.ToUInt16(commandmessage.Content.Split(' ')[1]);
            }
            catch { }
            try
            {
                size = Convert.ToUInt16(commandmessage.Content.Split(' ')[2]);
            }
            catch { }

            await Global.SendText(Target.GetAvatarUrl(Discord.ImageFormat.Auto, size), commandmessage.Channel);
        }
    }
}
