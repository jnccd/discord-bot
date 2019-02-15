using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Config
{
    public static class config
    {
        static string configPath = Global.CurrentExecutablePath + "\\config.json";
        public static configData Data = new configData();
        
        static config()
        {
            if (config.Exists())
                config.Load();
            else
                config.Data = new configData();
        }

        public static string getConfigPath()
        {
            return configPath;
        }
        public static bool Exists()
        {
            return File.Exists(configPath);
        }
        public static void Save()
        {
            //XmlSerializer serializer = new XmlSerializer(typeof(configData));
            //using (TextWriter writer = new StreamWriter(configPath))
            //    serializer.Serialize(writer, Data);
            File.WriteAllText(configPath, JsonConvert.SerializeObject(Data));
        }
        public static void Load()
        {
            //XmlSerializer deserializer = new XmlSerializer(typeof(configData));
            //using (TextReader reader = new StreamReader(configPath))
            //    Data = (configData)deserializer.Deserialize(reader);
            if (Exists())
                Data = JsonConvert.DeserializeObject<configData>(File.ReadAllText(configPath));
            else
                Data = new configData();
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
}
