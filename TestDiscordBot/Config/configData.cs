using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Config
{
    public class configData
    {
        // TODO: Add your variables here
        public string BotToken;
        public List<ulong> ChannelsWrittenOn;
        public List<DiscordUser> UserList;
        public List<DiscordServer> ServerList;
        public List<ulong> PatchNoteSubscribedChannels;
        public List<ulong> WarframeSubscribedChannels;
        public List<string> loadedMarkovTextFiles;
        public string LastCommitMessage;

        public configData()
        {
            // TODO: Add initilization logic here
            BotToken = "<INSERT BOT TOKEN HERE>";
            ChannelsWrittenOn = new List<ulong>();
            UserList = new List<DiscordUser>();
            ServerList = new List<DiscordServer>();
            PatchNoteSubscribedChannels = new List<ulong>();
            loadedMarkovTextFiles = new List<string>();
        }
    }
}
