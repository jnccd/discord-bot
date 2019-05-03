using Discord;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MEE7.Configuration;

namespace MEE7.Commands
{
    public class Profile : Command
    {
        public Profile() : base("profile", "Prints your bot profile (GDPR‎-Style)", false)
        {

        }

        public override async Task Execute(SocketMessage commandmessage)
        {
            EmbedBuilder Embed = new EmbedBuilder();
            Embed.WithColor(0, 128, 255);

            if (Config.Data.UserList.Exists(x => x.UserID == commandmessage.Author.Id))
            {
                DiscordUser User = Config.Data.UserList.Find(x => x.UserID == commandmessage.Author.Id);
                FieldInfo[] Infos = typeof(DiscordUser).GetFields();
                foreach (FieldInfo info in Infos)
                {
                    if (info.FieldType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(info.FieldType))
                    {
                        IEnumerable a = (IEnumerable)info.GetValue(User);
                        string value = "";
                        try
                        {
                            IEnumerator e = a.GetEnumerator();
                            e.Reset();
                            while (e.MoveNext())
                                value += e.Current + "\n";
                        }
                        catch { }
                        Embed.AddField(info.Name + ":", value == "" ? "null" : value);
                    }
                    else
                        Embed.AddField(info.Name + ":", info.GetValue(User));
                }

                Embed.WithDescription("Profile of " + commandmessage.Author.Mention);
            }
            else
            {
                Embed.AddField("Error!", "The bot hasn't made a profile of you yet.");
            }
            await Program.SendEmbed(Embed, commandmessage.Channel);
        }
    }
}
