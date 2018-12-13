using Discord;
using Discord.WebSocket;
using RemotableObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestDiscordBot.Config;

namespace TestDiscordBot.Commands
{
    public class SubscribeToWarframeAlerts : Command
    {
        frmRServer server;

        public SubscribeToWarframeAlerts() : base("toggleWarframeAlerts", "Get annoying alerts for NITAIN", false)
        {
            server = new frmRServer(new frmRServer.ReceivedMessage((string text) => {
                try
                {
                    foreach (ulong ID in config.Data.WarframeSubscribedChannels)
                        Global.SendText("@everyone \n" + text, (ISocketMessageChannel)Global.P.getChannelFromID(ID));
                } catch (Exception e) {
                    Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red);
                }
            }));
        }

        public override async Task execute(SocketMessage message)
        {
            if (config.Data.WarframeSubscribedChannels == null)
                config.Data.WarframeSubscribedChannels = new List<ulong>();

            if (message.Author.Id == Global.P.getGuildFromChannel(message.Channel).OwnerId || message.Author.Id == Global.Master.Id)
            {
                if (config.Data.WarframeSubscribedChannels.Contains(message.Channel.Id))
                {
                    config.Data.WarframeSubscribedChannels.Remove(message.Channel.Id);
                    await Global.SendText("Canceled NITAIIIN subscription for this channel!", message.Channel);
                }
                else
                {
                    config.Data.WarframeSubscribedChannels.Add(message.Channel.Id);
                    await Global.SendText("Subscribed to NITAIIIN SPAM!", message.Channel);
                }
            }
            else
            {
                await Global.SendText("Only the server/bot owner is authorized to use this command!", message.Channel);
            }
        }
    }
}
