using Discord;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using MEE7.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MEE7.Commands
{
    public class EmojiUsage : Command
    {
        public EmojiUsage() : base("emojiUsage", "Which emojis are actually used on this server?", false)
        {
            Program.OnConnected += OnConnected;
            Program.OnNonCommandMessageRecieved += OnNonCommandMessageRecieved;
        }

        public void OnConnected()
        {
            foreach (DiscordServer server in Config.Data.ServerList)
                server.UpdateEmojis();
        }

        public void OnNonCommandMessageRecieved(IMessage message)
        {
            int userIndex = Config.Data.UserList.FindIndex(x => x.UserID == message.Author.Id);
            if (message.Content.Count(x => x == ':') >= 2 && userIndex >= 0 && DateTime.Now.Subtract(Config.Data.UserList[userIndex].LastEmojiMessage).TotalMinutes > 5)
            {
                Config.Data.UserList[userIndex].LastEmojiMessage = DateTime.Now;
                string emoji = message.Content.GetEverythingBetween(":", ":");

                ulong serverID = message.GetServerID();
                DiscordServer server = Config.Data.ServerList.FirstOrDefault(x => x.ServerID == serverID);
                if (server == null)
                    return;

                if (server.EmojiUsage.ContainsKey(emoji))
                    server.EmojiUsage[emoji]++;

                server.EmojiUsage = server.EmojiUsage.OrderByDescending((KeyValuePair<string, uint> x) => x.Value).ToDictionary((KeyValuePair<string, uint> pair) => pair.Key, (KeyValuePair<string, uint> pair) => pair.Value);
            }
        }

        public override void Execute(IMessage message)
        {
            DiscordServer server = Config.Data.ServerList.FirstOrDefault(x => x.ServerID == message.GetServerID());
            if (server == null)
                DiscordNETWrapper.SendText("Impossible maybe the archives are incomplete!\nThis Server is not in my database yet.", message.Channel).Wait();
            else
            {
                EmbedBuilder embed = new EmbedBuilder();
                for (int i = 0; i < server.EmojiUsage.Keys.Count; i++)
                {
                    embed.Fields.Add(new EmbedFieldBuilder()
                    {
                        Name = ":" + server.EmojiUsage.Keys.ElementAt(i) + ":",
                        Value = "Used " + server.EmojiUsage[server.EmojiUsage.Keys.ElementAt(i)] + " times!"
                    });
                }
                embed.WithColor(0, 128, 255);
                DiscordNETWrapper.SendEmbed(embed, message.Channel).Wait();
            }
        }
    }
}
