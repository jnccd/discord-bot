using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using MEE7.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            while (true)
            {
                try
                {
                    var codeShitposts = Program.twitterService.GetUserProfileFor(new GetUserProfileForOptions() { UserId = 1013576989020774400 });
                    var text = codeShitposts.Status.GetContent();
                    var image = codeShitposts.Status.Entities.Media.First().MediaUrl;
                    var id = codeShitposts.Status.Id;
                    if (Config.Data.lastCodeMemeId != 0 && id != Config.Data.lastCodeMemeId)
                        arena.SendFileAsync(image.GetStreamFromUrl(), image.Split("/").Last(), text);
                    if (Config.Data.lastCodeMemeId == 0 || (Config.Data.lastCodeMemeId != 0 && id != Config.Data.lastCodeMemeId))
                        Config.Data.lastCodeMemeId = id;
                }
                catch (Exception e)
                {

                }

                Thread.Sleep(1 * 60 * 60 * 1000);
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
