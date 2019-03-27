using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TestDiscordBot.Commands
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
                    if (tuple.Item1 == Config.Config.Data.LastCommitMessage)
                        break;

                    foreach (ulong id in Config.Config.Data.PatchNoteSubscribedChannels)
                    {
                        try
                        {
                            EmbedBuilder Embed = new EmbedBuilder();
                            Embed.WithColor(0, 128, 255);
                            Embed.AddField("Patch Notes:", tuple.Item1 + "\n[Link to the github-commit.](" + tuple.Item2 + ")");
                            Embed.WithThumbnailUrl("https://community.canvaslms.com/community/image/2043/2.png?a=1646");
#if !DEBUG
                            Program.SendEmbed(Embed, (ISocketMessageChannel)Program.GetChannelFromID(id)).Wait();
#else
                            Extensions.ConvertToDouble("Patch Notes:" + tuple.Item1 + "\n[Link to the github-commit.](" + tuple.Item2 + ")", ConsoleColor.Cyan);
#endif
                        }
                        catch (Exception e)
                        {
                            e.ToString().ConsoleWriteLine(ConsoleColor.Red);
                        }
                    }
                }

                if (messages.Count > 0)
                    Config.Config.Data.LastCommitMessage = messages.First().Item1;
                Config.Config.Save();
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
                    if (Config.Config.Data.PatchNoteSubscribedChannels.Contains(commandmessage.Channel.Id))
                    {
                        Config.Config.Data.PatchNoteSubscribedChannels.Remove(commandmessage.Channel.Id);
                        await Program.SendText("Canceled Patch Note subscription for this channel!", commandmessage.Channel);
                    }
                    else
                    {
                        Config.Config.Data.PatchNoteSubscribedChannels.Add(commandmessage.Channel.Id);
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
