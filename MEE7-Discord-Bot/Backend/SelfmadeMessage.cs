using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MEE7.Backend
{
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

        public Func<RequestOptions, Task> DeleteFunc = (RequestOptions options) => Task.FromResult(default(object));

        public SelfmadeMessage()
        {

        }
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

        public bool IsSuppressed => throw new NotImplementedException();
        public MessageReference Reference => throw new NotImplementedException();
        public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions => throw new NotImplementedException();

        public IThreadChannel Thread => throw new NotImplementedException();

        public MessageRoleSubscriptionData RoleSubscriptionData => throw new NotImplementedException();

        public PurchaseNotification PurchaseNotification => throw new NotImplementedException();

        public MessageCallData? CallData => throw new NotImplementedException();

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

        bool IMessage.MentionedEveryone => throw new NotImplementedException();

        string IMessage.CleanContent => throw new NotImplementedException();

        IReadOnlyCollection<IMessageComponent> IMessage.Components => throw new NotImplementedException();

        IReadOnlyCollection<IStickerItem> IMessage.Stickers => throw new NotImplementedException();

        MessageFlags? IMessage.Flags => throw new NotImplementedException();

        IMessageInteraction IMessage.Interaction => throw new NotImplementedException();

        public Task AddReactionAsync(IEmote emote, RequestOptions options = null) => Task.FromResult("");
        public Task DeleteAsync(RequestOptions options = null) => DeleteFunc(options);
        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emoji, int limit, RequestOptions options = null) => (IAsyncEnumerable<IReadOnlyCollection<IUser>>)null;

        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emoji, int limit, RequestOptions options = null, ReactionType type = ReactionType.Normal)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAllReactionsAsync(RequestOptions options = null) => throw new NotImplementedException();
        public Task RemoveReactionAsync(IEmote emote, IUser user, RequestOptions options = null) => Task.FromResult("");
        public Task RemoveReactionAsync(IEmote emote, ulong userId, RequestOptions options = null) => Task.FromResult("");

        Task IMessage.RemoveAllReactionsForEmoteAsync(IEmote emote, RequestOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
