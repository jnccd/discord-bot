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
using MEE7.Backend.Configuration;

namespace MEE7.Commands
{
    public class ManageRoleByEmoteMessage
    {
        public ulong MessageID;
        public ulong ChannelID;
 
        public List<Tuple<
            DiscordEmote, // Emote by Name,EmoteID,GuildID
            ulong> // RoleID
            >EmoteRoleTuples = null;
    }

    public class ManageRolesByEmote : Command
    {
        public ManageRolesByEmote() : base("sendRoleEmojiMessage", "", isExperimental: false, isHidden: false)
        {
            Program.OnEmojiReactionAdded += OnEmojiReactionAdded;
            Program.OnEmojiReactionRemoved += OnEmojiReactionRemoved; ;
        }

        private void OnEmojiReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            foreach (var m in Config.Data.manageRoleByEmoteMessages)
                foreach (var t in m.EmoteRoleTuples) 
                    if (arg3.Emote.Name == t.Item1.Name)
                    {
                        var u = arg3.User.GetValueOrDefault();
                        var g = Program.GetGuildFromChannel(arg2);
                        g.Users.First(x => x.Id == u.Id).AddRoleAsync(g.Roles.First(x => x.Id == t.Item2));
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
                        g.Users.First(x => x.Id == u.Id).RemoveRoleAsync(g.Roles.First(x => x.Id == t.Item2));
                    }
        }

        public override void Execute(SocketMessage message)
        {
            //if (Program.GetGuildFromChannel(message.Channel).Roles.
            //    FirstOrDefault(x => x.Permissions.ManageRoles && x.Members.FirstOrDefault(y => y.Id == message.Author.Id) != null) == null)
            //{
            //    DiscordNETWrapper.SendText("You don't have permissions to give roles!", message.Channel).Wait();
            //    return;
            //}
            
            var g = Program.GetGuildFromChannel(message.Channel);
            if (message.Author.Id != g.Owner.Id && message.Author.Id != Program.Master.Id)
            {
                DiscordNETWrapper.SendText("You don't have permissions to use this command!", message.Channel).Wait();
                return;
            }

            var roles = Program.GetGuildFromChannel(message.Channel).Roles;

            List<Tuple<DiscordEmote, ulong>> emoteRoleTuples = message.Content.
                Remove(0, PrefixAndCommand.Length + 1).
                Split('|').
                Select(x => x.Trim(' ')).
                Select(x => x.Split(' ')).
                Select(s => {
                    IEmote e = null;
                    SocketRole r = null;

                    if (s.Length != 2)
                        throw new Exception(s.Combine(" "));

                    if (Emote.TryParse(s[0], out Emote et))
                        e = et;
                    else
                        e = new Emoji(s[0]);

                    r = roles.First(x => x.Name == s[1] || x.Id.ToString() == s[1]);

                    return new Tuple<DiscordEmote, ulong>(DiscordEmote.FromIEmote(e), r.Id);
                }).ToList();

            var roleMessage = DiscordNETWrapper.SendText("To get a role simply react to this message with the corresponding emote,\n" +
                "to remove a role simply remove your reaction to this message.\n\n Emotes are mapped as follows:\n" + 
                emoteRoleTuples.Select(x => $"{x.Item1.Print()} - {g.Roles.First(y => y.Id == x.Item2).Name}").
                Aggregate((x,y) => $"{x}\n{y}"), message.Channel).Result.Last();

            roleMessage.AddReactionsAsync(emoteRoleTuples.Select(x => x.Item1.ToIEmote()).ToArray()).Wait();
            Config.Data.manageRoleByEmoteMessages.Add(new ManageRoleByEmoteMessage() 
            { 
                EmoteRoleTuples = emoteRoleTuples, 
                MessageID = roleMessage.Id, 
                ChannelID = roleMessage.Channel.Id
            });
        }
    }
}
