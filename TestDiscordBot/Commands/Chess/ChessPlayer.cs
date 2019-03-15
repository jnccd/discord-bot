using System.Collections.Generic;

namespace TestDiscordBot.Chess
{
    public class ChessPlayer
    {
        public ChessBoard Parent;

        public ulong UserID;

        public ChessPlayer()
        {
            Parent = null;
        }
        public ChessPlayer(ChessBoard Parent)
        {
            this.Parent = Parent;
        }

        public virtual void MovePiece(ChessPoint From, ChessPoint To)
        {
            Parent.MovePiece(From, To);
        }

        public virtual void NewMatchStarted(bool IsTopPlayer)
        {

        }
        public virtual void TurnStarted()
        {

        }
        public virtual void Update()
        {

        }
    }
}
