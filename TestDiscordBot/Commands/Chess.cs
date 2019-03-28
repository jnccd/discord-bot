using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestDiscordBot.Chess;

namespace TestDiscordBot.Commands
{
    public class Chess : Command
    {
        List<ChessBoard> Boards = new List<ChessBoard>();

        public Chess() : base("chess", "Play chess", false)
        {

        }

        public override async Task Execute(SocketMessage commandmessage)
        {
            string[] split = commandmessage.Content.Split(new char[] { ' ', '\n' });

            if (split.Length < 2 || split[1] == "help")
            {
                EmbedBuilder Embed = new EmbedBuilder();
                Embed.WithColor(0, 128, 255);
                Embed.AddField(Prefix + CommandLine + " newGame + a mentioned user", "Creates a new game against the mentioned user");
                Embed.AddField(Prefix + CommandLine + " move + 2 coordinates", "Moves a chess piece from the first point to the second\n" +
                    "eg. " + Prefix + CommandLine + " move 1,1 3,1");
                Embed.AddField(Prefix + CommandLine + " game", "Prints the game you are currently in");
                Embed.WithDescription("Chess Commands:");
                await Program.SendEmbed(Embed, commandmessage.Channel);
            }
            else if (split[1] == "newGame")
            {
                if (Boards.Exists(x => x.PlayerBottom.UserID == commandmessage.Author.Id || x.PlayerTop.UserID == commandmessage.Author.Id))
                {
                    await Program.SendText("You are already in a game!", commandmessage.Channel);
                    return;
                }

                if (commandmessage.MentionedUsers.Count != 1)
                {
                    await Program.SendText("You need exactly one User to play against!", commandmessage.Channel);
                    return;
                }

                if (commandmessage.MentionedUsers.ElementAt(0).IsBot || commandmessage.Author.IsBot)
                {
                    await Program.SendText("You can't play against Bots!", commandmessage.Channel);
                    return;
                }

                //if (commandmessage.MentionedUsers.ElementAt(0).Id == commandmessage.Author.Id)
                //{
                //    await Program.SendText("You can't play against yourself!", commandmessage.Channel);
                //    return;
                //}

                if (Boards.Exists(x => x.PlayerBottom.UserID == commandmessage.MentionedUsers.ElementAt(0).Id ||
                        x.PlayerTop.UserID == commandmessage.MentionedUsers.ElementAt(0).Id))
                {
                    await Program.SendText(commandmessage.MentionedUsers.ElementAt(0).Mention + " is already in a game!", commandmessage.Channel);
                    return;
                }

                Boards.Add(new ChessBoard(new ChessPlayerDiscord(commandmessage.Author.Id),
                           new ChessPlayerDiscord(commandmessage.MentionedUsers.ElementAt(0).Id)));
                await Program.SendText("Created new chess game!", commandmessage.Channel);
            }
            else if (split[1] == "game")
            {
                if (!Boards.Exists(x => x.PlayerBottom.UserID == commandmessage.Author.Id || x.PlayerTop.UserID == commandmessage.Author.Id))
                {
                    await Program.SendText("You are not in a game!", commandmessage.Channel);
                    return;
                }

                await Program.SendBitmap(ChessBoardToPicture(Boards.Find(x => x.PlayerBottom.UserID == commandmessage.Author.Id || 
                    x.PlayerTop.UserID == commandmessage.Author.Id)), commandmessage.Channel);
            }
            else if (split[1] == "move")
            {
                int x1 = 0, y1 = 0, x2 = 0, y2 = 0;
                if (!Boards.Exists(x => x.PlayerBottom.UserID == commandmessage.Author.Id || x.PlayerTop.UserID == commandmessage.Author.Id))
                {
                    await Program.SendText("You are not in a game!", commandmessage.Channel);
                    return;
                }

                ChessBoard Board = Boards.Find(x => x.PlayerBottom.UserID == commandmessage.Author.Id || x.PlayerTop.UserID == commandmessage.Author.Id);

                if (split.Length != 4)
                {
                    await Program.SendText("I need exactly 2 coordinates", commandmessage.Channel);
                    return;
                }

                if (Board.PlayerWhoHasTheMove().UserID != commandmessage.Author.Id)
                {
                    await Program.SendText("Its not your turn", commandmessage.Channel);
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
                    await Program.SendText("I dont understand the first set of your coordinates!", commandmessage.Channel);
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
                    await Program.SendText("I dont understand the second set of your coordinates!", commandmessage.Channel);
                    return;
                }

                try
                {
                    Board.MovePiece(new ChessPoint(x1, y1), new ChessPoint(x2, y2));
                    await Program.SendBitmap(ChessBoardToPicture(Boards.Find(x => x.PlayerBottom.UserID == commandmessage.Author.Id ||
                        x.PlayerTop.UserID == commandmessage.Author.Id)), commandmessage.Channel);
                }
                catch
                {
                    await Program.SendText("Oi thats not a legal move!", commandmessage.Channel);
                    return;
                }
            }
            else
                await Program.SendText("Thats not a proper chess command, type \"$chess help\" if you need some", commandmessage.Channel);
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
