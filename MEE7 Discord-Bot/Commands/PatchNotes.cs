using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MEE7.Configuration;

namespace MEE7.Commands
{
    public class PatchNotes : Command
    {
        public PatchNotes() : base("togglePatchNotes", "Get annoying messages", false)
        {

        }

        public override void OnConnected()
        {
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            
            List<string> PatchNotes = new List<string>();

            string url = "https://github.com/niklasCarstensen/Discord-Bot/commits/master";
            WebRequest req = HttpWebRequest.Create(url);
            req.Timeout = 10000;
            WebResponse W = req.GetResponse();
            using (StreamReader sr = new StreamReader(W.GetResponseStream()))
            {
                string html = sr.ReadToEnd();
                List<Tuple<string, string>> messages = html.
                    GetEverythingBetweenAll("<p class=\"commit-title h5 mb-1 text-gray-dark \">", "</p>").
                    Select(x => new Tuple<string, string>(x.GetEverythingBetween("aria-label=\"", "\" "), 
                                                          "https://github.com" + x.GetEverythingBetween("href=\"", "\">"))).ToList();

                foreach (Tuple<string, string> tuple in messages)
                {
                    if (tuple.Item1 == Config.Data.LastCommitMessage)
                        break;

                    PatchNotes.Add($"{tuple.Item1}\n[Link to the github-commit.]({tuple.Item2})");
                }

                if (messages.Count > 0)
                    Config.Data.LastCommitMessage = messages.First().Item1;
                Config.Save();
            }

            PatchNotes.Reverse();
            if (PatchNotes.Count > 0)
            {
#if !DEBUG
                EmbedBuilder Embed = new EmbedBuilder { Title = "Patch Notes:" };
                Embed.WithThumbnailUrl("https://community.canvaslms.com/community/image/2043/2.png?a=1646");
                Embed.WithDescription(PatchNotes.Aggregate((x, y) => x + "\n\n" + y));
                foreach (ulong id in Config.Data.PatchNoteSubscribedChannels)
                    Program.SendEmbed(Embed, (ISocketMessageChannel)Program.GetChannelFromID(id)).Wait();
#else
                Program.ConsoleWriteLine("Patch Notes:" + PatchNotes.Aggregate((x, y) => x + "\n" + y), ConsoleColor.Cyan);
#endif
            }
        }
        bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public override async Task Execute(SocketMessage commandmessage)
        {
            if (commandmessage.Channel is SocketGuildChannel)
            {
                if (commandmessage.Author.Id == Program.GetGuildFromChannel(commandmessage.Channel).OwnerId || commandmessage.Author.Id == Program.Master.Id)
                {
                    if (Config.Data.PatchNoteSubscribedChannels.Contains(commandmessage.Channel.Id))
                    {
                        Config.Data.PatchNoteSubscribedChannels.Remove(commandmessage.Channel.Id);
                        await Program.SendText("Canceled Patch Note subscription for this channel!", commandmessage.Channel);
                    }
                    else
                    {
                        Config.Data.PatchNoteSubscribedChannels.Add(commandmessage.Channel.Id);
                        await Program.SendText("Subscribed to Patch Notes!", commandmessage.Channel);
                    }
                }
                else
                {
                    await Program.SendText("Only the server/bot owner is authorized to use this command!", commandmessage.Channel);
                }
            }
            else
            {
                await Program.SendText("You can't use this in DMs", commandmessage.Channel);
            }
        }
    }
}
