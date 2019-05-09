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

namespace MEE7.Configuration
{
    public static class Config
    {
        static readonly string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\";
        static readonly string configPath = exePath + "config.json";
        static readonly string configBackupPath = exePath + "config_backup.json";
        public static ConfigData Data = new ConfigData();
        
        static Config()
        {
            if (Config.Exists())
                Config.Load();
            else
                Config.Data = new ConfigData();
        }

        public static string GetConfigPath()
        {
            return configPath;
        }
        public static bool Exists()
        {
            return File.Exists(configPath);
        }
        public static void Save()
        {
            if (File.Exists(configPath))
                File.Copy(configPath, configBackupPath, true);
            File.WriteAllText(configPath, JsonConvert.SerializeObject(Data));
        }
        public static void Load()
        {
            if (Exists())
                Data = JsonConvert.DeserializeObject<ConfigData>(File.ReadAllText(configPath));
            else
                Data = new ConfigData();
        }
        public static new string ToString()
        {
            string output = "";

            FieldInfo[] Infos = typeof(ConfigData).GetFields();
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
                                ISocketMessageChannel Channel = (ISocketMessageChannel)Program.GetChannelFromID((ulong)e.Current);
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
