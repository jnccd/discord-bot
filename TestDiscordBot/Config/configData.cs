using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Config
{
    public class configData
    {
        public string BotToken;
        public List<ulong> ChannelsWrittenOn;
        public List<DiscordUser> UserList;
        public List<DiscordServer> ServerList;
        // -------------------------------------------------
        // TODO: Add your variables here
        public List<ulong> PatchNoteSubscribedChannels;
        public List<ulong> WarframeSubscribedChannels;
        public List<string> loadedMarkovTextFiles;
        public string LastCommitMessage;

        public configData()
        {
            BotToken = "<INSERT BOT TOKEN HERE>";
            ChannelsWrittenOn = new List<ulong>();
            UserList = new List<DiscordUser>();
            ServerList = new List<DiscordServer>();
            // ------------------------------------------------
            // TODO: Add initilization logic here
            PatchNoteSubscribedChannels = new List<ulong>();
            WarframeSubscribedChannels = new List<ulong>();
            loadedMarkovTextFiles = new List<string>();
        }
    }
}
