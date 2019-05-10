using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEE7.Chess
{
    class ChessPlayerDiscord : ChessPlayer
    {
        public ChessPlayerDiscord(ulong UserID) : base()
        {
            this.UserID = UserID;
        }
    }
}
