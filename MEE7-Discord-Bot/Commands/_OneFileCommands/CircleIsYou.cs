using Discord;
using Discord.Rest;
using Discord.WebSocket;
using MEE7.Backend;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MEE7.Commands
{
    public class CircleIsYou : Command
    {
        class Game
        {
            public IUserMessage Message;
            public int x = fieldsx / 2;
            public int y = fieldsy / 2;

            public string TextRenderImage()
            {
                string output = "";
                for (int i = 0; i < fieldsy; i++)
                {
                    for (int j = 0; j < fieldsx; j++)
                    {
                        if (i == y && j == x)
                            output += redCircle;
                        else
                            output += blackRect;
                    }
                    output += "\n";
                }
                return output;
            }
        }
        List<Game> games = new List<Game>();
        readonly static Emoji left = new Emoji("◀");
        readonly static Emoji up = new Emoji("🔼");
        readonly static Emoji down = new Emoji("🔽");
        readonly static Emoji right = new Emoji("▶");
        readonly static Emoji redCircle = new Emoji("🔴");
        readonly static Emoji blackRect = new Emoji("⬛");
        readonly static int fieldsx = 11;
        readonly static int fieldsy = 5;

        public CircleIsYou() : base("circleIsYou", "You are a circle :0", false, true)
        {
            Program.OnEmojiReactionUpdated += OnEmojiReactionUpdated;
            Program.OnExit += OnExit;
        }

        public void OnEmojiReactionUpdated(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            lock (this)
            {
                foreach (Game game in games)
                    if (game.Message.Id == arg1.Id)
                    {
                        if (arg3.Emote.Name == left.Name)
                            game.x--;
                        if (arg3.Emote.Name == up.Name)
                            game.y--;
                        if (arg3.Emote.Name == down.Name)
                            game.y++;
                        if (arg3.Emote.Name == right.Name)
                            game.x++;
                        if (game.x < 0)
                            game.x = 0;
                        if (game.x >= fieldsx)
                            game.x = fieldsx;
                        if (game.y < 0)
                            game.y = 0;
                        if (game.y >= fieldsy)
                            game.y = fieldsy;
                        game.Message.ModifyAsync(m => m.Content = game.TextRenderImage()).Wait();
                    }
            }
        }

        public void OnExit()
        {
            lock (this)
            {
                foreach (Game g in games)
                    g.Message.DeleteAsync().Wait();
            }
        }

        public override void Execute(SocketMessage message)
        {
            lock (this)
            {
                Game newgame = new Game();
                newgame.Message = DiscordNETWrapper.SendText(newgame.TextRenderImage(), message.Channel).Result.First();
                newgame.Message.AddReactionsAsync(new IEmote[] { left, up, down, right }).Wait();
                games.Add(newgame);
            }
        }
    }
}
