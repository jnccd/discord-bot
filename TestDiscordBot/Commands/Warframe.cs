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
    public class Warframe : Command
    {
        frmRServer server;
        string worldState;
        DateTime lastUpdated;

        public Warframe() : base("warframe", "Know when DE drops that NITAIIIN", false)
        {
            server = new frmRServer(new frmRServer.ReceivedMessage((string text) => {
                string[] split = text.Split('|');
                try
                {
                    if (split[0] != "")
                        foreach (ulong ID in config.Data.WarframeSubscribedChannels)
                            Global.SendText("@everyone \n" + split[0], (ISocketMessageChannel)Global.P.getChannelFromID(ID));
                    if (split.Length > 1)
                    {
                        worldState = split[1];
                        lastUpdated = DateTime.Now;
                    }
                } catch (Exception e) {
                    Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red);
                }
            }));
        }

        public override async Task execute(SocketMessage message)
        {
            string[] split = message.Content.Split(' ');
            if (split.Length == 1)
            {
                await Global.SendText("Use \"" + prefixAndCommand + " toggleNotify\" to toggle notifications", message.Channel);
                await Global.SendText("Use \"" + prefixAndCommand + " state\" to get worldstate information [WIP]", message.Channel);
            }
            else if (split[1] == "toggleNotify")
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
            else if (split[1] == "state")
            {
                await Global.SendText(worldState + "\nLast Updated: " + DateTime.Now.Subtract(lastUpdated), message.Channel);
            }
        }
    }
}
