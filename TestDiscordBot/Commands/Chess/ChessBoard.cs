using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace TestDiscordBot.Chess
{
    public class InvalidChessPiece : System.Exception { }

    public class ChessBoard : ICloneable
    {
        private ChessPlayer playerTop; // Blacc
        private ChessPlayer playerBottom; // White
        public ChessPlayer PlayerTop { get { return playerTop; } } // Blacc
        public ChessPlayer PlayerBottom { get { return playerBottom; } } // White
        public ChessPlayer PlayerBlack { get { return playerTop; } }
        public ChessPlayer PlayerWhite { get { return playerBottom; } }
        protected ChessPiece TopKing;
        protected ChessPiece BottomKing;

        protected ChessPiece[,] Pieces = new ChessPiece[8, 8];
        protected int[,,] ThreefoldRepetitionCheck = new int[8, 8, 12];
        protected bool Turn = false; // true = PlayerTop, flase = PlayerBottom
        public const int ChessStatusBarHeight = 30;
        public const int ChessAIStatusDisplayWidth = 400;
        public const int ChessFieldSize = 80;
        public List<int> GameLengths = new List<int>();
        protected int Turns = 0;
        public int EndedGameBecauseOfRecurrance = 0;
        public int NormallyEndedGames = 0;
        public int AllowedRepetitions = 3;
        bool IsThreefoldRepetitionCheckActive = false;

        bool slow = false;

        public bool GameEnded;
        public ChessPlayer Winner;
        
        public ChessBoard(ChessPlayer PlayerOne, ChessPlayer PlayerTwo)
        {
            playerTop = PlayerOne;
            playerBottom = PlayerTwo;

            PlayerTop.Parent = this;
            PlayerBottom.Parent = this;

            SetUpNewGame();
        }
        
        public void SetUpNewGame()
        {
            ClearPieces();
            ClearThreefoldRepetitionCheck();
            
            Turn = false;
            Winner = null;
            GameEnded = false;
            
            // Pawns
            for (int i = 0; i < 8; i++)
                Pieces[i, 1] = new ChessPiece(PlayerTop, ChessPieceType.Pawn);
            for (int i = 0; i < 8; i++)
                Pieces[i, 6] = new ChessPiece(PlayerBottom, ChessPieceType.Pawn);

            // Top Rest
            Pieces[0, 0] = new ChessPiece(PlayerTop, ChessPieceType.Rook);
            Pieces[1, 0] = new ChessPiece(PlayerTop, ChessPieceType.Knight);
            Pieces[2, 0] = new ChessPiece(PlayerTop, ChessPieceType.Bishop);
            Pieces[3, 0] = new ChessPiece(PlayerTop, ChessPieceType.King);
            Pieces[4, 0] = new ChessPiece(PlayerTop, ChessPieceType.Queen);
            Pieces[5, 0] = new ChessPiece(PlayerTop, ChessPieceType.Bishop);
            Pieces[6, 0] = new ChessPiece(PlayerTop, ChessPieceType.Knight);
            Pieces[7, 0] = new ChessPiece(PlayerTop, ChessPieceType.Rook);

            // Bottom Rest
            Pieces[0, 7] = new ChessPiece(PlayerBottom, ChessPieceType.Rook);
            Pieces[1, 7] = new ChessPiece(PlayerBottom, ChessPieceType.Knight);
            Pieces[2, 7] = new ChessPiece(PlayerBottom, ChessPieceType.Bishop);
            Pieces[3, 7] = new ChessPiece(PlayerBottom, ChessPieceType.King);
            Pieces[4, 7] = new ChessPiece(PlayerBottom, ChessPieceType.Queen);
            Pieces[5, 7] = new ChessPiece(PlayerBottom, ChessPieceType.Bishop);
            Pieces[6, 7] = new ChessPiece(PlayerBottom, ChessPieceType.Knight);
            Pieces[7, 7] = new ChessPiece(PlayerBottom, ChessPieceType.Rook);

            TopKing = Pieces[3, 0];
            BottomKing = Pieces[3, 7];
        }
        public void SetUpNewGame(ChessPlayer PlayerTop, ChessPlayer PlayerBottom)
        {
            ClearPieces();
            ClearThreefoldRepetitionCheck();

            this.playerTop = PlayerTop;
            this.playerBottom = PlayerBottom;
            PlayerTop.Parent = this;
            PlayerBottom.Parent = this;

            Turn = false;
            Winner = null;
            GameEnded = false;

            PlayerTop.NewMatchStarted(true);
            PlayerBottom.NewMatchStarted(false);

            // Pawns
            for (int i = 0; i < 8; i++)
                Pieces[i, 1] = new ChessPiece(PlayerTop, ChessPieceType.Pawn);
            for (int i = 0; i < 8; i++)
                Pieces[i, 6] = new ChessPiece(PlayerBottom, ChessPieceType.Pawn);

            // Top Rest
            Pieces[0, 0] = new ChessPiece(PlayerTop, ChessPieceType.Rook);
            Pieces[1, 0] = new ChessPiece(PlayerTop, ChessPieceType.Knight);
            Pieces[2, 0] = new ChessPiece(PlayerTop, ChessPieceType.Bishop);
            Pieces[3, 0] = new ChessPiece(PlayerTop, ChessPieceType.King);
            Pieces[4, 0] = new ChessPiece(PlayerTop, ChessPieceType.Queen);
            Pieces[5, 0] = new ChessPiece(PlayerTop, ChessPieceType.Bishop);
            Pieces[6, 0] = new ChessPiece(PlayerTop, ChessPieceType.Knight);
            Pieces[7, 0] = new ChessPiece(PlayerTop, ChessPieceType.Rook);

            // Bottom Rest
            Pieces[0, 7] = new ChessPiece(PlayerBottom, ChessPieceType.Rook);
            Pieces[1, 7] = new ChessPiece(PlayerBottom, ChessPieceType.Knight);
            Pieces[2, 7] = new ChessPiece(PlayerBottom, ChessPieceType.Bishop);
            Pieces[3, 7] = new ChessPiece(PlayerBottom, ChessPieceType.King);
            Pieces[4, 7] = new ChessPiece(PlayerBottom, ChessPieceType.Queen);
            Pieces[5, 7] = new ChessPiece(PlayerBottom, ChessPieceType.Bishop);
            Pieces[6, 7] = new ChessPiece(PlayerBottom, ChessPieceType.Knight);
            Pieces[7, 7] = new ChessPiece(PlayerBottom, ChessPieceType.Rook);

            TopKing = Pieces[3, 0];
            BottomKing = Pieces[3, 7];
        }
        public ChessPoint[] GetAllPossibleMovesForPiece(ChessPoint From)
        {
            if (From.X < 0 || From.Y < 0 || From.X >= 8 || From.Y >= 8 || Pieces[From.X, From.Y] == null)
                return new ChessPoint[0];

            List<ChessPoint> PossibleMoves = new List<ChessPoint>();
            ChessPoint UpLeft = new ChessPoint();
            ChessPoint UpRight = new ChessPoint();
            ChessPoint DownLeft = new ChessPoint();
            ChessPoint DownRight = new ChessPoint();
            ChessPoint RightUp = new ChessPoint();
            ChessPoint RightDown = new ChessPoint();
            ChessPoint LeftUp = new ChessPoint();
            ChessPoint LeftDown = new ChessPoint();
            ChessPoint Right = new ChessPoint();
            ChessPoint Down = new ChessPoint();
            ChessPoint Up = new ChessPoint();
            ChessPoint Left = new ChessPoint();
            ChessPoint P = new ChessPoint();

            switch (Pieces[From.X, From.Y].Type)
            {
                case ChessPieceType.Pawn:
                    if (GetChessPieceFromPoint(From).Parent == PlayerTop)
                    {
                        if (Pieces[From.X, From.Y].HasMoved)
                        {
                            if (IsFieldVacant(new ChessPoint(From.X, From.Y + 1)))
                                PossibleMoves.Add(new ChessPoint(From.X, From.Y + 1));
                        }
                        else
                        {
                            if (IsFieldVacant(new ChessPoint(From.X, From.Y + 1)))
                            {
                                PossibleMoves.Add(new ChessPoint(From.X, From.Y + 1));
                                if (IsFieldVacant(new ChessPoint(From.X, From.Y + 2)))
                                    PossibleMoves.Add(new ChessPoint(From.X, From.Y + 2));
                            }
                        }

                        Left = new ChessPoint(From.X - 1, From.Y + 1);
                        Right = new ChessPoint(From.X + 1, From.Y + 1);

                        if (IsFieldInBounds(Left) && GetChessPieceFromPoint(Left) != null && Pieces[Left.X, Left.Y].Parent == PlayerBottom)
                            PossibleMoves.Add(Left);
                        if (IsFieldInBounds(Right) && GetChessPieceFromPoint(Right) != null && Pieces[Right.X, Right.Y].Parent == PlayerBottom)
                            PossibleMoves.Add(Right);
                    }
                    if (GetChessPieceFromPoint(From).Parent == PlayerBottom)
                    {
                        if (Pieces[From.X, From.Y].HasMoved)
                        {
                            if (IsFieldVacant(new ChessPoint(From.X, From.Y - 1)))
                                PossibleMoves.Add(new ChessPoint(From.X, From.Y - 1));
                        }
                        else
                        {
                            if (IsFieldVacant(new ChessPoint(From.X, From.Y - 1)))
                            {
                                PossibleMoves.Add(new ChessPoint(From.X, From.Y - 1));
                                if (IsFieldVacant(new ChessPoint(From.X, From.Y - 2)))
                                    PossibleMoves.Add(new ChessPoint(From.X, From.Y - 2));
                            }
                        }

                        Left = new ChessPoint(From.X - 1, From.Y - 1);
                        Right = new ChessPoint(From.X + 1, From.Y - 1);

                        if (IsFieldInBounds(Left) && GetChessPieceFromPoint(Left) != null && Pieces[Left.X, Left.Y].Parent == PlayerTop)
                            PossibleMoves.Add(Left);
                        if (IsFieldInBounds(Right) && GetChessPieceFromPoint(Right) != null && Pieces[Right.X, Right.Y].Parent == PlayerTop)
                            PossibleMoves.Add(Right);
                    }
                    break;

                case ChessPieceType.Knight:
                    UpLeft = new ChessPoint(From.X - 1, From.Y - 2);
                    UpRight = new ChessPoint(From.X + 1, From.Y - 2);
                    DownLeft = new ChessPoint(From.X - 1, From.Y + 2);
                    DownRight = new ChessPoint(From.X + 1, From.Y + 2);
                    RightUp = new ChessPoint(From.X + 2, From.Y - 1);
                    RightDown = new ChessPoint(From.X + 2, From.Y + 1);
                    LeftUp = new ChessPoint(From.X - 2, From.Y - 1);
                    LeftDown = new ChessPoint(From.X - 2, From.Y + 1);

                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), UpLeft))
                        PossibleMoves.Add(UpLeft);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), UpRight))
                        PossibleMoves.Add(UpRight);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), DownLeft))
                        PossibleMoves.Add(DownLeft);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), DownRight))
                        PossibleMoves.Add(DownRight);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), RightUp))
                        PossibleMoves.Add(RightUp);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), RightDown))
                        PossibleMoves.Add(RightDown);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), LeftUp))
                        PossibleMoves.Add(LeftUp);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), LeftDown))
                        PossibleMoves.Add(LeftDown);
                    break;

                case ChessPieceType.King:
                    UpLeft = new ChessPoint(From.X - 1, From.Y - 1);
                    UpRight = new ChessPoint(From.X + 1, From.Y - 1);
                    DownLeft = new ChessPoint(From.X - 1, From.Y + 1);
                    DownRight = new ChessPoint(From.X + 1, From.Y + 1);
                    Right = new ChessPoint(From.X + 1, From.Y);
                    Down = new ChessPoint(From.X, From.Y + 1);
                    Up = new ChessPoint(From.X, From.Y - 1);
                    Left = new ChessPoint(From.X - 1, From.Y);

                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), UpLeft))
                        PossibleMoves.Add(UpLeft);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), UpRight))
                        PossibleMoves.Add(UpRight);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), DownLeft))
                        PossibleMoves.Add(DownLeft);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), DownRight))
                        PossibleMoves.Add(DownRight);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), Right))
                        PossibleMoves.Add(Right);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), Down))
                        PossibleMoves.Add(Down);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), Up))
                        PossibleMoves.Add(Up);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), Left))
                        PossibleMoves.Add(Left);
                    break;

                case ChessPieceType.Rook:
                    P = new ChessPoint(From.X, From.Y + 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.Y++;
                    }

                    P = new ChessPoint(From.X, From.Y - 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.Y--;
                    }

                    P = new ChessPoint(From.X + 1, From.Y);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X++;
                    }

                    P = new ChessPoint(From.X - 1, From.Y);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X--;
                    }
                    break;

                case ChessPieceType.Bishop:
                    P = new ChessPoint(From.X + 1, From.Y + 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X++; P.Y++;
                    }

                    P = new ChessPoint(From.X - 1, From.Y - 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X--; P.Y--;
                    }

                    P = new ChessPoint(From.X + 1, From.Y - 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X++; P.Y--;
                    }

                    P = new ChessPoint(From.X - 1, From.Y + 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X--; P.Y++;
                    }
                    break;

                case ChessPieceType.Queen:
                    P = new ChessPoint(From.X, From.Y + 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.Y++;
                    }

                    P = new ChessPoint(From.X, From.Y - 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.Y--;
                    }

                    P = new ChessPoint(From.X + 1, From.Y);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X++;
                    }

                    P = new ChessPoint(From.X - 1, From.Y);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X--;
                    }

                    P = new ChessPoint(From.X + 1, From.Y + 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X++; P.Y++;
                    }

                    P = new ChessPoint(From.X - 1, From.Y - 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X--; P.Y--;
                    }

                    P = new ChessPoint(From.X + 1, From.Y - 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X++; P.Y--;
                    }

                    P = new ChessPoint(From.X - 1, From.Y + 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X--; P.Y++;
                    }
                    break;
            }

            return PossibleMoves.ToArray();
        }
        public ChessPoint[] GetAllPossibleMovesForPiece(int fromX, int fromY)
        {
            if (fromX < 0 || fromY < 0 || fromX >= 8 || fromY >= 8 || Pieces[fromX, fromY] == null)
                return new ChessPoint[0];

            List<ChessPoint> PossibleMoves = new List<ChessPoint>();
            ChessPoint UpLeft = new ChessPoint();
            ChessPoint UpRight = new ChessPoint();
            ChessPoint DownLeft = new ChessPoint();
            ChessPoint DownRight = new ChessPoint();
            ChessPoint RightUp = new ChessPoint();
            ChessPoint RightDown = new ChessPoint();
            ChessPoint LeftUp = new ChessPoint();
            ChessPoint LeftDown = new ChessPoint();
            ChessPoint Right = new ChessPoint();
            ChessPoint Down = new ChessPoint();
            ChessPoint Up = new ChessPoint();
            ChessPoint Left = new ChessPoint();
            ChessPoint P = new ChessPoint();

            switch (Pieces[fromX, fromY].Type)
            {
                case ChessPieceType.Pawn:
                    if (GetChessPieceFromPoint(fromX, fromY).Parent == PlayerTop)
                    {
                        if (Pieces[fromX, fromY].HasMoved)
                        {
                            if (IsFieldVacant(new ChessPoint(fromX, fromY + 1)))
                                PossibleMoves.Add(new ChessPoint(fromX, fromY + 1));
                        }
                        else
                        {
                            if (IsFieldVacant(new ChessPoint(fromX, fromY + 1)))
                            {
                                PossibleMoves.Add(new ChessPoint(fromX, fromY + 1));
                                if (IsFieldVacant(new ChessPoint(fromX, fromY + 2)))
                                    PossibleMoves.Add(new ChessPoint(fromX, fromY + 2));
                            }
                        }

                        Left = new ChessPoint(fromX - 1, fromY + 1);
                        Right = new ChessPoint(fromX + 1, fromY + 1);

                        if (IsFieldInBounds(Left) && GetChessPieceFromPoint(Left) != null && Pieces[Left.X, Left.Y].Parent == PlayerBottom)
                            PossibleMoves.Add(Left);
                        if (IsFieldInBounds(Right) && GetChessPieceFromPoint(Right) != null && Pieces[Right.X, Right.Y].Parent == PlayerBottom)
                            PossibleMoves.Add(Right);
                    }
                    if (GetChessPieceFromPoint(fromX, fromY).Parent == PlayerBottom)
                    {
                        if (Pieces[fromX, fromY].HasMoved)
                        {
                            if (IsFieldVacant(new ChessPoint(fromX, fromY - 1)))
                                PossibleMoves.Add(new ChessPoint(fromX, fromY - 1));
                        }
                        else
                        {
                            if (IsFieldVacant(new ChessPoint(fromX, fromY - 1)))
                            {
                                PossibleMoves.Add(new ChessPoint(fromX, fromY - 1));
                                if (IsFieldVacant(new ChessPoint(fromX, fromY - 2)))
                                    PossibleMoves.Add(new ChessPoint(fromX, fromY - 2));
                            }
                        }

                        Left = new ChessPoint(fromX - 1, fromY - 1);
                        Right = new ChessPoint(fromX + 1, fromY - 1);

                        if (IsFieldInBounds(Left) && GetChessPieceFromPoint(Left) != null && Pieces[Left.X, Left.Y].Parent == PlayerTop)
                            PossibleMoves.Add(Left);
                        if (IsFieldInBounds(Right) && GetChessPieceFromPoint(Right) != null && Pieces[Right.X, Right.Y].Parent == PlayerTop)
                            PossibleMoves.Add(Right);
                    }
                    break;

                case ChessPieceType.Knight:
                    UpLeft = new ChessPoint(fromX - 1, fromY - 2);
                    UpRight = new ChessPoint(fromX + 1, fromY - 2);
                    DownLeft = new ChessPoint(fromX - 1, fromY + 2);
                    DownRight = new ChessPoint(fromX + 1, fromY + 2);
                    RightUp = new ChessPoint(fromX + 2, fromY - 1);
                    RightDown = new ChessPoint(fromX + 2, fromY + 1);
                    LeftUp = new ChessPoint(fromX - 2, fromY - 1);
                    LeftDown = new ChessPoint(fromX - 2, fromY + 1);

                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), UpLeft))
                        PossibleMoves.Add(UpLeft);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), UpRight))
                        PossibleMoves.Add(UpRight);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), DownLeft))
                        PossibleMoves.Add(DownLeft);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), DownRight))
                        PossibleMoves.Add(DownRight);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), RightUp))
                        PossibleMoves.Add(RightUp);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), RightDown))
                        PossibleMoves.Add(RightDown);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), LeftUp))
                        PossibleMoves.Add(LeftUp);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), LeftDown))
                        PossibleMoves.Add(LeftDown);
                    break;

                case ChessPieceType.King:
                    UpLeft = new ChessPoint(fromX - 1, fromY - 1);
                    UpRight = new ChessPoint(fromX + 1, fromY - 1);
                    DownLeft = new ChessPoint(fromX - 1, fromY + 1);
                    DownRight = new ChessPoint(fromX + 1, fromY + 1);
                    Right = new ChessPoint(fromX + 1, fromY);
                    Down = new ChessPoint(fromX, fromY + 1);
                    Up = new ChessPoint(fromX, fromY - 1);
                    Left = new ChessPoint(fromX - 1, fromY);

                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), UpLeft))
                        PossibleMoves.Add(UpLeft);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), UpRight))
                        PossibleMoves.Add(UpRight);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), DownLeft))
                        PossibleMoves.Add(DownLeft);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), DownRight))
                        PossibleMoves.Add(DownRight);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), Right))
                        PossibleMoves.Add(Right);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), Down))
                        PossibleMoves.Add(Down);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), Up))
                        PossibleMoves.Add(Up);
                    if (CanPlayerMoveThere(PlayerWhoHasTheMove(), Left))
                        PossibleMoves.Add(Left);
                    break;

                case ChessPieceType.Rook:
                    P = new ChessPoint(fromX, fromY + 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.Y++;
                    }

                    P = new ChessPoint(fromX, fromY - 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.Y--;
                    }

                    P = new ChessPoint(fromX + 1, fromY);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X++;
                    }

                    P = new ChessPoint(fromX - 1, fromY);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X--;
                    }
                    break;

                case ChessPieceType.Bishop:
                    P = new ChessPoint(fromX + 1, fromY + 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X++; P.Y++;
                    }

                    P = new ChessPoint(fromX - 1, fromY - 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X--; P.Y--;
                    }

                    P = new ChessPoint(fromX + 1, fromY - 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X++; P.Y--;
                    }

                    P = new ChessPoint(fromX - 1, fromY + 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X--; P.Y++;
                    }
                    break;

                case ChessPieceType.Queen:
                    P = new ChessPoint(fromX, fromY + 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.Y++;
                    }

                    P = new ChessPoint(fromX, fromY - 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.Y--;
                    }

                    P = new ChessPoint(fromX + 1, fromY);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X++;
                    }

                    P = new ChessPoint(fromX - 1, fromY);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X--;
                    }

                    P = new ChessPoint(fromX + 1, fromY + 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X++; P.Y++;
                    }

                    P = new ChessPoint(fromX - 1, fromY - 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X--; P.Y--;
                    }

                    P = new ChessPoint(fromX + 1, fromY - 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X++; P.Y--;
                    }

                    P = new ChessPoint(fromX - 1, fromY + 1);
                    while (CanPlayerMoveThere(PlayerWhoHasTheMove(), P))
                    {
                        PossibleMoves.Add(new ChessPoint(P.X, P.Y));
                        if (GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == PlayerWhoHasntTheMove())
                            break;
                        P.X--; P.Y++;
                    }
                    break;
            }

            return PossibleMoves.ToArray();
        }
        public bool IsFieldVacant(ChessPoint P)
        {
            return P.X >= 0 && P.Y >= 0 && P.X < 8 && P.Y < 8 && Pieces[P.X, P.Y] == null;
        }
        public bool IsFieldInBounds(ChessPoint P)
        {
            return P.X >= 0 && P.Y >= 0 && P.X < 8 && P.Y < 8;
        }
        public bool CanPlayerMoveThere(ChessPlayer CurrentPlayer, ChessPoint P)
        {
            ChessPlayer OtherPlayer = null;
            if (CurrentPlayer == PlayerBottom)
                OtherPlayer = PlayerTop;
            else if (CurrentPlayer == PlayerTop)
                OtherPlayer = PlayerBottom;
            else
                throw new ArgumentException();

            return IsFieldInBounds(P) && GetChessPieceFromPoint(P) == null ||
                    IsFieldInBounds(P) && GetChessPieceFromPoint(P) != null && GetChessPieceFromPoint(P).Parent == OtherPlayer;
        }
        public ChessPiece GetChessPieceFromPoint(ChessPoint P)
        {
            return Pieces[P.X, P.Y];
        }
        public ChessPiece GetChessPieceFromPoint(int x, int y)
        {
            return Pieces[x, y];
        }
        public ChessPlayer PlayerWhoHasTheMove()
        {
            if (Turn)
                return PlayerTop;
            else
                return PlayerBottom;
        }
        public ChessPlayer PlayerWhoHasntTheMove()
        {
            if (PlayerWhoHasTheMove() == PlayerBottom)
                return PlayerTop;
            else if (PlayerWhoHasTheMove() == PlayerTop)
                return PlayerBottom;
            else
                throw new ArgumentException();
        }
        void ClearThreefoldRepetitionCheck()
        {
            for (int x = 0; x < ThreefoldRepetitionCheck.GetLength(0); x++)
                for (int y = 0; y < ThreefoldRepetitionCheck.GetLength(1); y++)
                    for (int z = 0; z < ThreefoldRepetitionCheck.GetLength(2); z++)
                        ThreefoldRepetitionCheck[x, y, z] = 0;
        }
        void ClearPieces()
        {
            for (int x = 0; x < Pieces.GetLength(0); x++)
                for (int y = 0; y < Pieces.GetLength(1); y++)
                    Pieces[x, y] = null;
        }
        public void MovePiece(ChessPoint from, ChessPoint to)
        {
            if (slow)
                Thread.Sleep(1000);

            ChessPiece FromPiece = GetChessPieceFromPoint(from);
            ChessPlayer MovePlayer = PlayerWhoHasTheMove();
            List<ChessPoint> AllPossibleMoves = new List<ChessPoint>(GetAllPossibleMovesForPiece(from));
            if (FromPiece.Parent == MovePlayer && AllPossibleMoves.Exists(x => x.X == to.X && x.Y == to.Y))
            {
                Turns++;

                if (Pieces[to.X, to.Y] == TopKing)
                {
                    EndGameNormally(PlayerBottom, from);

                    if (slow)
                        Thread.Sleep(3000);
                }
                if (Pieces[to.X, to.Y] == BottomKing)
                {
                    EndGameNormally(PlayerTop, from);
                    
                    if (slow)
                        Thread.Sleep(3000);
                }

                Pieces[to.X, to.Y] = Pieces[from.X, from.Y];
                Pieces[from.X, from.Y] = null;
                Pieces[to.X, to.Y].HasMoved = true;

                if (IsThreefoldRepetitionCheckActive)
                {
                    ThreefoldRepetitionCheck[to.X, to.Y, (int)Pieces[to.X, to.Y].Type + (Pieces[to.X, to.Y].Parent == PlayerBottom ? 0 : 6)]++;
                    if (ThreefoldRepetitionCheck[to.X, to.Y, (int)Pieces[to.X, to.Y].Type + (Pieces[to.X, to.Y].Parent == PlayerBottom ? 0 : 6)] >= AllowedRepetitions)
                        EndGameBecauseOfRecurrence(PlayerWhoHasTheMove(), from);
                }

                Turn = !Turn;
                PlayerWhoHasTheMove().TurnStarted();
            }
            else
                throw new ArgumentException();
        }
        public void MovePiece(ChessMove M)
        {
            if (slow)
                Thread.Sleep(1000);

            ChessPoint from = M.From;
            ChessPoint to = M.To;

            ChessPiece FromPiece = GetChessPieceFromPoint(from);
            ChessPlayer MovePlayer = PlayerWhoHasTheMove();
            List<ChessPoint> AllPossibleMoves = new List<ChessPoint>(GetAllPossibleMovesForPiece(from));
            if (FromPiece.Parent == MovePlayer && AllPossibleMoves.Contains(to))
            {
                Turns++;

                if (Pieces[to.X, to.Y] == TopKing)
                {
                    EndGameNormally(PlayerBottom, from);

                    if (slow)
                        Thread.Sleep(3000);
                }
                if (Pieces[to.X, to.Y] == BottomKing)
                {
                    EndGameNormally(PlayerTop, from);

                    if (slow)
                        Thread.Sleep(3000);
                }

                Pieces[to.X, to.Y] = Pieces[from.X, from.Y];
                Pieces[from.X, from.Y] = null;
                Pieces[to.X, to.Y].HasMoved = true;

                if (IsThreefoldRepetitionCheckActive)
                {
                    ThreefoldRepetitionCheck[to.X, to.Y, (int)Pieces[to.X, to.Y].Type + (Pieces[to.X, to.Y].Parent == PlayerBottom ? 0 : 6)]++;
                    if (ThreefoldRepetitionCheck[to.X, to.Y, (int)Pieces[to.X, to.Y].Type + (Pieces[to.X, to.Y].Parent == PlayerBottom ? 0 : 6)] >= AllowedRepetitions)
                        EndGameBecauseOfRecurrence(PlayerWhoHasTheMove(), from);
                }

                Turn = !Turn;
                PlayerWhoHasTheMove().TurnStarted();
            }
            else
                throw new ArgumentException();
        }
        void EndGameBecauseOfRecurrence(ChessPlayer FaultyPlayer, ChessPoint from)
        {
            GameEnded = true;
            OnGameEnd();
            GameLengths.Add(Turns);
            Turns = 0;
            EndedGameBecauseOfRecurrance++;

            if (FaultyPlayer == PlayerTop)
                Winner = PlayerBottom;
            else if (FaultyPlayer == PlayerBottom)
                Winner = PlayerTop;
            else
                throw new Exception("wat");
        }
        void EndGameNormally(ChessPlayer Winner, ChessPoint from)
        {
            GameEnded = true;
            OnGameEnd();
            this.Winner = Winner;
            GameLengths.Add(Turns);
            NormallyEndedGames++;
            Turns = 0;
        }
        public void OnGameEnd()
        {
            //Debug.WriteLine("Ended");
        }
        public ChessPlayer GetOponent(ChessPlayer you)
        {
            if (PlayerTop == you)
                return PlayerBottom;
            else
                return PlayerTop;
        }

        public void Update()
        {
            if (!GameEnded)
                PlayerWhoHasTheMove().Update();
        }
        
        public object Clone()
        {
            ChessBoard re = (ChessBoard)MemberwiseClone();
            re.Pieces = new ChessPiece[8, 8];
            for (int x = 0; x < Pieces.GetLength(0); x++)
                for (int y = 0; y < Pieces.GetLength(1); y++)
                {
                    if (Pieces[x, y] != null)
                    {
                        re.Pieces[x, y] = (ChessPiece)Pieces[x, y].Clone();
                        if (re.Pieces[x, y].Type == ChessPieceType.King)
                        {
                            if (re.Pieces[x, y].Parent == PlayerTop)
                                re.TopKing = re.Pieces[x, y];
                            else
                                re.BottomKing = re.Pieces[x, y];
                        }
                    }
                }
            re.ThreefoldRepetitionCheck = new int[8, 8, 12];
            for (int x = 0; x < ThreefoldRepetitionCheck.GetLength(0); x++)
                for (int y = 0; y < ThreefoldRepetitionCheck.GetLength(1); y++)
                    for (int z = 0; z < ThreefoldRepetitionCheck.GetLength(2); z++)
                        re.ThreefoldRepetitionCheck[x, y, z] = ThreefoldRepetitionCheck[x, y, z];
            return re;
        }
    }
}
