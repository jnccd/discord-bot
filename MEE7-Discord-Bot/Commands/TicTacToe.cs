using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MEE7.Commands
{
    public class TicTacToe : Command
    {
        List<TicTacToeGame> Games = new List<TicTacToeGame>();

        public TicTacToe() : base("tictactoe", "Play TicTacToe against other people!", false, false)
        {

        }

        public override void Execute(IMessage commandmessage)
        {
            if (!(commandmessage is SocketMessage))
                return;

            var message = commandmessage as SocketMessage;

            if (message.Content.Split(new char[] { ' ', '\n' }).Length < 2 || message.Content.Split(new char[] { ' ', '\n' })[1] == "help")
            {
                EmbedBuilder Embed = new EmbedBuilder();
                Embed.WithColor(0, 128, 255);
                Embed.AddFieldDirectly(Prefix + CommandLine + " newGame + a mentioned user", "Creates a new game against the mentioned user");
                Embed.AddFieldDirectly(Prefix + CommandLine + " set + coordinates", "Sets your symbol at the specified coordinates in the form of \"1,2\" " +
                    "no spaces allowed eg. " + Prefix + CommandLine + " set 2,3\nWarning for Computer Science people: coordinates start at 1!");
                Embed.AddFieldDirectly(Prefix + CommandLine + " game", "Prints the game you are currently in");
                Embed.WithDescription("TicTacToe Commands:");
                DiscordNETWrapper.SendEmbed(Embed, message.Channel).Wait();
            }
            else if (message.Content.Split(new char[] { ' ', '\n' })[1] == "newGame")
            {
                if (message.MentionedUsers.Count < 1 || message.MentionedUsers.Count > 1)
                {
                    DiscordNETWrapper.SendText("You need exactly one player to play against!", message.Channel).Wait();
                }
                else
                {
                    if (Games.Exists(x => x.Player1 == message.MentionedUsers.ElementAt(0)) || Games.Exists(x => x.Player2 == message.MentionedUsers.ElementAt(0)))
                    {
                        DiscordNETWrapper.SendText(message.MentionedUsers.ElementAt(0).Mention + " is already in a game.", message.Channel).Wait();
                    }
                    else if (Games.Exists(x => x.Player1 == message.Author) || Games.Exists(x => x.Player2 == message.Author))
                    {
                        DiscordNETWrapper.SendText("You are already in a game.", message.Channel).Wait();
                    }
                    else
                    {
                        if (message.MentionedUsers.ElementAt(0).IsBot)
                        {
                            if (message.MentionedUsers.ElementAt(0).Id == Program.GetSelf().Id)
                                DiscordNETWrapper.SendText("You will be able to play against me once my master teaches me the game!", message.Channel).Wait();
                            else
                                DiscordNETWrapper.SendText("You cant play with a bot!", message.Channel).Wait();
                        }
                        else
                        {
                            if (message.MentionedUsers.ElementAt(0).Id == message.Author.Id)
                                DiscordNETWrapper.SendText("You can't play against yourself!", message.Channel).Wait();
                            else
                            {
                                Games.Add(new TicTacToeGame(message.MentionedUsers.ElementAt(0), message.Author));
                                DiscordNETWrapper.SendText("Created new game against " + message.MentionedUsers.ElementAt(0) + " successfully!", message.Channel).Wait();
                                DiscordNETWrapper.SendText(Games.Last().ToString(), message.Channel).Wait();
                            }
                        }
                    }
                }
            }
            else if (message.Content.Split(new char[] { ' ', '\n' })[1] == "set")
            {
                TicTacToeGame Game = null;
                try
                {
                    Game = Games.Find(x => x.Player1 == message.Author || x.Player2 == message.Author);
                }
                catch { }

                if (Game == null)
                {
                    DiscordNETWrapper.SendText("You are not in a game!", message.Channel).Wait();
                }
                else
                {
                    if (message.Content.Split(new char[] { ' ', '\n' }).Length < 3)
                    {
                        DiscordNETWrapper.SendText("Where are the coordinates?!", message.Channel).Wait();
                    }
                    else
                    {
                        if (message.Author == Game.Player1 && !Game.Player1sTurn || message.Author == Game.Player2 && Game.Player1sTurn)
                        {
                            DiscordNETWrapper.SendText("Its not your turn!", message.Channel).Wait();
                        }
                        else
                        {
                            byte x = 255, y = 255;
                            try
                            {
                                string coords = message.Content.Split(new char[] { ' ', '\n' })[2];
                                string[] xy = coords.Split(',');
                                x = Convert.ToByte(xy[0]);
                                y = Convert.ToByte(xy[1]);

                                if (x == 0 || y == 0)
                                {
                                    DiscordNETWrapper.SendText("You cant put your symbol there!\nRemember Coordinates start at 1, not 0.", message.Channel).Wait();
                                    return;
                                }

                                x--;
                                y--;
                            }
                            catch
                            {
                                DiscordNETWrapper.SendText("The coordinates Mason what do they mean?!", message.Channel).Wait();
                            }

                            if (Game.Field[x, y] == 0 && x < 3 && y < 3)
                            {
                                Game.Player1sTurn = !Game.Player1sTurn;
                                if (message.Author == Game.Player1)
                                    Game.Field[x, y] = 1;
                                else
                                    Game.Field[x, y] = 2;

                                DiscordNETWrapper.SendText(Game.ToString(), message.Channel).Wait();

                                if (Game.Draw())
                                {
                                    DiscordNETWrapper.SendText("Draw between " + Game.Player1.Mention + " and " + Game.Player2.Mention + "!", message.Channel).Wait();
                                    Games.Remove(Game);
                                }

                                SocketUser Won = Game.PlayerWon();
                                if (Won == Game.Player1)
                                {
                                    DiscordNETWrapper.SendText("The meatbag called " + Game.Player1.Mention + " won!", message.Channel).Wait();
                                    Games.Remove(Game);
                                }
                                else if (Won == Game.Player2)
                                {
                                    DiscordNETWrapper.SendText("The meatbag called " + Game.Player2.Mention + " won!", message.Channel).Wait();
                                    Games.Remove(Game);
                                }
                            }
                            else
                            {
                                DiscordNETWrapper.SendText("You cant put your symbol there!", message.Channel).Wait();
                            }
                        }
                    }
                }
            }
            else if (message.Content.Split(new char[] { ' ', '\n' })[1] == "game" || message.Content.Split(new char[] { ' ', '\n' })[1] == "Game")
            {
                TicTacToeGame Game = null;
                try
                {
                    Game = Games.Find(x => x.Player1 == message.Author || x.Player2 == message.Author);
                }
                catch { }

                if (Game == null)
                    DiscordNETWrapper.SendText("You are in no game!", message.Channel).Wait();
                else
                    DiscordNETWrapper.SendText(Game.ToString(), message.Channel).Wait();
            }
        }
    }
    public class TicTacToeGame
    {
        public bool Player1sTurn = true;
        public SocketUser Player1, Player2;
        public byte[,] Field = new byte[3, 3];

        public TicTacToeGame(SocketUser Player1, SocketUser Player2)
        {
            this.Player1 = Player1;
            this.Player2 = Player2;
        }

        public bool Draw()
        {
            bool Draw = true;
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    if (Field[x, y] == 0)
                        Draw = false;
            return Draw;
        }
        public SocketUser PlayerWon()
        {
            byte p1 = 0, p2 = 0;
            for (int x = 0; x < 3; x++)
            {
                p1 = 0; p2 = 0;
                for (int y = 0; y < 3; y++)
                {
                    if (Field[x, y] == 1)
                        p1++;
                    else if (Field[x, y] == 2)
                        p2++;
                }
                if (p1 == 3)
                    return Player1;
                if (p2 == 3)
                    return Player2;
            }

            for (int y = 0; y < 3; y++)
            {
                p1 = 0; p2 = 0;
                for (int x = 0; x < 3; x++)
                {
                    if (Field[x, y] == 1)
                        p1++;
                    else if (Field[x, y] == 2)
                        p2++;
                }
                if (p1 == 3)
                    return Player1;
                if (p2 == 3)
                    return Player2;
            }

            p1 = 0; p2 = 0;
            for (int i = 0; i < 3; i++)
            {
                if (Field[i, i] == 1)
                    p1++;
                else if (Field[i, i] == 2)
                    p2++;
            }
            if (p1 == 3)
                return Player1;
            if (p2 == 3)
                return Player2;

            p1 = 0; p2 = 0;
            for (int i = 0; i < 3; i++)
            {
                if (Field[2 - i, i] == 1)
                    p1++;
                else if (Field[2 - i, i] == 2)
                    p2++;
            }
            if (p1 == 3)
                return Player1;
            if (p2 == 3)
                return Player2;

            return null;
        }

        public override string ToString()
        {
            string re = "";

            re += Player1.Mention + " (X) vs. " + Player2.Mention + " (O)\n\n";

            re += "```\n";
            re += "╔═══╦═══╦═══╗\n";
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    if (Field[x, y] == 0)
                        re += "║   ";
                    else if (Field[x, y] == 1)
                        re += "║ X ";
                    else if (Field[x, y] == 2)
                        re += "║ O ";
                }
                if (y != 3 - 1)
                    re += "║\n╠═══╬═══╬═══╣\n";
                else
                    re += "║\n╚═══╩═══╩═══╝\n";
            }
            re += "\n```";

            return re;
        }
    }
}
