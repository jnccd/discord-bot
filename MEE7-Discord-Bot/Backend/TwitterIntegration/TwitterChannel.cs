using Discord;
using MEE7.Backend.HelperFunctions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TweetSharp;

namespace MEE7.Backend
{
    class TwitterChannel : IMessageChannel
    {
        readonly TwitterService service;
        readonly List<TwitterStatus> thread = new List<TwitterStatus>();
        readonly List<IMessage> messages = new List<IMessage>();

        public TwitterStatus InitialStatus;
        public IMessage InitialMessage = null;

        public TwitterChannel(TwitterStatus status, TwitterService service)
        {
            this.service = service;

            thread.Add(status);
            while (thread.Last().InReplyToStatusId.HasValue)
                thread.Add(service.GetTweet(new GetTweetOptions() { Id = thread.Last().InReplyToStatusId.GetValueOrDefault() }));

            foreach (var v in thread)
                messages.Add(new TwitterMessage(v, this) as IMessage);

            InitialMessage = messages.First();
            InitialStatus = thread.First();
        }

        string IChannel.Name => "Twitter";
        DateTimeOffset ISnowflakeEntity.CreatedAt => DateTimeOffset.Now;
        ulong IEntity<ulong>.Id => 0;

        public Task<IUserMessage> SendFileAsync(string filePath, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false)
        {
            var res = TweetSharpWrapper.SendReplyImage(service, thread.First(), 
                string.IsNullOrWhiteSpace(text) ? "here's ya image 😊" : text, filePath);
            if (res != null && res.Item1 != null && res.Item2.StatusCode == HttpStatusCode.OK)
                thread.Insert(0, res.Item1);
            else
                this.SendMessageAsync("That didn't work :c\n{res.Item2.Error}").Wait();
            return Task.FromResult(default(IUserMessage));
        }

        public Task<IUserMessage> SendFileAsync(Stream stream, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, bool isSpoiler = false)
        {
            var res = TweetSharpWrapper.SendReplyImage(service, thread.First(), 
                string.IsNullOrWhiteSpace(text) ? "here's ya image 😊" : text, stream, filename);
            if (res != null && res.Item1 != null && res.Item2.StatusCode == HttpStatusCode.OK)
                thread.Insert(0, res.Item1);
            else
                this.SendMessageAsync($"That didn't work :c\n{res.Item2.Error}").Wait();
            return Task.FromResult(default(IUserMessage));
        }

        public Task<IUserMessage> SendMessageAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            if (embed != null)
                text += $"\n{embed.Title} \n{embed.Description} \n{embed.Fields.Select(x => $"{x.Name} {x.Value}").Combine("\n")}";
            if (string.IsNullOrWhiteSpace(text))
                return Task.FromResult(default(IUserMessage));
            if (text.Length >= 280)
                text = text.Substring(0, 279);
            text = new string((from c in text
                               where c == ' ' || c == '\n' || (c >= '!' && c <= 'z')
                               select c).ToArray());
            var res = TweetSharpWrapper.SendReply(service, thread.First(), text);
            if (res != null && res.Item1 != null && res.Item2.StatusCode == HttpStatusCode.OK)
                thread.Insert(0, res.Item1);
            else
                this.SendMessageAsync($"That didn't work :c\nTwitter said: {res.Item2.Error}").Wait();
            return Task.FromResult(default(IUserMessage));
        }

        public Task DeleteMessageAsync(ulong messageId, RequestOptions options = null)
        {
            return Task.FromResult(default(object));
        }
        public Task DeleteMessageAsync(IMessage message, RequestOptions options = null)
        {
            return Task.FromResult(default(object));
        }
        public IDisposable EnterTypingState(RequestOptions options = null)
        {
            return Task.FromResult(default(object));
        }
        public Task<IMessage> GetMessageAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            return Task.FromResult(default(IMessage));
        }
        public async IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            await Task.Delay(1);
            foreach (var s in thread.Take(limit))
            {
                yield return new ReadOnlyCollection<TwitterMessage>((new TwitterMessage[] { new TwitterMessage(s, this) }).ToList());
            }
        }
        public async IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(ulong fromMessageId, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            await Task.Delay(1);
            yield return new ReadOnlyCollection<TwitterMessage>((new TwitterMessage[] { }).ToList());
        }
        public async IAsyncEnumerable<IReadOnlyCollection<IMessage>> GetMessagesAsync(IMessage fromMessage, Direction dir, int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            await Task.Delay(1);
            yield return new ReadOnlyCollection<TwitterMessage>((new TwitterMessage[] { }).ToList());
        }
        public Task<IReadOnlyCollection<IMessage>> GetPinnedMessagesAsync(RequestOptions options = null)
        {
            return Task.FromResult(new ReadOnlyCollection<IMessage>(new List<IMessage>()) as IReadOnlyCollection<IMessage>);
        }
        public Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            return Task.FromResult(default(IUser));
        }
        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }
        public Task TriggerTypingAsync(RequestOptions options = null)
        {
            return Task.FromResult(default(object));
        }
    }
}
