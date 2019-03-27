using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDiscordBot.Backend
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

        public SelfmadeMessage(IMessage m)
        {
            Content = m.Content;
            Timestamp = m.Timestamp;
            EditedTimestamp = m.EditedTimestamp;
            Channel = m.Channel;
            Author = m.Author;
            CreatedAt = m.CreatedAt;
            Id = m.Id;
        }
        
        string IMessage.Content => Content;
        DateTimeOffset IMessage.Timestamp => Timestamp;
        DateTimeOffset? IMessage.EditedTimestamp => EditedTimestamp;
        IMessageChannel IMessage.Channel => Channel;
        IUser IMessage.Author => Author;
        DateTimeOffset ISnowflakeEntity.CreatedAt => CreatedAt;
        ulong IEntity<ulong>.Id => Id;

        public MessageType Type => throw new NotImplementedException();
        public MessageSource Source => throw new NotImplementedException();
        public bool IsTTS => throw new NotImplementedException();
        public bool IsPinned => throw new NotImplementedException();
        
        public IReadOnlyCollection<IAttachment> Attachments => throw new NotImplementedException();
        public IReadOnlyCollection<IEmbed> Embeds => throw new NotImplementedException();
        public IReadOnlyCollection<ITag> Tags => throw new NotImplementedException();
        public IReadOnlyCollection<ulong> MentionedChannelIds => throw new NotImplementedException();
        public IReadOnlyCollection<ulong> MentionedRoleIds => throw new NotImplementedException();
        public IReadOnlyCollection<ulong> MentionedUserIds => throw new NotImplementedException();
        
        public Task DeleteAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }
    }
}
