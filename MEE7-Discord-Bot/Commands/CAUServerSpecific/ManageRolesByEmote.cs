using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MEE7.Backend.HelperFunctions;
using MEE7.Configuration;
using MEE7.Backend.HelperFunctions.Extensions;

namespace MEE7.Commands.CAUServerSpecific
{
    public class ManageRoleByEmoteMessage
    {
        public ulong MessageID;
        public ulong ChannelID;

        public List<Tuple<IEmote, SocketRole>> EmoteRoleTuples = null;
        public IMessage Message = null;
    }

    public class ManageRolesByEmote : Command
    {
        public ManageRolesByEmote() : base("sendRoleEmojiMessage", "", isExperimental: false, isHidden: true)
        {
            Program.OnConnected += OnConnected;
            Program.OnEmojiReactionAdded += OnEmojiReactionAdded;
            Program.OnEmojiReactionRemoved += OnEmojiReactionRemoved; ;
        }

        private void OnConnected()
        {
            foreach (var m in Config.Data.manageRoleByEmoteMessages)
                try { m.Message = (Program.GetChannelFromID(m.ChannelID) as IMessageChannel).GetMessageAsync(m.MessageID).Result; } catch { }

            Config.Data.manageRoleByEmoteMessages.RemoveAll(m => m.Message == null);
        }

        private void OnEmojiReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            foreach (var m in Config.Data.manageRoleByEmoteMessages)
                foreach (var t in m.EmoteRoleTuples) 
                    if (arg3.Emote.Name == t.Item1.Name)
                    {
                        var u = arg3.User.GetValueOrDefault();
                        var g = Program.GetGuildFromChannel(arg2);
                        g.Users.First(x => x.Id == u.Id).AddRoleAsync(g.Roles.First(x => x.Id == t.Item2.Id));
                    }
        }

        private void OnEmojiReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            foreach (var m in Config.Data.manageRoleByEmoteMessages)
                foreach (var t in m.EmoteRoleTuples)
                    if (arg3.Emote.Name == t.Item1.Name)
                    {
                        var u = arg3.User.GetValueOrDefault();
                        var g = Program.GetGuildFromChannel(arg2);
                        g.Users.First(x => x.Id == u.Id).RemoveRoleAsync(g.Roles.First(x => x.Id == t.Item2.Id));
                    }
        }

        public override void Execute(SocketMessage message)
        {
            var roles = Program.GetGuildFromChannel(message.Channel).Roles;

            List<Tuple<IEmote, SocketRole>> emoteRoleTuples = message.Content.
                Remove(0, PrefixAndCommand.Length + 1).
                Split('|').
                Select(x => x.Trim(' ')).
                Select(x => x.Split(' ')).
                Select(s => {
                    IEmote e = null;
                    SocketRole r = null;

                    if (Emote.TryParse(s[0], out Emote et))
                        e = et;
                    else
                        e = new Emoji(s[0]);

                    r = roles.First(x => x.Name == s[1]);

                    return new Tuple<IEmote, SocketRole>(e, r);
                }).ToList();

            var roleMessage = DiscordNETWrapper.SendText("To get a role simply react to this message with the corresponding emote,\n" +
                "to remove a role simply remove your reaction to this message.\n\n Emotes are mapped as follows:\n" + 
                emoteRoleTuples.Select(x => $"{x.Item1.Print()} - {x.Item2.Name}").Aggregate((x,y) => $"{x}\n{y}"), message.Channel).Result.Last();

            roleMessage.AddReactionsAsync(emoteRoleTuples.Select(x => x.Item1).ToArray()).Wait();
            Config.Data.manageRoleByEmoteMessages.Add(new ManageRoleByEmoteMessage() 
            { 
                EmoteRoleTuples = emoteRoleTuples, 
                MessageID = roleMessage.Id, 
                ChannelID = roleMessage.Channel.Id, 
                Message = roleMessage 
            });
        }
    }
}
