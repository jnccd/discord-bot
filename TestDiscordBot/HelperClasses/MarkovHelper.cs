using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace TestDiscordBot
{
    public class NoEmptyElementException : Exception { public NoEmptyElementException(string message) : base(message) { } }

    public static class MarkovHelper
    {
        static Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
        static string savePath = Global.CurrentExecutablePath + "\\markow.json";

        public static bool SaveFileExists()
        {
            return File.Exists(savePath);
        }
        public static void SaveDict()
        {
            File.WriteAllText(savePath, JsonConvert.SerializeObject(dict));
        }
        public static void LoadDict()
        {
            if (SaveFileExists())
                dict = JsonConvert.DeserializeObject<Dictionary<string, List<string>>> (File.ReadAllText(savePath));
        }

        public static void AddToDict(string addition)
        {
            lock (dict)
            {
                string[] split = new string[] { "" }.Union(addition.Split(' ')).ToArray();
                for (int i = 0; i < split.Length - 1; i++)
                {
                    if (split[i + 1] != "" && split[i + 1] != null)
                    {
                        List<string> list = null;
                        if (dict.TryGetValue(split[i], out list))
                            list.Add(split[i + 1]);
                        else
                            dict.Add(split[i], new string[] { split[i + 1] }.ToList());
                    }
                }
            }
        }

        public static string GetString(string start, int minLength, int maxLength)
        {
            lock (dict)
            {
                if (start == null)
                    start = dict.Keys.ElementAt(Global.RDM.Next(dict.Keys.Count));
                List<string> outputList = start.Split(' ').ToList();

                for (int i = 0; i < minLength; i++)
                    AddWord(outputList);
                while (!outputList.Last().EndsWith(".") && !outputList.Last().EndsWith("!") && !outputList.Last().EndsWith("?") && !outputList.Last().Contains("\n"))
                    AddWord(outputList);

                string output = outputList.Aggregate((x, y) => { return x + " " + y; });
                if (output.Length > maxLength)
                    output = output.Substring(0, maxLength);

                return output;
            }
        }
        static void AddWord(List<string> output)
        {
            List<string> list = null;
            if (dict.TryGetValue(output.Last(), out list))
                output.Add(list.ElementAt(Global.RDM.Next(list.Count)));
            else
            {
                if (dict.TryGetValue("", out list))
                    output.Add(list.ElementAt(Global.RDM.Next(list.Count)));
                else
                    throw new NoEmptyElementException("No \"\" Element!?");
            }
        }
    }
}
