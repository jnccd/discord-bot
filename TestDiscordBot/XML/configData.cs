using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.XML
{
    public class configData
    {
        // TODO: Add your variables here
        public string BotToken;
        public List<ulong> ChannelsWrittenOn;
        public List<DiscordUser> UserList;
        public List<ulong> PatchNoteSubscribedChannels;
        public List<string> loadedMarkovTextFiles;
        public string LastCommitMessage;

        public configData()
        {
            // TODO: Add initilization logic here
            BotToken = "<INSERT BOT TOKEN HERE>";
        }
    }
}
