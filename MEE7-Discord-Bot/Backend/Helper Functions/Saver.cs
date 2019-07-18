using Discord;
using Discord.Audio;
using Discord.Rest;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Commands;
using MEE7.Configuration;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace MEE7.Backend
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
        public static void SaveUser(ulong UserID)
        {
            if (!Config.Data.UserList.Exists(x => x.UserID == UserID))
                Config.Data.UserList.Add(new DiscordUser(UserID));
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
