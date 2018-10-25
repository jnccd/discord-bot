using Discord;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TestDiscordBot.XML;

namespace TestDiscordBot.Commands
{
    public class Profile : Command
    {
        public Profile() : base("profile", "Prints your bot profile. (DSGVO‎-Style)", false)
        {

        }

        public override async Task execute(SocketMessage commandmessage)
        {
            DiscordUser User = config.Data.UserList.Find(x => x.UserID == commandmessage.Author.Id);
            EmbedBuilder Embed = new EmbedBuilder();
            Embed.WithColor(0, 128, 255);

            FieldInfo[] Infos = typeof(DiscordUser).GetFields();
            foreach (FieldInfo info in Infos)
            {
                if (info.FieldType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(info.FieldType))
                {
                    IEnumerable a = (IEnumerable)info.GetValue(User);
                    IEnumerator e = a.GetEnumerator();
                    string value = "";
                    e.Reset();
                    while (e.MoveNext())
                        value += e.Current + "\n";
                    Embed.AddField(info.Name + ":", value);
                }
                else
                    Embed.AddField(info.Name + ":", info.GetValue(User));
            }
            
            Embed.WithDescription("Profile of " + commandmessage.Author.Mention);
            await Global.SendEmbed(Embed, commandmessage.Channel);
        }
    }
}
