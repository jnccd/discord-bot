using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot
{
    public static partial class Extensions
    {
        public static SelfmadeMessage EditContent(this SelfmadeMessage m, string newContent)
        {
            m.Content = newContent;
            return m;
        }
    }

    public class SelfmadeMessage : IMessage
    {
        public string Content;
        public DateTimeOffset Timestamp;
        public DateTimeOffset? EditedTimestamp;
        public IMessageChannel Channel;
        public IUser Author;
        public DateTimeOffset CreatedAt;
        public ulong Id;
        public MessageType Type;
        public MessageSource Source;
        public bool IsTTS;
        public bool IsPinned;
        public MessageActivity Activity;
        public MessageApplication Application;

        public List<IAttachment> Attachments;
        public List<IEmbed> Embeds;
        public List<ITag> Tags;
        public List<ulong> MentionedChannelIds;
        public List<ulong> MentionedRoleIds;
        public List<ulong> MentionedUserIds;

        public Func<RequestOptions, Task> DeleteFunc = (RequestOptions options) => { throw new NotImplementedException(); };

        public SelfmadeMessage(IMessage m)
        {
            Content = m.Content;
            Timestamp = m.Timestamp;
            EditedTimestamp = m.EditedTimestamp;
            Channel = m.Channel;
            Author = m.Author;
            CreatedAt = m.CreatedAt;
            Id = m.Id;
            Type = m.Type;
            Source = m.Source;
            IsTTS = m.IsTTS;
            IsPinned = m.IsPinned;

            Attachments = m.Attachments.ToList();
            Embeds = m.Embeds.ToList();
            Tags = m.Tags.ToList();
            MentionedChannelIds = m.MentionedChannelIds.ToList();
            MentionedRoleIds = m.MentionedRoleIds.ToList();
            MentionedUserIds = m.MentionedUserIds.ToList();

            DeleteFunc = m.DeleteAsync;
        }
        
        string IMessage.Content => Content;
        DateTimeOffset IMessage.Timestamp => Timestamp;
        DateTimeOffset? IMessage.EditedTimestamp => EditedTimestamp;
        IMessageChannel IMessage.Channel => Channel;
        IUser IMessage.Author => Author;
        DateTimeOffset ISnowflakeEntity.CreatedAt => CreatedAt;
        ulong IEntity<ulong>.Id => Id;
        MessageType IMessage.Type => Type;
        MessageSource IMessage.Source => Source;
        bool IMessage.IsTTS => IsTTS;
        bool IMessage.IsPinned => IsPinned;
        MessageActivity IMessage.Activity => Activity;
        MessageApplication IMessage.Application => Application;

        IReadOnlyCollection<IAttachment> IMessage.Attachments => Attachments;
        IReadOnlyCollection<IEmbed> IMessage.Embeds => Embeds;
        IReadOnlyCollection<ITag> IMessage.Tags => Tags;
        IReadOnlyCollection<ulong> IMessage.MentionedChannelIds => MentionedChannelIds;
        IReadOnlyCollection<ulong> IMessage.MentionedRoleIds => MentionedRoleIds;
        IReadOnlyCollection<ulong> IMessage.MentionedUserIds => MentionedUserIds;
        
        public Task DeleteAsync(RequestOptions options = null)
        {
            return DeleteFunc(options);
        }
    }
}
