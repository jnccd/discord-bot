using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class GetTimecode : Command
    {
        public GetTimecode() : base("getTimestamp", "Gets the exact timestamp of a message", false)
        {

        }

        public override async Task execute(SocketMessage message)
        {
            string[] split = message.Content.Split(' ');
            if (split.Length == 1)
                await Global.SendText("I need a messageID!", message.Channel);
            else if (split.Length == 2)
            {
                string messageID = split[1].Split('/').Last();
                IMessage m = await message.Channel.GetMessageAsync(Convert.ToUInt64(messageID));
                await Global.SendText("Posted: " + m.Timestamp + "\nEdited: " + m.EditedTimestamp, message.Channel);
            }
            else
                await Global.SendText("Thats too many parameters, I only need 2!", message.Channel);
        }
    }
}
