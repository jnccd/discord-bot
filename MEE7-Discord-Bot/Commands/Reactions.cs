using Discord;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System;
using System.Linq;

namespace MEE7.Commands
{
    public class Reactions : Command
    {
        Reaction[] reactions;

        public Reactions() : base("reaction", "Post reaction images", false)
        {
            reactions = new Reaction[]
            {
                // TODO: Add new reactions here
                new Reaction("You sure", "Send this if some people are too sure about themselves.", (message) => {
                    DiscordNETWrapper.SendText("https://cdn.discordapp.com/attachments/473991188974927884/510947142127452172/sure2smol.gif", message.Channel).Wait(); }),
                new Reaction("wat", "Send this if wat.", (message) => {
                    DiscordNETWrapper.SendText("https://cdn.discordapp.com/attachments/473991188974927884/510954862616379403/Screenshot_9223371422766423856_opera.png", message.Channel).Wait(); }),
                new Reaction("You cant scare me", "Send this if you cant be scared.", (message) => {
                    DiscordNETWrapper.SendText("https://youtu.be/ug2aoVZYgaU?t=157", message.Channel).Wait(); }),
                new Reaction("Who are you", "Send this if someone wants to know who you are.", (message) => {
                    DiscordNETWrapper.SendText("https://youtu.be/ug2aoVZYgaU?t=246", message.Channel).Wait(); }),
                new Reaction("I dont believe it", "Send this if you dont believe something.", (message) => {
                    DiscordNETWrapper.SendText("https://youtu.be/ug2aoVZYgaU?t=460", message.Channel).Wait(); }),
                new Reaction("I didnt say it would be easy", "Send this if something isnt easy.", (message) => {
                    DiscordNETWrapper.SendText("https://youtu.be/ug2aoVZYgaU?t=491", message.Channel).Wait(); }),
                new Reaction("we got em R2", "Send this if you got em.", (message) => {
                    DiscordNETWrapper.SendText("https://youtu.be/GQhKBn42XcU?t=24", message.Channel).Wait(); }),
                new Reaction("I came in like a wrecking ball", "Send this if you came in like a wrecking ball.", (message) => {
                    DiscordNETWrapper.SendText("https://youtu.be/My2FRPA3Gf8?t=93", message.Channel).Wait(); }),
                new Reaction("Shitty Mario", "Send this if you shitty.", (message) => {
                    DiscordNETWrapper.SendText("https://www.youtube.com/watch?v=x74bZjDYUTE", message.Channel).Wait(); }),
                new Reaction("Oh my gaaaa", "Send this if oh my gaaaaa.", (message) => {
                    DiscordNETWrapper.SendText("https://youtu.be/UnktCDi-BVs?t=9", message.Channel).Wait(); }),
                new Reaction("heh", "Send this if heh.", (message) => {
                    DiscordNETWrapper.SendText("https://pbs.twimg.com/media/DuPdNCOV4AA20F6?format=jpg&name=small", message.Channel).Wait(); })
            };
        }

        public override void Execute(IMessage message)
        {
            string[] split = message.Content.Split(new char[] { ' ', '\n' });
            if (split.Length == 1)
            {
                EmbedBuilder Embed = new EmbedBuilder();
                Embed.WithColor(0, 128, 255);
                foreach (Reaction react in reactions)
                {
                    Embed.AddFieldDirectly(Prefix + CommandLine + " " + react.name, react.desc);
                }
                Embed.WithDescription("Reactions:");
                DiscordNETWrapper.SendEmbed(Embed, message.Channel).Wait();
            }
            else
            {
                string command = split.Skip(1).Aggregate((x, y) => x + " " + y).ToLower();
                int[] scores = new int[reactions.Length];
                for (int i = 0; i < reactions.Length; i++)
                {
                    scores[i] = command.LevenshteinDistance(reactions[i].name.ToLower());
                }
                int index = Array.IndexOf(scores, scores.Min());
                reactions[index].execute(message);
            }
        }

        class Reaction
        {
            public delegate void Send(IMessage message);
            public string name, desc;
            public Send execute;

            public Reaction(string name, string desc, Send execute)
            {
                this.name = name;
                this.desc = desc;
                this.execute = execute;
            }
        }
    }
}
