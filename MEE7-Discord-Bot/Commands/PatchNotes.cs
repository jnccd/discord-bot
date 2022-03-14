using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using MEE7.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace MEE7.Commands
{
    public class PatchNotes : Command
    {
        public PatchNotes() : base("togglePatchNotes", "Get annoying messages", false, true)
        {
            Program.OnConnected += OnConnected;
        }

        public void OnConnected()
        {
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            List<string> PatchNotes = new List<string>();

            HtmlWeb web = new HtmlWeb();
            var searchDoc = web.Load("https://github.com/niklasCarstensen/Discord-Bot/commits/master");

            var commits = searchDoc.DocumentNode.SelectNodes("//a[contains(@class, 'Link--primary text-bold js-navigation-open markdown-title')]");

            foreach (var commit in commits)
            {
                if (commit.InnerText == Config.Data.LastCommitMessage)
                    break;

                PatchNotes.Add($"[{commit.InnerText}](https://github.com{commit.GetAttributeValue("href", "")})");
            }

            if (commits.Count > 0)
                Config.Data.LastCommitMessage = commits.First().InnerText;
            Config.Save();

            PatchNotes.Reverse();
            if (PatchNotes.Count > 0)
            {
#if !DEBUG
                EmbedBuilder Embed = new EmbedBuilder { Title = "Patch Notes:" };
                Embed.WithThumbnailUrl("https://community.canvaslms.com/community/image/2043/2.png?a=1646");
                Embed.WithDescription(PatchNotes.Aggregate((x, y) => x + "\n" + y));
                foreach (ulong id in Config.Data.PatchNoteSubscribedChannels)
                    DiscordNETWrapper.SendEmbed(Embed, (ISocketMessageChannel)Program.GetChannelFromID(id)).Wait();
#else
                ConsoleWrapper.WriteLine("Patch Notes:" + PatchNotes.Aggregate((x, y) => x + "\n" + y), ConsoleColor.Cyan);
#endif
            }
        }
        bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, 
            System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public override void Execute(IMessage commandmessage)
        {
            if (commandmessage.Channel is SocketGuildChannel)
            {
                if (commandmessage.Author.Id == Program.GetGuildFromChannel(commandmessage.Channel).OwnerId || commandmessage.Author.Id == Program.Master.Id)
                {
                    if (Config.Data.PatchNoteSubscribedChannels.Contains(commandmessage.Channel.Id))
                    {
                        Config.Data.PatchNoteSubscribedChannels.Remove(commandmessage.Channel.Id);
                        DiscordNETWrapper.SendText("Canceled Patch Note subscription for this channel!", commandmessage.Channel).Wait();
                    }
                    else
                    {
                        Config.Data.PatchNoteSubscribedChannels.Add(commandmessage.Channel.Id);
                        DiscordNETWrapper.SendText("Subscribed to Patch Notes!", commandmessage.Channel).Wait();
                    }
                }
                else
                {
                    DiscordNETWrapper.SendText("Only the server/bot owner is authorized to use this command!", commandmessage.Channel).Wait();
                }
            }
            else
            {
                DiscordNETWrapper.SendText("You can't use this in DMs", commandmessage.Channel).Wait();
            }
        }
    }
}
