using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace XNAChessAI
{
    public class ChessPlayer
    {
        [XmlIgnore]
        public ChessBoard Parent;

        public ChessPlayer()
        {
            Parent = null;
        }
        public ChessPlayer(ChessBoard Parent)
        {
            this.Parent = Parent;
        }

        public virtual void MovePiece(Point From, Point To)
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
        public virtual void Draw(SpriteBatch SB)
        {

        }
    }
}
