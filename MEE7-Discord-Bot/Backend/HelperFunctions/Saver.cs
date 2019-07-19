using Discord;
using MEE7.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MEE7.Backend.HelperFunctions
{
    public static class Saver
    {
        readonly static string LogPath = "Log.txt";

        public static void SaveChannel(IChannel Channel)
        {
            if (Config.Data.ChannelsWrittenOn == null)
                Config.Data.ChannelsWrittenOn = new List<ulong>();
            if (!Config.Data.ChannelsWrittenOn.Contains(Channel.Id))
            {
                Config.Data.ChannelsWrittenOn.Add(Channel.Id);
                Config.Save();
            }
        }
        public static void SaveServer(ulong ServerID)
        {
            if (!Config.Data.ServerList.Exists(x => x.ServerID == ServerID))
                Config.Data.ServerList.Add(new DiscordServer(ServerID));
        }
        public static DiscordUser SaveUser(ulong UserID)
        {
            DiscordUser user = Config.Data.UserList.FirstOrDefault(x => x.UserID == UserID);
            if (user == null)
            {
                user = new DiscordUser(UserID);
                Config.Data.UserList.Add(user);
            }
            return user;
        }
        public static void SaveToLog(string message)
        {
            using (StreamWriter sw = File.AppendText(LogPath))
            {
                sw.WriteLine();
                sw.WriteLine("==========================Logging========================");
                sw.WriteLine("============Start=============" + DateTime.Now);
                sw.WriteLine(message);
                sw.WriteLine("=============End=============");
            }
        }
    }
}
