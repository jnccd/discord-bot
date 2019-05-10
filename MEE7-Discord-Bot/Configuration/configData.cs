using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEE7.Configuration
{
    public class ConfigData
    {
        public string BotToken;
        public List<ulong> ChannelsWrittenOn;
        public List<DiscordUser> UserList;
        public List<DiscordServer> ServerList;
        // -------------------------------------------------
        // TODO: Add your variables here
        public List<ulong> PatchNoteSubscribedChannels;
        public List<ulong> WarframeSubscribedChannels;
        public List<string> LoadedMarkovTextFiles;
        public string LastCommitMessage;
        public List<ulong> MessagePreviewServers;
        public bool WarframeVoidTraderArrived;
        public List<string> WarframeIDList;
        public string TwtichChannelName;
        public string TwtichBotUsername;
        public string TwtichAccessToken;
        public string TwtichRefreshToken;
        public string TwtichClientID;

        public ConfigData()
        {
            BotToken = "<INSERT BOT TOKEN HERE>";
            ChannelsWrittenOn = new List<ulong>();
            UserList = new List<DiscordUser>();
            ServerList = new List<DiscordServer>();
            // ------------------------------------------------
            // TODO: Add initilization logic here
            PatchNoteSubscribedChannels = new List<ulong>();
            WarframeSubscribedChannels = new List<ulong>();
            LoadedMarkovTextFiles = new List<string>();
            MessagePreviewServers = new List<ulong>();
            WarframeIDList = new List<string>();
        }
    }
}
