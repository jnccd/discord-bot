using Discord;
using Discord.WebSocket;
using MEE7.Backend;

namespace MEE7.Commands
{
    public class DelMessages : Command
    {
        public DelMessages() : base("del", "Deletes n past messages", isExperimental: false, isHidden: true)
        {

        }

        public override void Execute(SocketMessage message)
        {
            if (message.Author.Id == Program.Master.Id)
            {
                var amount = 1;
                try { amount = int.Parse(message.Content.Split(' ')[1]); } catch { }

                var messages = message.Channel.GetMessagesAsync(amount).FlattenAsync().Result;
                foreach (var m in messages)
                    m.DeleteAsync().Wait();
            }
        }
    }
}
