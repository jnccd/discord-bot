using System;

namespace WarframeNET
{
    public class FortunaCycle
    {
        public string Id;
        public DateTime Expiry;
        public bool? IsWarm;
        public string TimeLeft;
        public string ShortString;

        public string Temerature()
        {
            return IsWarm.Value ? "Warm" : "Cold";
        }
    }
}
