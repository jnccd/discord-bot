using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace MEE7.Backend.Configuration
{
    public class DiscordEmote
    {
        public Tuple<string, ulong> e = null; 
        public string oji = null;

        public string Name
        {
            get
            {
                if (e != null)
                    return e.Item1;
                else
                    return oji;
            }
        }

        public IEmote ToIEmote()
        {
            if (e != null)
                return Emote.Parse($"<:{e.Item1}:{e.Item2}>");
            else
                return new Emoji(oji);
        }
        public static DiscordEmote FromIEmote(IEmote e)
        {
            DiscordEmote de = new DiscordEmote();
            if (e is Emote)
                de.e = new Tuple<string, ulong>(e.Name, (e as Emote).Id);
            else
                de.oji = e.Name;
            return de;
        }
    }
}
