using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.Configuration;
using MEE7.Backend.HelperFunctions;
using MEE7.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MEE7.Commands
{
    public class ManageRoleByEmoteMessage // uwu
    {
        public ulong MessageID;
        public ulong ChannelID;

        public List<Tuple<
            DiscordEmote, // Emote by Name,EmoteID,GuildID
            ulong> // RoleID
            > EmoteRoleTuples = null;
    }

    public class ManageRolesByEmote : Command
    {
        public ManageRolesByEmote() : base("sendRoleEmojiMessage", "Let users of your server get roles from emote reactions", isExperimental: false, isHidden: false)
        {
            Program.OnEmojiReactionAdded += OnEmojiReactionAdded;
            Program.OnEmojiReactionRemoved += OnEmojiReactionRemoved;
            Program.OnMessageDeleted += Program_OnMessageDeleted;
        }

        private void Program_OnMessageDeleted(Cacheable<IMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2)
        {
            for (int i = 0; i < Config.Data.manageRoleByEmoteMessages.Count; i++)
                if (Config.Data.manageRoleByEmoteMessages[i].MessageID == arg1.Id &&
                    Config.Data.manageRoleByEmoteMessages[i].ChannelID == arg2.Id)
                    Config.Data.manageRoleByEmoteMessages.RemoveAt(i--);
        }

        private void OnEmojiReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
        {
            foreach (var m in Config.Data.manageRoleByEmoteMessages)
                if (arg1.Id == m.MessageID)
                    foreach (var t in m.EmoteRoleTuples)
                        if (arg3.Emote.Name == t.Item1.Name)
                        {
                            var u = arg3.User.GetValueOrDefault();
                            var g = Program.GetGuildFromChannel(arg2.Value);
                            g.Users.First(x => x.Id == u.Id).AddRoleAsync(g.Roles.First(x => x.Id == t.Item2));
                        }
        }

        private void OnEmojiReactionRemoved(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
        {
            foreach (var m in Config.Data.manageRoleByEmoteMessages)
                if (arg1.Id == m.MessageID)
                    foreach (var t in m.EmoteRoleTuples)
                        if (arg3.Emote.Name == t.Item1.Name)
                        {
                            var u = arg3.User.GetValueOrDefault();
                            var g = Program.GetGuildFromChannel(arg2.Value);
                            g.Users.First(x => x.Id == u.Id).RemoveRoleAsync(g.Roles.First(x => x.Id == t.Item2));
                        }
        }

        public override void Execute(IMessage message)
        {
            var g = Program.GetGuildFromChannel(message.Channel);
            if (message.Author.Id != g.Owner.Id && message.Author.Id != Program.Master.Id 
                && (g.Roles.Where(x => x.Members.FirstOrDefault(x => x.Id == message.Author.Id) != null).Select(x => x.Position).Max() < 
                   g.Roles.Where(x => x.Members.FirstOrDefault(x => x.Id == Program.GetSelf().Id) != null).Select(x => x.Position).Max() ||
                   !g.Roles.Where(x => x.Members.FirstOrDefault(x => x.Id == message.Author.Id) != null).Any(x => x.Permissions.ManageRoles) ))
            {
                DiscordNETWrapper.SendText("You don't have permissions to use this command!", message.Channel).Wait();
                return;
            }

            var roles = Program.GetGuildFromChannel(message.Channel).Roles;

            var args = message.Content.Remove(0, PrefixAndCommand.Length + 1);
            var customMes = "";
            if (args.Contains("\""))
            {
                customMes = args.GetEverythingBetween("\"", "\"");
                args = args.Remove(0, args.IndexOf(customMes + "\"") + customMes.Length + 1);
            }

            List<Tuple<DiscordEmote, ulong>> emoteRoleTuples;
            try
            {
                emoteRoleTuples = args.
                    Split('|').
                    Select(x => x.Trim(' ')).
                    Select(x => x.Split(' ')).
                    Select(s =>
                    {
                        IEmote e = null;
                        SocketRole r = null;

                        s = s.Where(x => x.Length > 0).ToArray();

                        if (s.Length != 2)
                            throw new Exception(s.Combine(" "));

                        try
                        {
                            if (Emote.TryParse(s[0], out Emote et))
                                e = et;
                            else
                                e = new Emoji(s[0]);

                            r = roles.First(x => x.Name == s[1] || x.Id.ToString() == s[1]);
                        }
                        catch
                        {
                            DiscordNETWrapper.SendText("Can't find role and/or emote " + s.Combine(" "), message.Channel).Wait();
                            return null;
                        }

                        return new Tuple<DiscordEmote, ulong>(DiscordEmote.FromIEmote(e), r.Id);
                    }).ToList();
            }
            catch
            {
                DiscordNETWrapper.SendText("I couldnt read what you wanted to tell me, try using proper syntax, such as\n" +
                    "```[emote1] [role name/id]|[emote2] {[role name/id]...```\n" +
                    "Roles with a space in their name need to be imported using thier role id. You can get that id using the dev mode of discord\n" +
                    "\n" +
                    "Example:\n" +
                   $"{Program.Prefix}sendRoleEmojiMessage <:fingergun_L:619206752516309005> test1 | <a:padoru:744966713778372778> test2", message.Channel).Wait();
                return;
            }

            if (emoteRoleTuples.Contains(null))
                return;

            IUserMessage roleMessage;
            if (string.IsNullOrWhiteSpace(customMes))
                roleMessage = DiscordNETWrapper.SendText("To get a role simply react to this message with the corresponding emote,\n" +
                    "to remove a role simply remove your reaction to this message.\n\nEmotes are mapped as follows:\n" +
                    emoteRoleTuples.Select(x => $"{x.Item1.Print()} - {g.Roles.First(y => y.Id == x.Item2).Name}").
                    Aggregate((x, y) => $"{x}\n{y}"), message.Channel).Result.Last();
            else
                roleMessage = DiscordNETWrapper.SendText(customMes, message.Channel).Result.Last();

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
