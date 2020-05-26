using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace MEE7.Commands.MessageDB
{
    public class DBGuild
    {
        public ulong Id;
        public string Name;
        public List<DBTextChannel> TextChannels;
        public DateTime CreatedAt;
        public string IconUrl;
        public string SplashUrl;
        public ulong OwnerId;
    }

    public class DBGuildUser
    {
        public ulong Id;
        public string Name;
        public DateTime CreatedAt;
        public DateTime JoinedAt;
        public bool IsBot;
        public string Nickname;
    }

    public class DBGuildEmote
    {
        public ulong Id;
        public string Name;
        public DateTime CreatedAt;
        public string Url;
        public ulong CreatorId;
    }

    public class DBCategoryChannel
    {
        public ulong Id;
        public string Name;
        public List<DBTextChannel> TextChannels;
    }

    public class DBTextChannel
    {
        public ulong Id;
        public string Name;
        public List<DBMessage> Messages;
        public DateTime CreatedAt;
        public ulong GuildId;
    }

    public class DBMessage
    {
        public ulong Id;
        public ulong AuthorId;
        public string AuthorName;
        public DateTime Timestamp;
        public string[] Attachements;
        public string Content;
        public string Link;
        public List<DBEmbed> Embeds;
        public List<DBReaction> Reactions;
        public List<ulong> MentionedChannels;
        public List<ulong> MentionedUsers;
        public List<ulong> MentionedRoles;
        public ulong Channel;
        public bool IsPinned;
    }

    public class DBReaction
    {
        public ulong id;
        public string name;
        public string print;
        public int count;
    }

    public class DBEmbed
    {
        public string AuthorURL;
        public Color? Color;
        public string Desc;
        public List<DBEmbedField> Fields;
        public string Footer;
        public string Title;
        public string ImageUrl;
        public string ThumbnailUrl;
        public DateTime Timestamp;
    }

    public class DBEmbedField
    {
        public string Title;
        public string Content;
    }
}
