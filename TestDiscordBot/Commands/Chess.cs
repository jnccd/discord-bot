using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestDiscordBot.Chess;

namespace TestDiscordBot.Commands
{
    public class Chess : Command
    {
        List<ChessBoard> Boards = new List<ChessBoard>();

        public Chess() : base("chess", "Challenge someone to a chess duel!", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            string[] split = commandmessage.Content.Split(' ');

            if (split.Length < 2 || split[1] == "help")
            {
                EmbedBuilder Embed = new EmbedBuilder();
                Embed.WithColor(0, 128, 255);
                Embed.AddField(prefix + command + " newGame + a mentioned user", "Creates a new game against the mentioned user");
                Embed.AddField(prefix + command + " move + 2 coordinates", "Moves a chess piece from the first point to the second\n" +
                    "eg. " + prefix + command + " move 1,1 3,1");
                Embed.AddField(prefix + command + " game", "Prints the game you are currently in");
                Embed.WithDescription("Chess Commands:");
                await Global.SendEmbed(Embed, commandmessage.Channel);
            }
            else if (split[1] == "newGame")
            {
                if (Boards.Exists(x => x.PlayerBottom.UserID == commandmessage.Author.Id || x.PlayerTop.UserID == commandmessage.Author.Id))
                {
                    await Global.SendText("You are already in a game!", commandmessage.Channel);
                    return;
                }

                if (commandmessage.MentionedUsers.Count != 1)
                {
                    await Global.SendText("You need exactly one User to play against!", commandmessage.Channel);
                    return;
                }

                if (commandmessage.MentionedUsers.ElementAt(0).IsBot || commandmessage.Author.IsBot)
                {
                    await Global.SendText("You can't play against Bots!", commandmessage.Channel);
                    return;
                }

                if (commandmessage.MentionedUsers.ElementAt(0).Id == commandmessage.Author.Id)
                {
                    await Global.SendText("You can't play against yourself!", commandmessage.Channel);
                    return;
                }

                if (Boards.Exists(x => x.PlayerBottom.UserID == commandmessage.MentionedUsers.ElementAt(0).Id ||
                        x.PlayerTop.UserID == commandmessage.MentionedUsers.ElementAt(0).Id))
                {
                    await Global.SendText(commandmessage.MentionedUsers.ElementAt(0).Mention + " is already in a game!", commandmessage.Channel);
                    return;
                }

                Boards.Add(new ChessBoard(new ChessPlayerDiscord(commandmessage.Author.Id),
                           new ChessPlayerDiscord(commandmessage.MentionedUsers.ElementAt(0).Id)));
                await Global.SendText("Created new chess game!", commandmessage.Channel);
            }
            else if (split[1] == "game")
            {
                if (!Boards.Exists(x => x.PlayerBottom.UserID == commandmessage.Author.Id || x.PlayerTop.UserID == commandmessage.Author.Id))
                {
                    await Global.SendText("You are not in a game!", commandmessage.Channel);
                    return;
                }

                await Global.SendText(ChessBoardToDiscordString(Boards.Find(x => x.PlayerBottom.UserID == commandmessage.Author.Id || 
                x.PlayerTop.UserID == commandmessage.Author.Id)), commandmessage.Channel);
            }
            else if (split[1] == "move")
            {
                int x1 = 0, y1 = 0, x2 = 0, y2 = 0;
                if (!Boards.Exists(x => x.PlayerBottom.UserID == commandmessage.Author.Id || x.PlayerTop.UserID == commandmessage.Author.Id))
                {
                    await Global.SendText("You are not in a game!", commandmessage.Channel);
                    return;
                }

                ChessBoard Board = Boards.Find(x => x.PlayerBottom.UserID == commandmessage.Author.Id || x.PlayerTop.UserID == commandmessage.Author.Id);

                if (split.Length != 4)
                {
                    await Global.SendText("I need exactly 2 coordinates", commandmessage.Channel);
                    return;
                }

                if (Board.PlayerWhoHasTheMove().UserID != commandmessage.Author.Id)
                {
                    await Global.SendText("Its not your turn", commandmessage.Channel);
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
                    await Global.SendText("I dont understand the first set of your coordinates!", commandmessage.Channel);
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
                    await Global.SendText("I dont understand the second set of your coordinates!", commandmessage.Channel);
                    return;
                }

                try
                {
                    Board.MovePiece(new Point(x1, y1), new Point(x2, y2));
                }
                catch
                {
                    await Global.SendText("Oi thats not a legal move!", commandmessage.Channel);
                    return;
                }
            }
        }

        public string ChessBoardToDiscordString(ChessBoard Board)
        {
            string re = "";

            if (Board.PlayerTop.UserID != 0)
                re += Global.P.getUserFromId(Board.PlayerTop.UserID).Mention + "\n";
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
                    {
                        if (Piece.Parent == Board.PlayerBlack)
                        {
                            switch (Piece.Type)
                            {
                                case ChessPieceType.Bishop:
                                    re += "║ ♝ ";
                                    break;

                                case ChessPieceType.King:
                                    re += "║ ♚ ";
                                    break;

                                case ChessPieceType.Knight:
                                    re += "║ ♞ ";
                                    break;

                                case ChessPieceType.Pawn:
                                    re += "║ ♟ ";
                                    break;

                                case ChessPieceType.Queen:
                                    re += "║ ♛ ";
                                    break;

                                case ChessPieceType.Rook:
                                    re += "║ ♜ ";
                                    break;
                            }
                        }
                        else
                        {
                            switch (Piece.Type)
                            {
                                case ChessPieceType.Bishop:
                                    re += "║ ♗ ";
                                    break;

                                case ChessPieceType.King:
                                    re += "║ ♔ ";
                                    break;

                                case ChessPieceType.Knight:
                                    re += "║ ♘ ";
                                    break;

                                case ChessPieceType.Pawn:
                                    re += "║ ♙ ";
                                    break;

                                case ChessPieceType.Queen:
                                    re += "║ ♕ ";
                                    break;

                                case ChessPieceType.Rook:
                                    re += "║ ♖ ";
                                    break;
                            }
                        }
                    }
                }
                if (y != 8 - 1)
                    re += "║\n╠═══╬═══╬═══╬═══╬═══╬═══╬═══╬═══╣\n";
                else
                    re += "║\n╚═══╩═══╩═══╩═══╩═══╩═══╩═══╩═══╝\n";
            }
            re += "\n```";

            if (Board.PlayerBottom.UserID != 0)
                re += Global.P.getUserFromId(Board.PlayerBottom.UserID).Mention;
            else
                re += "Unknown Player";

            return re;
        }
    }
}
