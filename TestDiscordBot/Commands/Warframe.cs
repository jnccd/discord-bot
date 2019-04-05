using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using TestDiscordBot.Config;
using Warframe_Alerts;
using WarframeNET;

namespace TestDiscordBot.Commands
{
    public static partial class Extensions
    {
        public static string ToReadable(this TimeSpan t)
        {
            return string.Format("{0}{1}{2}{3}", t.Days > 0 ? t.Days + "d " : "",
                                                 t.Hours > 0 ? t.Hours + "h " : "",
                                                 t.Minutes > 0 ? t.Minutes + "m " : "",
                                                 t.Seconds > 0 ? t.Seconds + "s " : "0s ").Trim(' ');
        }
        public static string ToTitle(this Reward r)
        {
            List<string> inputs = new List<string> { (r.Items.Count == 0 ? "" : r.Items.Aggregate((x, y) => x + " " + y)),
                                                     (r.CountedItems.Count == 0 ? "" : r.CountedItems.Select(x => (x.Count > 1 ? x.Count + " " : "") + x.Type).Aggregate((x, y) => x + " " + y)),
                                                     (r.Credits == 0 ? "" : r.Credits + "c") };
            inputs.RemoveAll(x => string.IsNullOrWhiteSpace(x));
            return inputs.Count == 0 ? "" : inputs.Aggregate((x, y) => (x + " - " + y));
        }
        public static string ToTitle(this Alert a)
        {
            return a.Mission.Reward.ToTitle() + " - " + a.Mission.Node;
        }
        public static string ToTitle(this Invasion inv)
        {
            return inv.AttackingFaction + "(" + inv.AttackerReward.ToTitle() + ") vs. " + inv.DefendingFaction + "(" + inv.DefenderReward.ToTitle() + ") - " + inv.Node + " - " + inv.Description + " - " + inv.Completion + "%";
        }
        public static string ToTitle(this Fissure f)
        {
            return $"{f.Tier} {f.MissionType} on {f.Node} until {f.EndTime.ToLongTimeString()}";
        }
    }

    public class Warframe : Command
    {
        const int updateIntervalMin = 5;
        private readonly string lockject = "";

        public class Notif
        {
            public List<ulong> userID;
            public ulong ChannelID;
            public string line;
        }

        public Warframe() : base("warframe", "Get notifications for warframe rewards", false)
        {
            HelpMenu = new EmbedBuilder();
            HelpMenu.WithDescription("```ruby\n" +
                                      "Use \"" + PrefixAndCommand + " state\" to view the worldState.\n" +
                                      "Use \"" + PrefixAndCommand + " +FILTER\" to add a term to filter the alerts for.\n" +
                                      "Use \"" + PrefixAndCommand + " -FILTER\" to remove a filter.\n" +
                                      "Use \"" + PrefixAndCommand + " filters\" to view your filters.\n" +
                                      "eg. \"" + PrefixAndCommand + " +Nitain\" to get notified for nitain alerts\n" +
                                      "Advanced shit: You can add and remove multiple filters in one command by seperating them with a ,\n" +
                                      "               You can also add a 'multifilter' by binding two or more filters together with a &\n" +
                                      "               eg. \"+Detonite&Solaris\" to only get alerted for detonite injectors from solaris" +
                                      "```");
        }
        public override void OnConnected()
        {
            Task.Factory.StartNew(RunNotificationLoop);
        }
        public override async Task Execute(SocketMessage message)
        {
            string[] split = message.Content.Split(new char[] { ' ', '\n' });
            DiscordUser user = Config.Config.Data.UserList.Find(x => x.UserID == message.Author.Id);
            user.WarframeChannelID = message.Channel.Id;
            if (split.Length == 1)
                Program.SendEmbed(HelpMenu, message.Channel).Wait();
            else if(split[1] == "filters")
            {
                EmbedBuilder embed = new EmbedBuilder();
                embed.AddField("Your Filters: ", (user.WarframeFilters.Count == 0 ?
                    "Well that looks pretty empty" :
                    user.WarframeFilters.Aggregate((x, y) => x + "\n" + y)));
                await Program.SendEmbed(embed, message.Channel);
            }
            else if (split[1] == "state")
            {
                if (split.Length > 2)
                {
                    EmbedBuilder e = GetStateEmbeds().FirstOrDefault(x => x.Title.ToLower().Contains(split[2].ToLower()));
                    if (e == null)
                        await Program.SendText($"Could not find \"{split[2]}\"", message.Channel);
                    else
                        await Program.SendEmbed(e, message.Channel);
                }
                else
                    foreach (EmbedBuilder e in GetStateEmbeds())
                        await Program.SendEmbed(e, message.Channel);
            }
            else
            {
                EmbedBuilder embed = new EmbedBuilder();
                lock (lockject)
                {
                    string[] filterComs = split.Skip(1).Aggregate((x, y) => x + " " + y).Split(',');
                    foreach (string filterCom in filterComs)
                    {
                        string filterComTrim = filterCom.Trim(' ');
                        if (filterComTrim.StartsWith("+"))
                        {
                            string filter = filterComTrim.Remove(0, 1).Trim(' ');
                            if (user.WarframeFilters.Contains(filter))
                                embed.AddField("You already have that filter fam", filter);
                            else
                            {
                                user.WarframeFilters.Add(filter);
                                embed.AddField("Added filter: ", filter);
                            }
                        }
                        else if (filterComTrim.StartsWith("-"))
                        {
                            string filter = filterComTrim.Remove(0, 1).Trim(' ');
                            if (user.WarframeFilters.Contains(filter))
                            {
                                user.WarframeFilters.Remove(filter);
                                embed.AddField("Removed filter: ", filter);
                            }
                            else
                                embed.AddField("You don't even have that filter fam", filter);
                        }
                    }
                    embed.AddField("Your Filters are now: ", (user.WarframeFilters.Count == 0 ?
                        "\n\nWell that looks pretty empty" :
                        user.WarframeFilters.Aggregate((x, y) => x + "\n" + y)));
                }
                await Program.SendEmbed(embed, message.Channel);
            }
        }
        
        List<EmbedBuilder> GetStateEmbeds()
        {
            List<EmbedBuilder> re = new List<EmbedBuilder>();

            lock (lockject)
            {
                if (WarframeHandler.worldState.WS_Alerts.Count != 0)
                {
                    EmbedBuilder alerts = new EmbedBuilder();
                    alerts.WithColor(0, 128, 255);
                    alerts.WithTitle("Alerts:");
                    alerts.WithDescription(WarframeHandler.worldState.WS_Alerts.Select(x => x.ToTitle()).Aggregate((x, y) => x + "\n" + y));
                    re.Add(alerts);
                }
                
                if (WarframeHandler.worldState.WS_Invasions.Count != 0)
                {
                    EmbedBuilder invasions = new EmbedBuilder();
                    invasions.WithColor(0, 128, 255);
                    invasions.WithTitle("Invasions:");
                    foreach (Invasion inv in WarframeHandler.worldState.WS_Invasions.Where(x => !x.IsCompleted))
                        invasions.AddField($"{inv.AttackingFaction}({inv.AttackerReward.ToTitle()}) vs. {inv.DefendingFaction}({inv.DefenderReward.ToTitle()})",
                            $"{inv.Node} - {inv.Description} - {inv.Completion}%");
                    re.Add(invasions);
                }
                
                if (WarframeHandler.worldState.WS_Fissures.Count != 0)
                {
                    EmbedBuilder fissures = new EmbedBuilder();
                    fissures.WithColor(0, 128, 255);
                    fissures.WithTitle("Fissures:");
                    fissures.WithDescription(WarframeHandler.worldState.WS_Fissures.OrderBy(x => x.TierNumber).
                        Select(f => f.Tier + " - " + f.MissionType + " - " + (f.EndTime.ToLocalTime() - DateTime.Now).ToReadable()).
                        Aggregate((x, y) => x + "\n" + y));
                    re.Add(fissures);
                }
                
                EmbedBuilder voidtrader = new EmbedBuilder();
                voidtrader.WithColor(0, 128, 255);
                if (WarframeHandler.worldState.WS_VoidTrader.Inventory.Count == 0)
                    voidtrader.WithTitle("Baro-Senpai is currently gone\nBut don't despair he will come back at " + WarframeHandler.worldState.WS_VoidTrader.StartTime);
                else
                {
                    voidtrader.WithTitle("Baro-senpai is here :weary:\nBut only until" + WarframeHandler.worldState.WS_VoidTrader.EndTime);
                    foreach (VoidTraderItem item in WarframeHandler.worldState.WS_VoidTrader.Inventory)
                        voidtrader.AddField(item.Item, $"{item.Credits}:moneybag: {item.Ducats}D");
                }
                re.Add(voidtrader);

                if (WarframeHandler.worldState.WS_NightWave.ActiveChallenges.Count != 0)
                {
                    EmbedBuilder nightwave = new EmbedBuilder();
                    nightwave.WithColor(0, 128, 255);
                    nightwave.WithTitle($"Nightwave Challenges: ");
                    nightwave.WithDescription($"Season {WarframeHandler.worldState.WS_NightWave.Season} Phase " +
                        $"{WarframeHandler.worldState.WS_NightWave.Phase}");
                    foreach (NightwaveChallenge x in WarframeHandler.worldState.WS_NightWave.ActiveChallenges)
                        nightwave.AddField($"{x.Title} - {x.Desc}", $"{x.Reputation} :arrow_up: until {x.Expiry}");
                    re.Add(nightwave);
                }
                
                if (WarframeHandler.worldState.WS_Sortie.Variants.Count != 0)
                {
                    EmbedBuilder sortie = new EmbedBuilder();
                    sortie.WithColor(0, 128, 255);
                    sortie.WithTitle("Sortie:");
                    sortie.WithDescription(WarframeHandler.worldState.WS_Sortie.Variants.
                        Select(x => x.MissionType + " on " + x.Node + " with " + x.Modifier + "\n" + x.ModifierDescription).
                        Aggregate((x, y) => x + "\n\n" + y));
                    re.Add(sortie);
                }
                
                if (WarframeHandler.worldState.WS_Events.Count != 0)
                {
                    EmbedBuilder events = new EmbedBuilder();
                    events.WithColor(0, 128, 255);
                    events.WithTitle("Events:");
                    events.WithDescription(WarframeHandler.worldState.WS_Events.
                        Select(x => x.Description + " - Until: " + x.EndTime.ToLongDateString() + " - " + x.Rewards.
                            Select(y => y.ToTitle()).
                            Foldl("", (a, b) => a + " " + b).Trim(' ').Trim('-')).
                        Foldl("", (x, y) => x + "\n" + y));
                    re.Add(events);
                }

                {
                    EmbedBuilder cycles = new EmbedBuilder();
                    cycles.WithColor(0, 128, 255);
                    cycles.WithTitle("Cycles:");
                    cycles.AddField("Cetus: ", WarframeHandler.worldState.WS_CetusCycle.TimeOfDay() + " " +
                        (WarframeHandler.worldState.WS_CetusCycle.Expiry.ToLocalTime() - DateTime.Now).ToReadable());
                    cycles.AddField("Fortuna: ", WarframeHandler.worldState.WS_FortunaCycle.Temerature() + " " +
                        (WarframeHandler.worldState.WS_FortunaCycle.Expiry.ToLocalTime() - DateTime.Now).ToReadable());
                    re.Add(cycles);
                }

                {
                    EmbedBuilder syndicates = new EmbedBuilder();
                    syndicates.WithColor(0, 128, 255);
                    syndicates.WithTitle("Syndicate Missions: ");
                    foreach (SyndicateMission mission in WarframeHandler.worldState.WS_SyndicateMissions.Where(x => x.jobs != null && x.jobs.Count > 0))
                    {
                        syndicates.AddField(mission.Syndicate, "Missions: ");
                        for (int i = 0; i < mission.jobs.Count; i++)
                            if (mission.jobs[i].rewardPool != null)
                                syndicates.AddField("Mission #" + (i + 1), $"{mission.jobs[i].rewardPool.Aggregate((x, y) => y == "" ? x : x + ", " + y)} and " +
                                    $"{mission.jobs[i].standingStages.Sum()} :arrow_up: until {mission.EndTime.ToLocalTime().ToLongTimeString()}");
                    }
                    re.Add(syndicates);
                }
            }
            
            return re;
        }
        void RunNotificationLoop()
        {
            Thread.CurrentThread.Name = "Warframe Notification Loop";

            while (true)
            {
                NotifyAlerts();
                Thread.Sleep(updateIntervalMin * 60000);
            }
        }
        void NotifyAlerts()
        {
            lock (lockject)
            {
                if (UpdatedWarframeHandlerSuccessfully())
                {
                    NotifyVoidtrader();
                    SendNotifications(GetNotifications());
                }

                while (Config.Config.Data.WarframeIDList.Count > 400)
                    Config.Config.Data.WarframeIDList.RemoveAt(0);
            }
        }
        bool UpdatedWarframeHandlerSuccessfully()
        {
            string status = "";
            string jsonResponse = WarframeHandler.GetJson(ref status);
            if (status != "OK") return false;
            WarframeHandler.GetJsonObjects(jsonResponse);
            return true;
        }
        void NotifyVoidtrader()
        {
            if (!Config.Config.Data.WarframeVoidTraderArrived && WarframeHandler.worldState.WS_VoidTrader.Inventory.Count != 0)
            {
                List<ulong> channels = Config.Config.Data.UserList.Select(x => x.WarframeChannelID).Distinct().ToList();

                EmbedBuilder embed = new EmbedBuilder();
                embed.WithDescription("Void trader arrived at " + WarframeHandler.worldState.WS_VoidTrader.Location + " with: ");
                embed.WithFooter("He will leave again at " + WarframeHandler.worldState.WS_VoidTrader.EndTime);
                embed.WithTitle("Baro-senpai is here :weary:");
                embed.WithTimestamp(new DateTimeOffset(WarframeHandler.worldState.WS_VoidTrader.EndTime));
                foreach (VoidTraderItem item in WarframeHandler.worldState.WS_VoidTrader.Inventory)
                    embed.AddField(item.Item, "[Link](https://warframe.fandom.com/wiki/Special:Search?query=" +
                             HttpUtility.HtmlEncode(item.Item).Replace(' ', '+') + ") - " + item.Credits + "c " + item.Ducats + "D");

#if !DEBUG
                foreach (ulong id in channels)
                {
                    SocketChannel channel = Program.GetChannelFromID(id);
                    if (channel is ISocketMessageChannel)
                        Program.SendEmbed(embed, (ISocketMessageChannel)channel).Wait();
                }
#endif
            }
            Config.Config.Data.WarframeVoidTraderArrived = WarframeHandler.worldState.WS_VoidTrader.Inventory.Count != 0;
        }
        List<string> GetNotifications()
        {
            List<string> notifications = new List<string>();
            
            foreach (SyndicateMission mission in WarframeHandler.worldState.WS_SyndicateMissions)
                for (int i = 0; i < mission.jobs.Count; i++)
                    if (mission.jobs[i].rewardPool != null && !Config.Config.Data.WarframeIDList.Contains(mission.jobs[i].id))
                    {
                        Config.Config.Data.WarframeIDList.Add(mission.jobs[i].id);
                        foreach (string reward in mission.jobs[i].rewardPool)
                        {
                            notifications.Add(reward + " currently available from the " + mission.Syndicate + "'s " + (i + 1) + ". bounty until " + mission.EndTime.ToLocalTime().ToLongTimeString());
                        }
                    }
            
            foreach (Alert a in WarframeHandler.worldState.WS_Alerts)
                if (!Config.Config.Data.WarframeIDList.Contains(a.Id))
                {
                    Config.Config.Data.WarframeIDList.Add(a.Id);
                    notifications.Add(a.ToTitle() + " - Expires at " + a.EndTime.ToLocalTime().ToLongTimeString() + ", so in " + (int)(a.EndTime.ToLocalTime() - DateTime.Now).TotalMinutes + " minutes");
                }
            foreach (Invasion i in WarframeHandler.worldState.WS_Invasions)
                if (!Config.Config.Data.WarframeIDList.Contains(i.Id) && !i.IsCompleted)
                {
                    Config.Config.Data.WarframeIDList.Add(i.Id);
                    notifications.Add(i.ToTitle());
                }
            foreach (Fissure f in WarframeHandler.worldState.WS_Fissures)
                if (!Config.Config.Data.WarframeIDList.Contains(f.Id))
                {
                    Config.Config.Data.WarframeIDList.Add(f.Id);
                    notifications.Add(f.ToTitle());
                }

            return notifications;
        }
        async void SendNotifications(List<string> textNotifications)
        {
            List<Notif> notifications = new List<Notif>();
            foreach (string line in textNotifications)
                foreach (DiscordUser user in Config.Config.Data.UserList)
                    foreach (string filter in user.WarframeFilters)
                        if (BooleanContainsAllOf(line, filter))
                        {
                            Notif notification = notifications.FirstOrDefault(x => x.ChannelID == user.WarframeChannelID && x.line == line);
                            if (notification != null && !notification.userID.Contains(user.UserID))
                                notification.userID.Add(user.UserID);
                            else
                                notifications.Add(new Notif() { userID = new List<ulong>() { user.UserID }, ChannelID = user.WarframeChannelID, line = line });
                        }
            foreach (Notif n in notifications)
                await Program.SendText(n.userID.Select(x => Program.GetUserFromId(x).Mention).Aggregate((x, y) => x + " " + y) + "\n" + n.line, n.ChannelID);
        }
        bool BooleanContainsAllOf(string s, string match)
        {
            string[] matches = match.Split('&');
            foreach (string m in matches)
            {
                if (m[0] == '!')
                {
                    if (s.Contains(m.Remove(0, 1)))
                        return false;
                }
                else if (!s.Contains(m))
                    return false;
            }
            return true;
        }
    }
}
