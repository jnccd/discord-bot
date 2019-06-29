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

            string google = ("https://www.google.de/search?source=hp&ei=vJwXXbu-OcfMaJqEiugE&q=" + WebUtility.UrlEncode(split.Skip(1).Combine())).GetHTMLfromURL();
            string urban = ("https://www.urbandictionary.com/define.php?term=" + WebUtility.UrlEncode(split.Skip(1).Combine())).GetHTMLfromURL();
            string wiki = ("https://de.wikipedia.org/wiki/" + WebUtility.UrlEncode(split.Skip(1).Combine())).GetHTMLfromURL();

            string wikiParse = wiki.GetEverythingBetween("<div class=\"mw-parser-output\">", "<tbody><tr>");
            string urbanTopCard = urban.GetEverythingBetween("<span class=\"category right hide unknown\">", "<div class=\"def-footer\">");

            string regexParse = Regex.Replace(wikiParse, "<[^>]*>", "");
            string regexParseUrb = Regex.Replace(urbanTopCard.Replace("<br/>", "\n"), "<[^>]*>", "").Remove(0, "unknown".Length);

            EmbedBuilder b = new EmbedBuilder();
            try { b.AddField("Wikipedia:", regexParse);           } catch { }
            try { b.AddField("Urban Dictionary:", regexParseUrb); } catch { }
            Program.SendEmbed(b, message.Channel).Wait();
        }
    }
}
