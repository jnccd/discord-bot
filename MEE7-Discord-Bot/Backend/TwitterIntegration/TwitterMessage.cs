using Discord;
using MEE7.Backend.HelperFunctions;
using System.Collections.Generic;
using System.Compat.Web;
using System.Linq;
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
            Content = Content.Split(' ').SkipWhile(x => x.StartsWith("@")).Combine(" ");
            Content = Content.Replace("\\\"", "\"").Replace("\n", " ");
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

            this.Attachments = new List<IAttachment>();
            this.Attachments.AddRange(s.Entities.Media.Select(x => new TwitterDiscordAttachment(x.MediaUrl, x.Sizes.Large.Width, x.Sizes.Large.Height, x) as IAttachment));
            if (s.ExtendedEntities != null)
                this.Attachments.AddRange(s.ExtendedEntities.Media.
                    Where(x => x.VideoInfo != null).
                    Select(x => new TwitterDiscordAttachment(
                        x.VideoInfo.Variants.MaxElement(x => x.BitRate).Url.AbsoluteUri, x.Sizes.Large.Width, x.Sizes.Large.Height, x)).ToList());
            this.s = s;
        }
    }
}
