using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public static partial class Extensions
    {
        public static Point RotatePointAroundPoint(this Point P, Point RotationOrigin, double angle)
        {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            return new Point((int)(cos * (P.X - RotationOrigin.X) - sin * (P.Y - RotationOrigin.Y) + RotationOrigin.X),
                             (int)(sin * (P.X - RotationOrigin.X) + cos * (P.Y - RotationOrigin.Y) + RotationOrigin.Y));
        }
    }

    public class Uno : Command
    {
        public Uno() : base("uno", "Play uno with other humanoids", false, true)
        {
            HelpMenu = new EmbedBuilder();
            HelpMenu.WithColor(0, 128, 255);
            HelpMenu.WithDescription("Uno Commands:");
            HelpMenu.AddField(PrefixAndCommand + " new + mentioned users", "Creates a new game with the mentioned users");
            HelpMenu.AddField(PrefixAndCommand + " move + a card", "Puts the card on the stack\n" +
                $"eg. {PrefixAndCommand} move 1 green\n" +
                $"eg. {PrefixAndCommand} move plus4 rEd" +
                $"The latter will move a Plus4 and change the stacks color to Red, Plus4/ChangeColor cards don't have a color though");
            HelpMenu.AddField(PrefixAndCommand + " draw", "Draw a new card");
            HelpMenu.AddField(PrefixAndCommand + " print", "Prints the stack of the game you are currently in");
            HelpMenu.AddField("Valid Cards are: ", UnoCards.Select(x => x.Type + (x.Color == UnoColor.none ? "" : " " + x.Color.ToString())).Aggregate((x, y) => x + ", " + y));
        }

        class UnoGame
        {
            public List<Tuple<SocketUser, List<UnoCard>>> Players = new List<Tuple<SocketUser, List<UnoCard>>>();
            Bitmap curStack = new Bitmap(1000, 1000);
            UnoCard TopStackCard = null;
            bool ReversedTurns = false;
            int curPlayerIndex = 0;
            UnoColor CurColor;

            public UnoGame(List<SocketUser> Players)
            {
                this.Players = Players.Select(x => new Tuple<SocketUser, List<UnoCard>>(x, GetStartingDeck())).ToList();
            }
            private static List<UnoCard> GetStartingDeck()
            {
                List<UnoCard> cards = new List<UnoCard>(7);
                for (int i = 0; i < 7; i++)
                    cards.Add(UnoCards.GetRandomValue());
                return cards;
            }

            public bool CanPutCardOnTopOfStack(UnoCard newCard)
            {
                return newCard.Color == UnoColor.none ||
                       TopStackCard == null ||
                       CurColor == newCard.Color ||
                       TopStackCard.Type == newCard.Type;
            }

            public void PutCardOnTopOfStack(UnoCardType t, UnoColor c, ulong PlayerID, ISocketMessageChannel channel)
            {
                if (!HasColor(t))
                    c = UnoColor.none;
                UnoCard newCard = UnoCards.FirstOrDefault(x => x.Color == c && x.Type == t);
                if (HasColor(t) && c == UnoColor.none)
                    newCard = null;

                if (newCard != null)
                    if (CanPutCardOnTopOfStack(newCard))
                    {
                        Players.Find(x => x.Item1.Id == PlayerID).Item2.Remove(newCard);
                        DrawCardOnStack(newCard);
                        TopStackCard = newCard;
                        CurColor = c;
                        if (newCard.Type == UnoCardType.reverse)
                            ReversedTurns = !ReversedTurns;
                        NextPlayer();
                        if (newCard.Type == UnoCardType.skip)
                            NextPlayer();
                        if (newCard.Type == UnoCardType.plus2)
                            DrawCards(2, Players[curPlayerIndex]);
                        if (newCard.Type == UnoCardType.plus4)
                            DrawCards(4, Players[curPlayerIndex]);
                    }
                    else
                        Program.SendText("You can't put that card on top of the stack :thinking:", channel).Wait();
                else
                    Program.SendText("Oi that card doesn't exist!", channel).Wait();
            }
            private void DrawCardOnStack(UnoCard newCard)
            {
                Point topLeft = new Point(curStack.Width / 2 - newCard.Picture.Width / 2 + Program.RDM.Next(200) - 100,
                                              curStack.Width / 2 - newCard.Picture.Width / 2 + Program.RDM.Next(200) - 100);
                Point topRight = new Point(topLeft.X + newCard.Picture.Width, topLeft.Y);
                Point botLeft = new Point(topLeft.X, topLeft.Y + newCard.Picture.Height);

                Point Origin = new Point(500, 500);
                double rotationAngle = Program.RDM.NextDouble() * 2 - 1;
                topLeft = topLeft.RotatePointAroundPoint(Origin, rotationAngle);
                topRight = topRight.RotatePointAroundPoint(Origin, rotationAngle);
                botLeft = botLeft.RotatePointAroundPoint(Origin, rotationAngle);

                using (Graphics g = Graphics.FromImage(curStack))
                    g.DrawImage(newCard.Picture, new Point[] { topLeft, topRight, botLeft });
            }
            private void NextPlayer()
            {
                if (!ReversedTurns)
                {
                    curPlayerIndex++;
                    if (curPlayerIndex >= Players.Count)
                        curPlayerIndex = 0;
                }
                else
                {
                    curPlayerIndex--;
                    if (curPlayerIndex < 0)
                        curPlayerIndex = Players.Count - 1;
                }
            }
            public void DrawCards(int count, ulong playerID)
            {
                DrawCards(count, Players.First(y => y.Item1.Id == playerID));
            }
            public void DrawCards(int count, Tuple<SocketUser, List<UnoCard>> player)
            {
                if (player != null)
                    for (int i = 0; i < count; i++)
                        player.Item2.Add(UnoCards.GetRandomValue());
            }
            public static Bitmap RenderDeck(List<UnoCard> cards)
            {
                if (cards.Count == 0)
                    return new Bitmap(1, 1);

                int padding = 15;
                Bitmap re = new Bitmap(cards[0].Picture.Width * cards.Count + padding * (cards.Count - 1), cards[0].Picture.Height);
                using (Graphics g = Graphics.FromImage(re))
                    for (int i = 0; i < cards.Count; i++)
                        g.DrawImageUnscaled(cards[i].Picture, new Point((cards[0].Picture.Width + padding) * i, 0));
                return re;
            }

            public void Send(ISocketMessageChannel Channel)
            {
                Program.SendBitmap(curStack, Channel, $"Players in this game: " +
                    $"{Players.Select(x => $"`{x.Item1.Username}`[{x.Item2.Count}]").Aggregate((x, y) => x + " " + y)}\n" +
                    $"It's `{Players[curPlayerIndex].Item1.Username}'s` turn and the current color is `{CurColor.ToString()}`").Wait();
                foreach (Tuple<SocketUser, List<UnoCard>> player in Players)
                    Program.SendBitmap(RenderDeck(player.Item2), player.Item1.GetOrCreateDMChannelAsync().Result).Wait();
            }
        }
        class UnoCard
        {
            public UnoColor Color;
            public UnoCardType Type;
            public Bitmap Picture;

            public UnoCard(UnoColor Color, UnoCardType Type)
            {
                this.Color = Color;
                this.Type = Type;
                this.Picture = GetPicture(Color, Type);
            }
        }
        enum UnoColor { red, yellow, blue, green, none }
        enum UnoCardType { one, two, three, four, five, six, seven, eight, nine, skip, reverse, plus2, plus4, changecolor }

        readonly static Bitmap CardsTexture = (Bitmap)Bitmap.FromFile("Commands\\Uno\\UNO-Front.png");
        readonly static List<UnoCard> UnoCards = GetUnoCards();
        List<UnoGame> UnoGames = new List<UnoGame>();

        static bool HasColor(UnoCardType t)
        {
            return t != UnoCardType.changecolor && t != UnoCardType.plus4;
        }
        static bool IsNumber(UnoCardType t)
        {
            return t != UnoCardType.changecolor && t != UnoCardType.plus4 && t != UnoCardType.skip && t != UnoCardType.reverse && t != UnoCardType.plus2;
        }
        static int ToNumber(UnoCardType t)
        {
            if (IsNumber(t))
            {
                if (t == UnoCardType.one)
                    return 1;
                else if (t == UnoCardType.two)
                    return 2;
                else if (t == UnoCardType.three)
                    return 3;
                else if (t == UnoCardType.four)
                    return 4;
                else if (t == UnoCardType.five)
                    return 5;
                else if (t == UnoCardType.six)
                    return 6;
                else if (t == UnoCardType.seven)
                    return 7;
                else if (t == UnoCardType.eight)
                    return 8;
                else if (t == UnoCardType.nine)
                    return 9;
                else
                    return -1;
            }
            else
                return -1;
        }
        static UnoCardType ToUnoType(int i)
        {
            switch (i)
            {
                case 1:
                    return UnoCardType.one;
                case 2:
                    return UnoCardType.two;
                case 3:
                    return UnoCardType.three;
                case 4:
                    return UnoCardType.four;
                case 5:
                    return UnoCardType.five;
                case 6:
                    return UnoCardType.six;
                case 7:
                    return UnoCardType.seven;
                case 8:
                    return UnoCardType.eight;
                case 9:
                    return UnoCardType.nine;
            }
            return new UnoCardType();
        }
        private static List<UnoCard> GetUnoCards()
        {
            List<UnoCard> cards = new List<UnoCard>();
            foreach (UnoCardType t in Enum.GetValues(typeof(UnoCardType)))
                if (HasColor(t))
                {
                    cards.Add(new UnoCard(UnoColor.red, t));
                    cards.Add(new UnoCard(UnoColor.yellow, t));
                    cards.Add(new UnoCard(UnoColor.blue, t));
                    cards.Add(new UnoCard(UnoColor.green, t));
                }
                else
                    cards.Add(new UnoCard(UnoColor.none, t));
            return cards;
        }
        private static Bitmap GetPicture(UnoColor c, UnoCardType t)
        {
            float cardWidth = 4096 / 10f;
            float cardHeight = 4096 / 7f;
            int CutOutWidth = (int)cardWidth + 29;
            int CutOutHeight = (int)cardHeight + 40;
            if (IsNumber(t))
            {
                int X = (int)(cardWidth * (ToNumber(t) - 1));
                int YNumber = c == UnoColor.red ? 0 : (c == UnoColor.yellow ? 1 : (c == UnoColor.blue ? 2 : (c == UnoColor.green ? 3 : -1)));
                int Y = (int)(cardHeight * YNumber);
                return CardsTexture.CropImage(new Rectangle(X, Y, CutOutWidth, CutOutHeight));
            }
            else
            {
                if (t == UnoCardType.changecolor)
                    return CardsTexture.CropImage(new Rectangle((int)(9 * cardWidth), (int)(0 * cardHeight), CutOutWidth, CutOutHeight));
                else if (t == UnoCardType.plus4)
                    return CardsTexture.CropImage(new Rectangle((int)(9 * cardWidth), (int)(2 * cardHeight), CutOutWidth, CutOutHeight));
                else if (t == UnoCardType.skip)
                {
                    int XNumber = c == UnoColor.red ? 0 : (c == UnoColor.yellow ? 1 : (c == UnoColor.blue ? 2 : (c == UnoColor.green ? 3 : -1)));
                    return CardsTexture.CropImage(new Rectangle((int)(XNumber * cardWidth), (int)(4 * cardHeight), CutOutWidth, CutOutHeight));
                }
                else if (t == UnoCardType.plus2)
                {
                    int XNumber = 4 + (c == UnoColor.red ? 0 : (c == UnoColor.yellow ? 1 : (c == UnoColor.blue ? 2 : (c == UnoColor.green ? 3 : -1))));
                    return CardsTexture.CropImage(new Rectangle((int)(XNumber * cardWidth), (int)(4 * cardHeight), CutOutWidth, CutOutHeight));
                }
                else if (t == UnoCardType.reverse)
                {
                    if (c == UnoColor.red)
                        return CardsTexture.CropImage(new Rectangle((int)(8 * cardWidth), (int)(4 * cardHeight), CutOutWidth, CutOutHeight));
                    else if (c == UnoColor.yellow)
                        return CardsTexture.CropImage(new Rectangle((int)(9 * cardWidth), (int)(4 * cardHeight), CutOutWidth, CutOutHeight));
                    else if (c == UnoColor.blue)
                        return CardsTexture.CropImage(new Rectangle((int)(0 * cardWidth), (int)(5 * cardHeight), CutOutWidth, CutOutHeight));
                    else if (c == UnoColor.green)
                        return CardsTexture.CropImage(new Rectangle((int)(1 * cardWidth), (int)(5 * cardHeight), CutOutWidth, CutOutHeight));
                }
            }
            return null;
        }
        
        public override Task Execute(SocketMessage message)
        {
            string[] split = message.Content.Split(' ');
            if (split.Contains("new"))
            {
                UnoGame newGame = new UnoGame(message.MentionedUsers.Union(new SocketUser[] { message.Author }).ToList());
                if (newGame.Players.Count < 2)
                {
                    Program.SendText("You need more players to play Uno!", message.Channel).Wait();
                    return Task.FromResult(default(object));
                }
                if (newGame.Players.Exists(x => x.Item1.IsBot))
                {
                    Program.SendText("Bots can't play Uno!", message.Channel).Wait();
                    return Task.FromResult(default(object));
                }
                UnoGames.Add(newGame);
                newGame.Send(message.Channel);
            }
            else if (split.Contains("print_cards")) // Size = {Width = 434 Height = 621}
                foreach (UnoCard c in UnoCards)
                    Program.SendBitmap(c.Picture, message.Channel, c.Type.ToString() + " " + c.Color.ToString()).Wait();
            else if (split.Contains("print"))
            {
                UnoGame game = UnoGames.FirstOrDefault(x => x.Players.Exists(y => y.Item1.Id == message.Author.Id));
                if (game != null)
                    game.Send(message.Channel);
                else
                    Program.SendText("You are not in a game :thinking:", message.Channel).Wait();
            }
            else if (split.Length >= 2 && split[1] == "draw")
            {
                var game = UnoGames.FirstOrDefault(x => x.Players.Exists(y => y.Item1.Id == message.Author.Id));
                if (game != null)
                    game.DrawCards(1, message.Author.Id);
                {
                    Program.SendText("You are not in a game :thinking:", message.Channel).Wait();
                    return Task.FromResult(default(object));
                }
            }
            else if (split.Length >= 4 && split[1] == "move")
            {
                UnoGame game = UnoGames.FirstOrDefault(x => x.Players.Exists(y => y.Item1.Id == message.Author.Id));
                if (game == null)
                {
                    Program.SendText("You are not in a game :thinking:", message.Channel).Wait();
                    return Task.FromResult(default(object));
                }
                
                UnoCardType t = UnoCardType.one;
                UnoColor c = UnoColor.none;
                Enum.TryParse(split[2].ToLower(), out t);
                Enum.TryParse(split[3].ToLower(), out c);
                if (split[2].Length > 0 && char.IsDigit(split[2][0]))
                    t = ToUnoType(Convert.ToInt32(split[2][0]));
                game.PutCardOnTopOfStack(t, c, message.Author.Id, message.Channel);
                game.Send(message.Channel);
            }
            else
                Program.SendEmbed(HelpMenu, message.Channel).Wait();
            return Task.FromResult(default(object));
        }
    }
}
