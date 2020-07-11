using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEE7.Commands
{
    class Nice : Command
    {
        public Nice()
        {

        }

        public override void Execute(IMessage message)
        {
            string[] split = message.Content.Split(' ');

            if (split.Length != 2)
                return;

            var id = split[1].Trim(new char[] { ' ', '<', '>', '@', '!' });
            try
            {
                if (split[1].Contains('#'))
                {
                    var users = message.Channel.GetUsersAsync().FlattenAsync().Result;
                    id = users.First(x => x.ToString() == split[1]).Id.ToString();
                }
                else if (id.Any(x => !char.IsDigit(x)))
                {
                    var users = message.Channel.GetUsersAsync().FlattenAsync().Result;
                    id = users.First(x => x.Username == split[1]).Id.ToString();
                }
            }
            catch
            {
                DiscordNETWrapper.SendText("Can't find that user :/", message.Channel).Wait();
                return;
            }

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
