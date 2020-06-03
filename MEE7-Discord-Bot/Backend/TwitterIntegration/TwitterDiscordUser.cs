using Discord;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using TweetSharp;

namespace MEE7.Backend
{
    public class TwitterDiscordUser : IUser
    {
        public readonly TwitterUser user;

        public TwitterDiscordUser(TwitterUser User)
        {
            this.user = User;
        }

        public string AvatarId => "";

        public string Discriminator => "";

        public ushort DiscriminatorValue => 0;

        public bool IsBot => false;

        public bool IsWebhook => false;

        public string Username => user.Name;

        public DateTimeOffset CreatedAt => user.CreatedDate;

        public ulong Id => 0;

        public string Mention => user.ScreenName;

        public IActivity Activity => null;

        public UserStatus Status => UserStatus.Online;

        public IImmutableSet<ClientType> ActiveClients => ImmutableSortedSet.Create<ClientType>();

        public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
        {
            return user.ProfileImageUrl.Replace("_normal", "_400x400");
        }

        public string GetDefaultAvatarUrl()
        {
            return user.ProfileImageUrl.Replace("_normal", "_400x400");
        }

        public Task<IDMChannel> GetOrCreateDMChannelAsync(RequestOptions options = null)
        {
            return Task.FromResult<IDMChannel>(null);
        }
    }
}
