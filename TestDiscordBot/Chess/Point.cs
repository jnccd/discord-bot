using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Chess
{
    public class Point
    {
        public int X, Y;
        public static Point Zero
        {
            get
            {
                return new Point(0, 0);
            }
        }

        public Point()
        {
            X = 0; Y = 0;
        }
        public Point(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }
}
