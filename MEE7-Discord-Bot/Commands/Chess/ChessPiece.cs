using System;

namespace MEE7.Commands
{
    public enum ChessPieceType
    {
        Queen,
        King,
        Rook,
        Bishop,
        Knight,
        Pawn
    }

    public class ChessPiece : ICloneable
    {
        public ChessPlayer Parent;
        public ChessPieceType Type;
        public bool HasMoved = false;

        public ChessPiece(ChessPlayer Parent, ChessPieceType Type)
        {
            this.Parent = Parent;
            this.Type = Type;
        }

        public object Clone()
        {
            ChessPiece P = (ChessPiece)this.MemberwiseClone();
            P.Type = (ChessPieceType)((int)Type);
            return P;
        }
    }
}
