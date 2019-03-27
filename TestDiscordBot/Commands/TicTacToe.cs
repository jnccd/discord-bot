using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class TicTacToe : Command
    {
        List<TicTacToeGame> Games = new List<TicTacToeGame>();

        public TicTacToe() : base("tictactoe", "Play TicTacToe against random people!", false)
        {

        }

        public override async Task Execute(SocketMessage commandmessage)
        {
            if (commandmessage.Content.Split(new char[] { ' ', '\n' }).Length < 2 || commandmessage.Content.Split(new char[] { ' ', '\n' })[1] == "help")
            {
                EmbedBuilder Embed = new EmbedBuilder();
                Embed.WithColor(0, 128, 255);
                Embed.AddField(Prefix + CommandLine + " newGame + a mentioned user", "Creates a new game against the mentioned user");
                Embed.AddField(Prefix + CommandLine + " set + coordinates", "Sets your symbol at the specified coordinates in the form of \"1,2\" " +
                    "no spaces allowed eg. " + Prefix + CommandLine + " set 2,3\nWarning for Computer Science people: coordinates start at 1!");
                Embed.AddField(Prefix + CommandLine + " game", "Prints the game you are currently in");
                Embed.WithDescription("TicTacToe Commands:");
                await Program.SendEmbed(Embed, commandmessage.Channel);
            }
            else if (commandmessage.Content.Split(new char[] { ' ', '\n' })[1] == "newGame")
            {
                if (commandmessage.MentionedUsers.Count < 1 || commandmessage.MentionedUsers.Count > 1)
                {
                    await Program.SendText("You need exactly one player to play against!", commandmessage.Channel);
                }
                else
                {
                    if (Games.Exists(x => x.Player1 == commandmessage.MentionedUsers.ElementAt(0)) || Games.Exists(x => x.Player2 == commandmessage.MentionedUsers.ElementAt(0)))
                    {
                        await Program.SendText(commandmessage.MentionedUsers.ElementAt(0).Mention + " is already in a game.", commandmessage.Channel);
                    }
                    else if (Games.Exists(x => x.Player1 == commandmessage.Author) || Games.Exists(x => x.Player2 == commandmessage.Author))
                    {
                        await Program.SendText("You are already in a game.", commandmessage.Channel);
                    }
                    else
                    {
                        if (commandmessage.MentionedUsers.ElementAt(0).IsBot)
                        {
                            if (commandmessage.MentionedUsers.ElementAt(0).Id == Program.GetSelf().Id)
                                await Program.SendText("You will be able to play against me once my master teaches me the game!", commandmessage.Channel);
                            else
                                await Program.SendText("You cant play with a bot!", commandmessage.Channel);
                        }
                        else
                        {
                            if (commandmessage.MentionedUsers.ElementAt(0).Id == commandmessage.Author.Id)
                                await Program.SendText("You can't play against yourself!", commandmessage.Channel);
                            else
                            {
                                Games.Add(new TicTacToeGame(commandmessage.MentionedUsers.ElementAt(0), commandmessage.Author));
                                await Program.SendText("Created new game against " + commandmessage.MentionedUsers.ElementAt(0) + " successfully!", commandmessage.Channel);
                                await Program.SendText(Games.Last().ToString(), commandmessage.Channel);
                            }
                        }
                    }
                }
            }
            else if (commandmessage.Content.Split(new char[] { ' ', '\n' })[1] == "set")
            {
                TicTacToeGame Game = null;
                try
                {
                    Game = Games.Find(x => x.Player1 == commandmessage.Author || x.Player2 == commandmessage.Author);
                }
                catch { }

                if (Game == null)
                {
                    await Program.SendText("You are not in a game!", commandmessage.Channel);
                }
                else
                {
                    if (commandmessage.Content.Split(new char[] { ' ', '\n' }).Length < 3)
                    {
                        await Program.SendText("Where are the coordinates?!", commandmessage.Channel);
                    }
                    else
                    {
                        if (commandmessage.Author == Game.Player1 && !Game.Player1sTurn || commandmessage.Author == Game.Player2 && Game.Player1sTurn)
                        {
                            await Program.SendText("Its not your turn!", commandmessage.Channel);
                        }
                        else
                        {
                            byte x = 255, y = 255;
                            try
                            {
                                string coords = commandmessage.Content.Split(new char[] { ' ', '\n' })[2];
                                string[] xy = coords.Split(',');
                                x = Convert.ToByte(xy[0]);
                                y = Convert.ToByte(xy[1]);

                                if (x == 0 || y == 0)
                                {
                                    await Program.SendText("You cant put your symbol there!\nRemember Coordinates start at 1, not 0.", commandmessage.Channel);
                                    return;
                                }

                                x--;
                                y--;
                            }
                            catch
                            {
                                await Program.SendText("The coordinates Mason what do they mean?!", commandmessage.Channel);
                            }

                            if (Game.Field[x, y] == 0 && x < 3 && y < 3)
                            {
                                Game.Player1sTurn = !Game.Player1sTurn;
                                if (commandmessage.Author == Game.Player1)
                                    Game.Field[x, y] = 1;
                                else
                                    Game.Field[x, y] = 2;

                                await Program.SendText(Game.ToString(), commandmessage.Channel);

                                if (Game.Draw())
                                {
                                    await Program.SendText("Draw between " + Game.Player1.Mention + " and " + Game.Player2.Mention + "!", commandmessage.Channel);
                                    Games.Remove(Game);
                                }

                                SocketUser Won = Game.PlayerWon();
                                if (Won == Game.Player1)
                                {
                                    await Program.SendText("The meatbag called " + Game.Player1.Mention + " won!", commandmessage.Channel);
                                    Games.Remove(Game);
                                }
                                else if (Won == Game.Player2)
                                {
                                    await Program.SendText("The meatbag called " + Game.Player2.Mention + " won!", commandmessage.Channel);
                                    Games.Remove(Game);
                                }
                            }
                            else
                            {
                                await Program.SendText("You cant put your symbol there!", commandmessage.Channel);
                            }
                        }
                    }
                }
            }
            else if (commandmessage.Content.Split(new char[] { ' ', '\n' })[1] == "game" || commandmessage.Content.Split(new char[] { ' ', '\n' })[1] == "Game")
            {
                TicTacToeGame Game = null;
                try
                {
                    Game = Games.Find(x => x.Player1 == commandmessage.Author || x.Player2 == commandmessage.Author);
                }
                catch { }

                if (Game == null)
                    await Program.SendText("You are in no game!", commandmessage.Channel);
                else
                    await Program.SendText(Game.ToString(), commandmessage.Channel);
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
