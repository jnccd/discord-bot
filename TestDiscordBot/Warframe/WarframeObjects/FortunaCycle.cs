using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeNET
{
    public class FortunaCycle
    {
        public string Id { get; set; }
        public DateTime Expiry { get; set; }
        public bool IsWarm { get; set; }
        public string TimeLeft { get; set; }
        public string ShortString { get; set; }

        public string Temerature()
        {
            return IsWarm ? "Warm" : "Cold";
        }
    }
}
