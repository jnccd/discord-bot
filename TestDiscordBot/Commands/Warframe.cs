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

        public struct notif
        {
            public List<ulong> userID;
            public ulong ChannelID;
            public string line;
        }

        public Warframe() : base("warframe", "Get notifications for warframe rewards", false)
        {
            server = new frmRServer(new frmRServer.ReceivedMessage(async (string text) => {
                try
                {
                    if (string.IsNullOrWhiteSpace(text) || !text.Contains('ↅ'))
                        return;

                    string[] encoding = text.Split('ↅ');
                    if (encoding.Length != 2)
                        throw new ArgumentException("Wrong encoding");

                    if (encoding[0] == "Update")
                    {
                        string[] split = encoding[1].Split('\n');
                        //foreach (string line in split)
                        //    foreach (DiscordUser user in config.Data.UserList)
                        //        foreach (string filter in user.WarframeFilters)
                        //            if (line.Contains(filter))
                        //            {
                        //                IDMChannel dm = await Global.P.getUserFromId(user.UserID).GetOrCreateDMChannelAsync();
                        //                await dm.SendMessageAsync(line);
                        //            }
                        List<notif> notifications = new List<notif>();
                        foreach (string line in split)
                            foreach (DiscordUser user in config.Data.UserList)
                                foreach (string filter in user.WarframeFilters)
                                    if (line.ContainsAllOf(filter.Split('&')))
                                        if (notifications.Exists(x => x.ChannelID == user.WarframeChannelID && x.line == line))
                                            notifications.Find(x => x.ChannelID == user.WarframeChannelID && x.line == line).userID.Add(user.UserID);
                                        else
                                            notifications.Add(new notif() { userID = new List<ulong>() { user.UserID }, ChannelID = user.WarframeChannelID, line = line });
                        foreach (notif n in notifications)
                            await Global.SendText(n.userID.Select(x => Global.P.getUserFromId(x).Mention).Aggregate((x, y) => x + " " + y) + n.line, n.ChannelID);
                    }
                    else if (encoding[0] == "Void-Trader")
                    {
                        List<ulong> channels = config.Data.UserList.Select(x => x.WarframeChannelID).Distinct().ToList();
                        foreach (ulong id in channels)
                        {
                            SocketChannel channel = Global.P.getChannelFromID(id);
                            if (channel is ISocketMessageChannel)
                                await Global.SendText(config.Data.UserList.Where(x => x.WarframeChannelID == id && x.WarframeFilters.Count != 0)
                                                                          .Select(x => Global.P.getUserFromId(x.UserID).Mention)
                                                                          .Aggregate((x, y) => x + " " + y) + " " + encoding[1], (ISocketMessageChannel)channel);
                        }
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
                await Global.SendText("```ruby" + 
                                      "Use \"" + prefixAndCommand + " +FILTER\" to add a term to filter the alerts for.\n" +
                                      "Use \"" + prefixAndCommand + " -FILTER\" to remove a filter.\n" +
                                      "Use \"" + prefixAndCommand + " filters\" to view your fitlers.\n" +
                                      "eg. \"" + prefixAndCommand + " +Nitain\" to get notified for nitain alerts\n" + 
                                      "Advanced shit: You can add and remove multiple filters in one command by seperating them with a ," + 
                                      "               You can also add a 'multifilter' by binding two or more filters together with a &" + 
                                      "               eg. \"+Detonite&Solaris\" to only get alerted for detonite injectors from solaris" +
                                      "```", message.Channel);
            }
            else if(split[1] == "filters")
                await Global.SendText("Your filters: \n" + (user.WarframeFilters.Count == 0 ?
                    "\n\nWell that looks pretty empty" :
                    user.WarframeFilters.Aggregate((x, y) => x + "\n" + y)), message.Channel);
            else
            {
                string answer = "";
                string[] filterComs = split.Skip(1).Aggregate((x, y) => x + " " + y).Split(',');
                foreach (string filterCom in filterComs)
                {
                    if (filterCom.StartsWith("+"))
                    {
                        string filter = filterCom.Remove(0, 1).Trim(' ');
                        if (user.WarframeFilters.Contains(filter))
                            answer += "You already have that filter fam";
                        else
                        {
                            user.WarframeFilters.Add(filter);
                            answer += "Added filter: " + filter + "\n";
                        }
                    }
                    else if (filterCom.StartsWith("-"))
                    {
                        string filter = filterCom.Remove(0, 1).Trim(' ');
                        if (user.WarframeFilters.Contains(filter))
                        {
                            user.WarframeFilters.Remove(filter);
                            answer += "Removed filter: " + filter + "\n";
                        }
                        else
                            answer += "You don't even have that filter fam";
                    }
                }
                await Global.SendText(answer + "\nYour Filters are now: \n" + (user.WarframeFilters.Count == 0 ?
                    "\n\nWell that looks pretty empty" : 
                    user.WarframeFilters.Aggregate((x, y) => x + "\n" + y)), message.Channel);
            }
        }
    }
}
