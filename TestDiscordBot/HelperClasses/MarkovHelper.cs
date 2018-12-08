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
        const byte inputLength = 2;
        static string savePath = Global.CurrentExecutablePath + "\\markow" + inputLength + ".json";
        static Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();

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
                List<string> presplit = new List<string>();
                for (int i = 0; i < inputLength; i++)
                    presplit.Add("");
                presplit.AddRange(addition.Split(' '));
                string[] split = presplit.ToArray();

                for (int i = 0; i < split.Length - inputLength; i++)
                {
                    if (split[i + inputLength] != "" && split[i + inputLength] != null)
                    {
                        List<string> list = null;
                        string key = split.ToList().GetRange(i, inputLength).Aggregate((x, y) => { return x + " " + y; }).Trim(' ');
                        if (dict.TryGetValue(key, out list))
                            list.Add(split[i + inputLength]);
                        else
                            dict.Add(key, new string[] { split[i + inputLength] }.ToList());
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
                while (!outputList.Last().EndsWith(".") && !outputList.Last().EndsWith("!") && !outputList.Last().EndsWith("?") && !outputList.Last().EndsWith("\n"))
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
            string key = output.Skip(Math.Max(0, output.Count() - inputLength)).Aggregate((x, y) => { return x + " " + y; });
            if (dict.TryGetValue(key, out list))
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