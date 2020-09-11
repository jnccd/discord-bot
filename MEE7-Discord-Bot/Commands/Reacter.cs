using Discord;
using Discord.WebSocket;
using MEE7.Backend;
using System;
using System.Collections.Generic;

namespace MEE7.Commands
{
    class Reacter : Command
    {
        readonly Dictionary<string, IEmote> emoteDict = new Dictionary<string, IEmote>();

        public Reacter()
        {
            Program.OnConnected += Program_OnConnected;
            Program.OnNonCommandMessageRecieved += OnNonCommandMessageRecieved;
        }

        private void Program_OnConnected()
        {
            emoteDict.Add("kenobi", Emote.Parse("<a:general_kenobi:729800823239999498>"));
            emoteDict.Add("padoru", Emote.Parse("<a:padoru:744966861648560277>"));
            emoteDict.Add("hentai", Emote.Parse("<a:FeelsHentaiMan:744966841402916925>"));
            emoteDict.Add("sosig", Emote.Parse("<a:sosig:746026002119131308>"));

            emoteDict.Add("eyes", new Emoji("👀"));

            for (int i = 'A'; i <= 'Z'; i++)
                emoteDict.Add(((char)i).ToString(), new Emoji(char.ConvertFromUtf32(0x1F1E6 + i - 'A')));
        }

        private void OnNonCommandMessageRecieved(IMessage messageIn)
        {
            if (!(messageIn is SocketMessage))
                return;
            var message = messageIn as SocketMessage;

            if (message.Content.Contains("Hello there", StringComparison.OrdinalIgnoreCase))
                message.AddReactionAsync(emoteDict.GetValueOrDefault("kenobi")).Wait();
            if (message.Content.Contains("Padoru", StringComparison.OrdinalIgnoreCase))
                message.AddReactionAsync(emoteDict.GetValueOrDefault("padoru")).Wait();
            if (message.Content.Contains("Hentai", StringComparison.OrdinalIgnoreCase))
                message.AddReactionAsync(emoteDict.GetValueOrDefault("hentai")).Wait();
            if (message.Content.Contains("I saw that", StringComparison.OrdinalIgnoreCase))
                message.AddReactionAsync(emoteDict.GetValueOrDefault("eyes")).Wait();
            if (message.Content.Contains("the sauce", StringComparison.OrdinalIgnoreCase))
                message.AddReactionAsync(emoteDict.GetValueOrDefault("sosig")).Wait();
            if (message.Content.Contains("Hotel", StringComparison.OrdinalIgnoreCase))
                (message as IUserMessage).AddReactionsAsync(new IEmote[] { 
                    emoteDict.GetValueOrDefault("T"),
                    emoteDict.GetValueOrDefault("R"),
                    emoteDict.GetValueOrDefault("I"),
                    emoteDict.GetValueOrDefault("V"),
                    emoteDict.GetValueOrDefault("A"),
                    emoteDict.GetValueOrDefault("G"),
                    emoteDict.GetValueOrDefault("O"), }).Wait();
        }

        public override void Execute(IMessage message) { }
    }
}
