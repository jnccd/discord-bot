namespace MEE7.Commands
{
    public class ChessPoint
    {
        public int X, Y;
        public static ChessPoint Zero
        {
            get
            {
                return new ChessPoint(0, 0);
            }
        }

        public ChessPoint()
        {
            X = 0; Y = 0;
        }
        public ChessPoint(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }
}
