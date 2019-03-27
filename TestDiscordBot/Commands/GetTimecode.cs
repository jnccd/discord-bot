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
        public GetTimecode() : base("getTimestamp", "Get exact timestamps", false)
        {

        }

        public override async Task Execute(SocketMessage message)
        {
            string[] split = message.Content.Split(new char[] { ' ', '\n' });
            if (split.Length == 1)
                await Program.SendText("I need a messageID!", message.Channel);
            else if (split.Length == 2)
            {
                string messageID = split[1].Split('/').Last();

                ulong id = 0;
                try { id = Convert.ToUInt64(messageID); }
                catch { await Program.SendText("The messageID needs to be a number!", message.Channel); return; }

                if (id == 0) { await Program.SendText("The messageID can't be 0!", message.Channel); return; }

                IMessage m = await message.Channel.GetMessageAsync(id);
                if (m == null)
                    await Program.SendText("I can't find that message!", message.Channel);
                else
                    await Program.SendText("Posted: " + m.Timestamp + "\nEdited: " + m.EditedTimestamp, message.Channel);
            }
            else
                await Program.SendText("Thats too many parameters, I only need 2!", message.Channel);
        }
    }
}
