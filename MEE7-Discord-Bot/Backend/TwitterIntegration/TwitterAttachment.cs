using Discord;
using System;
using System.Collections.Generic;
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
            this.size = width * height;
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

        public bool Ephemeral => false;

        public string Description => "";

        public string ContentType => "";

        public double? Duration => throw new NotImplementedException();

        public string Waveform => throw new NotImplementedException();

        public byte[] WaveformBytes => throw new NotImplementedException();

        public AttachmentFlags Flags => throw new NotImplementedException();

        public IReadOnlyCollection<IUser> ClipParticipants => throw new NotImplementedException();

        public string Title => throw new NotImplementedException();

        public DateTimeOffset? ClipCreatedAt => throw new NotImplementedException();

        public DateTimeOffset CreatedAt => throw new NotImplementedException();
    }
}
