using Discord;
using System.Linq;
using TweetSharp;

namespace MEE7.Backend
{
    class TwitterDiscordAttachment : IAttachment
    {
        public string url;
        public int size, width, height;
        public TwitterEntity m;

        public TwitterDiscordAttachment(string url, int width, int height, TwitterEntity source)
        {
            this.url = url;
            this.size = size;
            this.width = width;
            this.height = height;
            this.m = source;
        }

        public ulong Id => 0;

        public string Filename => url.Split("/").Last();

        public string Url => url;

        public string ProxyUrl => url;

        public int Size => 100;

        public int? Height => 10;

        public int? Width => 10;
    }
}
