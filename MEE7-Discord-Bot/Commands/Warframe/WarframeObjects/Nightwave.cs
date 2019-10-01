using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeNET
{
    public class Nightwave
    {
        public string Id;
        public DateTime Activation;
        public string startString;
        public DateTime Expiry;
        public bool? Active;
        public int? Season;
        public string Tag;
        public int? Phase;
        public List<NightwaveChallenge> ActiveChallenges;
    }
}
