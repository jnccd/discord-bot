using Discord.WebSocket;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using MEE7.Backend.HelperFunctions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace MEE7
{
    class AnimalCrossing : Command
    {
        public AnimalCrossing() : base("animalCrossing", "Finds a npc", false, false)
        {

        }

        public override void Execute(SocketMessage message)
        {
            var split = message.Content.Split(' ');
            if (split.Length <= 1)
                return;
            else if (!split[1].StartsWith("-"))
                germanWikiQuery(message.Content.Split(' ').Skip(1).Combine(" "), message);
            else
            {
                if (split[1] == "-de")
                    germanWikiQuery(message.Content.Split(' ').Skip(2).Combine(" "), message);
                else if (split[1] == "-en")
                    englishWikiQuery(message.Content.Split(' ').Skip(2).Combine(" "), message);
            }
        }

        void germanWikiQuery(string searchQuery, SocketMessage message)
        {
            var searchPage = $"https://animalcrossingwiki.de/nachbarn/jens?do=search&sf=1&q={HttpUtility.UrlEncode(searchQuery)}%20%40nachbarn".GetHTMLfromURL();
            if (searchPage.Contains("<div class=\"nothing\">Nichts gefunden.</div>"))
            {
                DiscordNETWrapper.SendText("Wasn das bidde?!?", message.Channel).Wait();
                return;
            }

            string charPagelink = searchPage.GetEverythingBetween("search_quickresult", "======");
            string charPagelink2 = "https://animalcrossingwiki.de" + charPagelink.GetEverythingBetween("<a href=\"", "\" class");
            string charPageHTML = charPagelink2.GetHTMLfromURL();

            string table = charPageHTML.GetEverythingBetween("wrap_nachbarntabelle", "</table>");
            var tableEntries = table.GetEverythingBetweenAll("<tr class=\"row", "</tr>");
            string charName = tableEntries[0].GetEverythingBetween(">  ", "  <");
            string imgSrc = "https://animalcrossingwiki.de" + tableEntries[1].GetEverythingBetween("<img src=\"", "?");

            DiscordNETWrapper.SendEmbed(DiscordNETWrapper.
                CreateEmbedBuilder(charName, "", imgSrc, null, imgSrc).
                AddFieldDirectly("Tierart", tableEntries[2].GetEverythingBetweenAll("\">", "<")[3]).
                AddFieldDirectly("Persönlichkeit", tableEntries[3].GetEverythingBetweenAll("\">", "<")[3]).
                AddFieldDirectly("Geschlecht", tableEntries[4].GetEverythingBetween("col1 centeralign\"> ", " </td>")).
                AddFieldDirectly("Geburtstag", tableEntries[5].GetEverythingBetween("centeralign\">  ", " <")).
                AddFieldDirectly("Floskel", tableEntries[6].GetEverythingBetween(">  „", "“  <")).
                AddFieldDirectly("Fotospruch", tableEntries[7].GetEverythingBetween(">  „", "“  <")).
                AddFieldDirectly("Auftreten", tableEntries[8].GetEverythingBetweenAll("\">", "<").Skip(3).Combine(", ")),
                message.Channel).Wait();
        }

        void englishWikiQuery(string searchQuery, SocketMessage message)
        {
            var searchPage = ("https://animalcrossing.fandom.com/wiki/Special:Search?query=" + HttpUtility.UrlEncode(searchQuery)).GetHTMLfromURL();
            if (searchPage.Contains("<i>No results found.</i>"))
            {
                DiscordNETWrapper.SendText("Couldn't find that :/", message.Channel).Wait();
                return;
            }

            string charPagelink = searchPage.Substring(searchPage.IndexOf("data-pos=\"1\"") - 100).GetEverythingBetween("<a href=\"", "\" class=\"result-link\"");
            string charPageHTML = charPagelink.GetHTMLfromURL();

            var infoTable = charPageHTML.GetEverythingBetweenAll("<div class=\"pi-data-value pi-font\">", "</div>");
            if (infoTable.Count <= 10)
            {
                DiscordNETWrapper.SendText("Thats not a character page", message.Channel).Wait();
                return;
            }

            string charImgUrl = charPageHTML.GetEverythingBetween("<figure class=\"pi-item", "class=\"image");
            string charImgUrl2 = charImgUrl.GetEverythingBetween("<a href=\"", "");
            string charImgUrl3 = charImgUrl2.Substring(0, charImgUrl2.Length - 2);

            string charName = charPageHTML.GetEverythingBetween("<h1 class=\"page-header__title\">", "</h1>");
            //string charDesc = charPageHTML.

            DiscordNETWrapper.SendEmbed(DiscordNETWrapper.
                CreateEmbedBuilder(charName, "", charImgUrl3, null, charImgUrl3).
                AddFieldDirectly("Gender", infoTable[0]).
                AddFieldDirectly("Personality", infoTable[1].GetEverythingBetween(">", "<")).
                AddFieldDirectly("Species", infoTable[2].GetEverythingBetween(">", "<")).
                AddFieldDirectly("Birthday", infoTable[3].GetEverythingBetween(">", "<")).
                AddFieldDirectly("Initial phrase", infoTable[4].GetEverythingBetween("", "<")).
                AddFieldDirectly("Initial clothes", infoTable[5]).
                AddFieldDirectly("Home request", infoTable[6]).
                AddFieldDirectly("Skill", infoTable[7]).
                AddFieldDirectly("Goal", infoTable[8]).
                AddFieldDirectly("Coffee", infoTable[9].GetEverythingBetween("", ",")).
                AddFieldDirectly("Style", infoTable[10]).
                AddFieldDirectly("Favorite song", infoTable[11].StartsWith("[[") ?
                    infoTable[11].GetEverythingBetween("[[", " <") :
                    infoTable[11].GetEverythingBetween(">", "<")).
                AddFieldDirectly("Appearances", infoTable[12].GetEverythingBetweenAll(">", "</a>").Combine(", ")),
                message.Channel).Wait();
        }
    }
}
