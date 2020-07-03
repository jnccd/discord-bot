using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using Newtonsoft.Json;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MEE7.Commands.MessageDB
{
    class MessageDB : Command
    {
        readonly string dbDirPath = $"Commands{Path.DirectorySeparatorChar}MessageDB{Path.DirectorySeparatorChar}";
        readonly string dbPath = $"Commands{Path.DirectorySeparatorChar}MessageDB{Path.DirectorySeparatorChar}db.json";
        readonly object dbLock = new object();

        class WeirdCommandThingy
        {
            public string name;
            public Action<DBGuild, IMessage, string[]> doStuffLul;
        }
        class EmoteCounter
        {
            public Emote e;
            public int messageHits;
            public int hitMessages;
            public int reactionHits;
            public int reactedMessages;
        }
        readonly WeirdCommandThingy[] commands = new WeirdCommandThingy[]
        {
            new WeirdCommandThingy()
            {
                name = "countMessages",
                doStuffLul = (DBGuild dbGuild, IMessage message, string[] args) => {
                    List<DBMessage> allGuildMessages = new List<DBMessage>();
                    foreach (var channel in dbGuild.TextChannels)
                        allGuildMessages.AddRange(channel.Messages);
                    DiscordNETWrapper.SendText(allGuildMessages.Count.ToString(), message.Channel).Wait();
                }
            },
            new WeirdCommandThingy()
            {
                name = "countChannelMessages",
                doStuffLul = (DBGuild dbGuild, IMessage message, string[] args) => {
                    string re = "";
                    foreach (var channel in dbGuild.TextChannels.OrderByDescending(x => x.Messages.Count).Take(10))
                        re += $"`{channel.Name}`: **{channel.Messages.Count}** messages\n";
                    DiscordNETWrapper.SendText(re, message.Channel).Wait();
                }
            },
            new WeirdCommandThingy()
            {
                name = "countUserMessages",
                doStuffLul = (DBGuild dbGuild, IMessage message, string[] args) => {
                    string re = "";

                    List<DBMessage> allGuildMessages = new List<DBMessage>();
                    foreach (var channel in dbGuild.TextChannels)
                        allGuildMessages.AddRange(channel.Messages);

                    var grouping = allGuildMessages.GroupBy(x => x.AuthorName);

                    foreach (var g in grouping.OrderByDescending(g => g.Count()).Take(10))
                        re += $"`{g.Key}`: **{g.Count()}** messages\n";

                    DiscordNETWrapper.SendText(re, message.Channel).Wait();
                }
            },
            new WeirdCommandThingy()
            {
                name = "getMostReactedToMessages",
                doStuffLul = (DBGuild dbGuild, IMessage message, string[] args) => {
                    List<DBMessage> allGuildMessages = new List<DBMessage>();
                    foreach (var channel in dbGuild.TextChannels)
                        allGuildMessages.AddRange(channel.Messages);

                    var top5 = allGuildMessages.OrderByDescending(x => x.Reactions.Sum(y => y.count)).Take(10);

                    DiscordNETWrapper.SendText(top5.Select(x => $"{x.Reactions.Sum(y => y.count)}: {x.Link}").Combine("\n"), message.Channel).Wait();
                }
            },
            new WeirdCommandThingy()
            {
                name = "plotActivityOverTime",
                doStuffLul = (DBGuild dbGuild, IMessage message, string[] args) => {
                    List<DBMessage> allGuildMessages = new List<DBMessage>();
                    try
                    {
                        ulong channelID = Convert.ToUInt64(args[0]);
                        allGuildMessages = dbGuild.TextChannels.First(x => x.Id == channelID).Messages;
                    }
                    catch
                    {
                        allGuildMessages.Clear();
                        foreach (var channel in dbGuild.TextChannels)
                            allGuildMessages.AddRange(channel.Messages);
                    }

                    if (allGuildMessages.Count == 0)
                    {
                        DiscordNETWrapper.SendText("No messages to plot :/", message.Channel).Wait();
                        return;
                    }

                    var messagesGroupedByDay = allGuildMessages.
                        OrderBy(x => x.Timestamp).
                        Select(x => new Tuple<DBMessage, string>(x, x.Timestamp.ToShortDateString())).
                        GroupBy(x => x.Item2).
                        Select(x => new Tuple<string, int>(x.Key, x.Count())).
                        ToArray();

                    Plot plt = new Plot();
                    plt.PlotScatter(
                        messagesGroupedByDay.
                        Select(x => DateTime.Parse(x.Item1).ToOADate()).
                        ToArray(),
                        messagesGroupedByDay.
                        Select(x => (double)x.Item2).
                        ToArray());
                    plt.Ticks(dateTimeX: true);
                    plt.Legend();
                    plt.Title("Server Activity over Time");
                    plt.YLabel("Messages send on that day");
                    plt.XLabel("Day");

                    DiscordNETWrapper.SendBitmap(plt.GetBitmap(), message.Channel).Wait();
                }
            },
            new WeirdCommandThingy()
            {
                name = "getMostUsedEmotes",
                doStuffLul = (DBGuild dbGuild, IMessage message, string[] args) => {
                    var serverMessages = dbGuild.TextChannels.SelectMany(x => x.Messages);

                    var emotes = Program.GetGuildFromID(dbGuild.Id).Emotes.Select(x => new EmoteCounter() { e=x }).ToArray();

                    foreach (var m in serverMessages)
                        foreach (var e in emotes)
                        {
                            var hits = m.Content.AllIndexesOf(e.e.Print()).Count;
                            e.messageHits += hits;
                            if (hits > 0)
                                e.hitMessages++;

                            DBReaction react;
                            if (m.Reactions != null && (react = m.Reactions.FirstOrDefault(x => x.id == e.e.Id)) != null)
                            {
                                e.reactionHits += react.count;
                                e.reactedMessages++;
                            }
                        }

                    emotes = emotes.OrderByDescending(x => x.messageHits).ToArray();

                    DiscordNETWrapper.SendText(emotes.Select(x => $"{x.e.Name} - {x.e.Print()} used {x.messageHits} times in {x.hitMessages} " +
                        $"messages and used as a reaction {x.reactionHits} times under {x.reactedMessages} messages").Combine("\n"), message.Channel).Wait();
                }
            },
            new WeirdCommandThingy()
            {
                name = "",
                doStuffLul = (DBGuild dbGuild, IMessage message, string[] args) => {

                }
            },
        };

        public MessageDB() : base("messageDB", "makes database stuff without databases because no", false, true)
        {

        }

        public override void Execute(IMessage message)
        {
            var split = message.Content.Split(' ');

            lock (dbLock)
            {
                if (split.Length <= 1)
                {
                    DiscordNETWrapper.SendText(
                        "Hi im a database, I save data. (secretly im just a chonky json file but psssst, tell no one)\n" +
                        "If you want to build a database for the discord server you are writing this message on type `$messageDB build`, " +
                        "but only trained professionals are allowed to use that command because it takes a lot of resources to exectue it\n" +
                       $"Other commands that you can use once a database has been build are: {commands.Select(x => $"`{x.name}`").Combine(", ")}", message.Channel).Wait();
                    return;
                }
                else if (split[1] == "build")
                {
                    if (message.Author.Id != Program.Master.Id)
                    {
                        DiscordNETWrapper.SendText("I'm sorry Dave, I'm afraid I can't do that", message.Channel).Wait();
                        return;
                    }

                    if (!File.Exists(dbPath))
                        return;

                    var curGuild = Program.GetGuildFromChannel(message.Channel);
                    DBGuild db = DBFromGuild(curGuild);
                    File.WriteAllText(GetFilePath(db), CreateJson(db));

                    DiscordNETWrapper.SendText("Server db built!", message.Channel).Wait();
                }
                else
                {
                    var command = commands.FirstOrDefault(x => x.name == split[1]);
                    if (command == null)
                        return;

                    DBGuild dbGuild = null;
                    if ((dbGuild = GetGuild(message)) == null)
                        return;

                    command.doStuffLul(dbGuild, message, split.Skip(2).ToArray());
                }
            }
        }

        DBGuild GetGuild(IMessage message)
        {
            var curGuild = Program.GetGuildFromChannel(message.Channel);
            if (!File.Exists(GetFilePath(curGuild)))
            {
                DiscordNETWrapper.SendText("I don't have data saved for this server yet :/", message.Channel).Wait();
                return null;
            }
            return ParseJson(File.ReadAllText(GetFilePath(curGuild)));
        }

        string GetFilePath(SocketGuild g) => $"{dbDirPath}{g.Id}db.json";
        string GetFilePath(DBGuild g) => $"{dbDirPath}{g.Id}db.json";

        DBGuild ParseJson(string json) => JsonConvert.DeserializeObject<DBGuild>(json);
        string CreateJson(DBGuild db) => JsonConvert.SerializeObject(db);

        public DBGuild DBFromGuild(SocketGuild guild)
        {
            DBGuild re = new DBGuild
            {
                CreatedAt = guild.CreatedAt.UtcDateTime,
                IconUrl = guild.IconUrl,
                Id = guild.Id,
                Name = guild.Name,
                OwnerId = guild.OwnerId,
                SplashUrl = guild.SplashUrl,

                TextChannels = new List<DBTextChannel>()
            };
            foreach (var textChannel in guild.TextChannels)
            {
                DBTextChannel reChannel = new DBTextChannel
                {
                    CreatedAt = textChannel.CreatedAt.UtcDateTime,
                    GuildId = re.Id,
                    Id = textChannel.Id,
                    Name = textChannel.Name,

                    Messages = new List<DBMessage>()
                };
                foreach (var message in DiscordNETWrapper.EnumerateMessages(textChannel as IMessageChannel))
                {
                    DBMessage reMessage = new DBMessage
                    {
                        Attachements = message.Attachments.Select(x => x.Url).ToArray(),
                        AuthorId = message.Author.Id,
                        AuthorName = message.Author.Username,
                        Channel = message.Channel.Id,
                        Content = message.Content,
                        Link = message.GetJumpUrl(),
                        Embeds = message.Embeds.Select(x => new DBEmbed()
                        {
                            AuthorURL = x.Author.HasValue ? x.Author.Value.Url : "",
                            Color = x.Color,
                            Desc = x.Description,
                            Fields = x.Fields.Select(y => new DBEmbedField()
                            {
                                Title = y.Name,
                                Content = y.Value
                            }).ToList(),
                            Footer = x.Footer.HasValue ? x.Footer.Value.Text : "",
                            ImageUrl = x.Image.HasValue ? x.Image.Value.Url : "",
                            ThumbnailUrl = x.Thumbnail.HasValue ? x.Thumbnail.Value.Url : "",
                            Timestamp = x.Timestamp.HasValue ? x.Timestamp.Value.UtcDateTime : DateTime.MinValue,
                            Title = x.Title
                        }).ToList(),
                        Reactions = message.Reactions.Select(x => new DBReaction()
                        {
                            id = x.Key is Emote ? (x.Key as Emote).Id : 0,
                            name = x.Key.Name,
                            print = x.Key.Print(),
                            count = x.Value.ReactionCount
                        }).ToList(),
                        Id = message.Id,
                        IsPinned = message.IsPinned,
                        MentionedChannels = message.MentionedChannelIds.ToList(),
                        MentionedRoles = message.MentionedRoleIds.ToList(),
                        MentionedUsers = message.MentionedUserIds.ToList(),
                        Timestamp = message.Timestamp.UtcDateTime
                    };

                    reChannel.Messages.Add(reMessage);
                }

                re.TextChannels.Add(reChannel);
            }

            return re;
        }
    }
}
