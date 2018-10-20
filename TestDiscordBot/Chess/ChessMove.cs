using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Threading;

namespace TestDiscordBot.Chess
{
    public class ChessMove : ICloneable
    {
        public Point From, To;
        public int rating;

        public ChessMove()
        {
            this.From = Point.Zero;
            this.To = Point.Zero;
            this.rating = 0;
        }
        public ChessMove(Point From, Point To)
        {
            this.From = From;
            this.To = To;
            this.rating = 0;
        }
        public ChessMove(Point From, Point To, int rating)
        {
            this.From = From;
            this.To = To;
            this.rating = rating;
        }

        public object Clone()
        {
            ChessMove re = new ChessMove(new Point(), new Point(), rating);
            re.From.X = From.X;
            re.From.Y = From.Y;
            re.To.X = To.X;
            re.To.Y = To.Y;
            return re;
        }
    }
}
