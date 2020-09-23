using Discord;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using MEE7.Configuration;
using System.Linq;
using System.Threading;
using TweetSharp;

namespace MEE7.Commands.CAUServerSpecific
{
    class Shitpost : Command
    {
        IMessageChannel arena;

        public Shitpost() : base("shitpost", "", false, true)
        {
            Program.OnConnected += Program_OnConnected;
        }

        private void Program_OnConnected()
        {
            arena = (IMessageChannel)Program.GetChannelFromID(552976757217820693);

            while (Program.IsInReleaseMode())
            {
                try
                {
                    var newTweets = Program.twitterService.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions() 
                        { UserId = 1013576989020774400, Count = 10, SinceId = Config.Data.lastCodeMemeId });
                    if (newTweets != null && newTweets.Count() > 0)
                    {
                        var codeShitpost = newTweets.MaxElement(x => x.FavoriteCount + x.RetweetCount * 2);
                        if (codeShitpost.FavoriteCount + codeShitpost.RetweetCount * 2 < 100)
                            return;

                        var text = codeShitpost.GetContent();
                        var image = codeShitpost.Entities.Media.First().MediaUrl;
                        var id = codeShitpost.Id;
                        var source = text.GetEverythingBetweenAll("(", ")").Last();
                        var link = source.Replace("source: ", "").Trim(' ');
                        var title = text.Replace($"({source})", "").Trim(' ');
                        if (Config.Data.lastCodeMemeId != 0 && id != Config.Data.lastCodeMemeId)
                            arena.SendFileAsync(image.GetStreamFromUrl(), image.Split("/").Last(), $"<{link}> {title}");
                        if (Config.Data.lastCodeMemeId == 0 || (Config.Data.lastCodeMemeId != 0 && id != Config.Data.lastCodeMemeId))
                            Config.Data.lastCodeMemeId = id;
                    }
                }
                catch { }

                Thread.Sleep(30 * 60 * 1000);
            }
        }

        public override void Execute(IMessage message)
        {
            string Content = message.Content.Split(" ").Skip(1).Combine(" ");

            try
            {
                if (message.Attachments.Count == 0)
                    arena.SendMessageAsync(Content);
                else
                    arena.SendFileAsync(message.Attachments.First().Url.GetStreamFromUrl(), message.Attachments.First().Filename, Content);
            }
            catch { }
        }
    }
}
