using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeNET
{
    public class FortunaCycle
    {
        public string Id;
        public DateTime Expiry;
        public bool IsWarm;
        public string TimeLeft;
        public string ShortString;

        public string Temerature()
        {
            return IsWarm ? "Warm" : "Cold";
        }
    }
}
