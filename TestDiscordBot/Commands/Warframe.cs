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

        public Warframe() : base("warframe", "Get notifications for warframe rewards", false)
        {
            server = new frmRServer(new frmRServer.ReceivedMessage(async (string text) => {
                try
                {
                    if (string.IsNullOrWhiteSpace(text))
                        return;

                    string[] split = text.Split('\n');
                    foreach (string line in split)
                        foreach (DiscordUser user in config.Data.UserList)
                            foreach (string filter in user.WarframeFilters)
                                if (line.Contains(filter))
                                {
                                    //IDMChannel dm = await Global.P.getUserFromId(user.UserID).GetOrCreateDMChannelAsync();
                                    //await dm.SendMessageAsync(line);

                                    SocketChannel channel = Global.P.getChannelFromID(user.WarframeChannelID);
                                    if (channel is ISocketMessageChannel)
                                        await Global.SendText(Global.P.getUserFromId(user.UserID).Mention + "\n" + line, (ISocketMessageChannel)channel);
                                }
                } catch (Exception e) {
                    Global.ConsoleWriteLine(e.ToString(), ConsoleColor.Red);
                }
            }));
        }

        public override async Task execute(SocketMessage message)
        {
            string[] split = message.Content.Split(new char[] { ' ', '\n' });
            DiscordUser user = config.Data.UserList.Find(x => x.UserID == message.Author.Id);
            user.WarframeChannelID = message.Channel.Id;
            if (split.Length == 1)
            {
                await Global.SendText("Use \"" + prefixAndCommand + " +FILTER\" to add a term to filter the alerts for.\n" +
                                      "Use \"" + prefixAndCommand + " -FILTER\" to remove a filter.\n" +
                                      "Use \"" + prefixAndCommand + " filters\" to view your fitlers.\n" +
                                      "eg. \"" + prefixAndCommand + " +FILTER\" Nitain\" to get notified for nitain alerts", message.Channel);
            }
            else if (split[1].StartsWith("+"))
            {
                string filter = split[1].Remove(0, 1);
                if (user.WarframeFilters.Contains(filter))
                    await Global.SendText("You already have that filter fam", message.Channel);
                else
                {
                    user.WarframeFilters.Add(filter);
                    await Global.SendText("Added filter: " + filter, message.Channel);
                }
            }
            else if (split[1].StartsWith("-"))
            {
                string filter = split[1].Remove(0, 1);
                if (user.WarframeFilters.Contains(filter))
                {
                    user.WarframeFilters.Remove(filter);
                    await Global.SendText("Removed filter: " + filter, message.Channel);
                }
                else
                    await Global.SendText("You don't even have that filter fam", message.Channel);
            }
            else if(split[1] == "filters")
            {
                await Global.SendText("Your filters: \n" + user.WarframeFilters.Aggregate((x, y) => x + "\n" + y) + (user.WarframeFilters.Count == 0 ? "\nWell that looks pretty emtpy" : ""), message.Channel);
            }
        }
    }
}
