using Discord;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEE7.Commands
{
    class EmbedCommand : Command
    {
        public EmbedCommand() : base("embed", "Posts an embed", false, true) 
        {
            HelpMenu = DiscordNETWrapper.CreateEmbedBuilder("How to embed:", $"Example: `{PrefixAndCommand} hi im a title;" +
                $"hewwo im a desc;https://i.ytimg.com/vi/YWcrfp_dXKM/maxresdefault.jpg; 300699566041202699 ; " +
                $"https://pbs.twimg.com/profile_images/1092203417357844480/2fKiSxZW_400x400.jpg;Im 1 field;hi;Im 2 field;ho`");
        }

        public override void Execute(IMessage message)
        {
            string[] split = message.Content.Split(" ").Skip(1).Combine(" ").Split(";");

            if (split.Length == 0)
                DiscordNETWrapper.SendText("Gimme Cowntent >:C", message.Channel).Wait();
            else
            {
                string title = "", desc = "", imgURL = "", author = message.Author.ToString(), thumbURL = "";
                List<Tuple<string, string>> fields = new List<Tuple<string, string>>();
                try
                {
                    title = split[0].Trim(' ');
                    desc = split[1].Trim(' ');
                    imgURL = split[2].Trim(' ');
                    author = split[3].Trim(' ');
                    thumbURL = split[4].Trim(' ');
                    for (int i = 5;;)
                        fields.Add(new Tuple<string, string>(split[i++].Trim(' '), split[i++].Trim(' ')));
                }
                catch { }
                var iauthor = DiscordNETWrapper.ParseUser(author, message.Channel);
                var embed = DiscordNETWrapper.CreateEmbedBuilder(title, desc, imgURL, iauthor, thumbURL);
                foreach (var f in fields)
                    embed.AddFieldDirectly(f.Item1, f.Item2);
                DiscordNETWrapper.SendEmbed(embed, message.Channel).Wait();
            }
        }
    }
}
