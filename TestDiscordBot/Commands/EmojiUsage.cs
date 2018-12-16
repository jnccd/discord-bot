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
        
        public override void onNonCommandMessageRecieved(SocketMessage message)
        {
            int userIndex = config.Data.UserList.FindIndex(x => x.UserID == message.Author.Id);
            if (message.Content.Count(x => x == ':') >= 2 && userIndex >= 0 && DateTime.Now.Subtract(config.Data.UserList[userIndex].LastEmojiMessage).TotalMinutes > 5)
            {
                config.Data.UserList[userIndex].LastEmojiMessage = DateTime.Now;
                string emoji = message.Content.GetEverythingBetween(":", ":");

                if (emoji.Contains(" ") || emoji.Contains("\n"))
                    return;

                ulong serverID = message.GetServerID();
                DiscordServer server = config.Data.ServerList.FirstOrDefault(x => x.ServerID == serverID);
                if (server == null)
                {
                    server = new DiscordServer(serverID);
                    config.Data.ServerList.Add(server);
                }
                if (server.Emoji == null || server.Emoji.Count == 0)
                    server.Emoji = Global.P.getGuildFromID(server.ServerID).Emotes.Select(x => x.Name).ToList();
                
                if (server.Emoji.Contains(emoji) || emoji.StartsWith("GW"))
                {
                    if (server.EmojiUsage.ContainsKey(emoji))
                        server.EmojiUsage[emoji]++;
                    else
                        server.EmojiUsage.Add(emoji, 1);

                    server.EmojiUsage = server.EmojiUsage.OrderByDescending(x => x.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
                }
            }
        }

        public override async Task execute(SocketMessage message)
        {
            DiscordServer server = config.Data.ServerList.FirstOrDefault(x => x.ServerID == message.GetServerID());
            if (server == null)
                await Global.SendText("Impossible maybe the archives are incomplete!\nThis Server is not in my database yet.", message.Channel);
            else
            {
                EmbedBuilder embed = new EmbedBuilder();
                for (int i = 0; i < server.EmojiUsage.Keys.Count; i++)
                {
                    embed.AddField(":" + server.EmojiUsage.Keys.ElementAt(i) + ":", "Used " + server.EmojiUsage[server.EmojiUsage.Keys.ElementAt(i)] + " times!");
                }
                embed.WithColor(0, 128, 255);
                await Global.SendEmbed(embed, message.Channel);
            }
        }
    }
}
