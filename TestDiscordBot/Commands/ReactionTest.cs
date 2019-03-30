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
    public class ReactionTest : Command
    {
        class ReactionMessage
        {
            public ulong MessageID;
            public int x = 0;
            public int y = 0;
        }

        List<ReactionMessage> messages = new List<ReactionMessage>();

        public ReactionTest() : base("", "", true, true)
        {

        }
        
        public override void OnEmojiReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            
        }

        public override async Task Execute(SocketMessage message)
        {
            string[] split = message.Content.Split(' ');
            if (split.Length > 1 && split[1] == "new")
            {
                var m = await Program.SendBitmap(new Bitmap(1, 1), message.Channel);
                messages.Add(new ReactionMessage() { MessageID = m.Id, x = 5, y = 5 });
            }
        }
    }
}
