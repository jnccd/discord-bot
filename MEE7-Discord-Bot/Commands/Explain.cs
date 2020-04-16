using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace MEE7.Commands
{
    class Explain : Command
    {
        public override void Execute(SocketMessage message)
        {
            string[] split = message.Content.Split(new char[] { ' ', '\n' });
            if (split.Length <= 1)
            {
                DiscordNETWrapper.SendText("I need something to search!", message.Channel).Wait();
                return;
            }

            string search = WebUtility.UrlEncode(split.Skip(1).Combine(" "));
            string wikiSearch = split.Skip(1).Combine("_");

            string urban = ($"https://www.urbandictionary.com/define.php?term={search}").GetHTMLfromURL();
            string wiki = ($"https://en.wikipedia.org/wiki/{wikiSearch}").GetHTMLfromURL();

            string wikiParse = "";
            try
            {
                string wikiArticle = wiki.GetEverythingBetween("<div class=\"mw-parser-output\">", "<tbody><tr>");
                wikiParse = WebUtility.HtmlDecode(Regex.Replace(wikiArticle, "<[^>]*>", ""));
            }
            catch { }
            if (string.IsNullOrWhiteSpace(wikiParse))
            {
                try
                {
                    string wikiArticle = wiki.GetEverythingBetween("</table>", "<div id=\"toc\" class=\"toc\">");
                    wikiParse = WebUtility.HtmlDecode(Regex.Replace(wikiArticle, "<[^>]*>", ""));
                }
                catch { }
            }

            string urbanParse = "";
            try
            {
                string urbanTopCard = urban.GetEverythingBetween("<span class=\"category right hide unknown\">", "<div class=\"def-footer\">");
                urbanParse = WebUtility.HtmlDecode(
                    Regex.Replace(urbanTopCard.
                        Replace("<br/>", "\n").
                        Replace("<div class=\"example\">", "\n\n").
                        Replace("<div class=\"tags\">", "\n\n"), "<[^>]*>", "").
                    Remove(0, "unknown".Length));
            }
            catch { }

            EmbedBuilder b = new EmbedBuilder();
            if (!string.IsNullOrWhiteSpace(wikiParse))
                b.AddFieldDirectly("Wikipedia:", wikiParse);
            if (!string.IsNullOrWhiteSpace(urbanParse))
                b.AddFieldDirectly("Urban Dictionary:", urbanParse);
            DiscordNETWrapper.SendEmbed(b, message.Channel).Wait();
        }
    }
}
