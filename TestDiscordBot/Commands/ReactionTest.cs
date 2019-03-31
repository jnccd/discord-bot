using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class ReactionTest : Command
    {
        class ReactionMessage
        {
            public RestUserMessage Message;
            public int x = 0;
            public int y = 0;
        }
        List<ReactionMessage> messages = new List<ReactionMessage>();
        readonly Emoji left = new Emoji("◀");
        readonly Emoji up = new Emoji("🔼");
        readonly Emoji down = new Emoji("🔽");
        readonly Emoji right = new Emoji("▶");
        readonly Emoji redCircle = new Emoji("🔴");
        readonly Emoji blackRect = new Emoji("⬛");
        readonly int fieldsx = 11;
        readonly int fieldsy = 5;
        readonly int fieldSize = 10;

        public ReactionTest() : base("reactionTest", "Test discords reaction", false, true)
        {

        }
        
        public override void OnEmojiReactionUpdated(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            foreach (ReactionMessage mes in messages)
                if (mes.Message.Id == arg1.Id)
                {
                    if (arg3.Emote.Name == left.Name)
                        mes.x--;
                    if (arg3.Emote.Name == up.Name)
                        mes.y--;
                    if (arg3.Emote.Name == down.Name)
                        mes.y++;
                    if (arg3.Emote.Name == right.Name)
                        mes.x++;
                    if (mes.x < 0)
                        mes.x = 0;
                    if (mes.x >= fieldsx)
                        mes.x = fieldsx;
                    if (mes.y < 0)
                        mes.y = 0;
                    if (mes.y >= fieldsy)
                        mes.y = fieldsy;
                    mes.Message.ModifyAsync(m => m.Content = TextRenderImage(mes.x, mes.y)).Wait();
                }
        }

        public override void OnExit()
        {
            foreach (ReactionMessage m in messages)
                m.Message.DeleteAsync().Wait();
        }

        public override Task Execute(SocketMessage message)
        {
            lock (this)
            {
                RestUserMessage m = Program.SendText(TextRenderImage(0, 0), message.Channel).Result.First();
                messages.Add(new ReactionMessage() { Message = m, x = 0, y = 0 });
                m.AddReactionAsync(left).Wait();
                m.AddReactionAsync(up).Wait();
                m.AddReactionAsync(down).Wait();
                m.AddReactionAsync(right).Wait();
            }
            return Task.FromResult(0);
        }

        string TextRenderImage(int x, int y)
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
        Bitmap RenderImage(int x, int y)
        {
            Bitmap re = new Bitmap(fieldSize * fieldsx, fieldSize * fieldsy);
            using (Graphics g = Graphics.FromImage(re))
            {
                g.FillRectangle(Brushes.Black, new Rectangle(0, 0, re.Width, re.Height));
                g.FillRectangle(Brushes.Red, new Rectangle(x * fieldSize, y * fieldSize, fieldSize, fieldSize));
            }
            return re;
        }
    }
}
