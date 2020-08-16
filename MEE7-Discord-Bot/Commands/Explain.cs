using Discord;
using HtmlAgilityPack;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System.Linq;
using System.Net;
using System.Text;

namespace MEE7.Commands
{
    class Explain : Command
    {
        public override void Execute(IMessage message)
        {
            string[] split = message.Content.Split(new char[] { ' ', '\n' });
            if (split.Length <= 1)
            {
                DiscordNETWrapper.SendText("I need something to search!", message.Channel).Wait();
                return;
            }

            string search = WebUtility.UrlEncode(split.Skip(1).Combine(" "));
            string wikiSearch = split.Skip(1).Combine("_");

            string urbanURL = ($"https://www.urbandictionary.com/define.php?term={search}");
            string wikiURL = ($"https://en.wikipedia.org/wiki/{wikiSearch}");

            HtmlWeb web = new HtmlWeb
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8
            };
            try
            {
                var searchDoc = web.Load(urbanURL);
                var words = searchDoc.DocumentNode.SelectNodes("//a[contains(@class, 'word')]");
                var meanings = searchDoc.DocumentNode.SelectNodes("//div[contains(@class, 'meaning')]");
                var examples = searchDoc.DocumentNode.SelectNodes("//div[contains(@class, 'example')]");

                DiscordNETWrapper.SendEmbed(DiscordNETWrapper.
                    CreateEmbedBuilder(words.First().InnerText).
                    AddFieldDirectly("Meaning", WebUtility.HtmlDecode(meanings.First().InnerText)).
                    AddFieldDirectly("Example", WebUtility.HtmlDecode(examples.First().InnerText)), message.Channel).Wait();
            }
            catch
            {
                DiscordNETWrapper.SendText("Error occured while parsing HTML stuff", message.Channel).Wait();
                return;
            }
        }
    }
}
