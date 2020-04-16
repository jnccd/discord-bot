using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using WarframeNET;

namespace MEE7.Commands
{
    public static class WarframeHandler
    {
        public static WorldState worldState;
        public static string WebSite = "https://ws.warframestat.us/pc/";

        public static string GetJson(ref string ret)
        {
            if (ret == null) throw new ArgumentNullException(nameof(ret));
            try
            {
                var request = WebRequest.Create(WebSite);
                var response = request.GetResponse();
                var status = ((HttpWebResponse)response).StatusDescription;
                ret = status;
                var dataStream = response.GetResponseStream();
                if (dataStream != null)
                {
                    var reader = new StreamReader(dataStream);
                    var responseFromServer = reader.ReadToEnd();
                    return responseFromServer;
                }
                else
                    return "";
            }
            catch (Exception ex)
            {
                ret = "ERROR";
                return "An Error Occurred: " + ex.Message;
            }
        }

        public static void GetJsonObjects(string response)
        {
            worldState = JsonConvert.DeserializeObject<WorldState>(response);
        }
    }
}
