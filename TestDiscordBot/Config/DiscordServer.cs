using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Config
{
    public class DiscordServer
    {
        public ulong ServerID = 0;
        public Dictionary<string, uint> EmojiUsage = new Dictionary<string, uint>();

        public DiscordServer()
        {

        }
        public DiscordServer(ulong ServerID)
        {
            this.ServerID = ServerID;
            UpdateEmojis();
        }

        public void UpdateEmojis()
        {
            SocketGuild guild = Global.P.GetGuildFromID(ServerID);
            IReadOnlyCollection<GuildEmote> emotes = guild.Emotes;
            for (int i = 0; i < EmojiUsage.Keys.Count; i++)
            {
                if (emotes.FirstOrDefault(x => x.Name == EmojiUsage.Keys.ElementAt(i)) == null)
                    EmojiUsage.Remove(EmojiUsage.Keys.ElementAt(i));
            }
            for (int i = 0; i < emotes.Count; i++)
            {
                if (!EmojiUsage.ContainsKey(emotes.ElementAt(i).Name))
                    EmojiUsage.Add(emotes.ElementAt(i).Name, 0);
            }
        }
    }
}
