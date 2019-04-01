using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class Uno : Command
    {
        public Uno() : base("uno", "Play uno with other humanoids", false, true)
        {
            // TODO: Build HelpMenu
        }

        class UnoGame
        {
            List<ulong> Players = new List<ulong>();
            Bitmap curStack = new Bitmap(1000, 1000);
            UnoCard TopStackCard = null;
            bool ReversedTurns = false;
            int curPlayerIndex = 0;

            public UnoGame(List<ulong> Players)
            {
                this.Players = Players;
                if (Players.Count < 2)
                    throw new ArgumentOutOfRangeException("Too few players!");
            }
        }
        class UnoCard
        {
            UnoColor Color;
            UnoCardType Type;

            public UnoCard(UnoColor Color, UnoCardType Type)
            {
                this.Color = Color;
                this.Type = Type;
            }
        }
        enum UnoColor { Red, Yellow, Blue, Green, None }
        enum UnoCardType { One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Skip, Reverse, Plus2, Plus4, ChangeColor }

        readonly List<UnoCard> UnoCards = GetUnoCards();
        List<UnoGame> UnoGames = new List<UnoGame>();

        static bool HasColor(UnoCardType t)
        {
            if (t == UnoCardType.ChangeColor ||
                t == UnoCardType.Plus4)
                return false;
            else
                return true;
        }
        private static List<UnoCard> GetUnoCards()
        {
            List<UnoCard> cards = new List<UnoCard>();
            foreach (UnoCardType t in Enum.GetValues(typeof(UnoCardType)))
                if (HasColor(t))
                {
                    cards.Add(new UnoCard(UnoColor.Red, t));
                    cards.Add(new UnoCard(UnoColor.Yellow, t));
                    cards.Add(new UnoCard(UnoColor.Blue, t));
                    cards.Add(new UnoCard(UnoColor.Green, t));
                }
                else
                    cards.Add(new UnoCard(UnoColor.None, t));
            return cards;
        }
        
        public override Task Execute(SocketMessage message)
        {
            string[] split = message.Content.Split(' ');
            if (split.Contains("new"))
                UnoGames.Add(new UnoGame(message.MentionedUsers.Select(x =>
                {
                    if (x.IsBot)
                        throw new Exception("Bots cant play Uno!");
                    return x.Id;
                }).ToList()));
            else if (split.Length > 2)
            {

            }
            else
                Program.SendEmbed(HelpMenu, message.Channel).Wait();
            return Task.FromResult(default(object));
        }
    }
}
