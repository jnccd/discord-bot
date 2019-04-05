using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
{
    public class Poll : Command
    {
        public Poll() : base("poll", "Creates a poll for Darky", false)
        {

        }

        public override Task Execute(SocketMessage message)
        {
            string[] split = message.Content.Split(' ');
            string[] pollOptions = split[2].Split(',');

            IUserMessage pollMessage = Program.SendText(split[1] + "\n\n" + pollOptions.Select(x => pollOptions.ToList().IndexOf(x) + ". " + x).Aggregate((x, y) => x + "\n" + y), message.Channel).Result.First();
            pollMessage.AddReactionsAsync(new IEmote[] { new Emoji("0⃣"), new Emoji("1⃣"), new Emoji("1⃣"), new Emoji("2⃣"), new Emoji("3⃣"), new Emoji("4⃣"), new Emoji("5⃣"), new Emoji("6⃣"), new Emoji("7⃣"), new Emoji("8⃣"), new Emoji("9⃣") }.Take(pollOptions.Length).ToArray());

            return Task.FromResult(default(object));
        }
    }
}
