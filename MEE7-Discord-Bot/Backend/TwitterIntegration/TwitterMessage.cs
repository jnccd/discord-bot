using Discord;
using System.Collections.Generic;
using System.Compat.Web;
using System.Linq;
using System.Text.Encodings.Web;
using TweetSharp;

namespace MEE7.Backend
{
    class TwitterMessage : SelfmadeMessage
    {
        public TwitterStatus s;

        public TwitterMessage(TwitterStatus s, TwitterChannel channel)
        {
            this.Content = s.Text;
            Content = HttpUtility.HtmlDecode(Content);
            if (Content.StartsWith("@MEE7_Bot "))
                Content = Content.Remove(0, "@MEE7_Bot ".Length);
            Content = Content.Replace("\\\"", "\"");
            Content = Content.Trim(' ');

            foreach (var v in s.Entities.Urls)
                Content = Content.Replace(v.Value, v.ExpandedValue);

            this.Author = new TwitterDiscordUser(s.Author as TwitterUser);
            this.Channel = channel;
            this.CreatedAt = s.CreatedDate;
            this.EditedTimestamp = s.CreatedDate;
            this.Embeds = new List<Discord.IEmbed>();
            this.Id = 0;
            this.IsPinned = false;
            this.IsTTS = false;
            this.MentionedChannelIds = new List<ulong>();
            this.MentionedRoleIds = new List<ulong>();
            this.MentionedUserIds = new List<ulong>();
            this.Source = Discord.MessageSource.User;
            this.Tags = new List<Discord.ITag>();
            this.Timestamp = s.CreatedDate;
            this.Type = Discord.MessageType.Default;
            this.Attachments = s.Entities.Media.Select(x => new TwitterDiscordAttachment(x) as IAttachment).ToList();
            this.s = s;
        }
    }
}
