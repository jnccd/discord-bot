using Discord;
using TweetSharp;

namespace MEE7.Backend
{
    class TwitterDiscordAttachment : IAttachment
    {
        public TwitterMedia m;

        public TwitterDiscordAttachment(TwitterMedia m)
        {
            this.m = m;
        }

        public ulong Id => 0;

        public string Filename => m.MediaUrl;

        public string Url => m.MediaUrl;

        public string ProxyUrl => m.MediaUrl;

        public int Size => 100;

        public int? Height => 10;

        public int? Width => 10;
    }
}
