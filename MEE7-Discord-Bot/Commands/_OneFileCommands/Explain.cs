using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;

namespace MEE7.Commands._OneFileCommands
{
    class Explain : Command
    {
        public override void Execute(SocketMessage message)
        {
            string[] split = message.Content.Split(new char[] { ' ', '\n' });
            if (split.Length <= 1)
            {
                Program.SendText("I need something to search!", message.Channel).Wait();
                return;
            }

            string search = WebUtility.UrlEncode(split.Skip(1).Combine(" "));

            //string google = ("https://www.google.de/search?source=hp&ei=vJwXXbu-OcfMaJqEiugE&q=" + WebUtility.UrlEncode(search)).GetHTMLfromURL();
            string urban = ("https://www.urbandictionary.com/define.php?term=" + search).GetHTMLfromURL();
            string wiki = ("https://de.wikipedia.org/wiki/" + search).GetHTMLfromURL();

            string wikiParse = "";
            try {
                string wikiArticle = wiki.GetEverythingBetween("<div class=\"mw-parser-output\">", "<tbody><tr>");
                wikiParse = WebUtility.HtmlDecode(Regex.Replace(wikiArticle, "<[^>]*>", ""));
            } catch { }

            string urbanParse = "";
            try {
                string urbanTopCard = urban.GetEverythingBetween("<span class=\"category right hide unknown\">", "<div class=\"def-footer\">");
                urbanParse = WebUtility.HtmlDecode(
                    Regex.Replace(urbanTopCard.
                        Replace("<br/>", "\n").
                        Replace("<div class=\"example\">", "\n\n").
                        Replace("<div class=\"tags\">", "\n\n"), "<[^>]*>", "").
                    Remove(0, "unknown".Length));
            } catch { }

            EmbedBuilder b = new EmbedBuilder();
            if (!string.IsNullOrWhiteSpace(wikiParse))
                b.AddFieldDirectly("Wikipedia:", wikiParse);
            if (!string.IsNullOrWhiteSpace(urbanParse))
                b.AddFieldDirectly("Urban Dictionary:", urbanParse);
            Program.SendEmbed(b, message.Channel).Wait();
        }
    }
}
