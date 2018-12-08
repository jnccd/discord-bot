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
        public EmojiUsage() : base("emojiUsage", "Tracks which emojis were used the most", false)
        {

        }

        public override void onNonCommandMessageRecieved(SocketMessage message)
        {
            if (message.Content.Count(x => x == ':') >= 2)
            {
                string emoji = message.Content.GetEverythingBetween(":", ":");
                ulong serverID = message.GetServerID();
                DiscordServer server = config.Data.ServerList.FirstOrDefault(x => x.ServerID == serverID);
                if (server == null || server.EmojiUsage == null)
                {
                    server = new DiscordServer(serverID);
                    config.Data.ServerList.Add(server);
                }

                if (server.EmojiUsage.ContainsKey(emoji))
                    server.EmojiUsage[emoji]++;
                else
                    server.EmojiUsage.Add(emoji, 1);
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
                await Global.SendEmbed(embed, message.Channel);
            }
        }
    }
}
