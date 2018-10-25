using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestDiscordBot.XML;

namespace TestDiscordBot.Commands
{
    public class SubscribeToPatchNotes : Command
    {
        public SubscribeToPatchNotes() : base("togglePatchNotes", "Add this channel to the list of channels that will be notified when patch notes for this bot get published.", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            if (commandmessage.Author.Id == Global.P.getGuildFromChannel(commandmessage.Channel).OwnerId || commandmessage.Author.Id == Global.Master.Id)
            {
                if (config.Data.PatchNoteSubscribedChannels.Contains(commandmessage.Channel.Id))
                {
                    config.Data.PatchNoteSubscribedChannels.Remove(commandmessage.Channel.Id);
                    await Global.SendText("Canceled Patch Note subscribtion for this channel!", commandmessage.Channel);
                }
                else
                {
                    config.Data.PatchNoteSubscribedChannels.Add(commandmessage.Channel.Id);
                    await Global.SendText("Subscribed to Patch Notes!", commandmessage.Channel);
                }
            }
            else
            {
                await Global.SendText("Only the server/bot owner is authorized to use this command!", commandmessage.Channel);
            }
        }
    }
}
