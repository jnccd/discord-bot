using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading;

namespace XNAChessAI
{
    public class Move : ICloneable
    {
        public Point From, To;
        public int rating;

        public Move()
        {
            this.From = Point.Zero;
            this.To = Point.Zero;
            this.rating = 0;
        }
        public Move(Point From, Point To)
        {
            this.From = From;
            this.To = To;
            this.rating = 0;
        }
        public Move(Point From, Point To, int rating)
        {
            this.From = From;
            this.To = To;
            this.rating = rating;
        }

        public object Clone()
        {
            Move re = new Move(new Point(), new Point(), rating);
            re.From.X = From.X;
            re.From.Y = From.Y;
            re.To.X = To.X;
            re.To.Y = To.Y;
            return re;
        }
    }
}
