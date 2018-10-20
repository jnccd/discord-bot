using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace TestDiscordBot
{
    public class configData
    {
        // TODO: Add your variables here
        public string BotToken;
        public List<ulong> ChannelsWrittenOn;
        public List<DiscordUser> UserList;

        public configData()
        {
            // TODO: Add initilization logic here
            BotToken = "<INSERT BOT TOKEN HERE>";
            ChannelsWrittenOn = new List<ulong>();
            UserList = new List<DiscordUser>();
            LastLeetedDay = DateTime.Now.Subtract(new TimeSpan(24, 0, 0));
        }
    }

    #region Backend Stuff
    public static class config
    {
        static string configPath = Global.CurrentExecutablePath + "\\config.xml";
        public static configData Data = new configData();
        static Loader L = new Loader();
        
        public static bool Exists()
        {
            return File.Exists(configPath);
        }
        public static void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(configData));
            using (TextWriter writer = new StreamWriter(configPath))
                serializer.Serialize(writer, Data);
        }
        public static void Load()
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(configData));
            using (TextReader reader = new StreamReader(configPath))
                Data = (configData)deserializer.Deserialize(reader);
        }
        public static new string ToString()
        {
            string output = "";

            FieldInfo[] Infos = typeof(configData).GetFields();
            foreach (FieldInfo info in Infos)
            {
                output += "\n" + info.Name + ": ";

                if (info.FieldType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(info.FieldType))
                {
                    output += "\n";
                    IEnumerable a = (IEnumerable)info.GetValue(Data);
                    IEnumerator e = a.GetEnumerator();
                    e.Reset();
                    while (e.MoveNext())
                    {
                        output += e.Current;
                        if (e.Current.GetType() == typeof(ulong))
                        {
                            try
                            {
                                ISocketMessageChannel Channel = (ISocketMessageChannel)Global.P.getChannelFromID((ulong)e.Current);
                                output += " - Name: " + Channel.Name + " - Server: " + ((SocketGuildChannel)Channel).Guild.Name + "\n";
                            }
                            catch { output += "\n"; }
                        }
                        else
                            output += "\n";
                    }
                }
                else
                {
                    output += info.GetValue(Data) + "\n";
                }
            }

            return output;
        }
    }
    public class Loader
    {
        public Loader()
        {
            if (config.Exists())
                config.Load();
            else
                config.Data = new configData();
        }
    }
    #endregion
}
