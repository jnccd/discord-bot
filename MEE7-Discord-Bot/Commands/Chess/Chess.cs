using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MEE7.Chess;

namespace MEE7.Commands
{
    public class Chess : Command
    {
        List<ChessBoard> Boards = new List<ChessBoard>();

        public Chess() : base("chess", "Play chess", false)
        {
            HelpMenu = new EmbedBuilder();
            HelpMenu.WithColor(0, 128, 255);
            HelpMenu.AddFieldDirectly(Prefix + CommandLine + " newGame + a mentioned user", "Creates a new game against the mentioned user");
            HelpMenu.AddFieldDirectly(Prefix + CommandLine + " move + 2 coordinates", "Moves a chess piece from the first point to the second\n" +
                "eg. " + Prefix + CommandLine + " move 1,1 3,1");
            HelpMenu.AddFieldDirectly(Prefix + CommandLine + " game", "Prints the game you are currently in");
            HelpMenu.WithDescription("Chess Commands:");
        }

        public override void Execute(SocketMessage message)
        {
            string[] split = message.Content.Split(new char[] { ' ', '\n' });

            if (split.Length < 2 || split[1] == "help")
                DiscordNETWrapper.SendEmbed(HelpMenu, message.Channel).Wait();
            else if (split[1] == "newGame")
            {
                if (Boards.Exists(x => x.PlayerBottom.UserID == message.Author.Id || x.PlayerTop.UserID == message.Author.Id))
                {
                    DiscordNETWrapper.SendText("You are already in a game!", message.Channel).Wait();
                    return;
                }

                if (message.MentionedUsers.Count != 1)
                {
                    DiscordNETWrapper.SendText("You need exactly one User to play against!", message.Channel).Wait();
                    return;
                }

                if (message.MentionedUsers.ElementAt(0).IsBot || message.Author.IsBot)
                {
                    DiscordNETWrapper.SendText("You can't play against Bots!", message.Channel).Wait();
                    return;
                }
                
                if (Boards.Exists(x => x.PlayerBottom.UserID == message.MentionedUsers.ElementAt(0).Id ||
                        x.PlayerTop.UserID == message.MentionedUsers.ElementAt(0).Id))
                {
                    DiscordNETWrapper.SendText(message.MentionedUsers.ElementAt(0).Mention + " is already in a game!", message.Channel).Wait();
                    return;
                }

                Boards.Add(new ChessBoard(new ChessPlayerDiscord(message.Author.Id),
                           new ChessPlayerDiscord(message.MentionedUsers.ElementAt(0).Id)));
                DiscordNETWrapper.SendText("Created new chess game!", message.Channel).Wait();
                SendBoard(message);
            }
            else if (split[1] == "game")
                SendBoard(message);
            else if (split[1] == "move")
            {
                int x1 = 0, y1 = 0, x2 = 0, y2 = 0;
                if (!Boards.Exists(x => x.PlayerBottom.UserID == message.Author.Id || x.PlayerTop.UserID == message.Author.Id))
                {
                    DiscordNETWrapper.SendText("You are not in a game!", message.Channel).Wait();
                    return;
                }

                ChessBoard Board = Boards.Find(x => x.PlayerBottom.UserID == message.Author.Id || x.PlayerTop.UserID == message.Author.Id);

                if (split.Length != 4)
                {
                    DiscordNETWrapper.SendText("I need exactly 2 coordinates", message.Channel).Wait();
                    return;
                }

                if (Board.PlayerWhoHasTheMove().UserID != message.Author.Id)
                {
                    DiscordNETWrapper.SendText("Its not your turn", message.Channel).Wait();
                    return;
                }

                try
                {
                    string[] temp = split[2].Split(',');
                    x1 = Convert.ToInt32(temp[0]);
                    y1 = Convert.ToInt32(temp[1]);
                }
                catch
                {
                    DiscordNETWrapper.SendText("I dont understand the first set of your coordinates!", message.Channel).Wait();
                    return;
                }

                try
                {
                    string[] temp = split[3].Split(',');
                    x2 = Convert.ToInt32(temp[0]);
                    y2 = Convert.ToInt32(temp[1]);
                }
                catch
                {
                    DiscordNETWrapper.SendText("I dont understand the second set of your coordinates!", message.Channel).Wait();
                    return;
                }

                try
                {
                    Board.MovePiece(new ChessPoint(x1, y1), new ChessPoint(x2, y2));
                    SendBoard(message);

                    if (Board.GameEnded)
                    {
                        DiscordNETWrapper.SendText($"Player {(Board.Winner == Board.PlayerWhite ? "White" : "Black")} won!", message.Channel).Wait();
                        Boards.Remove(Board);
                    }
                }
                catch
                {
                    DiscordNETWrapper.SendText("Oi thats not a legal move!", message.Channel).Wait();
                    return;
                }
            }
            else
                DiscordNETWrapper.SendText("Thats not a proper chess command, type \"$chess help\" if you need some", message.Channel).Wait();
        }

        public void SendBoard(SocketMessage message)
        {
            ChessBoard Board = Boards.Find(x => x.PlayerBottom.UserID == message.Author.Id || 
            x.PlayerTop.UserID == message.Author.Id);
            DiscordNETWrapper.SendBitmap(ChessBoardToPicture(Board), message.Channel, $"<@{Board.PlayerWhite.UserID}>(White) " +
                $"vs. <@{Board.PlayerBlack.UserID}>(Black)\nCurrently its the {(Board.PlayerWhoHasTheMove() == Board.PlayerWhite ? "White" : "Black")} " +
                $"players turn").Wait();
        }
        public string ChessPieceToCharacter(ChessPiece Piece, ChessBoard Board)
        {
            if (Piece == null)
                return "";
            if (Piece.Parent == Board.PlayerBlack)
            {
                switch (Piece.Type)
                {
                    case ChessPieceType.Bishop:
                        return "♝";

                    case ChessPieceType.King:
                        return "♚";

                    case ChessPieceType.Knight:
                        return "♞";

                    case ChessPieceType.Pawn:
                        return "♟";

                    case ChessPieceType.Queen:
                        return "♛";

                    case ChessPieceType.Rook:
                        return "♜";
                }
            }
            else
            {
                switch (Piece.Type)
                {
                    case ChessPieceType.Bishop:
                        return "♗";

                    case ChessPieceType.King:
                        return "♔";

                    case ChessPieceType.Knight:
                        return "♘";

                    case ChessPieceType.Pawn:
                        return "♙";

                    case ChessPieceType.Queen:
                        return "♕";

                    case ChessPieceType.Rook:
                        return "♖";
                }
            }
            return "";
        }
        public string ChessBoardToDiscordString(ChessBoard Board)
        {
            string re = "";

            if (Board.PlayerTop.UserID != 0)
                re += Program.GetUserFromId(Board.PlayerTop.UserID).Mention + "\n";
            else
                re += "Unknown Player\n";

            re += "```\n";
            re += "╔═══╦═══╦═══╦═══╦═══╦═══╦═══╦═══╗\n";
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    ChessPiece Piece = Board.GetChessPieceFromPoint(x, y);
                    if (Piece == null)
                        re += "║   ";
                    else
                        re += $"║ {ChessPieceToCharacter(Board.GetChessPieceFromPoint(x, y), Board)} ";
                }
                if (y != 8 - 1)
                    re += "║\n╠═══╬═══╬═══╬═══╬═══╬═══╬═══╬═══╣\n";
                else
                    re += "║\n╚═══╩═══╩═══╩═══╩═══╩═══╩═══╩═══╝\n";
            }
            re += "\n```";

            if (Board.PlayerBottom.UserID != 0)
                re += Program.GetUserFromId(Board.PlayerBottom.UserID).Mention;
            else
                re += "Unknown Player";

            return re;
        }
        public Bitmap ChessBoardToPicture(ChessBoard Board)
        {
            int FieldOffset = 25;
            int FieldSize = 100;
            Bitmap picture = new Bitmap(8 * FieldSize + FieldOffset, 8 * FieldSize + FieldOffset);
            using (Graphics g = Graphics.FromImage(picture))
            {
                g.FillRectangle(Brushes.White, new Rectangle(0, 0, picture.Width, picture.Height));
                for (int x = 0; x < 8; x++)
                    for (int y = 0; y < 8; y++)
                    {
                        RectangleF FieldRect = new Rectangle(FieldSize * x + FieldOffset, FieldSize * y + FieldOffset, FieldSize, FieldSize);
                        if ((x + y) % 2 == 0)
                            g.FillRectangle(Brushes.Beige, FieldRect);
                        g.DrawString(ChessPieceToCharacter(Board.GetChessPieceFromPoint(x, y), Board), new Font("Arial", (int)(FieldSize / 1.6f)), Brushes.Black, FieldRect);

                        if (y == 0)
                            g.DrawString(x.ToString(), new Font("Arial", (int)(FieldOffset / 1.6f)), Brushes.Black, FieldSize * x + FieldOffset, 0);
                        if (x == 0)
                            g.DrawString(y.ToString(), new Font("Arial", (int)(FieldOffset / 1.6f)), Brushes.Black, 0, FieldSize * y + FieldOffset);
                    }
            }
            return picture;
        }
    }
}
