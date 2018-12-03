using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace TestDiscordBot
{
    static public class MarkovHelper
    {
        static Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();

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
                List<string> outputList = new string[] { start }.ToList();

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
            if (dict.TryGetValue(output.Last(), out list))
                output.Add(list.ElementAt(Global.RDM.Next(list.Count)));
            else
            {
                if (dict.TryGetValue("", out list))
                    output.Add(list.ElementAt(Global.RDM.Next(list.Count)));
                else
                    throw new System.Exception("No \"\" Element!?");
            }
        }
    }
}
