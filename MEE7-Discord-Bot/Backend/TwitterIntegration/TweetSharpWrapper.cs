using MEE7.Backend.HelperFunctions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using TweetSharp;

namespace MEE7.Backend
{
    public class TweetSharpWrapper
    {
        public static Tuple<TwitterStatus, TwitterResponse> SendReply(TwitterService service, TwitterStatus m, string text)
        {
            TwitterStatus sta = null; TwitterResponse res = null; bool finished = false;

            service.SendTweet(new SendTweetOptions() { Status = text, AutoPopulateReplyMetadata = true, InReplyToStatusId = m.Id },
                (TwitterStatus s, TwitterResponse r) => { sta = s; res = r; finished = true; });

            while (!finished)
                Thread.Sleep(500);

            return new Tuple<TwitterStatus, TwitterResponse>(sta, res);
        }

        public static Tuple<TwitterStatus, TwitterResponse> SendReplyImage(TwitterService service, TwitterStatus m, string text, string ImagePath)
        {
            TwitterStatus sta = null; TwitterResponse res = null; bool finished = false;

            using var stream = new FileStream(ImagePath, FileMode.Open);
            var Media = service.UploadMedia(new UploadMediaOptions() { Media = new MediaFile() { FileName = ImagePath, Content = stream } });

            List<string> MediaIds = new List<string> { Media.Media_Id };

            var Result = service.SendTweet(new SendTweetOptions() { Status = text, AutoPopulateReplyMetadata = true, InReplyToStatusId = m.Id, MediaIds = MediaIds },
                (TwitterStatus s, TwitterResponse r) => { sta = s; res = r; finished = true; });

            while (!finished)
                Thread.Sleep(500);

            return new Tuple<TwitterStatus, TwitterResponse>(sta, res);
        }

        public static Tuple<TwitterStatus, TwitterResponse> SendReplyImage(TwitterService service, TwitterStatus m, string text, Stream imageStream, string fileName)
        {
            TwitterStatus sta = null; TwitterResponse res = null; bool finished = false;

            MemoryStream mem = new MemoryStream();
            try { imageStream.Position = 0; } catch { }
            imageStream.CopyTo(mem);
            mem.Position = 0;

            var Media = service.UploadMedia(new UploadMediaOptions() { Media = new MediaFile() { FileName = fileName, Content = mem } });

            List<string> MediaIds = new List<string> { Media.Media_Id };

            var Result = service.SendTweet(new SendTweetOptions() { Status = text, AutoPopulateReplyMetadata = true, InReplyToStatusId = m.Id, MediaIds = MediaIds },
                (TwitterStatus s, TwitterResponse r) => {
                    sta = s; res = r; finished = true;
                });

            while (!finished)
                Thread.Sleep(500);

            return new Tuple<TwitterStatus, TwitterResponse>(sta, res);
        }

        public static void PrintTwitterRateLimitStatus(TwitterService service)
        {
            TwitterRateLimitStatus rate = service.Response.RateLimitStatus;
            ConsoleWrapper.WriteLine($"{DateTime.Now.ToLongTimeString()} Twitter rate limit status: {rate.RemainingHits}/{rate.HourlyLimit}");
        }
    }
}
