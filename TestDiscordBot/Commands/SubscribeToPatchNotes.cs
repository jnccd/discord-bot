using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TestDiscordBot.XML;

namespace TestDiscordBot.Commands
{
    public class SubscribeToPatchNotes : Command
    {
        public SubscribeToPatchNotes() : base("togglePatchNotes", "Get notified when a new bot version is published.", false)
        {

        }

        public override void onConnected()
        {
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string url = "https://github.com/niklasCarstensen/Discord-Bot";
            WebRequest req = HttpWebRequest.Create(url);
            req.Timeout = 10000;
            WebResponse W = req.GetResponse();
            using (StreamReader sr = new StreamReader(W.GetResponseStream()))
            {
                string html = sr.ReadToEnd();
                string message = html.GetEverythingBetween("<a data-pjax=\"true\" title=\"", "\" class=\"message\"");
                string link = "https://github.com/niklasCarstensen/Discord-Bot/commit/" + html.GetEverythingBetween("href=\"/niklasCarstensen/Discord-Bot/commit/", "\">");

                if (message != config.Data.LastCommitMessage && message != "")
                {
                    config.Data.LastCommitMessage = message;
                    config.Save();

                    foreach (ulong id in config.Data.PatchNoteSubscribedChannels)
                    {
                        try
                        {
                            
                            EmbedBuilder Embed = new EmbedBuilder();
                            Embed.WithColor(0, 128, 255);
                            Embed.AddField("Patch Notes:", message + "\n[Link to the github-commit.](" + link + ")");
                            Embed.WithThumbnailUrl("https://community.canvaslms.com/community/image/2043/2.png?a=1646");
                            Global.SendEmbed(Embed, (ISocketMessageChannel)Global.P.getChannelFromID(id));
                        } catch (Exception e) {
                            Console.WriteLine(e); }
                    }
                }
            }
        }
        bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public override async Task execute(SocketMessage commandmessage)
        {
            if (commandmessage.Author.Id == Global.P.getGuildFromChannel(commandmessage.Channel).OwnerId || commandmessage.Author.Id == Global.Master.Id)
            {
                if (config.Data.PatchNoteSubscribedChannels.Contains(commandmessage.Channel.Id))
                {
                    config.Data.PatchNoteSubscribedChannels.Remove(commandmessage.Channel.Id);
                    await Global.SendText("Canceled Patch Note subscription for this channel!", commandmessage.Channel);
                }
                else
                {
                    config.Data.PatchNoteSubscribedChannels.Add(commandmessage.Channel.Id);
                    await Global.SendText("Subscribed to Patch Notes!", commandmessage.Channel);
                }
            }
            else
            {
                await Global.SendText("Only the server/bot owner is authorized to use this command!", commandmessage.Channel);
            }
        }
    }
}
