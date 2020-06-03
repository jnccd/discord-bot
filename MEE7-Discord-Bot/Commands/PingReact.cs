using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System.Linq;

namespace MEE7.Commands
{
    public class PingReact : Command
    {
        Emote pingRage;

        public PingReact() : base("", "", false, true)
        {
            pingRage = Emote.Parse("<a:pingRage:702500510481121411>");
            Program.OnNonCommandMessageRecieved += OnNonCommandMessageRecieved;
        }

        public void OnNonCommandMessageRecieved(IMessage messageIn)
        {
            if (!(messageIn is SocketMessage))
            {
                DiscordNETWrapper.SendText("This command only works on discord", messageIn.Channel).Wait();
                return;
            }
            var message = messageIn as SocketMessage;

            if (message.MentionedUsers.Count == 0 && message.MentionedRoles.Count == 0 || !(message is IUserMessage))
                return;

            SocketRole[] r = Program.GetGuildFromChannel(message.Channel).GetUser(Program.GetSelf().Id).Roles.ToArray();
            if (message.MentionedUsers.FirstOrDefault(x => x.Id == Program.GetSelf().Id) != null)
                PingReaction(message as IUserMessage);
            if (message.MentionedRoles.ContainsAny(r))
                PingReaction(message as IUserMessage);
        }
        void PingReaction(IUserMessage message)
        {
            message.AddReactionsAsync(new IEmote[] { pingRage }).Wait();
        }

        public override void Execute(IMessage message)
        {
            return;
        }
    }
}
