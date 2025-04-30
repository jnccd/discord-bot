﻿using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MEE7.Commands
{
    class Reacter : Command
    {
        bool loaded = false;
        readonly Dictionary<string, IEmote> emoteDict = new Dictionary<string, IEmote>();

        public Reacter()
        {
            Program.OnConnected += Program_OnConnected;
            Program.OnNonCommandMessageRecieved += OnNonCommandMessageRecieved;
        }

        private void Program_OnConnected()
        {
            emoteDict.Add("kenobi", Emote.Parse("<a:general_kenobi:729800823239999498>"));
            emoteDict.Add("padoru", Emote.Parse("<a:padoru:744966713778372778>"));
            emoteDict.Add("hentai", Emote.Parse("<a:FeelsHentaiMan:744966726848086168>"));
            emoteDict.Add("sosig", Emote.Parse("<a:sosig:746025962248077352>"));
            emoteDict.Add("spooky", Emote.Parse("<a:spooky:754856306476580965>"));
            emoteDict.Add("goodMorning", Emote.Parse("<a:GoodMorning:757939668221427742>"));
            emoteDict.Add("goodNight", Emote.Parse("<a:GoodNight:801591068571336705>")); 
            emoteDict.Add("no", Emote.Parse("<:no:778447862735568916>"));
            emoteDict.Add("sus", Emote.Parse("<:sussy_baka:1022741633923551232>"));

            emoteDict.Add("eyes", new Emoji("👀"));
            emoteDict.Add("wave", new Emoji("👋"));

            for (int i = 'A'; i <= 'Z'; i++)
                emoteDict.Add(((char)i).ToString(), new Emoji(char.ConvertFromUtf32(0x1F1E6 + i - 'A')));

            loaded = true;
        }

        private void OnNonCommandMessageRecieved(IMessage messageIn)
        {
            if (!loaded)
                return;
            if (!(messageIn is SocketMessage))
                return;
            var message = messageIn as SocketMessage;

            if (message.Content.ToLower().Contains("hello there"))
                message.AddReactionAsync(emoteDict.GetValueOrDefault("kenobi")).Wait();

            if (message.Content.ToLower().Contains("padoru"))
                message.AddReactionAsync(emoteDict.GetValueOrDefault("padoru")).Wait();

            if (message.Content.ToLower().Contains("hentai"))
                message.AddReactionAsync(emoteDict.GetValueOrDefault("hentai")).Wait();

            if (message.Content.ToLower().Contains("i saw that"))
                message.AddReactionAsync(emoteDict.GetValueOrDefault("eyes")).Wait();

            if (message.Content.ToLower().Contains("the sauce"))
                message.AddReactionAsync(emoteDict.GetValueOrDefault("sosig")).Wait();

            if (message.Content.ToLower().Contains("spooky"))
                message.AddReactionAsync(emoteDict.GetValueOrDefault("spooky")).Wait();

            if (message.Author.Id == 501691245790232596)
                message.AddReactionAsync(emoteDict.GetValueOrDefault("no")).Wait();

            if (Regex.IsMatch(message.Content.ToLower(), "\\bgu+ten mo+rg[e,ä]+n\\b") ||
                Regex.IsMatch(message.Content.ToLower(), "\\bgoo+d mo+rning\\b") ||
                Regex.IsMatch(message.Content.ToLower(), "\\bmo+in\\b") ||
                message.Content == "早上好" || message.Content == "早好")
                message.AddReactionAsync(emoteDict.GetValueOrDefault("goodMorning")).Wait();

            if (Regex.IsMatch(message.Content.ToLower(), "\\bgu+te nacht\\b") ||
                Regex.IsMatch(message.Content.ToLower(), "\\bschlaft gut\\b") ||
                Regex.IsMatch(message.Content.ToLower(), "\\bgoo+d night\\b"))
                message.AddReactionAsync(emoteDict.GetValueOrDefault("goodNight")).Wait();

            if (Regex.IsMatch(message.Content.ToLower(), "\\bhi+\\b"))
                message.AddReactionAsync(emoteDict.GetValueOrDefault("wave")).Wait();

            if (message.Content.ToLower().Contains("hotel"))
                (message as IUserMessage).AddReactionsAsync(new IEmote[] { 
                    emoteDict.GetValueOrDefault("T"),
                    emoteDict.GetValueOrDefault("R"),
                    emoteDict.GetValueOrDefault("I"),
                    emoteDict.GetValueOrDefault("V"),
                    emoteDict.GetValueOrDefault("A"),
                    emoteDict.GetValueOrDefault("G"),
                    emoteDict.GetValueOrDefault("O"), }).Wait();

            if (message.Content.ToLower().Contains("brille"))
                (message as IUserMessage).AddReactionsAsync(new IEmote[] {
                    emoteDict.GetValueOrDefault("F"),
                    emoteDict.GetValueOrDefault("I"),
                    emoteDict.GetValueOrDefault("E"),
                    emoteDict.GetValueOrDefault("L"),
                    emoteDict.GetValueOrDefault("M"),
                    emoteDict.GetValueOrDefault("A"),
                    emoteDict.GetValueOrDefault("N"), }).Wait();

            if (message.Content.ToLower().Contains("sus"))
                message.AddReactionAsync(emoteDict.GetValueOrDefault("sus")).Wait();

        }

        public override void Execute(IMessage message) { }
    }
}
