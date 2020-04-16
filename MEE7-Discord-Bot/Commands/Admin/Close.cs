using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;

namespace MEE7.Commands
{
    public class Close : Command
    {
        public Close() : base("close", "Closes the bot", false, true)
        {

        }

        public override void Execute(SocketMessage message)
        {
            if (message.Author.Id == Program.Master.Id)
            {
                DiscordNETWrapper.SendText("Closing...", message.Channel).Wait();
                Program.Exit(0);
            }
        }
    }
}
