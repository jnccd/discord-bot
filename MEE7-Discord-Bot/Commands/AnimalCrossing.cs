using Discord.WebSocket;
using HtmlAgilityPack;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System;
using System.Linq;
using System.Text;
using System.Web;

namespace MEE7.Commands
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
            HtmlWeb web = new HtmlWeb();
            web.AutoDetectEncoding = false;
            web.OverrideEncoding = Encoding.UTF8;
            var searchDoc = web.Load($"https://animalcrossingwiki.de/nachbarn/jens?do=search&sf=1&q={HttpUtility.UrlEncode(searchQuery)}+%40nachbarn");
            string charLink;
            try
            {
                var result = searchDoc.DocumentNode.SelectNodes("//a[contains(@class, 'wikilink1')]");
                charLink = result.Skip(3).First().GetAttributeValue("href", "");
                if (charLink == "")
                    throw new Exception();
                if (charLink.Contains("?"))
                    charLink = charLink.GetEverythingBetween("", "?");

                var charDoc = web.Load("https://animalcrossingwiki.de" + charLink);

                var table = charDoc.DocumentNode.SelectSingleNode("//table[contains(@class, 'inline')]");
                var charName = table.SelectSingleNode("//th[contains(@class, 'col0 centeralign')]").InnerText.Trim(' ');
                var tableEntries = table.ChildNodes.Where(x => x.Name == "tr");
                var charImg = "https://animalcrossingwiki.de" + tableEntries.First().
                    ChildNodes.ElementAt(1).
                    ChildNodes.ElementAt(1).
                    GetAttributeValue("src", "");
                if (charImg.Contains("?")) charImg = charImg.GetEverythingBetween("", "?");
                var tierart = tableEntries.ElementAt(1).ChildNodes.ElementAt(2).InnerText.Trim(' ');
                var personality = tableEntries.ElementAt(2).ChildNodes.ElementAt(2).InnerText.Trim(' ');
                var gender = tableEntries.ElementAt(3).ChildNodes.ElementAt(2).InnerText.Trim(' ');
                var birthday = tableEntries.ElementAt(4).ChildNodes.ElementAt(2).InnerText.Trim(' ');
                var floskel = tableEntries.ElementAt(5).ChildNodes.ElementAt(2).InnerText.Trim(' ');
                var fotospruch = tableEntries.ElementAt(6).ChildNodes.ElementAt(2).InnerText.Trim(' ');
                var auftreten = tableEntries.ElementAt(7).ChildNodes.ElementAt(2).InnerText.Trim(' ').Split('\n').Combine(", ");

                var otherNamesTitle = charDoc.GetElementbyId("name-in-anderen-sprachen");
                var namesTable = otherNamesTitle.NextSibling.NextSibling.ChildNodes.ElementAt(1).FirstChild;
                var nameEntries = namesTable.ChildNodes.Where(x => x.Name == "tr");
                string names = nameEntries.Select(x => x.ChildNodes.ElementAt(1).InnerText + " - " + x.ChildNodes.ElementAt(2).InnerText).Combine("\n");

                DiscordNETWrapper.SendEmbed(DiscordNETWrapper.
                    CreateEmbedBuilder(charName, names, charImg, null, "").
                    AddFieldDirectly("Tierart", HttpUtility.HtmlDecode(tierart)).
                    AddFieldDirectly("Persönlichkeit", HttpUtility.HtmlDecode(personality)).
                    AddFieldDirectly("Geschlecht", HttpUtility.HtmlDecode(gender)).
                    AddFieldDirectly("Geburtstag", HttpUtility.HtmlDecode(birthday)).
                    AddFieldDirectly("Floskel", HttpUtility.HtmlDecode(floskel)).
                    AddFieldDirectly("Fotospruch", HttpUtility.HtmlDecode(fotospruch)).
                    AddFieldDirectly("Auftreten", HttpUtility.HtmlDecode(auftreten)),
                    message.Channel).Wait();
            }
            catch
            {
                DiscordNETWrapper.SendText("Couldn't find that villager", message.Channel).Wait();
                return;
            }
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
