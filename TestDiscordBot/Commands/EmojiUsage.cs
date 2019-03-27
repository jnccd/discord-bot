using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestDiscordBot.Config;

namespace TestDiscordBot.Commands
{
    public class EmojiUsage : Command
    {
        public EmojiUsage() : base("emojiUsage", "Which emojis are actually used?", false)
        {
            
        }

        public override void OnConnected()
        {
            foreach (DiscordServer server in Config.Config.Data.ServerList)
                server.UpdateEmojis();
        }

        public override void OnNonCommandMessageRecieved(SocketMessage message)
        {
            int userIndex = Config.Config.Data.UserList.FindIndex(x => x.UserID == message.Author.Id);
            if (message.Content.Count(x => x == ':') >= 2 && userIndex >= 0 && DateTime.Now.Subtract(Config.Config.Data.UserList[userIndex].LastEmojiMessage).TotalMinutes > 5)
            {
                Config.Config.Data.UserList[userIndex].LastEmojiMessage = DateTime.Now;
                string emoji = message.Content.GetEverythingBetween(":", ":");

                ulong serverID = message.GetServerID();
                DiscordServer server = Config.Config.Data.ServerList.FirstOrDefault(x => x.ServerID == serverID);
                if (server == null)
                    return;

                if (server.EmojiUsage.ContainsKey(emoji))
                    server.EmojiUsage[emoji]++;

                server.EmojiUsage = server.EmojiUsage.OrderByDescending((KeyValuePair<string, uint> x) => x.Value).ToDictionary((KeyValuePair<string, uint> pair) => pair.Key, (KeyValuePair<string, uint> pair) => pair.Value);
            }
        }

        public override async Task Execute(SocketMessage message)
        {
            DiscordServer server = Config.Config.Data.ServerList.FirstOrDefault(x => x.ServerID == message.GetServerID());
            if (server == null)
                await Program.SendText("Impossible maybe the archives are incomplete!\nThis Server is not in my database yet.", message.Channel);
            else
            {
                EmbedBuilder embed = new EmbedBuilder();
                for (int i = 0; i < server.EmojiUsage.Keys.Count; i++)
                {
                    embed.AddField(":" + server.EmojiUsage.Keys.ElementAt(i) + ":", "Used " + server.EmojiUsage[server.EmojiUsage.Keys.ElementAt(i)] + " times!");
                }
                embed.WithColor(0, 128, 255);
                await Program.SendEmbed(embed, message.Channel);
            }
        }
    }
}
