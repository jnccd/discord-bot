using System;

namespace MEE7.Commands
{
    public class ChessMove : ICloneable
    {
        public ChessPoint From, To;
        public int rating;

        public ChessMove()
        {
            this.From = ChessPoint.Zero;
            this.To = ChessPoint.Zero;
            this.rating = 0;
        }
        public ChessMove(ChessPoint From, ChessPoint To)
        {
            this.From = From;
            this.To = To;
            this.rating = 0;
        }
        public ChessMove(ChessPoint From, ChessPoint To, int rating)
        {
            this.From = From;
            this.To = To;
            this.rating = rating;
        }

        public object Clone()
        {
            ChessMove re = new ChessMove(new ChessPoint(), new ChessPoint(), rating);
            re.From.X = From.X;
            re.From.Y = From.Y;
            re.To.X = To.X;
            re.To.Y = To.Y;
            return re;
        }
    }
}
